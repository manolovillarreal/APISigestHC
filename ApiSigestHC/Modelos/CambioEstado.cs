using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_CambioEstado")]
    public class CambioEstado
    {
        [Key]
        public int Id { get; set; }

        [Column("atencion_id")]
        public int AtencionId { get; set; }

        [Column("estadoInicial")]
        public int EstadoInicial { get; set; }

        [Column("estadoNuevo")]
        public int EstadoNuevo { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("observacion")]
        public string Observacion { get; set; }
    }

}
