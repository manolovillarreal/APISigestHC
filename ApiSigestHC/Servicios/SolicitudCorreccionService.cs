using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Metadata;

namespace ApiSigestHC.Servicios
{
    public class SolicitudCorreccionService : ISolicitudCorreccionService
    {
        private readonly ISolicitudCorreccionRepositorio _solicitudRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IMapper _mapper;
        private readonly IAlmacenamientoArchivoService _almacenamientoArchivoService;
        private readonly IValidacionCargaArchivoService _validacionCargaDocumento;
        private readonly IDocumentoService _documentoService;

        public SolicitudCorreccionService(
            ISolicitudCorreccionRepositorio solicitudRepo,
            IDocumentoRepositorio documentoRepo,
            IUsuarioContextService usuarioContext,
            IMapper mapper,
            IAlmacenamientoArchivoService almacenamientoArchivoService,
            IValidacionCargaArchivoService validacionCargaDocumento,
            IDocumentoService documentoService)
        {
            _solicitudRepo = solicitudRepo;
            _documentoRepo = documentoRepo;
            _usuarioContextService = usuarioContext;
            _mapper = mapper;
            _almacenamientoArchivoService = almacenamientoArchivoService;
            _validacionCargaDocumento = validacionCargaDocumento;
            _documentoService = documentoService;
        }

        public async Task<RespuestaAPI> AprobarSolicitudAsync(int id, SolicitudCorreccionAprobarDto dto)
        {
            var respuesta = new RespuestaAPI();

            // Validar DTO y archivo
            if (dto == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("Erro en la informacion enviada.");
                return respuesta;
            }
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id);
            if (solicitud == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("No se encontr� la solicitud de correcci�n.");
                return respuesta;
            }
            if (solicitud.EstadoCorreccionId != 2)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.Conflict;
                respuesta.ErrorMessages.Add("La solicitud no est� en estado de aprobaci�n.");
                return respuesta;
            }

            var archivo = await _almacenamientoArchivoService.ObtenerArchivoTemporalComoFormFileAsync("correcciones", id);

            if (archivo == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("No se encontr� el archivo de correcci�n.");
                return respuesta;
            }
            if (dto.ConservarDocumentoAnterior)
            {
                respuesta = await _documentoService.CargarDocumentoAsync(new DocumentoCargarDto
                {
                    AtencionId = solicitud.Documento.AtencionId,
                    NumeroRelacion = solicitud.Documento.NumeroRelacion,
                    TipoDocumentoId = solicitud.Documento.TipoDocumentoId,
                    Fecha = DateTime.Now,
                    Archivo = archivo,
                    Observacion = solicitud.Documento.Observacion,
                });
            }
            else
            {
                respuesta = await _documentoService.ReemplazarDocumentoCorreccionAsync(new DocumentoReemplazarDto
                {
                    Id = solicitud.DocumentoId,
                    Archivo = archivo,
                },solicitud.UsuarioCorrigeId.Value);
            }

            if (!respuesta.Ok)
            {
                return respuesta;
            }

            // Actualizar la solicitud

            solicitud.EstadoCorreccionId = 3; // Aprobada
            await _solicitudRepo.ActualizarAsync(solicitud);

            await _almacenamientoArchivoService.EliminarArchivoTemporalAsync("correcciones", id);

