using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IValidacionCargaDocumentoService
    {
        Task<RespuestaAPI> ValidarCargaDocumentoAsync(DocumentoCargarDto dto);
        Task<RespuestaAPI> ValidarReemplazoDocumentoAsync(DocumentoReemplazarDto dto);
    }
}
