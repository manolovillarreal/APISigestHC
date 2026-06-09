namespace ApiSigestHC.Modelos.Dtos
{
    public class FiltroCorreccionesDto
    {
        /// <summary>Número de historia clínica / ID del paciente (búsqueda parcial)</summary>
        public string? PacienteId { get; set; }

        public int? EstadoCorreccionId { get; set; }

        public DateTime? FechaInicial { get; set; }

        public DateTime? FechaFinal { get; set; }

        public int? TipoDocumentoId { get; set; }

        public int? UsuarioSolicitaId { get; set; }
    }
}
