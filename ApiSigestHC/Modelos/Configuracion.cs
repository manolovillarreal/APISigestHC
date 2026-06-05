using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Configuracion")]
    public class Configuracion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Clave { get; set; }

        [Required]
        [MaxLength(500)]
        public string Valor { get; set; }

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FechaActualizacion { get; set; }

        public int? UsuarioActualizacion { get; set; }
    }
}
