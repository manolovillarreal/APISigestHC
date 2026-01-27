using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IEstadoAtencionRepositorio
    {
        Task<IEnumerable<EstadoAtencion>> ObtenerTodosAsync();
        Task<IEnumerable<EstadoAtencion>> ObtenerPermitidosAsync();
        Task<EstadoAtencion?> ObtenerPorIdAsync(int id);
    }
}
