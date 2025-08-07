using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using System.Threading.Tasks;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        Task<ICollection<Usuario>> GetUsuariosAsync();
        Task<Usuario> GetUsuarioAsync(int usuarioId);    
        Task<bool> IsUniqueUsername(string usuario);      
        Task<bool> IsUniqueEmail(string correo);
        Task<bool> IsUniqueDni(string dni);
        Task<Usuario> CrearUsuario(UsuarioCrearDto usuarioRegistroDto);
        Task<Usuario> EditarUsuario(int id , UsuarioEditarDto usuarioRegistroDto);
        Task<Usuario?> ObtenerPorCredencialesAsync(string username, string password);


    }
}
