using Microsoft.AspNetCore.Mvc;
using ApiSigestHC.Repositorio;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Modelos.Dtos;
using System.Net.WebSockets;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;
using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Controllers

{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AtencionesController : ControllerBase
    {
        private readonly IAtencionRepositorio _repositorio;
        private readonly IMapper _mapper;
        private readonly IValidacionDocumentosObligatoriosService _validacionDocumentosService;
        private readonly ICambioEstadoService _cambioEstadoService;

        public AtencionesController(IAtencionRepositorio repositorio, 
            IValidacionDocumentosObligatoriosService validacionDocumentosService,
            IMapper mapper,
            ICambioEstadoService cambioEstadoService)
        {
            _repositorio = repositorio;
            _mapper = mapper;
            _validacionDocumentosService = validacionDocumentosService;
            _cambioEstadoService = cambioEstadoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAtenciones()
        {
            var atenciones = await _repositorio.ObtenerAtencionesAsync();
            return Ok(atenciones);
        }
        [HttpGet("FiltrarPorEstado")]
        public async Task<IActionResult> GetAtencionesPorEstado([FromQuery] int estadoMin, [FromQuery] int estadoMax)
        {
            var atenciones = await _repositorio.GetAtencionesByStateAsync(estadoMin,estadoMax);
            return Ok(atenciones);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearAtencion(AtencionCrearDto crearAtencionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (crearAtencionDto == null)
            {
                return BadRequest(ModelState);
            }

            var atencion = _mapper.Map<Atencion>(crearAtencionDto);
            await _repositorio.CrearAtencionAsync(atencion);
            return CreatedAtAction(nameof(CrearAtencion), atencion);
        }

        [HttpPut("editar")]
        [Authorize(Roles = "Admin,Admisiones")] // Puedes ajustar los roles permitidos
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditarAtencion([FromBody] AtencionEditarDto atencionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (atencionDto == null)
            {
                return BadRequest(ModelState);
            }

            var atencionExistente = await _repositorio.ObtenerAtencionPorIdAsync(atencionDto.AtencionId);

            if (atencionExistente == null)
            {
                return NotFound(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Atención no encontrada" }
                });
            }

            // Actualiza los campos permitidos
            atencionExistente.TerceroId = atencionDto.TerceroId;

            await _repositorio.EditarAtencionAsync(atencionExistente);

            return Ok(new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = $"Atención {atencionDto.AtencionId} actualizada correctamente"
            });
        }


        [HttpPost("cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromBody] AtencionCambioEstadoDto dto)
        {
            var resultado = await _cambioEstadoService.CambiarEstadoAsync(dto);

            if (!resultado.IsSuccess)
                return StatusCode((int)resultado.StatusCode, resultado);

            return Ok(resultado);
        }


    }
}
