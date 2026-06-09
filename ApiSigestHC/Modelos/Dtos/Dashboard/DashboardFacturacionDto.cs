namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardFacturacionDto
    {
        public int AtencionesPendientes { get; set; }      // EstadoAtencionId == 6 (Facturacion)
        public int AtencionesenRiesgo { get; set; }        // En Radicacion (ID=7) hace >22 días sin pasar a Archivado (ID=8)
    }
}