            var solicituDto = _mapper.Map<SolicitudCorreccionDocDto>(solicitud);
            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Solicitud de correcci�n aprobada y documento actualizado correctamente.",
                Result= solicituDto
            };
        }

        public async Task<RespuestaAPI> CrearAsync(SolicitudCorreccionCrearDto dto)
        {
            var respuesta = new RespuestaAPI();

            if (dto == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("Solicitud inv�lida.");
                return respuesta;
            }

            if (dto.DocumentoId <= 0)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("DocumentoId inv�lido.");
                return respuesta;
            }

            // Evitar duplicados: si ya existe una solicitud pendiente para el documento, no crear otra
            var existente = await _solicitudRepo.ObtenerPendientePorDocumentoIdAsync(dto.DocumentoId);
            if (existente != null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.Conflict;
                respuesta.ErrorMessages.Add("Ya existe una solicitud de correcci�n pendiente para este documento.");
                return respuesta;
            }

            var usuarioId = _usuarioContextService.ObtenerUsuarioId();

            var entidad = new SolicitudCorreccion
            {
                DocumentoId = dto.DocumentoId,
                UsuarioSolicitaId = usuarioId,
                FechaSolicitud = DateTime.Now,
                EstadoCorreccionId = 1,                
                Observacion = dto.Observacion ?? string.Empty,                
                UsuarioCorrigeId = null,
                FechaCorrige = null
            };

            await _solicitudRepo.AgregarAsync(entidad);

            entidad.EstadoCorreccion = new EstadoCorreccion { Id = 1, Nombre = "Pendiente" };

            respuesta.Ok = true;
            respuesta.StatusCode = HttpStatusCode.Created;
            respuesta.Message = "Solicitud de correcci�n creada correctamente.";
            respuesta.Result = entidad;
            return respuesta;
        }

        public async Task<RespuestaAPI> ObtenerPorDocumentoAsync(int documentoId)
        {
            var respuesta = new RespuestaAPI();

            if (documentoId <= 0)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("DocumentoId inv�lido.");
                return respuesta;
            }

            var items = await _solicitudRepo.ObtenerPorDocumentoAsync(documentoId);
            var itemsDto = _mapper.Map<IEnumerable<SolicitudCorreccionDocDto>>(items);
            respuesta.Ok = true;
            respuesta.StatusCode = HttpStatusCode.OK;
            respuesta.Result = items;

            return respuesta;
        }

        public async Task<RespuestaAPI> ObtenerPorRolUsuarioAsync(FiltroCorreccionesDto filtro)
        {
            var respuesta = new RespuestaAPI();
            var rolId = _usuarioContextService.ObtenerRolId();

            // Consulta usando LINQ para filtrar por rol y filtros opcionales
            var solicitudes = await _solicitudRepo.ObtenerSolicitudesPorRolAsync(rolId, filtro);

            if( solicitudes == null || !solicitudes.Any())
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NoContent;
                respuesta.ErrorMessages.Add("No se encontraron solicitudes de correcci�n para el rol actual.");
                return respuesta;
            }

            var solicitudesDtos = _mapper.Map<IEnumerable<SolicitudCorreccionDto>>(solicitudes);
            respuesta.Ok = true;
            respuesta.StatusCode = HttpStatusCode.OK;
            respuesta.Result = solicitudesDtos;

            return respuesta;
        }

        public async Task<RespuestaAPI> ObtenerEnviadasPorRolAsync()
        {
            var respuesta = new RespuestaAPI();
            var rolId = _usuarioContextService.ObtenerRolId();

            var solicitudes = await _solicitudRepo.ObtenerSolicitudesEnviadasPorRolAsync(rolId);

            if (solicitudes == null || !solicitudes.Any())
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NoContent;
                respuesta.ErrorMessages.Add("No se encontraron solicitudes enviadas para el rol actual.");
                return respuesta;
            }

            var solicitudesDtos = _mapper.Map<IEnumerable<SolicitudCorreccionDto>>(solicitudes);
            respuesta.Ok = true;
            respuesta.StatusCode = HttpStatusCode.OK;
            respuesta.Result = solicitudesDtos;

            return respuesta;
        }

        public async  Task<RespuestaAPI> RechazarSolicitudAsync(int solicitudId, string  observacion)
        {
            var respuesta = new RespuestaAPI();

            // Validar DTO y archivo
            if (string.IsNullOrEmpty(observacion))
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("Erro en la informacion enviada.");
                return respuesta;
            }
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(solicitudId);
            if (solicitud == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("No se encontr� la solicitud de correcci�n.");
                return respuesta;
            }
            if (solicitud.EstadoCorreccionId != 2)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.Conflict;
                respuesta.ErrorMessages.Add("La solicitud no est� en estado de aprobaci�n.");
                return respuesta;
            }
           
            // Actualizar la solicitud

            solicitud.EstadoCorreccionId = 4; // Rechcazada
            solicitud.Observacion += "|" + observacion;

            await _solicitudRepo.ActualizarAsync(solicitud);

            solicitud.EstadoCorreccion = new EstadoCorreccion { Id = 4, Nombre = "Rechazada" };
            var solicituDto = _mapper.Map<SolicitudCorreccionDocDto>(solicitud);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Solicitud de correcci�n aprobada y documento actualizado correctamente.",
                Result = solicituDto
            };
        }

        public async Task<RespuestaAPI> ResponderSolicitudAsync(int id, SolicitudCorreccionRespuestaDto dto)
        {
            var respuesta = new RespuestaAPI();

            // Validar DTO y archivo
            if (dto == null || dto.Archivo == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("Debe adjuntar un archivo de respuesta.");
                return respuesta;
            }
           
            // Buscar la solicitud
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id);
            if (solicitud == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("No se encontr� la solicitud de correcci�n.");
                return respuesta;
            }

            // Validar que est� pendiente
            if (solicitud.EstadoCorreccionId == 2 || solicitud.EstadoCorreccionId == 3)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.Conflict;
                respuesta.ErrorMessages.Add("La solicitud no est� pendiente de respuesta.");
                return respuesta;
            }
            // Validar el archivo con el servicio
            var resultadoValidacion = await _validacionCargaDocumento.ValidarCargaArchivoCorreccionAsync(dto.Archivo,solicitud.Documento.TipoDocumento);
            if (!resultadoValidacion.Ok)
            {                
                return resultadoValidacion;
            }
            try
            {
                // Guardar el archivo temporal en la carpeta "correcciones"
                var resultadoArchivo = await _almacenamientoArchivoService.GuardarArchivoTemporal("correcciones", dto.Archivo, id);

                // Actualizar la solicitud
                solicitud.UsuarioCorrigeId = _usuarioContextService.ObtenerUsuarioId();
                solicitud.FechaCorrige = DateTime.Now;
                solicitud.EstadoCorreccionId = 2; // Respondida

                await _solicitudRepo.ActualizarAsync(solicitud);

                respuesta.Ok = true;
                respuesta.StatusCode = HttpStatusCode.OK;
                respuesta.Message = "Solicitud de correcci�n respondida correctamente.";
                respuesta.Result = new
                {
                };

                return respuesta;
            }
            catch (Exception ex)
            {

                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al intenrar guardar el archivo.", ex.Message }
                };
            }
            
        }

        public async Task<IActionResult> VerDocumentoCorreccion(int solicitudId)
        {
            try
            {
                var respuesta = new RespuestaAPI();

                var rolId = _usuarioContextService.ObtenerRolId();

                var solicitud = await _solicitudRepo.ObtenerPorIdAsync(solicitudId);
                if (solicitud == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Documento no encontrado." }
                    });
                }

                var puedeVerTipo = await _documentoRepo.PuedeVerDocumento(rolId, solicitud.Documento.TipoDocumentoId);
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

                var resultado = await _almacenamientoArchivoService.ObtenerArchivoTemporalParaVisualizacionAsync("correcciones",solicitudId);
                if (resultado == null)
                {
                    return new NotFoundObjectResult(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "El archivo f�sico no fue encontrado." }
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