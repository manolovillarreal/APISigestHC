using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("R_PAC_EPS")]
    public class RelacionPacienteAdministradora
    {
        [Column("NU_HIST_PAC_RPE")]
        [ForeignKey("PACIENTES")]
        public string PacienteId { get; set; }
        [Column("CD_NIT_EPS_RPE")]
        [ForeignKey("EPS")]
        public string NitEps { get; set; }
        [Column("TX_ACTI_RPE")]
        public string Activo { get; set; }

        
    }
}
