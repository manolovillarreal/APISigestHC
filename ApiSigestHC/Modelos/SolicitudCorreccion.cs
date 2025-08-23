using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_SolicitudCorreccion")]
    public class SolicitudCorreccion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("documento_id")]
        public int DocumentoId { get; set; }        

        [Required]
        [Column("usuarioSolicita_id")]
        public int UsuarioSolicitaId { get; set; }


        [Column("fechaSolicitud")]
        public DateTime FechaSolicitud { get; set; }


        [Column("observacion")]
        [StringLength(255)]
        public string Observacion { get; set; }

        [Column("usuarioCorrige_id")]
        public int? UsuarioCorrigeId { get; set; } // Puede ser nulo hasta que se corrija

        [Column("fechaCorrige")]
        public DateTime? FechaCorrige { get; set; }

        [Column("estadoCorreccion_id")]
        public int EstadoCorreccionId { get; set; }

        public Documento Documento { get; set; }
        public EstadoCorreccion EstadoCorreccion { get; set; }


    }
}
