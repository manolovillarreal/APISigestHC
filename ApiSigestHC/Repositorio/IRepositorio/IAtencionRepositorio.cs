using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IAtencionRepositorio
    {
        Task<ICollection<Atencion>> ObtenerAtencionesAsync();
        Task<Atencion> ObtenerAtencionPorIdAsync(int id);
        Task<IEnumerable<Atencion>> GetAtencionesByStateAsync(int minState, int maxState);
        Task CrearAtencionAsync(Atencion atencion);
        Task EditarAtencionAsync(Atencion atencion);
        Task EliminarAtencionAsync(int id);
    }
}
