using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ICrearDocumentoRequeridoService
    {
        Task<RespuestaAPI> CrearAsync(DocumentoRequeridoDto dto);
    }
}
