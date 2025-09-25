using Microsoft.EntityFrameworkCore;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;   // <-- IMPORTANT
using Optique.Domain.Entities;
using Optique.Infrastructure.Data;

namespace Optique.Infrastructure.Services;

public sealed class OrdonnanceService : IOrdonnanceService
{
    private readonly OptiqueDbContext _db;
    public OrdonnanceService(OptiqueDbContext db) => _db = db;

    public async Task<int> CreateAsync(OrdonnanceCreateDto dto, CancellationToken ct = default)
    {
        _ = await _db.Clients.FirstOrDefaultAsync(x => x.Id == dto.ClientId, ct)
            ?? throw new InvalidOperationException("Client introuvable.");
        if (dto.MedecinId is not null)
            _ = await _db.Medecins.FirstOrDefaultAsync(x => x.Id == dto.MedecinId, ct)
                ?? throw new InvalidOperationException("Médecin introuvable.");

        var e = new Ordonnance
        {
            ClientId = dto.ClientId,
            MedecinId = dto.MedecinId,
            DateOrdonnance = dto.DateOrdonnance == default ? DateTime.Today : dto.DateOrdonnance,
            Loin_OD_Sph = dto.Loin_OD_Sph, Loin_OD_Cyl = dto.Loin_OD_Cyl, Loin_OD_Axe = dto.Loin_OD_Axe,
            Loin_OG_Sph = dto.Loin_OG_Sph, Loin_OG_Cyl = dto.Loin_OG_Cyl, Loin_OG_Axe = dto.Loin_OG_Axe,
            Pres_OD_Sph = dto.Pres_OD_Sph, Pres_OD_Cyl = dto.Pres_OD_Cyl, Pres_OD_Axe = dto.Pres_OD_Axe,
            Pres_OG_Sph = dto.Pres_OG_Sph, Pres_OG_Cyl = dto.Pres_OG_Cyl, Pres_OG_Axe = dto.Pres_OG_Axe,
            ADD_PRES = dto.ADD_PRES
        };

        _db.Ordonnances.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<OrdonnanceDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var o = await _db.Ordonnances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? null :
            new OrdonnanceDto(
                o.Id, o.ClientId, o.MedecinId, o.DateOrdonnance,
                o.Loin_OD_Sph, o.Loin_OD_Cyl, o.Loin_OD_Axe,
                o.Loin_OG_Sph, o.Loin_OG_Cyl, o.Loin_OG_Axe,
                o.Pres_OD_Sph, o.Pres_OD_Cyl, o.Pres_OD_Axe,
                o.Pres_OG_Sph, o.Pres_OG_Cyl, o.Pres_OG_Axe,
                o.ADD_PRES
            );
    }
}
