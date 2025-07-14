using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CambioEstadoController : ControllerBase
    {
        private readonly ICambioEstadoRepositorio _cambioEstadoRepo;
        private readonly IMapper _mapper;

        public CambioEstadoController(ICambioEstadoRepositorio cambioEstadoRepo, IMapper mapper)
        {
            _cambioEstadoRepo = cambioEstadoRepo;
            _mapper = mapper;
        }

        [HttpGet("historial/{atencionId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> ObtenerHistorial(int atencionId)
        {
            var cambios = await _cambioEstadoRepo.ObtenerCambiosPorAtencionAsync(atencionId);

            if (cambios == null || !cambios.Any())
            {
                return NotFound(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "No se encontró historial para la atención solicitada." }
                });
            }

            var cambiosDto = _mapper.Map<List<CambioEstado>>(cambios);

            return Ok(new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = cambiosDto
            });
        }
    }
}
