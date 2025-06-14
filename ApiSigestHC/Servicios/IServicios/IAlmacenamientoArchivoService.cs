using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IAlmacenamientoArchivoService
    {
        Task<ResultadoGuardadoArchivo> GuardarArchivoAsync(DocumentoCargarDto dto);
        Task<ResultadoGuardadoArchivo> ReemplazarArchivoAsync(Documento documento, IFormFile archivo);
        Task<ResultadoGuardadoArchivo> RenombrarArchivoAsync(DocumentoEditarDto dto);
        Task<FileStreamResult?> DescargarDocumentoAsync(Documento doc);
        Task<FileStreamResult?> ObtenerArchivoParaVisualizacionAsync(Documento doc);
    }
}
