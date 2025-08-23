namespace ApiSigestHC.Modelos.Dtos
{
    public class AtencionFiltroDto
    {
        public int? AtencionId { get; set; }
        public int? EstadoAtencionId { get; set; }
        public string? TerceroId { get; set; }
        public string? PacienteId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool consultarAnuladas { get; set; }
    }
}
