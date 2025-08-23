using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IAlmacenamientoArchivoService
    {
        Task<ResultadoGuardadoArchivo> GuardarArchivoAsync(DocumentoCargarDto dto);
        Task<ResultadoGuardadoArchivo> ReemplazarArchivoCorreccionAsync(Documento documento, IFormFile archivo);
        Task<ResultadoGuardadoArchivo> GuardarArchivoTemporal(string coleccion, IFormFile archivo, int id);
        Task<ResultadoGuardadoArchivo> ActualizarNombreSiEsNecesarioAsync(DocumentoEditarDto dto);
        Task EliminarArchivoAsync(Documento documento);
        Task<FileStreamResult?> DescargarDocumentoAsync(Documento doc);
        Task<FileStreamResult?> ObtenerArchivoParaVisualizacionAsync(Documento doc);
        Task<FileStreamResult?> ObtenerArchivoTemporalParaVisualizacionAsync(string coleccion,int SolicitudId);
        Task<IFormFile?> ObtenerArchivoTemporalComoFormFileAsync(string coleccion, int solicitudId);
        Task EliminarArchivoTemporalAsync(string v, int id);
    }
}
