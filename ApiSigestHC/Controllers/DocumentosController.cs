using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using XAct.Library.Settings;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;

namespace ApiSigestHC.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentosController : ControllerBase
    {
        private readonly IDocumentoService _documentoService;
        private readonly IThumbnailPdfService _thumbnailPdfService;
        private readonly IDocumentoRepositorio _documentoRepo;

        public DocumentosController(IDocumentoService documentoService, IThumbnailPdfService thumbnailPdfService, IDocumentoRepositorio documentoRepo)
        {
            _documentoService = documentoService;
            _thumbnailPdfService = thumbnailPdfService;
            _documentoRepo = documentoRepo;
        }


        [HttpGet("por-atencion/{atencionId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerDocumentosPorAtencion(int atencionId)
        {
            var respuesta = await _documentoService.ObtenerDocumentosPorAtencionAsync(atencionId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }


        [HttpPost("cargar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))] // Documento cargado exitosamente
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))] // Datos inválidos o validación fallida
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(RespuestaAPI))] // Usuario no autorizado para cargar ese documento
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))] // Atención o tipo de documento no encontrados
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))] // Error del sistema al guardar archivo

        public async Task<IActionResult> CargarDocumento([FromForm] DocumentoCargarDto dto)
        {
            var respuesta = await _documentoService.CargarDocumentoAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }


        [HttpPut("editar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> EditarDocumento([FromBody] DocumentoEditarDto dto)
        {
            var resultado = await _documentoService.EditarDocumentoAsync(dto);
            return StatusCode((int)resultado.StatusCode, resultado);
        }


   


        [HttpPost("corregir")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> CorregirDocumento([FromForm] DocumentoReemplazarDto dto)
        {
            var resultado = await _documentoService.CorregirDocumentoAsync(dto);
            return StatusCode((int)resultado.StatusCode, resultado);
        }



        [HttpGet("descargar/{documentoId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> DescargarDocumento(int documentoId)
        {
            return await _documentoService.DescargarDocumentoAsync(documentoId);
        }


        [HttpGet("ver/{documentoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerDocumento(int documentoId)
        {
            return await _documentoService.VerDocumentoAsync(documentoId);
        }

        [HttpDelete("{documentoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> EliminarDocumento(int documentoId)
        {
            var resultado = await _documentoService.EliminarDocumentoAsync(documentoId);

            if (!resultado.Ok)
                return StatusCode((int)resultado.StatusCode, resultado);

            return Ok(resultado);
        }

        [HttpGet("thumbnails/{documentoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> ObtenerThumbnailsPdf(int documentoId)
        {
            var documento = await _documentoRepo.ObtenerPorIdAsync(documentoId);
            if (documento == null)
            {
                return NotFound(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Documento no encontrado." }
                });
            }
            if (!documento.NombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "El documento no es un PDF." }
                });
            }
            var filePath = Path.Combine(documento.RutaBase, documento.RutaRelativa, documento.NombreArchivo);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Archivo físico no encontrado." }
                });
            }
            await using var stream = System.IO.File.OpenRead(filePath);
            var thumbnails = await _thumbnailPdfService.GenerarThumbnailsAsync(stream, 150, 150);
            return Ok(new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = thumbnails
            });
        }

    }
}
