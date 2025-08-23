using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IValidacionCargaArchivoService
    {
        Task<RespuestaAPI> ValidarCargaDocumentoAsync(DocumentoCargarDto dto);
        Task<RespuestaAPI> ValidarReemplazoDocumentoAsync(DocumentoReemplazarDto dto);
        Task<RespuestaAPI> ValidarCargaArchivoCorreccionAsync(IFormFile archivo, TipoDocumento tipoDoc);
        Task<RespuestaAPI> ValidarArchivoAsync(IFormFile archivo, TipoDocumento tipoDoc);
    }
}
