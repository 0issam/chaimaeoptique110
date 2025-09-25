namespace Optique.Application.DTOs;

public sealed record LigneFactureCreateDto(string Designation, int Qte, decimal PrixUnitaire);

// public sealed record FactureCreateDto(
//     int ClientId,
//     int? MedecinId,
//     int? OrdonnanceId,                 // ➊ on peut passer un Id existant
//     OrdonnanceCreateDto? Ordonnance,   // ➋ ou bien créer inline
//     List<LigneFactureCreateDto> Lignes,
//     string? Notes
// );

public sealed record FactureCreateDto(
    int ClientId,
    int? MedecinId,
    int? OrdonnanceId,                 // ➊ on peut passer un Id existant
    string? Notes,
    List<LigneFactureCreateDto> Lignes,
    OrdonnanceCreateInlineDto? Ordonnance // ← optionnel
);
public sealed record OrdonnanceCreateInlineDto(
    int ClientId,
    int? MedecinId,
    DateTime DateOrdonnance,
    // LOIN
    decimal? Loin_OD_Sph, decimal? Loin_OD_Cyl, int? Loin_OD_Axe,
    decimal? Loin_OG_Sph, decimal? Loin_OG_Cyl, int? Loin_OG_Axe,
    // PRES
    decimal? Pres_OD_Sph, decimal? Pres_OD_Cyl, int? Pres_OD_Axe,
    decimal? Pres_OG_Sph, decimal? Pres_OG_Cyl, int? Pres_OG_Axe,
    decimal? ADD_PRES,
    string? PhotoUrl        // ← doc (image/PDF) uploadé
);
public sealed record LigneFactureDto(int Id, string Designation, int Qte, decimal PrixUnitaire, decimal Total);

public sealed record FactureDto(
    int Id,
    string Numero,
    DateTime DateFacture,
    int ClientId,
    int? MedecinId,
    int? OrdonnanceId,
    decimal TotalHT,
    decimal TotalTTC,
    string? MontantLettres,
    string? Notes,
    List<LigneFactureDto> Lignes
);
