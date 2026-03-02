using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("INGRESO_URGENCIAS")]
    public class IngresoUrgencias
    {
        [Column("NU_NUME_INUR")]
        public int Id { get; set; }
        [Column("TX_OBSER_INUR")]
        public string Observacion { get; set; }
        [Column("FE_FECING_INUR")]
        public DateTime FechaIngreso { get; set; }
        [Column("FE_FECH_INUR")]
        public DateTime Fecha { get; set; }

        [Column("NU_HIST_PAC_INUR")]
        public string PacienteId { get; set; }

        public Paciente Paciente { get; set; }
    }
}
