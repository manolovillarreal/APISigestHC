namespace ApiSigestHC.Modelos.Dtos
{
    public class SolicitudCorreccionDocDto
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Observacion { get; set; }
        public int EstadoCorreccionId { get; set; }
        public EstadoCorreccion EstadoCorreccion { get; set; }

    }
}
