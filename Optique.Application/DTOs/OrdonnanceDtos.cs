namespace Optique.Application.DTOs;

public sealed record OrdonnanceCreateDto(
    int ClientId,
    int? MedecinId,
    DateTime DateOrdonnance,
    // Loin
    decimal? Loin_OD_Sph, decimal? Loin_OD_Cyl, int? Loin_OD_Axe,
    decimal? Loin_OG_Sph, decimal? Loin_OG_Cyl, int? Loin_OG_Axe,
    // Près
    decimal? Pres_OD_Sph, decimal? Pres_OD_Cyl, int? Pres_OD_Axe,
    decimal? Pres_OG_Sph, decimal? Pres_OG_Cyl, int? Pres_OG_Axe,
    decimal? ADD_PRES
);

public sealed record OrdonnanceDto(
    int Id, int ClientId, int? MedecinId, DateTime DateOrdonnance,
    decimal? Loin_OD_Sph, decimal? Loin_OD_Cyl, int? Loin_OD_Axe,
    decimal? Loin_OG_Sph, decimal? Loin_OG_Cyl, int? Loin_OG_Axe,
    decimal? Pres_OD_Sph, decimal? Pres_OD_Cyl, int? Pres_OD_Axe,
    decimal? Pres_OG_Sph, decimal? Pres_OG_Cyl, int? Pres_OG_Axe,
    decimal? ADD_PRES
);


