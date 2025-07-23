using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Servicios
{
    public class DocumentoService : IDocumentoService
    {
        private readonly IAlmacenamientoArchivoService _almacenamientoArchivoService;
        private readonly IValidacionCargaDocumentoService _validacionCargaDocumentoService;
        private readonly ISolicitudCorreccionRepositorio _solicitudCorreccionRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly ITipoDocumentoRolRepositorio _tipoDocumentoRolRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IMapper _mapper;

        public DocumentoService(
        IAlmacenamientoArchivoService almacenamientoArchivoService,
        ISolicitudCorreccionRepositorio solicitudCorreccionRepo,
        IValidacionCargaDocumentoService validacionCargaDocumentoService,
        IDocumentoRepositorio documentoRepo,
        ITipoDocumentoRolRepositorio tipoDocumentoRepo,
        IUsuarioContextService usuarioContextService,
        IMapper mapper)
        {
            _almacenamientoArchivoService = almacenamientoArchivoService;
            _validacionCargaDocumentoService = validacionCargaDocumentoService;
            _solicitudCorreccionRepo = solicitudCorreccionRepo;
            _documentoRepo = documentoRepo;
            _tipoDocumentoRolRepo = tipoDocumentoRepo;
            _usuarioContextService = usuarioContextService;
            _mapper = mapper;
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
                var resultado = await _almacenamientoArchivoService.GuardarArchivoAsync(dto);

                var nuevoDocumento = new Documento
                {
                    AtencionId = dto.AtencionId,
                    TipoDocumentoId = dto.TipoDocumentoId,
                    UsuarioId = _usuarioContextService.ObtenerUsuarioId(),
                    RutaBase = resultado.RutaBase,
                    RutaRelativa = resultado.RutaRelativa,
                    NombreArchivo = resultado.NombreArchivo,
                    Fecha = dto.Fecha,
                    NumeroRelacion = dto.NumeroRelacion,
                    Observacion = dto.Observacion,
                    FechaCarga = DateTime.UtcNow,
                };

                await _documentoRepo.GuardarAsync(nuevoDocumento);

                var dtoResultado = _mapper.Map<DocumentoDto>(nuevoDocumento);

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
                    ErrorMessages = new List<string> { "Error interno al editar el documento.", ex.Message }
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

        public async Task<RespuestaAPI> ReemplazarDocumentoAsync(DocumentoReemplazarDto dto)
        {
            // 1. Validar si es posible reemplazar el documento
            var validacion = await _validacionCargaDocumentoService.ValidarReemplazoDocumentoAsync(dto);

            if (!validacion.Ok)
                return validacion;

            try
            {
                // 2. Obtener el documento original
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

                // 3. Reemplazar el archivo físico
                await _almacenamientoArchivoService.ReemplazarArchivoAsync(documento, dto.Archivo);

                // 4. Actualizar metadatos del documento
                documento.FechaCarga = DateTime.UtcNow;
                documento.UsuarioId = _usuarioContextService.ObtenerUsuarioId();

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

        public async Task<RespuestaAPI> CorregirDocumentoAsync(DocumentoReemplazarDto dto)
        {
            // 1. Verificar si hay una corrección pendiente
            var correccion = await _solicitudCorreccionRepo.ObtenerPendientePorDocumentoIdAsync(dto.Id);
            if (correccion == null)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Este documento no tiene una solicitud de corrección pendiente." }
                };
            }

            // 2. Validar si es posible reemplazar (permite en estado auditoría si hay corrección pendiente)
            var validacion = await _validacionCargaDocumentoService.ValidarReemplazoDocumentoAsync(dto);
            if (!validacion.Ok)
                return validacion;

            try
            {
                // 3. Obtener documento original
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

                // 4. Reemplazar archivo físico
                await _almacenamientoArchivoService.ReemplazarArchivoAsync(documento, dto.Archivo);

                // 5. Actualizar información del documento
                var usuarioId = _usuarioContextService.ObtenerUsuarioId();
                documento.FechaCarga = DateTime.UtcNow;
                documento.UsuarioId = usuarioId;
                await _documentoRepo.ActualizarAsync(documento);

                // 6. Marcar corrección como aplicada
                correccion.Pendiente = false;
                correccion.FechaCorrige = DateTime.UtcNow;
                correccion.UsuarioCorrigeId = usuarioId;
                await _solicitudCorreccionRepo.ActualizarAsync(correccion);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = "Corrección aplicada exitosamente."
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Ocurrió un error al corregir el documento.", ex.Message }
                };
            }
        }

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

                var roleId = _usuarioContextService.ObtenerRolId();
                var puedeEliminar = await _tipoDocumentoRolRepo.PuedeCargarTipoDocumento(roleId,documento.TipoDocumentoId);

                if (!puedeEliminar)
                {
                    respuesta.Ok = false;
                    respuesta.StatusCode = HttpStatusCode.Unauthorized;
                    respuesta.ErrorMessages.Add($"No tiene pemisos para eliminar este documento ");
                    return respuesta;
                }

                await _almacenamientoArchivoService.EliminarArchivoAsync(documento);

                await _documentoRepo.EliminarAsync(documento);

                respuesta.Ok = true;
                respuesta.StatusCode = HttpStatusCode.OK;
                respuesta.Result = $"Documento con id {documentoId} eliminado correctamente";
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
