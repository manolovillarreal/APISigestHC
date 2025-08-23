using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Servicios
{
    public class VisualizacionEstadoService : IVisualizacionEstadoService
    {
        private readonly IUsuarioContextService _usuarioContextService;

        public VisualizacionEstadoService(IUsuarioContextService usuarioContextService)
        {
            _usuarioContextService = usuarioContextService;
        }
        private static readonly Dictionary<string, List<int>> EstadosPorRol = new()
            {
                { "Admisiones", new List<int> { 1, 2, 3, 4 } },
                { "Medico", new List<int> { 1, 2, 3 } },
                { "Enfermeria", new List<int> { 3 } },
                { "Laboratorio", new List<int> { 3 } },
                { "Radiologia", new List<int> { 3 } },
                { "Auditoria", new List<int> {1,2,3,4 } },
                { "Facturacion", new List<int> { } },
                { "Archivo", new List<int> {  } },
                { "Admin", new List<int> { 1, 2, 3, 4 } }
            };

        public List<int> ObtenerEstadosVisiblesPorRol()
        {
            var rolNombre = _usuarioContextService.ObtenerRolNombre();

            if (string.IsNullOrWhiteSpace(rolNombre))
                throw new UnauthorizedAccessException("No se pudo determinar el rol del usuario");
            var estados = EstadosPorRol[rolNombre];
            if (estados!=null )
                return estados;

            return new List<int>();
        }
    }

}
