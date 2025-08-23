using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos.Dtos
{
    public class DocumentoDto
    {
        public int Id { get; set; }
        public int AtencionId { get; set; }
        public int UsuarioId { get; set; }
        public int TipoDocumentoId { get; set; }
        public string NumeroRelacion { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime? FechaFinaliza { get; set; }

        public string? Observacion { get; set; }
        public TipoDocumentoDto TipoDocumento { get; set; }
        public Usuario Usuario { get; set; }

        public bool puedeCargar { get; set; }

        public ICollection<SolicitudCorreccionDocDto> SolicitudesCorreccion { get; set; }


    }
}
