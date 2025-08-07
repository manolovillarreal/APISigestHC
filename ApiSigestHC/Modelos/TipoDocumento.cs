using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_TipoDocumento")]
    public class TipoDocumento
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool EsAsistencial { get; set; }
        public bool PermiteMultiples { get; set; } // indica si se pueden subir varios del mismo tipo por atención
        public bool RequiereNumeroRelacion { get; set; } // si requiere campo adicional
        public string ExtensionPermitida { get; set; } // 'pdf', 'xml', 'json', etc.
        public int LimiteDePaginas { get; set; }

        [Column("estadoAtencionInicial_id")]
        public int EstadoAtencionInicialId { get; set; }
        public int PesoPorPagina { get; set; }
        public bool Activo { get; set; }
        public ICollection<TipoDocumentoRol> TipoDocumentoRoles { get; set; }
        public DocumentoRequerido DocumentoRequerido { get; set; }
        public EstadoAtencion EstadoAtencionInicial { get; set; }

    }
}
