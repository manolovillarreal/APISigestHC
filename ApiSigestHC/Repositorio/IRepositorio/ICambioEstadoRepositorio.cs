using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ICambioEstadoRepositorio
    {
        Task RegistrarCambioAsync(CambioEstado cambio);
        Task<List<CambioEstado>> ObtenerCambiosPorAtencionAsync(int atencionId);
    }
}
