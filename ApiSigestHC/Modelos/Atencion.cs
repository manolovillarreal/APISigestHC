using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Atencion")]
    public class Atencion
    {
        public int Id { get; set; }

        [Column("paciente_id")]
        public string PacienteId { get; set; }

        [Column("tercero_id")]
        public string TerceroId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime Fecha { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FechaFinaliza { get; set; }

        [Column("estadoAtencion_id")]
        public int EstadoAtencionId { get; set; }
        
        [Column("usuario_id")]
        [ForeignKey("SIG_Usuario")]
        public int UsuarioId { get; set; }

        public Paciente? Paciente { get; set; }
        public EstadoAtencion? EstadoAtencion { get; set; }
    }
}
