using Optique.Application.DTOs;

namespace Optique.Application.Interfaces;

public interface IFactureService
{
    Task<int> CreateAsync(FactureCreateDto dto, CancellationToken ct = default);
    Task<FactureDto?> GetAsync(int id, CancellationToken ct = default);
    Task<(byte[] Pdf, string Numero)?> BuildPdfAsync(int id, CancellationToken ct = default);


}
