using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        Task<ICollection<Usuario>> GetUsuariosAsync();
        Task<Usuario> GetUsuarioAsync(int usuarioId);    
        Task<bool> IsUniqueUser(string usuario);      
        Task<Usuario> CrearUsuario(UsuarioCrearDto usuarioRegistroDto);
        Task<Usuario?> ObtenerPorCredencialesAsync(string username, string password);


    }
}
