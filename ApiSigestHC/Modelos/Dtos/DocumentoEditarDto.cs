namespace ApiSigestHC.Modelos.Dtos
{
    public class DocumentoEditarDto
    {
        public int Id { get; set; }
        public string? NumeroRelacion { get; set; }
        public string? Observacion { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
