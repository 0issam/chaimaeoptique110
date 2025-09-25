namespace Optique.Domain.Entities;

public class LigneFacture
{
    public int Id { get; set; }
    public int FactureId { get; set; }

    public string Designation { get; set; } = null!;
    public int Qte { get; set; } = 1;
    public decimal PrixUnitaire { get; set; }
    public decimal Total { get; set; }

    public Facture Facture { get; set; } = null!;
}
