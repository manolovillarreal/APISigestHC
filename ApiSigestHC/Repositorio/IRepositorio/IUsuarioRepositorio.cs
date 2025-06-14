using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        ICollection<Usuario> GetUsuarios();
        Usuario GetUsuario(int usuarioId);    
        bool IsUniqueUser(string usuario);      
        Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto);
        Task<Usuario> CrearUsuario(UsuarioCrearDto usuarioRegistroDto);


    }
}
