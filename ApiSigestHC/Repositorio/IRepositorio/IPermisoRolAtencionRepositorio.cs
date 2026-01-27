using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IPermisoRolAtencionRepositorio
    {
        Task<List<int>> ObtenerEstadosVisiblesPorRolAsync(int rolId);
        Task<List<int>> ObtenerEstadosPermitidosPorRolAsync(int rolId);
        Task<bool> TienePermisoAsync(int rolId, int estadoAtencionId);
        Task<bool> EsVisibleAsync(int rolId, int estadoAtencionId);
    }
}
