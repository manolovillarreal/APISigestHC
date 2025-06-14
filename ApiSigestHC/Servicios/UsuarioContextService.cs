using ApiSigestHC.Servicios.IServicios;
using System.Security.Claims;

namespace ApiSigestHC.Servicios
{
    public class UsuarioContextService : IUsuarioContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int ObtenerRolId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var rolClaim = user?.FindFirst("rol_id")?.Value;

            if (string.IsNullOrEmpty(rolClaim))
                throw new UnauthorizedAccessException("El rol no se encuentra en el token.");

            if (!int.TryParse(rolClaim, out int rolId))
                throw new UnauthorizedAccessException("El rol no tiene un formato válido.");

            return rolId;
        }

        public string ObtenerNombreUsuario()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        public int ObtenerUsuarioId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("No se encontró el usuario");
        }

        public string ObtenerRolNombre()
        {
            var rolNombreClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            return rolNombreClaim?.Value ?? throw new UnauthorizedAccessException("Rol no especificado en el token.");
        }
    }

}
