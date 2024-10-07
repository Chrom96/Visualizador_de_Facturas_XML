using Microsoft.EntityFrameworkCore;
using Visualizador_de_Facturas_XML.Models;

namespace Visualizador_de_Facturas_XML.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Factura>()
            .HasOne(i => i.Perfil)
            .WithMany(p => p.Facturas)
            .HasForeignKey(i => i.PerfilID);
        }
        // DbSet que representan tablas en la base de datos
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
    }
}
