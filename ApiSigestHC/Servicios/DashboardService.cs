using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos.Dashboard;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepositorio _dashboardRepo;
        private readonly IUsuarioContextService _usuarioContextService;

        public DashboardService(
            IDashboardRepositorio dashboardRepo,
            IUsuarioContextService usuarioContextService)
        {
            _dashboardRepo = dashboardRepo;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<RespuestaAPI> ObtenerDashboardAsync()
        {
            /*
             * Nombres de rol exactos en el sistema (CambioEstadoService.cs):
             *   "Admisiones", "Medico", "Enfermeria",
             *   "Auditoria", "Facturacion", "Administrador"
             */
            var rolNombre = _usuarioContextService.ObtenerRolNombre();
            var global = await _dashboardRepo.ObtenerMetricasGlobalesAsync();

            object porRol = rolNombre switch
            {
                "Admisiones"    => await _dashboardRepo.ObtenerMetricasAdmisionesAsync(),
                "Medico"        => await _dashboardRepo.ObtenerMetricasMedicoAsync(),
                "Enfermeria"    => await _dashboardRepo.ObtenerMetricasEnfermeriaAsync(),
                "Auditoria"     => await _dashboardRepo.ObtenerMetricasAuditoriaAsync(),
                "Facturacion"   => await _dashboardRepo.ObtenerMetricasFacturacionAsync(),
                "Administrador" => await _dashboardRepo.ObtenerMetricasAdminAsync(),
                _               => null
            };

            var dto = new DashboardDto { Global = global, PorRol = porRol };

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = dto
            };
        }
    }
}
