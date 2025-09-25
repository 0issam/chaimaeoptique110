namespace Optique.Application.DTOs;

public sealed record MedecinCreateDto(
    string Nom,
    string? Prenom,
    string? NumeroPro,
    string? Telephone
);

public sealed record MedecinDto(
    int Id,
    string Nom,
    string? Prenom,
    string? NumeroPro,
    string? Telephone
);
