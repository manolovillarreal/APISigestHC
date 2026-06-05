using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Linq;

namespace ApiSigestHC.Servicios
{
    public class DocumentoService : IDocumentoService
    {
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly ISolicitudCorreccionRepositorio _solicitudCorreccionRepo;
        private readonly ITipoDocumentoRolRepositorio _tipoDocumentoRolRepo;

        private readonly IAlmacenamientoArchivoService _almacenamientoArchivoService;
        private readonly IValidacionCargaArchivoService _validacionCargaDocumentoService;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IMapper _mapper;
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IUsuarioRepositorio _usuarioRepo;

        public DocumentoService(
        IAlmacenamientoArchivoService almacenamientoArchivoService,
        ISolicitudCorreccionRepositorio solicitudCorreccionRepo,
        IValidacionCargaArchivoService validacionCargaDocumentoService,
        IDocumentoRepositorio documentoRepo,
        ITipoDocumentoRolRepositorio tipoDocumentoRepo,
        IUsuarioContextService usuarioContextService,
        IAtencionRepositorio atencionRepo,
        IUsuarioRepositorio usuarioRepo,
        IMapper mapper)
        {
            _almacenamientoArchivoService = almacenamientoArchivoService;
            _validacionCargaDocumentoService = validacionCargaDocumentoService;
            _solicitudCorreccionRepo = solicitudCorreccionRepo;
            _documentoRepo = documentoRepo;
            _tipoDocumentoRolRepo = tipoDocumentoRepo;
            _usuarioContextService = usuarioContextService;
            _atencionRepo = atencionRepo;
            _usuarioRepo = usuarioRepo;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> ImportarDocumentoIdentidadAsync(int atencionId)
        {
            try
            {
                // 1. Obtener atencion destino
                var atencionDestino = await _atencionRepo.ObtenerAtencionPorIdAsync(atencionId);
                if (atencionDestino == null)
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = new List<string> { "Atención destino no encontrada." } };

                var pacienteId = atencionDestino.PacienteId;
                if (string.IsNullOrEmpty(pacienteId))
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.BadRequest, ErrorMessages = new List<string> { "La atención destino no tiene paciente asociado." } };

                // 2. Obtener atenciones anteriores del paciente
                var atencionesPrevias = await _atencionRepo.ObtenerAtencionesPorPacienteAsync(pacienteId, atencionId);
                if (atencionesPrevias == null || !atencionesPrevias.Any())
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NoContent };

                var atencionIds = atencionesPrevias.Select(a => a.Id).ToList();

                // 3. Buscar documentos tipo ID en atenciones previas
                var documentosPrevios = await _documentoRepo.ObtenerPorAtencionesAsync(atencionIds);
                var documentoIdOrigen = documentosPrevios.FirstOrDefault(d => d.TipoDocumento != null && d.TipoDocumento.Codigo == "ID" && d.FechaEliminacion == null);

                if (documentoIdOrigen == null)
                {
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NoContent };
                }

                var origen = documentoIdOrigen;

