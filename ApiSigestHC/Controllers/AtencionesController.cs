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
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IMapper _mapper;

        private readonly IValidacionDocumentosObligatoriosService _validacionDocumentosService;
        private readonly IVisualizacionEstadoService _visualizacionEstadoService;
        private readonly ICambioEstadoService _cambioEstadoService;

        public AtencionesController(IAtencionRepositorio atencionRepo, 
                IValidacionDocumentosObligatoriosService validacionDocumentosService,
                ICambioEstadoService cambioEstadoService,
                IVisualizacionEstadoService visualizacionEstadoService,
                IMapper mapper
           )
        {
            _atencionRepo = atencionRepo;
            _mapper = mapper;
            _validacionDocumentosService = validacionDocumentosService;
            _cambioEstadoService = cambioEstadoService;
            _visualizacionEstadoService = visualizacionEstadoService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAtenciones()
        //{
        //    var atenciones = await _atencionRepo.ObtenerAtencionesAsync();
        //    return Ok(atenciones);
        //}

        [HttpGet("visibles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AtencionDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAtencionesVisibles()
        {
            var estados = _visualizacionEstadoService.ObtenerEstadosVisiblesPorRol();

            var atenciones = await _atencionRepo.GetAtencionesPorEstadoAsync(estados);

            var atencionesDto = _mapper.Map<IEnumerable<AtencionDto>>(atenciones);

            return Ok(new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = atencionesDto
            });
        }


        [HttpGet("rango-fechas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAtencionesPorRangoFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (fechaInicio > fechaFin)
                return BadRequest(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "La fecha inicial no puede ser mayor que la final" }
                });

            var atenciones = await _atencionRepo.ObtenerPorFechasAsync(fechaInicio, fechaFin, page, pageSize);

            return Ok(new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = atenciones
            });
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearAtencion(AtencionCrearDto crearAtencionDto)
        {
            if (!ModelState.IsValid || crearAtencionDto == null)
            {
                return BadRequest(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Datos inválidos para crear la atención." }
                });
            }

            var atencion = _mapper.Map<Atencion>(crearAtencionDto);
            await _atencionRepo.CrearAtencionAsync(atencion);
            return CreatedAtAction(nameof(CrearAtencion), new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created,
                Result = atencion
            });
        }

        [HttpPut("editar")]
        [Authorize(Roles = "Admin,Admisiones")] // Puedes ajustar los roles permitidos
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditarAtencion([FromBody] AtencionEditarDto atencionDto)
        {
            if (!ModelState.IsValid || atencionDto == null)
            {
                return BadRequest(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Datos inválidos para editar la atención." }
                });
            }

            var atencionExistente = await _atencionRepo.ObtenerAtencionPorIdAsync(atencionDto.AtencionId);

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

            await _atencionRepo.EditarAtencionAsync(atencionExistente);

            return Ok(new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Atención {atencionDto.AtencionId} actualizada correctamente",
                Result = atencionExistente,
            });
        }


        [HttpPost("cambiar-estado")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> CambiarEstado([FromBody] AtencionCambioEstadoDto dto)
        {
            var resultado = await _cambioEstadoService.CambiarEstadoAsync(dto);

            var respuesta = new RespuestaAPI
            {
                IsSuccess = resultado.IsSuccess,
                StatusCode = resultado.StatusCode,
                ErrorMessages = resultado.ErrorMessages,
                Result = resultado.IsSuccess ? resultado.Result : null
            };

            return StatusCode((int)resultado.StatusCode, respuesta);
        }



    }
}
