namespace Optique.Domain.Entities;

public class Facture
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public DateTime DateFacture { get; set; } = DateTime.UtcNow;

    public int ClientId { get; set; }
    public int? MedecinId { get; set; }
    public int? OrdonnanceId { get; set; }

    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }
    public string? MontantLettres { get; set; }
    public string? Notes { get; set; }

    public Client Client { get; set; } = null!;
    public Medecin? Medecin { get; set; }
    public Ordonnance? Ordonnance { get; set; }
    public ICollection<LigneFacture> Lignes { get; set; } = new List<LigneFacture>();
}
