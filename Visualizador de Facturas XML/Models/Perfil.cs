using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Visualizador_de_Facturas_XML.Models
{
    public class Perfil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }  // Nombre del perfil

        public virtual ICollection<Factura>? Facturas { get; set; }  // Navegación a las facturas asociadas
    }
}
