using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IConfiguracionRepositorio
    {
        Task<Configuracion?> ObtenerPorClaveAsync(string clave);
        Task<IEnumerable<Configuracion>> ObtenerTodasAsync();
        Task ActualizarAsync(Configuracion configuracion);
    }
}
