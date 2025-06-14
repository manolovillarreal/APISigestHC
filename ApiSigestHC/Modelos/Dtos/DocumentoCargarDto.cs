namespace ApiSigestHC.Modelos.Dtos
{
    public class DocumentoCargarDto
    {
        public int AtencionId { get; set; }
        public int TipoDocumentoId { get; set; }
        public string? NumeroRelacion { get; set; } // Opcional según el tipoDocumento
        public IFormFile Archivo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
