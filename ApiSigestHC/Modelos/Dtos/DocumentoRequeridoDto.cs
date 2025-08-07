namespace ApiSigestHC.Modelos.Dtos
{
    public class DocumentoRequeridoDto
    {
        public int EstadoAtencionId { get; set; }
        public int TipoDocumentoId { get; set; }

        public EstadoAtencionDto? EstadoAtencion { get; set; }
        public TipoDocumentoDto? TipoDocumento { get; set; }
    }
}
