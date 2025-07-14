using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ITipoDocumentoRolService
    {
        Task<RespuestaAPI> ObtenerPorTipoDocumentoAsync(int tipoDocumentoId);
        Task<RespuestaAPI> CrearAsync(TipoDocumentoRolDto dto);
        Task<RespuestaAPI> ActualizarAsync(TipoDocumentoRolDto dto);
        Task<RespuestaAPI> EliminarAsync(int tipoDocumentoId, int rolId);
    }
}
