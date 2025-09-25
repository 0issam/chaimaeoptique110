using Microsoft.EntityFrameworkCore;
using Optique.Domain.Entities;
using Optique.Infrastructure.Data;

namespace Optique.Infrastructure.Services;

public interface INumeroService
{
    Task<string> NextFactureNumeroAsync(CancellationToken ct = default);
}

public sealed class NumeroService : INumeroService
{
    private readonly OptiqueDbContext _db;
    private const string Key = "LAST_INVOICE_NUMBER";

    public NumeroService(OptiqueDbContext db) => _db = db;

    public async Task<string> NextFactureNumeroAsync(CancellationToken ct = default)
    {
        var row = await _db.Params.FirstOrDefaultAsync(x => x.Cle == Key, ct);
        var last = 0;
        if (row is null)
        {
            row = new Param { Cle = Key, Valeur = "0" };
            _db.Params.Add(row);
        }
        else
        {
            int.TryParse(row.Valeur, out last);
        }

        var next = last + 1;
        row.Valeur = next.ToString();
        await _db.SaveChangesAsync(ct);

        return next.ToString("D7"); // 0000001
    }
}
