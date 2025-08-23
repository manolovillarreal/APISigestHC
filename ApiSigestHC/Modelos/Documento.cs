using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Documento")]
    public class Documento
    {
        public int Id { get; set; }
        [Column("atencion_id")]
        public int AtencionId { get; set; }


        [Column("usuario_id")]
        [ForeignKey("SIG_EstadoAtencion")]
        public int UsuarioId { get; set; }
        [Column("tipoDocumento_id")]
        public int TipoDocumentoId { get; set; }
        public string RutaBase { get; set; }
        public string RutaRelativa { get; set; }
        public string NombreArchivo { get; set; }
        public string? NumeroRelacion { get; set; }
        public string? Observacion { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaCarga { get; set; }

        public TipoDocumento? TipoDocumento { get; set; }
        public Atencion? Atencion { get; set; }
        public Usuario Usuario { get; set; }
        // 🔹 Relación real (1 Documento -> muchas Solicitudes)
        public ICollection<SolicitudCorreccion> SolicitudesCorreccion { get; set; }
    }
}
