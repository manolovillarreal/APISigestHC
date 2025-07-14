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

        public DocumentosController(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
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


        [HttpPost("reemplazar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> ReemplazarDocumento([FromForm] DocumentoReemplazarDto dto)
        {
            var resultado = await _documentoService.ReemplazarDocumentoAsync(dto);
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

            if (!resultado.IsSuccess)
                return StatusCode((int)resultado.StatusCode, resultado);

            return Ok(resultado);
        }


    }
}
