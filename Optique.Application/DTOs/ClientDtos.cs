namespace Optique.Application.DTOs;

public sealed record ClientCreateDto(
    string? Civilite,
    string Nom,
    string? Prenom,
    string? Telephone,
    string? Email,
    string? Adresse
);

public sealed record ClientDto(
    int Id,
    string? Civilite,
    string Nom,
    string? Prenom,
    string? Telephone,
    string? Email,
    string? Adresse
);
