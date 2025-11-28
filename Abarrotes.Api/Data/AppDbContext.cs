using Abarrotes.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Abarrotes.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<EstadoProducto> EstadosProducto { get; set; } = null!;
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<EstadoPago> EstadosPago => Set<EstadoPago>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Boleta> Boletas => Set<Boleta>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<NotaPedido> NotasPedido => Set<NotaPedido>();
    public DbSet<VentaRequest> VentaRequests => Set<VentaRequest>();
    public DbSet<ConsumoLote> ConsumosLote => Set<ConsumoLote>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();
    public DbSet<HistorialPrecio> HistorialPrecios { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Índices únicos
        mb.Entity<Producto>().HasIndex(p => p.CodigoBarras).IsUnique();
        mb.Entity<Boleta>().HasIndex(b => b.NumeroBoleta).IsUnique();
        mb.Entity<Factura>().HasIndex(f => f.NumeroFactura).IsUnique();

        // Idempotencia
        mb.Entity<VentaRequest>()
            .HasIndex(x => x.Key)
            .IsUnique();

        // Relaciones 1-1
        mb.Entity<Venta>()
            .HasOne(v => v.Boleta)
            .WithOne(b => b.Venta)
            .HasForeignKey<Boleta>(b => b.VentaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Venta>()
            .HasOne(v => v.Factura)
            .WithOne(f => f.Venta)
            .HasForeignKey<Factura>(f => f.VentaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Mapeo tabla detalleventas
        mb.Entity<DetalleVenta>().ToTable("detalleventas");

        // Precisión
        mb.Entity<DetalleVenta>().Property(d => d.Cantidad).HasPrecision(12, 3);
        mb.Entity<DetalleVenta>().Property(d => d.PrecioUnitario).HasPrecision(12, 2);
        mb.Entity<DetalleVenta>().Property(d => d.Subtotal).HasPrecision(12, 2);

        mb.Entity<Producto>().Property(p => p.PrecioUnitario).HasPrecision(12, 2);

        // 🔥 NUEVO: precisión para campos relacionados a peso
        mb.Entity<Producto>().Property(p => p.FactorBase);
        mb.Entity<Producto>().Property(p => p.EsPorPeso);
        mb.Entity<Producto>().Property(p => p.UnidadBase).HasMaxLength(10);

        // LOTES
        mb.Entity<Lote>(e =>
        {
            e.ToTable("lotes", "public");
            e.HasKey(l => l.Id);

            e.Property(l => l.CantidadInicial).HasPrecision(12, 3);
            e.Property(l => l.CantidadActual).HasPrecision(12, 3);
            e.Property(l => l.CostoUnitario).HasPrecision(12, 2);
            e.Property(l => l.Estado).HasMaxLength(20).HasDefaultValue("ACTIVO");

            e.Property(l => l.RowVersion).IsRowVersion();

            e.HasOne(l => l.Compra)
             .WithMany(c => c.Lotes)
             .HasForeignKey(l => l.CompraId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Auditoría consumo lote
        mb.Entity<ConsumoLote>().Property(c => c.Cantidad).HasPrecision(12, 3);

        mb.Entity<ConsumoLote>()
            .HasOne(c => c.Venta)
            .WithMany(v => v.ConsumosLote)
            .HasForeignKey(c => c.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ConsumoLote>()
            .HasOne(c => c.Lote)
            .WithMany()
            .HasForeignKey(c => c.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Venta>().Property(v => v.Total).HasPrecision(12, 2);

        // EstadoPago
        mb.Entity<EstadoPago>(e =>
        {
            e.ToTable("estados_pago", "public");
            e.HasKey(x => x.Id).HasName("pk_estados_pago");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        });

        // Seeds
        mb.Entity<EstadoProducto>().HasData(
            new EstadoProducto { Id = 1, Nombre = "activo" },
            new EstadoProducto { Id = 2, Nombre = "inactivo" }
        );

        mb.Entity<MetodoPago>().HasData(
            new MetodoPago { Id = 1, Nombre = "efectivo" },
            new MetodoPago { Id = 2, Nombre = "yape" },
            new MetodoPago { Id = 3, Nombre = "tarjeta" }
        );

        mb.Entity<EstadoPago>().HasData(
            new EstadoPago { Id = 1, Nombre = "pagado" },
            new EstadoPago { Id = 2, Nombre = "pendiente" },
            new EstadoPago { Id = 3, Nombre = "anulado" }
        );

        // Usuario
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.Id);

            e.Property(x => x.UsuarioLogin).HasMaxLength(60).IsRequired();
            e.Property(x => x.Nombres).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(120);
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();

            e.HasIndex(x => x.UsuarioLogin).IsUnique();
            e.HasIndex(x => x.Email);
        });

        // Proveedores
        mb.Entity<Proveedor>(e =>
        {
            e.ToTable("proveedores");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            e.Property(p => p.Ruc).HasMaxLength(20);
            e.Property(p => p.Telefono).HasMaxLength(20);
            e.Property(p => p.Email).HasMaxLength(100);
            e.Property(p => p.Activo).HasDefaultValue(true);
        });

        // Cliente vigencia
        mb.Entity<Cliente>(e =>
        {
            e.Property(x => x.Vigente)
             .HasDefaultValue(true);
        });

        // Compras
        mb.Entity<Compra>(e =>
        {
            e.ToTable("compras", "public");
            e.HasKey(x => x.Id);

            e.Property(x => x.EstadoCompra)
             .HasMaxLength(20)
             .HasDefaultValue("REGISTRADA");

            e.Property(x => x.Subtotal).HasPrecision(12, 2);
            e.Property(x => x.Igv).HasPrecision(12, 2);
            e.Property(x => x.Total).HasPrecision(12, 2);

            e.HasOne(x => x.Proveedor)
             .WithMany()
             .HasForeignKey(x => x.ProveedorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // CompraDetalle
        mb.Entity<CompraDetalle>(e =>
        {
            e.ToTable("compras_detalle", "public");
            e.HasKey(x => x.Id);

            e.Property(x => x.Cantidad).HasPrecision(12, 3);
            e.Property(x => x.CostoUnitario).HasPrecision(12, 2);
            e.Property(x => x.Subtotal).HasPrecision(12, 2);

            e.HasOne(x => x.Compra)
             .WithMany(c => c.Detalles)
             .HasForeignKey(x => x.CompraId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Producto)
             .WithMany()
             .HasForeignKey(x => x.ProductoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // -------------------------------
    // Totales
    // -------------------------------
    private void CalcularTotales()
    {
        foreach (var detEntry in ChangeTracker.Entries<DetalleVenta>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            var d = detEntry.Entity;
            d.Subtotal = decimal.Round(d.Cantidad * d.PrecioUnitario, 2, MidpointRounding.AwayFromZero);
        }

        foreach (var venEntry in ChangeTracker.Entries<Venta>()
                         .Where(e => e.State == EntityState.Added))
        {
            var v = venEntry.Entity;
            var detalles = v.Detalleventa ?? new List<DetalleVenta>();

            foreach (var d in detalles)
            {
                d.Subtotal = decimal.Round(d.Cantidad * d.PrecioUnitario, 2, MidpointRounding.AwayFromZero);
            }

            v.Total = decimal.Round(detalles.Sum(d => d.Subtotal), 2, MidpointRounding.AwayFromZero);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CalcularTotales();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        CalcularTotales();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
