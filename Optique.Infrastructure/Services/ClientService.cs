using Microsoft.EntityFrameworkCore;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;
using Optique.Domain.Entities;
using Optique.Infrastructure.Data;

namespace Optique.Infrastructure.Services;

public sealed class ClientService : IClientService
{
    private readonly OptiqueDbContext _db;
    public ClientService(OptiqueDbContext db) => _db = db;

    public async Task<int> CreateAsync(ClientCreateDto dto, CancellationToken ct = default)
    {
        var e = new Client {
            Civilite = dto.Civilite, Nom = dto.Nom, Prenom = dto.Prenom,
            Telephone = dto.Telephone, Email = dto.Email, Adresse = dto.Adresse
        };
        _db.Clients.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<ClientDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Clients.FindAsync([id], ct);
        return e is null ? null :
            new ClientDto(e.Id, e.Civilite, e.Nom, e.Prenom, e.Telephone, e.Email, e.Adresse);
    }

    public async Task<List<ClientDto>> ListAsync(string? q = null, CancellationToken ct = default)
    {
        var query = _db.Clients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Nom.Contains(q) || (c.Prenom ?? "").Contains(q));
        return await query
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .Select(e => new ClientDto(e.Id, e.Civilite, e.Nom, e.Prenom, e.Telephone, e.Email, e.Adresse))
            .ToListAsync(ct);
    }
}
