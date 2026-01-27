using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Servicios
{
    public class VisualizacionEstadoService : IVisualizacionEstadoService
    {
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IPermisoRolAtencionRepositorio _permisoRolAtencionRepo;

        public VisualizacionEstadoService(
            IUsuarioContextService usuarioContextService,
            IPermisoRolAtencionRepositorio permisoRolAtencionRepo)
        {
            _usuarioContextService = usuarioContextService;
            _permisoRolAtencionRepo = permisoRolAtencionRepo;
        }

        public List<int> ObtenerEstadosVisiblesPorRol()
        {
            var rolId = _usuarioContextService.ObtenerRolId();

            if (rolId <= 0)
                throw new UnauthorizedAccessException("No se pudo determinar el rol del usuario");

            var estados = _permisoRolAtencionRepo.ObtenerEstadosVisiblesPorRolAsync(rolId).Result;
            
            return estados ?? new List<int>();
        }

        public List<int> ObtenerEstadosPermitidosPorRol()
        {
            var rolId = _usuarioContextService.ObtenerRolId();

            if (rolId <= 0)
                throw new UnauthorizedAccessException("No se pudo determinar el rol del usuario");

            var estados = _permisoRolAtencionRepo.ObtenerEstadosPermitidosPorRolAsync(rolId).Result;
            
            return estados ?? new List<int>();
        }
    }
}
