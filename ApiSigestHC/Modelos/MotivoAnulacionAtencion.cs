
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_MotivoAnulacionAtencion")]
    public class MotivoAnulacionAtencion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}
