using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models
{
    public partial class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("codigo_barras")]
        public string? CodigoBarras { get; set; }

        [Column("categoria_id")]
        public int? CategoriaId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0")]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("stock_real")]
        public decimal StockReal { get; set; }

        // 🔥 NUEVOS CAMPOS PARA VENTA POR UNIDADES O POR PESO
        [Column("es_por_peso")]
        public bool EsPorPeso { get; set; } = false;

        [Column("unidad_base")]
        [MaxLength(10)]
        public string UnidadBase { get; set; } = "unidad"; // 'unidad' o 'kg'

        [Column("factor_base")]
        public int FactorBase { get; set; } = 1;  // 1 unidad, 1000 kg

        [Column("estado_producto_id")]
        public int? EstadoProductoId { get; set; }

        public virtual Categoria? Categoria { get; set; }
        public virtual EstadoProducto? EstadoProducto { get; set; }

        public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
        public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
