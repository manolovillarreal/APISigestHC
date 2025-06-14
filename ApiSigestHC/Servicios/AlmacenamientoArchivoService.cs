using ApiSigestHC.Helpers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XAct.Library.Settings;

namespace ApiSigestHC.Servicios
{
    public class AlmacenamientoArchivoService:IAlmacenamientoArchivoService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;

        public AlmacenamientoArchivoService(IWebHostEnvironment env, 
                                            IDocumentoRepositorio documentoRepo,
                                            ITipoDocumentoRepositorio tipoDocumentoRepositorio,
                                            IAtencionRepositorio atencionRepo)
        {
            _env = env;
            _documentoRepo = documentoRepo;
            _atencionRepo = atencionRepo;
            _tipoDocumentoRepo= tipoDocumentoRepositorio;
        }

        public async Task<ResultadoGuardadoArchivo> GuardarArchivoAsync(DocumentoCargarDto dto)
        {
            var archivo = dto.Archivo;
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);

            // Validación básica
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Archivo no válido.");

            var fecha = atencion.FechaAtencion;
            var year = fecha.Year.ToString();
            var mes = fecha.Month.ToString("D2");
            var nroDocumentoPaciente = atencion.PacienteId ?? "000000";

            var carpetaFinal = $"{atencion.Id}_{fecha:yyyyMMdd}";
            var rutaCarpetaRelativa = Path.Combine("documentos", year, mes, nroDocumentoPaciente, carpetaFinal);
            var rutaBase = _env.ContentRootPath;
            var rutaCarpetaAbsoluta = Path.Combine(_env.ContentRootPath, rutaCarpetaRelativa);

            if (!Directory.Exists(rutaCarpetaAbsoluta))
                Directory.CreateDirectory(rutaCarpetaAbsoluta);

            // Consecutivo
            int consecutivo = await _documentoRepo.ContarPorTipoYAtencionAsync(atencion.Id, tipoDocumento.Id) + 1;

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            string nombreArchivo = $"{tipoDocumento.Codigo}_{consecutivo}_{dto.Fecha:yyyyMMdd}{extension}";
            string rutaCompleta = Path.Combine(rutaCarpetaAbsoluta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return new ResultadoGuardadoArchivo
            {
                RutaBase = rutaBase.Replace("\\", "/"),
                RutaRelativa = rutaCarpetaRelativa.Replace("\\", "/"),
                NombreArchivo = nombreArchivo,
                Consecutivo = consecutivo
            };
        }

        public async Task<ResultadoGuardadoArchivo> ReemplazarArchivoAsync(Documento documento, IFormFile archivo)
        {
            // Validación básica
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Archivo no válido.");

            // Ruta absoluta del archivo según datos en BD
            var rutaAbsoluta = Path.Combine(documento.RutaBase, documento.RutaRelativa, documento.NombreArchivo);

            // Crear directorio si no existe (por seguridad)
            var directorio = Path.GetDirectoryName(rutaAbsoluta);
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            // Guardar nuevo archivo, sobreescribiendo si ya existe
            using (var stream = new FileStream(rutaAbsoluta, FileMode.Create)) // Create sobrescribe si ya existe
            {
                await archivo.CopyToAsync(stream);
            }


            return new ResultadoGuardadoArchivo
            {
                RutaBase = documento.RutaBase.Replace("\\", "/"),
                RutaRelativa = documento.RutaRelativa.Replace("\\", "/"),
                NombreArchivo = documento.NombreArchivo
            };
        }

        public async Task<FileStreamResult?> DescargarDocumentoAsync(Documento doc)
        {

            if (doc == null) return null;

            var path = Path.Combine(doc.RutaBase, doc.RutaRelativa,doc.NombreArchivo);
            if (!File.Exists(path)) return null;

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var tipoMime = MimeTypeHelper.GetMimeType(doc.NombreArchivo);

            var atencion =  doc.Atencion;
            return new FileStreamResult(stream, tipoMime)
            {
                FileDownloadName = $"{atencion.Id}_{atencion.PacienteId}_{atencion.FechaAtencion:yyyyMMdd}_{doc.NombreArchivo}"
            };
        }

        public async Task<FileStreamResult?> ObtenerArchivoParaVisualizacionAsync(Documento doc)
        {
            if (doc == null) return null;

            var path = Path.Combine(doc.RutaBase, doc.RutaRelativa, doc.NombreArchivo);
            if (!File.Exists(path)) return null;

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var mimeType = MimeTypeHelper.GetMimeType(doc.NombreArchivo);

            // No se define FileDownloadName => el navegador intenta mostrarlo en lugar de descargarlo
            return new FileStreamResult(stream, mimeType);
        }


        public Task<ResultadoGuardadoArchivo> RenombrarArchivoAsync(DocumentoEditarDto dto)
        {
            throw new NotImplementedException();
        }
    }

    public class ResultadoGuardadoArchivo
    {

        public string RutaBase { get; set; }
        public string RutaRelativa { get; set; }
        public string NombreArchivo { get; set; }
        public int Consecutivo { get; set; }

    }

}
