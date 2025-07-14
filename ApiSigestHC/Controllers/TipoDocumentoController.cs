using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiSigestHC.Modelos; // Reemplaza con el namespace real
using ApiSigestHC.Repositorio; // Reemplaza con el namespace real
using ApiSigestHC.Modelos.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApiSigestHC.Servicios.IServicios;
using System.Net;

namespace ApiSigestHC.Controllers
{  

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly ITipoDocumentoService _tipoDocumentoService;

        public TipoDocumentoController(
            ITipoDocumentoService tipoDocumentoService
          )
        {
            _tipoDocumentoService = tipoDocumentoService;
        }

        // GET: api/TipoDocumento
        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTodos()
        {
            var respuesta = await _tipoDocumentoService.ObtenerTodosAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // GET: api/TipoDocumento/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var respuesta = await _tipoDocumentoService.ObtenerPorIdAsync(id);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // GET: api/TipoDocumento/Autorizados
        [Authorize]
        [HttpGet("Autorizados")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTiposPermitidosPorRol()
        {
            var respuesta = await _tipoDocumentoService.ObtenerTiposPermitidosPorRolAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // POST: api/TipoDocumento
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] TipoDocumentoCrearDto dto)
        {
            var respuesta = await _tipoDocumentoService.CrearAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // PUT: api/TipoDocumento/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Editar(int id, [FromBody] TipoDocumentoCrearDto dto)
        {
            var respuesta = await _tipoDocumentoService.EditarAsync(id, dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }
    }


}
