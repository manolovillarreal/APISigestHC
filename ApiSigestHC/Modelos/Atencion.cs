using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Atencion")]
    public class Atencion
    {
        public int Id { get; set; }

        [Column("paciente_id", TypeName = "varchar(20)")]
        public string PacienteId { get; set; }

        [Column("tercero_id")]
        public string TerceroId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FechaAtencion { get; set; }

        public DateTime FechaFinaliza { get; set; }

        public DateTime FechaArchivo { get; set; }

        public string? NroFactura { get; set; }

        public int EstadoAtencion { get; set; }
        
        [Column("usuario_id")]
        [ForeignKey("SIG_Usuario")]
        public int UsuarioId { get; set; }

        public Paciente? Paciente { get; set; }
    }
}
