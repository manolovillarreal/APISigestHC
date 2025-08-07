using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ApiSigestHC.Repositorio.IRepositorio;
using System.Net;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;

namespace ApiSigestHC.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TipoDocumentoRolController : ControllerBase
    {
        private readonly ITipoDocumentoRolService _service;

        public TipoDocumentoRolController(ITipoDocumentoRolService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene las relaciones de un tipo de documento con roles.
        /// </summary>
        /// 
        [HttpGet("por-tipo/{tipoDocumentoId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPorTipoDocumento(int tipoDocumentoId)
        {
            var respuesta = await _service.ObtenerPorTipoDocumentoAsync(tipoDocumentoId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        /// <summary>
        /// Obtiene las relaciones de un tipo de documento con roles.
        /// </summary>
        /// 
        [HttpGet("por-rol/{rolId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPorRol(int rolId)
        {
            var respuesta = await _service.ObtenerPorRolAsync(rolId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        /// <summary>
        /// Crea una nueva relación entre tipo de documento y rol.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] TipoDocumentoRolCrearDto dto)
        {
            var respuesta = await _service.CrearAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        /// <summary>
        /// Actualiza una relación existente entre tipo de documento y rol.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Actualizar([FromBody] TipoDocumentoRolCrearDto dto)
        {
            var respuesta = await _service.ActualizarAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        /// <summary>
        /// Elimina una relación entre tipo de documento y rol.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Eliminar([FromQuery] int tipoDocumentoId, [FromQuery] int rolId)
        {
            var respuesta = await _service.EliminarAsync(tipoDocumentoId, rolId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }
    }



}
