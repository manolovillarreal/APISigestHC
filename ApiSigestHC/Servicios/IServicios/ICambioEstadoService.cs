using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ICambioEstadoService
    {
        Task<RespuestaAPI> CambiarEstadoAsync(AtencionCambioEstadoDto atencionDto);
        Task<RespuestaAPI> CerrarAtencionAsync(AtencionCambioEstadoDto dto);
    }
}
