using Optique.Application.DTOs;

namespace Optique.Application.Interfaces;

public interface IOrdonnanceService
{
    Task<int> CreateAsync(OrdonnanceCreateDto dto, CancellationToken ct = default);
    Task<OrdonnanceDto?> GetAsync(int id, CancellationToken ct = default);
}
