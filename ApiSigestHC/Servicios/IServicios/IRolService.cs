using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IRolService
    {
        Task<RespuestaAPI> ObtenerTodosAsync();
    }
}
