using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(IConfiguracionService configuracionService)
        {
            _configuracionService = configuracionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodas()
        {
            var respuesta = await _configuracionService.ObtenerTodasAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [HttpPut("{clave}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(string clave, [FromBody] ActualizarConfiguracionDto dto)
        {
            var respuesta = await _configuracionService.ActualizarAsync(clave, dto.Valor);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }
    }
}
