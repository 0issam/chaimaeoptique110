using Microsoft.EntityFrameworkCore;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;
using Optique.Domain.Entities;
using Optique.Infrastructure.Data;

namespace Optique.Infrastructure.Services;

public sealed class MedecinService : IMedecinService
{
    private readonly OptiqueDbContext _db;
    public MedecinService(OptiqueDbContext db) => _db = db;

    public async Task<int> CreateAsync(MedecinCreateDto dto, CancellationToken ct = default)
    {
        var e = new Medecin { Nom = dto.Nom, Prenom = dto.Prenom, NumeroPro = dto.NumeroPro, Telephone = dto.Telephone };
        _db.Medecins.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<MedecinDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Medecins.FindAsync([id], ct);
        return e is null ? null : new MedecinDto(e.Id, e.Nom, e.Prenom, e.NumeroPro, e.Telephone);
    }

    public async Task<List<MedecinDto>> ListAsync(string? q = null, CancellationToken ct = default)
    {
        var query = _db.Medecins.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => m.Nom.Contains(q) || (m.Prenom ?? "").Contains(q));
        return await query
            .OrderBy(m => m.Nom).ThenBy(m => m.Prenom)
            .Select(e => new MedecinDto(e.Id, e.Nom, e.Prenom, e.NumeroPro, e.Telephone))
            .ToListAsync(ct);
    }
}
