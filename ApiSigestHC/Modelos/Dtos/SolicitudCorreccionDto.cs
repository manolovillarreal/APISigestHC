namespace ApiSigestHC.Modelos.Dtos
{
    public class SolicitudCorreccionDto
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Observacion { get; set; }
        public int EstadoCorreccionId { get; set; }
        public DocumentoConAtencionDto Documento { get; set; }
        public EstadoCorreccion EstadoCorreccion { get; set; }
    }
}
