using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IRolRepositorio
    {
        Task<IEnumerable<Rol>> ObtenerTodosAsync();
    }
}
