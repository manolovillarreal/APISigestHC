namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardAuditoriaDto
    {
        public int AtencionesPendientesRevision { get; set; }  // EstadoAtencionId == 5 (Auditoria)
        public int CorrecccionesPendientes { get; set; }
        public double TiempoPromedioRevisionHoy { get; set; }  // minutos Auditoria→Facturacion
    }
}
