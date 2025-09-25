using Optique.Application.DTOs;

namespace Optique.Application.Interfaces;

public interface IClientService
{
    Task<int> CreateAsync(ClientCreateDto dto, CancellationToken ct = default);
    Task<ClientDto?> GetAsync(int id, CancellationToken ct = default);
    Task<List<ClientDto>> ListAsync(string? q = null, CancellationToken ct = default);
}