                // 4. Copiar archivo físico
                var rutaOrigen = Path.Combine(origen.RutaBase, origen.RutaRelativa, origen.NombreArchivo);
                if (!File.Exists(rutaOrigen))
                {
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = new List<string> { "Archivo origen no encontrado." } };
                }

                // Crear carpeta destino siguiendo patrón de atencion destino
                var fecha = atencionDestino.Fecha;
                var year = fecha.Year.ToString();
                var mes = fecha.Month.ToString("D2");
                var nroDocumentoPaciente = atencionDestino.PacienteId ?? "000000";
                var carpetaFinal = $"{atencionDestino.Id}_{fecha:yyyyMMdd}";
                var rutaCarpetaRelativa = Path.Combine("documentos", year, mes, nroDocumentoPaciente, carpetaFinal);
                var rutaBase = origen.RutaBase; // use same base as origen
                var rutaCarpetaAbsoluta = Path.Combine(rutaBase, rutaCarpetaRelativa);
                if (!Directory.Exists(rutaCarpetaAbsoluta))
                    Directory.CreateDirectory(rutaCarpetaAbsoluta);

                // Generar nuevo nombre único
                var extension = Path.GetExtension(origen.NombreArchivo);
                var nuevoNombre = $"{Guid.NewGuid()}{extension}";
                var rutaDestino = Path.Combine(rutaCarpetaAbsoluta, nuevoNombre);

                File.Copy(rutaOrigen, rutaDestino, overwrite: false);

                // 5. Crear nuevo registro Documento en BD
                var nuevo = new Documento
                {
                    AtencionId = atencionId,
                    TipoDocumentoId = origen.TipoDocumentoId,
                    NombreArchivo = nuevoNombre,
                    RutaRelativa = rutaCarpetaRelativa.Replace("\\", "/"),
                    RutaBase = rutaBase.Replace("\\", "/"),
                    FechaCarga = DateTime.Now,
                    UsuarioId = _usuarioContextService.ObtenerUsuarioId(),
                    Fecha = origen.Fecha,
                    TamanoBytes = new FileInfo(rutaDestino).Length,
                    NumeroPaginas = origen.NumeroPaginas
                };

                await _documentoRepo.GuardarAsync(nuevo);

                var documentoBd = await _documentoRepo.ObtenerPorIdAsync(nuevo.Id);
                var dto = _mapper.Map<DocumentoDto>(documentoBd);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = dto };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.InternalServerError, ErrorMessages = new List<string> { "Error interno al importar documento.", ex.Message } };
            }
        }

        public async Task<RespuestaAPI> ObtenerPapeleraAsync(int atencionId)
        {
            try
            {
                var documentos = await _documentoRepo.ObtenerEliminadosPorAtencionAsync(atencionId);
                var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(atencionId);
                var rolNombre = _usuarioContextService.ObtenerRolNombre();
                var usuarioId = _usuarioContextService.ObtenerUsuarioId();

                bool esAdministrador = !string.IsNullOrEmpty(rolNombre) && (
                    rolNombre.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Auditoria", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Auditoría", StringComparison.OrdinalIgnoreCase)
                );

                var resultList = new List<object>();

                foreach (var d in documentos)
                {
                    if (esAdministrador || (d.UsuarioEliminacion.HasValue && d.UsuarioEliminacion.Value == usuarioId && (atencion?.EstadoAtencionId ?? int.MaxValue) <= 4))
                    {
                        ApiSigestHC.Modelos.Usuario usuarioElim = null;
                        if (d.UsuarioEliminacion.HasValue)
                        {
                            usuarioElim = await _usuarioRepo.GetUsuarioAsync(d.UsuarioEliminacion.Value);
                        }

                        resultList.Add(new
                        {
                            id = d.Id,
                            nombreArchivo = d.NombreArchivo,
                            tipoDocumentoId = d.TipoDocumentoId,
                            tipoDocumento = d.TipoDocumento == null ? null : new {
                                id = d.TipoDocumento.Id,
                                nombre = d.TipoDocumento.Nombre,
                                esAsistencial = d.TipoDocumento.EsAsistencial,
                                extensionPermitida = d.TipoDocumento.ExtensionPermitida
                            },
                            atencion = d.Atencion == null ? null : new {
                                id = d.Atencion.Id,
                                paciente = d.Atencion.Paciente == null ? null : new {
                                    nombre = (d.Atencion.Paciente.PrimerNombre ?? "") + " " + (d.Atencion.Paciente.SegundoNombre ?? ""),
                                    apellidos = (d.Atencion.Paciente.PrimerApellido ?? "") + " " + (d.Atencion.Paciente.SegundoApellido ?? ""),
                                    numeroDocumento = d.Atencion.Paciente.Id
                                }
                            },
                            fecha = d.Fecha,
                            fechaCarga = d.FechaCarga,
                            rutaRelativa = d.RutaRelativa,
                            usuario = d.Usuario == null ? null : new {
                                nombre = d.Usuario.Nombre,
                                apellidos = d.Usuario.Apellidos,
                                nombreUsuario = d.Usuario.NombreUsuario
                            },
                            fechaEliminacion = d.FechaEliminacion,
                            usuarioEliminacion = d.UsuarioEliminacion,
                            usuarioEliminacionNombre = usuarioElim == null ? null : new {
                                nombre = usuarioElim.Nombre,
                                apellidos = usuarioElim.Apellidos,
                                nombreUsuario = usuarioElim.NombreUsuario
                            }
                        });
                    }
                }

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = resultList };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.InternalServerError, ErrorMessages = new List<string> { "Error interno al obtener la papelera.", ex.Message } };
            }
        }

        public async Task<RespuestaAPI> RestaurarDocumentoAsync(int documentoId)
        {
            try
            {
                var documento = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (documento == null)
                {
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = new List<string> { $"Documento con id {documentoId} no encontrado." } };
                }

                var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(documento.AtencionId);
                var rolNombre = _usuarioContextService.ObtenerRolNombre();
                var usuarioId = _usuarioContextService.ObtenerUsuarioId();

                bool esAdministrador = !string.IsNullOrEmpty(rolNombre) && (
                    rolNombre.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Auditoria", StringComparison.OrdinalIgnoreCase) ||
                    rolNombre.Equals("Auditoría", StringComparison.OrdinalIgnoreCase)
                );

                if (!esAdministrador && !(documento.UsuarioEliminacion.HasValue && documento.UsuarioEliminacion.Value == usuarioId && (atencion?.EstadoAtencionId ?? int.MaxValue) <= 4))
                {
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.Forbidden, ErrorMessages = new List<string> { "No tiene permisos para restaurar este documento." } };
                }

                var tipoDoc = documento.TipoDocumento ?? new TipoDocumento();
                if (!tipoDoc.PermiteMultiples)
                {
                    var existe = await _documentoRepo.ExisteDocumentoAsync(documento.AtencionId, documento.TipoDocumentoId);
                    if (existe)
                    {
                        return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.Conflict, ErrorMessages = new List<string> { "Ya existe un documento activo de este tipo en la atención." } };
                    }
                }

                if (documento.SolicitudesCorreccion != null && documento.SolicitudesCorreccion.Any(s => s.EstadoCorreccionId != 3))
                {
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.BadRequest, ErrorMessages = new List<string> { "No se puede restaurar el documento porque tiene solicitudes de corrección pendientes." } };
                }

                documento.FechaEliminacion = null;
                documento.UsuarioEliminacion = null;

                await _documentoRepo.ActualizarAsync(documento);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = "Documento restaurado correctamente" };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.InternalServerError, ErrorMessages = new List<string> { "Error interno al restaurar documento.", ex.Message } };
            }
        }

        public async Task<RespuestaAPI> ObtenerDocumentosPorAtencionAsync(int atencionId)
        {
            try
            {
                int rolId = _usuarioContextService.ObtenerRolId();

                var documentos = await _documentoRepo.ObtenerPermitidosParaVer(atencionId, rolId);                       
                var documentosDto = _mapper.Map<IEnumerable<DocumentoDto>>(documentos);

                foreach (var doc in documentosDto)
                {
                    doc.puedeCargar = await _tipoDocumentoRolRepo.PuedeCargarTipoDocumento(rolId, doc.TipoDocumentoId);
                }
                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = documentosDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al obtener los documentos.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> CargarDocumentoAsync(DocumentoCargarDto dto)
        {
            var validacion = await _validacionCargaDocumentoService.ValidarCargaDocumentoAsync(dto);
            if (!validacion.Ok)
                return validacion;

            try
            {
                // 1. Crear documento inicial en BD (sin archivo físico aún)
                var nuevoDocumento = new Documento
                {
                    AtencionId = dto.AtencionId,
                    TipoDocumentoId = dto.TipoDocumentoId,
                    UsuarioId = _usuarioContextService.ObtenerUsuarioId(),
                    RutaBase = string.Empty, // se actualiza después
                    RutaRelativa = string.Empty, // se actualiza después
                    NombreArchivo = "temporal.tmp", // se actualiza después
                    Fecha = dto.Fecha,
                    NumeroRelacion = dto.NumeroRelacion,
                    Observacion = dto.Observacion,
                    FechaCarga = DateTime.UtcNow,
                    TamanoBytes = 0, // se actualiza después
                    NumeroPaginas = 0 // se actualiza después
                };

                await _documentoRepo.GuardarAsync(nuevoDocumento);
                
                // 2. Guardar archivo físico (ahora con el ID generado)
                var resultado = await _almacenamientoArchivoService.GuardarArchivoAsync(dto, nuevoDocumento.Id);

                // 3. Actualizar documento con información del archivo
                nuevoDocumento.RutaBase = resultado.RutaBase;
                nuevoDocumento.RutaRelativa = resultado.RutaRelativa;
                nuevoDocumento.NombreArchivo = resultado.NombreArchivo;
                nuevoDocumento.TamanoBytes = resultado.TamanoBytes;
                nuevoDocumento.NumeroPaginas = resultado.NumeroPaginas;

                await _documentoRepo.ActualizarAsync(nuevoDocumento);
                
                // 4. Obtener documento completo y retornar
                var documentoBD = await _documentoRepo.ObtenerPorIdAsync(nuevoDocumento.Id);
                var dtoResultado = _mapper.Map<DocumentoDto>(documentoBD);

                return new RespuestaAPI 
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = dtoResultado
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al cargar el documento.", ex.Message }
                };
            }
            
        }

        public async Task<RespuestaAPI> EditarDocumentoAsync(DocumentoEditarDto dto)
        {
            try
            {
                // 1. Obtener documento existente
                var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
                if (documento == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Documento con id {dto.Id} no encontrado." }
                    };
                }

                // 2. Actualizar campos simples
                if (!string.IsNullOrWhiteSpace(dto.NumeroRelacion))
                    documento.NumeroRelacion = dto.NumeroRelacion;
                if (!string.IsNullOrWhiteSpace(dto.Observacion))
                    documento.Observacion = dto.Observacion;
                if (dto.Fecha != null)
                    documento.Fecha = (DateTime)dto.Fecha;

                // 3. Renombrar archivo si es necesario (usa reglas del tipo de documento)
                var resultado = await _almacenamientoArchivoService.ActualizarNombreSiEsNecesarioAsync(dto);
                documento.NombreArchivo = resultado.NombreArchivo;

                // 4. Guardar cambios
                await _documentoRepo.ActualizarAsync(documento);

                DocumentoDto documentoDto = _mapper.Map<DocumentoDto>(documento);
                // 5. Retornar respuesta
                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = documentoDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al editar el documento.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> ReemplazarDocumentoCorreccionAsync(DocumentoReemplazarDto dto,int usuarioId)
        {         
            try
            {
                // 1. Obtener el documento original
                var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
                if (documento == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Documento con id {dto.Id} no encontrado." }
                    };
                }
                // 2. Validar si es posible reemplazar el documento
                var validacion = await _validacionCargaDocumentoService.ValidarArchivoAsync(dto.Archivo,documento.TipoDocumento);
                if (!validacion.Ok)
                    return validacion;

                // 4. Actualizar metadatos del documento
                documento.FechaCarga = DateTime.Now;
                documento.UsuarioId = usuarioId;


                // 3. Reemplazar el archivo físico
                await _almacenamientoArchivoService.ReemplazarArchivoDocuemntoAsync(documento, dto.Archivo);
               

                // 5. Guardar en base de datos
                await _documentoRepo.ActualizarAsync(documento);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = "Documento reemplazado exitosamente"
                };
            }
            catch (ArgumentException ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al reemplazar el documento.", ex.Message }
                };
            }
        }
        public async Task<RespuestaAPI> ReemplazarDocumentoPorFirma(DocumentoReemplazarDto dto, int documentoId)
        {
            try
            {
                // 1. Obtener el documento original
                var documento = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (documento == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Documento con id {dto.Id} no encontrado." }
                    };
                }
                // 2. Validar si es posible reemplazar el documento
                var validacion = await _validacionCargaDocumentoService.ValidarArchivoAsync(dto.Archivo, documento.TipoDocumento);
                if (!validacion.Ok)
                    return validacion;

                // 4. Actualizar metadatos del documento
                //documento.FechaCarga = DateTime.Now;
                //documento.UsuarioId = usuarioId;


                // 3. Reemplazar el archivo físico
                await _almacenamientoArchivoService.ReemplazarArchivoDocuemntoAsync(documento, dto.Archivo);


                // 5. Guardar en base de datos
                await _documentoRepo.ActualizarAsync(documento);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = "Documento reemplazado exitosamente"
                };
            }
            catch (ArgumentException ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al reemplazar el documento.", ex.Message }
                };
            }
        }


        //public async Task<RespuestaAPI> CorregirDocumentoAsync(DocumentoReemplazarDto dto)
        //{
        //    // 1. Verificar si hay una corrección pendiente
        //    var correccion = await _solicitudCorreccionRepo.ObtenerPendientePorDocumentoIdAsync(dto.Id);
        //    if (correccion == null)
        //    {
        //        return new RespuestaAPI
        //        {
        //            Ok = false,
        //            StatusCode = HttpStatusCode.BadRequest,
        //            ErrorMessages = new List<string> { "Este documento no tiene una solicitud de corrección pendiente." }
        //        };
        //    }

        //    // 2. Validar si es posible reemplazar (permite en estado auditoría si hay corrección pendiente)
        //    var validacion = await _validacionCargaDocumentoService.ValidarReemplazoDocumentoAsync(dto);
        //    if (!validacion.Ok)
        //        return validacion;

        //    try
        //    {
        //        // 3. Obtener documento original
        //        var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
        //        if (documento == null)
        //        {
        //            return new RespuestaAPI
        //            {
        //                Ok = false,
        //                StatusCode = HttpStatusCode.NotFound,
        //                ErrorMessages = new List<string> { $"Documento con id {dto.Id} no encontrado." }
        //            };
        //        }

        //        // 4. Reemplazar archivo físico
        //        await _almacenamientoArchivoService.ReemplazarArchivoCorreccionAsync(documento, dto.Archivo);

        //        // 5. Actualizar información del documento
        //        var usuarioId = _usuarioContextService.ObtenerUsuarioId();
        //        documento.FechaCarga = DateTime.UtcNow;
        //        documento.UsuarioId = usuarioId;
        //        await _documentoRepo.ActualizarAsync(documento);

        //        // 6. Marcar corrección como aplicada
        //        correccion.EstadoCorreccionId = 3;
        //        correccion.FechaCorrige = DateTime.UtcNow;
        //        correccion.UsuarioCorrigeId = usuarioId;
        //        await _solicitudCorreccionRepo.ActualizarAsync(correccion);

        //        return new RespuestaAPI
        //        {
        //            Ok = true,
        //            StatusCode = HttpStatusCode.OK,
        //            Result = "Corrección aplicada exitosamente."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new RespuestaAPI
        //        {
        //            Ok = false,
        //            StatusCode = HttpStatusCode.InternalServerError,
        //            ErrorMessages = new List<string> { "Ocurrió un error al corregir el documento.", ex.Message }
        //        };
        //    }
        //}

        public async Task<RespuestaAPI> EliminarDocumentoAsync(int documentoId)
        {
            var respuesta = new RespuestaAPI();

            try
            {
                var documento = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (documento == null)
                {
                    respuesta.Ok = false;
                    respuesta.StatusCode = HttpStatusCode.NotFound;
                    respuesta.ErrorMessages.Add($"No se encontró el documento con id {documentoId}");
                    return respuesta;
                }

                // Validar si ya fue eliminado
                if (documento.FechaEliminacion.HasValue)
                {
                    respuesta.Ok = false;
                    respuesta.StatusCode = HttpStatusCode.BadRequest;
                    respuesta.ErrorMessages.Add($"El documento ya fue eliminado el {documento.FechaEliminacion:yyyy-MM-dd HH:mm}");
                    return respuesta;
                }
                // Validar que no existan solicitudes de corrección pendientes o respondidas
                if (documento.SolicitudesCorreccion != null && documento.SolicitudesCorreccion.Any(s => s.EstadoCorreccionId != 3))
                {
                    respuesta.Ok = false;
                    respuesta.StatusCode = HttpStatusCode.BadRequest;
                    respuesta.ErrorMessages.Add("No se puede eliminar un documento con solicitudes de corrección pendientes.");
                    return respuesta;
                }

                var roleId = _usuarioContextService.ObtenerRolId();
                var puedeEliminar = await _tipoDocumentoRolRepo.PuedeCargarTipoDocumento(roleId, documento.TipoDocumentoId);

                if (!puedeEliminar)
                {
                    respuesta.Ok = false;
                    respuesta.StatusCode = HttpStatusCode.Forbidden;
                    respuesta.ErrorMessages.Add($"No tiene permisos para eliminar este documento");
                    return respuesta;
                }

                // Mover archivo físico a carpeta _deleted
                var nuevaRutaRelativa = await _almacenamientoArchivoService.MoverArchivoAEliminadosAsync(documento);

                // Actualizar registro con eliminación lógica
                documento.FechaEliminacion = DateTime.UtcNow;
                documento.UsuarioEliminacion = _usuarioContextService.ObtenerUsuarioId();
                documento.RutaRelativa = nuevaRutaRelativa;

                await _documentoRepo.ActualizarAsync(documento);

                respuesta.Ok = true;
                respuesta.StatusCode = HttpStatusCode.OK;
                respuesta.Result = $"Documento con id {documentoId} eliminado correctamente";
                return respuesta;
            }
            catch (FileNotFoundException ex)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("El archivo físico no fue encontrado.");
                respuesta.ErrorMessages.Add(ex.Message);
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.InternalServerError;
                respuesta.ErrorMessages.Add("Error al eliminar el documento.");
                respuesta.ErrorMessages.Add(ex.Message);
                return respuesta;
            }
        }
        public async Task<IActionResult> DescargarDocumentoAsync(int documentoId)
        {
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();

                var doc = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (doc == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Documento con id {documentoId} no encontrado." }
                    });
                }

                var puedeVer = await _documentoRepo.PuedeVerDocumento(rolId, doc.TipoDocumentoId);
                if (!puedeVer)
                {
                    return new ObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.Forbidden,
                        ErrorMessages = new List<string> { "No tiene permiso para ver este documento." }
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var archivo = await _almacenamientoArchivoService.DescargarDocumentoAsync(doc);
                if (archivo == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Archivo no encontrado físicamente." }
                    });
                }

                return archivo;
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Ocurrió un error al intentar descargar el documento.", ex.Message }
                });
            }
        }

        public async Task<IActionResult> VerDocumentoAsync(int documentoId)
        {
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();

                var doc = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (doc == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Documento no encontrado." }
                    });
                }

                var puedeVerTipo = await _documentoRepo.PuedeVerDocumento(rolId, doc.TipoDocumentoId);
                if (!puedeVerTipo)
                {
                    return new ObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.Forbidden,
                        ErrorMessages = new List<string> { "No tienes permisos para ver este documento." }
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var resultado = await _almacenamientoArchivoService.ObtenerArchivoParaVisualizacionAsync(doc);
                if (resultado == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "El archivo físico no fue encontrado." }
                    });
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Error al intentar ver el documento.", ex.Message }
                });
            }
        }

        

    }

}
