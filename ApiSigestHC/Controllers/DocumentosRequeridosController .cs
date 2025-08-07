using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using XAct.Library.Settings;
using System.Net;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosRequeridosController : ControllerBase
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly IDocumentoRequeridoService _documentoRequeridoService;
        private readonly IMapper _mapper;

        public DocumentosRequeridosController(IDocumentoRequeridoRepositorio documentoRequeridoRepo,
                                              IDocumentoRequeridoService documentoRequeridoService,
                                                IMapper mapper)
        {
            _documentoRequeridoRepo = documentoRequeridoRepo;
            _documentoRequeridoService = documentoRequeridoService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var docs = await _documentoRequeridoRepo.ObtenerTodosAsync();
                var docsDto = _mapper.Map<IEnumerable<DocumentoRequeridoCrearDto>>(docs);

                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = docsDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener los documentos requeridos.", ex.Message }
                });
            }
        }

        [HttpGet("por-estado/{estadoAtencionId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorEstado(int estadoAtencionId)
        {
            try
            {
                var docs = await _documentoRequeridoRepo.ObtenerPorEstadoAsync(estadoAtencionId);
                var docsDto = _mapper.Map<IEnumerable<DocumentoRequeridoCrearDto>>(docs);

                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = docsDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener documentos por estado.", ex.Message }
                });
            }
        }

        [HttpGet("por-tipo/{tipoDocumentoId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorTipo(int tipoDocumentoId)
        {
            try
            {
                var doc = await _documentoRequeridoRepo.ObtenerPorTipoAsync(tipoDocumentoId);
                if (doc == null)
                {
                    return Ok( new RespuestaAPI
                    {
                        Ok = true,
                        StatusCode = HttpStatusCode.OK,
                        ErrorMessages = new List<string> { "No hay documentos requeridos asociados." }
                    });
                }
                var docDto = _mapper.Map<DocumentoRequeridoDto>(doc);

                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = docDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener documentos por estado.", ex.Message }
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] DocumentoRequeridoCrearDto dto)
        {
            var respuesta = await _documentoRequeridoService.CrearAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{tipoDocumentoId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Eliminar( int tipoDocumentoId)
        {
            var respuesta = await _documentoRequeridoService.EliminarAsync(tipoDocumentoId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

    }

}
