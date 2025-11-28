using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abarrotes.Api.Models
{
    public partial class Lote
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public int? ProveedorId { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        // 🔥 Cantidad inicial en unidades o gramos (según producto)
        public decimal CantidadInicial { get; set; }

        // 🔥 Cantidad actual en unidades o gramos (según producto)
        public decimal CantidadActual { get; set; }

        // 🔥 Costo por unidad o por kg (según producto)
        public decimal CostoUnitario { get; set; }

        public string Estado { get; set; } = "ACTIVO";

        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        public virtual Producto Producto { get; set; } = null!;

        public virtual Proveedor? Proveedor { get; set; } = null!;

        /// <summary>
        /// Token de concurrencia optimista
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
