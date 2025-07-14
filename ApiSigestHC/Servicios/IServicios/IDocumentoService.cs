using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IDocumentoService
    {
        Task<RespuestaAPI> ObtenerDocumentosPorAtencionAsync(int atencionId);
        Task<RespuestaAPI> CargarDocumentoAsync(DocumentoCargarDto dto);
        Task<RespuestaAPI> EditarDocumentoAsync(DocumentoEditarDto dto);
        Task<RespuestaAPI> ReemplazarDocumentoAsync(DocumentoReemplazarDto dto);
        Task<RespuestaAPI> CorregirDocumentoAsync(DocumentoReemplazarDto dto);
        Task<RespuestaAPI> EliminarDocumentoAsync(int documentoId);
        Task<IActionResult> DescargarDocumentoAsync(int documentoId);
        Task<IActionResult> VerDocumentoAsync(int documentoId);



    }
}
