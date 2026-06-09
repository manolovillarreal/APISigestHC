using ApiSigestHC.Modelos.Dtos.Dashboard;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IDashboardRepositorio
    {
        Task<DashboardGlobalDto> ObtenerMetricasGlobalesAsync();
        Task<DashboardAdmisionesDto> ObtenerMetricasAdmisionesAsync();
        Task<DashboardMedicoDto> ObtenerMetricasMedicoAsync();
        Task<DashboardEnfermeriaDto> ObtenerMetricasEnfermeriaAsync();
        Task<DashboardAuditoriaDto> ObtenerMetricasAuditoriaAsync();
        Task<DashboardFacturacionDto> ObtenerMetricasFacturacionAsync();
        Task<DashboardAdminDto> ObtenerMetricasAdminAsync();
    }
}
