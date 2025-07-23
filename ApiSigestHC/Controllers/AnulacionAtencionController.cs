using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiSigestHC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnulacionAtencionController : ControllerBase
    {
        private readonly IAnulacionAtencionService _anulacionService;
        private readonly IMotivoAnulacionAtencionRepositorio _motivoRepo;

        public AnulacionAtencionController(
            IAnulacionAtencionService anulacionService,
            IMotivoAnulacionAtencionRepositorio motivoRepo)
        {
            _anulacionService = anulacionService;
            _motivoRepo = motivoRepo;
        }

        // POST: api/AnulacionAtencion
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] AnulacionAtencionCrearDto dto)
        {
            var respuesta = await _anulacionService.AnularAtencionAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // GET: api/AnulacionAtencion/motivos
        [HttpGet("motivos")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerMotivos()
        {
            var motivos = await _motivoRepo.ObtenerTodosAsync();
            return Ok(new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = motivos
            });
        }
    }
}
