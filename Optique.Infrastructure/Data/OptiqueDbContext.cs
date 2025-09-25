using Microsoft.EntityFrameworkCore;
using Optique.Domain.Entities; // ← IMPORTANT

namespace Optique.Infrastructure.Data;

public class OptiqueDbContext : DbContext
{
    public OptiqueDbContext(DbContextOptions<OptiqueDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Medecin> Medecins => Set<Medecin>();
    public DbSet<Ordonnance> Ordonnances => Set<Ordonnance>();
    public DbSet<Facture> Factures => Set<Facture>();
    public DbSet<LigneFacture> LignesFacture => Set<LigneFacture>();
    public DbSet<Param> Params => Set<Param>(); // ← ajoute ce DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Param>(e =>
        {
            e.HasIndex(x => x.Cle).IsUnique();
            e.Property(x => x.Cle).HasMaxLength(100).IsRequired();
            e.Property(x => x.Valeur).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Facture>()
            .HasIndex(f => f.Numero)
            .IsUnique();

        modelBuilder.Entity<LigneFacture>()
            .Property(l => l.Total)
            .HasComputedColumnSql("CAST([Qte] * [PrixUnitaire] AS DECIMAL(10,2))", stored: false);
    }
}
