namespace Optique.Domain.Entities;

public class Medecin
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public string? Prenom { get; set; }
    public string? NumeroPro { get; set; }
    public string? Telephone { get; set; }

    public ICollection<Ordonnance> Ordonnances { get; set; } = new List<Ordonnance>();
    public ICollection<Facture> Factures { get; set; } = new List<Facture>();
}
