using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("PACIENTES")]
    public class Paciente
    {
        [Key]
        [Column("NU_HIST_PAC", TypeName = "varchar(20)")]
        public string Id { get; set; }

        [Column("FE_NACI_PAC")]
        public DateTime FechaNacimiento { get; set; }

        [Column("NO_NOMB_PAC")]
        public string PrimerNombre { get; set; }

        [Column("NO_SGNO_PAC")]
        public string SegundoNombre { get; set; }

        [Column("DE_PRAP_PAC")]
        public string PrimerApellido { get; set; }
        [Column("DE_SGAP_PAC")]
        public string SegundoApellido { get; set; }


        [NotMapped]
        public List<Administradora> Administradoras { get; set; } = new();


        public string NombreCorto()
        {
            return $"{PrimerNombre} {PrimerApellido}";
        }

    }
}
