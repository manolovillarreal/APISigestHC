using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using System.Security.Claims;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IUsuarioService
    {
        Task<RespuestaAPI> ObtenerUsuariosAsync();
        Task<RespuestaAPI> ObtenerUsuarioPorIdAsync(int usuarioId);
        Task<RespuestaAPI> CrearUsuarioAsync(UsuarioCrearDto dto);
        Task<RespuestaAPI> LoginAsync(UsuarioLoginDto dto);
        Task<RespuestaAPI> ObtenerPerfilAsync();
    }
}
