namespace Optique.Domain.Entities;

public class Ordonnance
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? MedecinId { get; set; }
    public DateTime DateOrdonnance { get; set; }

    // Vision Loin
    public decimal? Loin_OD_Sph { get; set; }
    public decimal? Loin_OD_Cyl { get; set; }
    public int? Loin_OD_Axe { get; set; }
    public decimal? Loin_OG_Sph { get; set; }
    public decimal? Loin_OG_Cyl { get; set; }
    public int? Loin_OG_Axe { get; set; }

    // Vision Près
    public decimal? Pres_OD_Sph { get; set; }
    public decimal? Pres_OD_Cyl { get; set; }
    public int? Pres_OD_Axe { get; set; }
    public decimal? Pres_OG_Sph { get; set; }
    public decimal? Pres_OG_Cyl { get; set; }
    public int? Pres_OG_Axe { get; set; }
    public decimal? ADD_PRES { get; set; }

    public Client Client { get; set; } = null!;
    public Medecin? Medecin { get; set; }
    public string? PhotoUrl { get; set; }   // lien public/chemin relatif du fichier

}
