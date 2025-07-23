using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ITipoDocumentoService
    {
        Task<RespuestaAPI> ObtenerTiposPermitidosPorRolAsync();
        Task<RespuestaAPI> ObtenerTodosAsync();
        Task<RespuestaAPI> ObtenerPorIdAsync(int id);
        Task<RespuestaAPI> CrearAsync(TipoDocumentoCrearDto dto);
        Task<RespuestaAPI> EditarAsync(int id, TipoDocumentoCrearDto dto);
        Task<RespuestaAPI> EliminarAsync(int id);
    }
}
