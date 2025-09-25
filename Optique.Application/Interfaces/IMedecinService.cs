using Optique.Application.DTOs;

namespace Optique.Application.Interfaces;

public interface IMedecinService
{
    Task<int> CreateAsync(MedecinCreateDto dto, CancellationToken ct = default);
    Task<MedecinDto?> GetAsync(int id, CancellationToken ct = default);
    Task<List<MedecinDto>> ListAsync(string? q = null, CancellationToken ct = default);
}
