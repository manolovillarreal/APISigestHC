namespace ApiSigestHC.Modelos.Dtos
{
    public class DocumentoEditarDto
    {
        public string Id { get; set; }
        public string? NumeroRelacion { get; set; }
        public string? Observacion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
