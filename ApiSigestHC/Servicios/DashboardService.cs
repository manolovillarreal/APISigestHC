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
            // El nombre del rol llega del claim del token y, según el origen de los
            // datos, puede venir con o sin tilde ("Auditoría") o como alias del
            // administrador ("Admin"). Lo normalizamos a una clave canónica para que
            // el switch sea robusto (mismo criterio que DocumentoService).
            var rolNombre = _usuarioContextService.ObtenerRolNombre();
            var rolCanonico = NormalizarRol(rolNombre);
            var global = await _dashboardRepo.ObtenerMetricasGlobalesAsync();

            object porRol = rolCanonico switch
            {
                "Admisiones"    => await _dashboardRepo.ObtenerMetricasAdmisionesAsync(),
                "Medico"        => await _dashboardRepo.ObtenerMetricasMedicoAsync(),
                "Enfermeria"    => await _dashboardRepo.ObtenerMetricasEnfermeriaAsync(),
                "Auditoria"     => await _dashboardRepo.ObtenerMetricasAuditoriaAsync(),
                "Facturacion"   => await _dashboardRepo.ObtenerMetricasFacturacionAsync(),
                "Administrador" => await _dashboardRepo.ObtenerMetricasAdminAsync(),
                _               => null
            };

            var dto = new DashboardDto { Rol = rolCanonico, Global = global, PorRol = porRol };

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = dto
            };
        }

        private static string NormalizarRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return string.Empty;

            var r = rol.Trim();

            bool Es(string objetivo) => r.Equals(objetivo, StringComparison.OrdinalIgnoreCase);

            if (Es("Admin") || Es("Administrador")) return "Administrador";
            if (Es("Auditoria") || Es("Auditoría")) return "Auditoria";
            if (Es("Admisiones") || Es("Admision") || Es("Admisión")) return "Admisiones";
            if (Es("Medico") || Es("Médico")) return "Medico";
            if (Es("Enfermeria") || Es("Enfermería")) return "Enfermeria";
            if (Es("Facturacion") || Es("Facturación")) return "Facturacion";

            return r;
        }
    }
}
