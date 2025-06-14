using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("EPS")]
    public class Administradora
    {
        [Key]
        [Column("CD_NIT_EPS")]
        public string Nit { get; set; }
        [Column("NO_NOMB_EPS")]
        public string Nombre { get; set; }


        override
        public string ToString()
        {
            return Nombre;
        }
    }
}
