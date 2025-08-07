using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IUsuarioService
    {
        Task<RespuestaAPI> ObtenerUsuariosAsync();
        Task<RespuestaAPI> ObtenerUsuarioPorIdAsync(int usuarioId);
        Task<RespuestaAPI> CrearUsuarioAsync(UsuarioCrearDto dto);
        Task<RespuestaAPI> EditarUsuarioAsync(int id, UsuarioEditarDto dto);
        Task<RespuestaAPI> LoginAsync(UsuarioLoginDto dto);
        Task<RespuestaAPI> ObtenerPerfilAsync();
    }
}
