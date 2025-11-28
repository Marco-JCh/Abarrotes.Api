using System.ComponentModel.DataAnnotations;

namespace Abarrotes.Api.Dtos
{
    public class ProductoCreateDto
    {
        [Required, StringLength(150)]
        public string Nombre { get; set; } = null!;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? CodigoBarras { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        // 🔥 NUEVO: indicar si es por peso
        public bool EsPorPeso { get; set; } = false;

        // 🔥 NUEVO: unidad base: "unidad" o "kg"
        [Required, RegularExpression("^(unidad|kg)$")]
        public string UnidadBase { get; set; } = "unidad";

        // 🔥 NUEVO: factor base (1 para unidad, 1000 para kg)
        [Range(1, int.MaxValue)]
        public int FactorBase { get; set; } = 1;

        public int? CategoriaId { get; set; }
        public int? EstadoProductoId { get; set; }
    }
    public class ProductoUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? CodigoBarras { get; set; }
        public decimal? PrecioUnitario { get; set; }

        // 🔥 Nuevos campos
        public bool? EsPorPeso { get; set; }
        public string? UnidadBase { get; set; }   // "unidad" o "kg"
        public int? FactorBase { get; set; }      // 1 o 1000

        public int? CategoriaId { get; set; }
        public int? EstadoProductoId { get; set; }
    }
    public class ProductoReadDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? CodigoBarras { get; set; }
        public decimal PrecioUnitario { get; set; }

        // 🔥 Nuevos campos
        public bool EsPorPeso { get; set; }
        public string UnidadBase { get; set; } = "unidad";
        public int FactorBase { get; set; }

        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }

        public int? EstadoProductoId { get; set; }
        public string? EstadoProductoNombre { get; set; }
        public decimal StockReal { get; set; }
        public CategoriaMini? Categoria { get; set; }
        public EstadoProductoMini? EstadoProducto { get; set; }
    }
}
