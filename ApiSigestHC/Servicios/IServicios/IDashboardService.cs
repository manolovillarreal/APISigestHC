using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IDashboardService
    {
        Task<RespuestaAPI> ObtenerDashboardAsync();
    }
}
