using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ITipoDocumentoRolService
    {
        Task<RespuestaAPI> ObtenerPorTipoDocumentoAsync(int tipoDocumentoId);
        Task<RespuestaAPI> ObtenerPorRolAsync(int rolId);
        Task<RespuestaAPI> CrearAsync(TipoDocumentoRolCrearDto dto);
        Task<RespuestaAPI> ActualizarAsync(TipoDocumentoRolCrearDto dto);
        Task<RespuestaAPI> EliminarAsync(int tipoDocumentoId, int rolId);
    }
}
