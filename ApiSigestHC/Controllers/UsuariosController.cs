using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiSigestHC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarios()
        {
            var respuesta = await _usuarioService.ObtenerUsuariosAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{usuarioId:int}", Name = "GetUsuario")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuario(int usuarioId)
        {
            var respuesta = await _usuarioService.ObtenerUsuarioPorIdAsync(usuarioId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("crear")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioCrearDto dto)
        {
            var respuesta = await _usuarioService.CrearUsuarioAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto dto)
        {
            var respuesta = await _usuarioService.LoginAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [Authorize]
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var respuesta = await _usuarioService.ObtenerPerfilAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

    }
}
