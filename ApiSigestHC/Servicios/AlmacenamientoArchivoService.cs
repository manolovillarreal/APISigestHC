using ApiSigestHC.Helpers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Net;
using XAct.Library.Settings;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ApiSigestHC.Servicios
{
    public class AlmacenamientoArchivoService: IAlmacenamientoArchivoService
    {
        const string rutaTemporal = "documentos/temporal";

        private readonly IWebHostEnvironment _env;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IUsuarioContextService _usuarioContext;

        public AlmacenamientoArchivoService(IWebHostEnvironment env, 
                                            IDocumentoRepositorio documentoRepo,
                                            ITipoDocumentoRepositorio tipoDocumentoRepositorio,
                                            IAtencionRepositorio atencionRepo,
                                            IUsuarioRepositorio usuarioRepo,
                                            IUsuarioContextService usuarioContext   )
        {
            _env = env;
            _documentoRepo = documentoRepo;
            _atencionRepo = atencionRepo;
            _tipoDocumentoRepo = tipoDocumentoRepositorio;
            _usuarioRepo = usuarioRepo;
            _usuarioContext = usuarioContext;
        }

        public async Task<ResultadoGuardadoArchivo> GuardarArchivoAsync(DocumentoCargarDto dto)
        {
            var archivo = dto.Archivo;
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);
           
            // Validación básica
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Archivo vacio no válido.");
            if (atencion == null)
                throw new ArgumentException($"Atencion con id {dto.AtencionId} no encontrada.");
            if (tipoDocumento == null)
                throw new ArgumentException($"Tipo de documento con id {dto.TipoDocumentoId} no encontrado.");
            if (string.IsNullOrEmpty(atencion.PacienteId))
                throw new ArgumentException($"No se encontro el id del pacinete en la atencion de id {dto.AtencionId}");

            try
            {          

                var fecha = atencion.Fecha;
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

                string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                string nombreArchivo = await GenerarNombreArchivo(tipoDocumento,new Documento {
                                                                                        AtencionId = dto.AtencionId,
                                                                                        Fecha = dto.Fecha,
                                                                                        NumeroRelacion = dto.NumeroRelacion,
                                                                                    } ,dto.Archivo.FileName);
                string rutaCompleta = Path.Combine(rutaCarpetaAbsoluta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }
                if (extension == ".pdf")
                {
                    ActualizarMetadatosPdf(
                        rutaCompleta,
                        tipoDocumento.Nombre+" - "+atencion.Paciente.NombreCorto(),
                        _usuarioContext.ObtenerNombreUsuario(),
                         tipoDocumento.Nombre // tema opcional
                    );
                }

                return new ResultadoGuardadoArchivo
                {
                    RutaBase = rutaBase.Replace("\\", "/"),
                    RutaRelativa = rutaCarpetaRelativa.Replace("\\", "/"),
                    NombreArchivo = nombreArchivo,
                };
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public async Task<ResultadoGuardadoArchivo> ReemplazarArchivoCorreccionAsync(Documento documento, IFormFile archivo)
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

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension == ".pdf")
            {
                ActualizarMetadatosPdf(
                    rutaAbsoluta,
                    documento.TipoDocumento.Nombre + " - " + documento.Atencion.Paciente.NombreCorto(),
                    _usuarioRepo.GetUsuario(documento.UsuarioId).NombreUsuario,
                    documento.TipoDocumento.Nombre // tema opcional
                );
            }
            return new ResultadoGuardadoArchivo
            {
                RutaBase = documento.RutaBase.Replace("\\", "/"),
                RutaRelativa = documento.RutaRelativa.Replace("\\", "/"),
                NombreArchivo = documento.NombreArchivo
            };
        }

        public async Task<ResultadoGuardadoArchivo> GuardarArchivoTemporal(string coleccion, IFormFile archivo, int id)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Archivo no válido.");

            // Construir la ruta absoluta y relativa
            var nombreArchivo = $"{id}.pdf";
            var rutaRelativa = Path.Combine(rutaTemporal+"/" + coleccion, nombreArchivo);
            var rutaAbsoluta = Path.Combine(_env.ContentRootPath, rutaRelativa);

            // Crear el directorio si no existe
            var directorio = Path.GetDirectoryName(rutaAbsoluta);
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            // Guardar el archivo
            using (var stream = new FileStream(rutaAbsoluta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }           

            return new ResultadoGuardadoArchivo
            {
                RutaBase = _env.ContentRootPath.Replace("\\", "/"),
                RutaRelativa = rutaRelativa.Replace("\\", "/"),
                NombreArchivo = nombreArchivo,
                IsSuccess = true,
                Message = "Archivo temporal guardado correctamente."
            };
        }
        public async Task EliminarArchivoAsync(Documento documento)
        {
            if (documento == null)
                throw new ArgumentNullException(nameof(documento));

            var rutaCarpeta = Path.Combine(documento.RutaBase, documento.RutaRelativa);
            var rutaCompleta = Path.Combine(rutaCarpeta, documento.NombreArchivo);

            if (File.Exists(rutaCompleta))
            {
                try
                {
                    File.Delete(rutaCompleta);
                    await Task.CompletedTask; // Solo para mantener la firma async
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error al intentar eliminar el archivo: {rutaCompleta}", ex);
                }
            }
            else
            {
                // Si el archivo no existe, puedes elegir lanzar excepción o simplemente ignorar
                throw new FileNotFoundException("No se encontró el archivo para eliminar.", rutaCompleta);
            }
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
                FileDownloadName = $"{atencion.Id}_{atencion.PacienteId}_{atencion.Fecha:yyyyMMdd}_{doc.NombreArchivo}"
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
        public async Task<FileStreamResult?> ObtenerArchivoTemporalParaVisualizacionAsync(string coleccion, int solicitudId)
        {
            var nombreArchivo = solicitudId + ".pdf";

            var path = Path.Combine(rutaTemporal, coleccion, nombreArchivo);
            if (!File.Exists(path)) return null;

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var mimeType = MimeTypeHelper.GetMimeType(nombreArchivo);

            // No se define FileDownloadName => el navegador intenta mostrarlo en lugar de descargarlo
            return new FileStreamResult(stream, mimeType);
        }
        public async Task<IFormFile?> ObtenerArchivoTemporalComoFormFileAsync(string coleccion, int solicitudId)
        {
            var nombreArchivo = $"{solicitudId}.pdf";
            var rutaAbsoluta = Path.Combine(_env.ContentRootPath, rutaTemporal, coleccion, nombreArchivo);

            if (!File.Exists(rutaAbsoluta))
                return null;

            await using var stream = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "archivo", nombreArchivo)
            {
                Headers = new HeaderDictionary(),
                ContentType = MimeTypeHelper.GetMimeType(nombreArchivo)
            };

            return formFile;
        }
        public Task EliminarArchivoTemporalAsync(string coleccion, int id)
        {
            var nombreArchivo = $"{id}.pdf";
            var rutaAbsoluta = Path.Combine(_env.ContentRootPath, rutaTemporal, coleccion, nombreArchivo);
            if (File.Exists(rutaAbsoluta))
            {
                try
                {
                    File.Delete(rutaAbsoluta);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error al intentar eliminar el archivo temporal: {rutaAbsoluta}", ex);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<ResultadoGuardadoArchivo> ActualizarNombreSiEsNecesarioAsync(DocumentoEditarDto dto)
        {
            // 1. Obtener el documento actual y su tipo
            var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id)
                ?? throw new ArgumentException($"Documento con id {dto.Id} no encontrado.");

            var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(documento.TipoDocumentoId)
                ?? throw new ArgumentException($"Tipo de documento con id {documento.TipoDocumentoId} no encontrado.");

            // 2. Determinar nuevo nombre
            var nuevoNombre = await GenerarNombreArchivo(tipoDocumento, documento, documento.NombreArchivo);

            if (!string.Equals(documento.NombreArchivo, nuevoNombre, StringComparison.OrdinalIgnoreCase))
            {
                // 3. Renombrar físicamente el archivo
                var carpeta = Path.Combine(documento.RutaBase, documento.RutaRelativa);
                var rutaVieja = Path.Combine(carpeta, documento.NombreArchivo);
                var rutaNueva = Path.Combine(carpeta, nuevoNombre);

                if (!File.Exists(rutaVieja))
                    throw new FileNotFoundException($"No existe el archivo físico: {rutaVieja}");

                File.Move(rutaVieja, rutaNueva, overwrite: true);

                documento.NombreArchivo = nuevoNombre;

                if (!string.Equals(documento.NombreArchivo, nuevoNombre, StringComparison.OrdinalIgnoreCase))
                {
                    await _documentoRepo.ActualizarAsync(documento);
                }
            }

            
           
            // 5. Devolver resultado
            return new ResultadoGuardadoArchivo
            {
                NombreArchivo = nuevoNombre,
            };
        }


        /// <summary>
        /// Genera el nombre de archivo según las reglas de negocio:
        /// 1. Siempre incluye el código del tipo de documento.
        /// 2. Si PermiteMultiples, antepone el consecutivo.
        /// 3. Si RequiereNumeroRelacion y dto.NumeroRelacion no está vacío, usa ese número (prioritario sobre fecha).
        /// 4. Si EsAsistencial, usa la fecha.
        /// 5. Nunca fecha y número de relación juntos.
        /// </summary>
        private async Task<string> GenerarNombreArchivo(TipoDocumento tipoDoc, Documento dto ,string fileName)
        {
            var partes = new List<string>
                {
                    tipoDoc.Codigo
                };

            // 1. Consecutivo solo si permite múltiples
            if (tipoDoc.PermiteMultiples)
            {
                int consecutivo = await _documentoRepo.ContarPorTipoYAtencionAsync(dto.AtencionId, tipoDoc.Id) + 1;
                partes.Add(consecutivo.ToString());
            }

            // 2. Número de relación tiene prioridad sobre fecha
            if (tipoDoc.RequiereNumeroRelacion && !string.IsNullOrWhiteSpace(dto.NumeroRelacion))
            {
                partes.Add(dto.NumeroRelacion.Trim());
            }
            else if (tipoDoc.EsAsistencial) // 3. fecha solo si es asistencial
            {
                partes.Add(dto.Fecha.ToString("yyyyMMdd"));
            }

            // 4. Extensión siempre al final
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return string.Join("_", partes) + extension;
        }
        /// <summary>
        /// Actualiza los metadatos de un PDF (título, autor, tema).
        /// </summary>
        private void ActualizarMetadatosPdf(string rutaPdf, string titulo, string? autor = null, string? tema = null)
        {
            using (PdfDocument pdf = PdfReader.Open(rutaPdf, PdfDocumentOpenMode.Modify))
            {
                pdf.Info.Title = titulo;

                if (!string.IsNullOrWhiteSpace(autor))
                    pdf.Info.Author = autor;

                if (!string.IsNullOrWhiteSpace(tema))
                    pdf.Info.Subject = tema;

                pdf.Save(rutaPdf); // Sobrescribe el mismo archivo
            }
        }

        
    }


}
