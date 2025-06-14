using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_DocumentosRequeridos")]
    public class DocumentoRequerido
    {
        [Column("estadoAtencion_id")]
        [ForeignKey("SIG_EstadoAtencion")]
        public int EstadoAtencionId { get; set; }

        [Column("tipoDocumento_id")]
        [ForeignKey("SIG_TipoDocumento")]
        public int TipoDocumentoId { get; set; }

        public EstadoAtencion EstadoAtencion { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
    }

}
