using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IDocumentoRequeridoService
    {
        Task<RespuestaAPI> CrearAsync(DocumentoRequeridoCrearDto dto);
        Task<RespuestaAPI> EliminarAsync(int TipoDocumentoId);
    }
}
