namespace Optique.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string? Civilite { get; set; }
    public string Nom { get; set; } = null!;
    public string? Prenom { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? Adresse { get; set; }
    
    public ICollection<Ordonnance> Ordonnances { get; set; } = new List<Ordonnance>();
    public ICollection<Facture> Factures { get; set; } = new List<Facture>();
}
