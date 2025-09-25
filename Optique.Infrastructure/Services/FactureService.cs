using Microsoft.EntityFrameworkCore;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;
using Optique.Domain.Entities;
using Optique.Infrastructure.Data;
using Optique.Infrastructure.Utils; // <-- en haut du fichier


namespace Optique.Infrastructure.Services;

public sealed class FactureService : IFactureService
{
    private readonly OptiqueDbContext _db;
    private readonly INumeroService _numero;
    private readonly IPdfRenderer _pdf; // ← injecte-le


    public FactureService(OptiqueDbContext db, INumeroService numero, IPdfRenderer pdf)
    {
        _db = db;
        _numero = numero;
        _pdf = pdf;
    }


public async Task<(byte[] Pdf, string Numero)?> BuildPdfAsync(int id, CancellationToken ct = default)
{
    var f = await _db.Factures
        .Include(x => x.Lignes)
        .Include(x => x.Client)
        .Include(x => x.Medecin)
        .Include(x => x.Ordonnance)
        .FirstOrDefaultAsync(x => x.Id == id, ct);

    if (f is null) return null;

    // ⬇️ Recalcule si absent ou si l’ancienne valeur ressemble à un nombre
    if (string.IsNullOrWhiteSpace(f.MontantLettres) ||
        char.IsDigit(f.MontantLettres.TrimStart()[0]))
    {
        f.MontantLettres = AmountToWordsFr.ToDirhams(f.TotalTTC);
        // on ne persiste pas ici (pas de SaveChanges), juste pour l'affichage
    }

    var pdf = _pdf.RenderFacture(f, f.Client, f.Medecin, f.Ordonnance);
    return (pdf, f.Numero);
}


    public async Task<int> CreateAsync(FactureCreateDto dto, CancellationToken ct = default)
{
    // Vérifs client / médecin
    _ = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId, ct)
        ?? throw new InvalidOperationException("Client introuvable.");
    if (dto.MedecinId is not null)
        _ = await _db.Medecins.FirstOrDefaultAsync(m => m.Id == dto.MedecinId, ct)
            ?? throw new InvalidOperationException("Médecin introuvable.");

    // Si OrdonnanceId non fournie mais Ordonnance inline fournie => on crée
    int? ordonnanceId = dto.OrdonnanceId;
    if (ordonnanceId is null && dto.Ordonnance is not null)
    {
        var o = new Ordonnance
        {
            ClientId = dto.Ordonnance.ClientId,
            MedecinId = dto.Ordonnance.MedecinId,
            DateOrdonnance = dto.Ordonnance.DateOrdonnance == default ? DateTime.Today : dto.Ordonnance.DateOrdonnance,

            Loin_OD_Sph = dto.Ordonnance.Loin_OD_Sph,
            Loin_OD_Cyl = dto.Ordonnance.Loin_OD_Cyl,
            Loin_OD_Axe = dto.Ordonnance.Loin_OD_Axe,
            Loin_OG_Sph = dto.Ordonnance.Loin_OG_Sph,
            Loin_OG_Cyl = dto.Ordonnance.Loin_OG_Cyl,
            Loin_OG_Axe = dto.Ordonnance.Loin_OG_Axe,

            Pres_OD_Sph = dto.Ordonnance.Pres_OD_Sph,
            Pres_OD_Cyl = dto.Ordonnance.Pres_OD_Cyl,
            Pres_OD_Axe = dto.Ordonnance.Pres_OD_Axe,
            Pres_OG_Sph = dto.Ordonnance.Pres_OG_Sph,
            Pres_OG_Cyl = dto.Ordonnance.Pres_OG_Cyl,
            Pres_OG_Axe = dto.Ordonnance.Pres_OG_Axe,

            ADD_PRES = dto.Ordonnance.ADD_PRES,
            PhotoUrl = dto.Ordonnance.PhotoUrl
        };
        _db.Ordonnances.Add(o);
        await _db.SaveChangesAsync(ct);
        ordonnanceId = o.Id;
    }

    // Numérotation + totaux
    var numero = await _numero.NextFactureNumeroAsync(ct);
    decimal total = dto.Lignes.Sum(l => l.Qte * l.PrixUnitaire);

    var f = new Facture
    {
        Numero = numero,
        ClientId = dto.ClientId,
        MedecinId = dto.MedecinId,
        OrdonnanceId = ordonnanceId,
        Notes = dto.Notes,
        TotalHT = total,
        TotalTTC = total,
        MontantLettres  = AmountToWordsFr.ToDirhams(total)
    };

    foreach (var l in dto.Lignes)
    {
        f.Lignes.Add(new LigneFacture
        {
            Designation = l.Designation,
            Qte = l.Qte,
            PrixUnitaire = l.PrixUnitaire,
            Total = l.Qte * l.PrixUnitaire
        });
    }

    _db.Factures.Add(f);
    await _db.SaveChangesAsync(ct);
    return f.Id;
}


    public async Task<FactureDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var f = await _db.Factures
            .Include(x => x.Lignes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (f is null) return null;

        return new FactureDto(
            f.Id, f.Numero, f.DateFacture, f.ClientId, f.MedecinId, f.OrdonnanceId,
            f.TotalHT, f.TotalTTC, f.MontantLettres, f.Notes,
            f.Lignes.Select(l => new LigneFactureDto(l.Id, l.Designation, l.Qte, l.PrixUnitaire, l.Total)).ToList()
        );
    }


    // Version simple/fr-naïve pour démarrer
    private static string ConvertirMontantEnLettres(decimal montant)
        => $"{montant:0.00} dirhams"; // à remplacer par un convertisseur complet plus tard
}
