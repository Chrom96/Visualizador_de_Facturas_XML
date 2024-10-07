using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Visualizador_de_Facturas_XML.Models
{
    public class Factura
    {
        [Key]
        public string UUID { get; set; }  // UUID que se extraerá del XML

        [Required]
        public byte[] XmlContent { get; set; }  // BLOB que almacena el XML

        [ForeignKey("Profile")]
        public int PerfilID { get; set; }  // ID del perfil asociado

        public virtual Perfil Perfil { get; set; }  // Navegación a la entidad Profile
    }
}
