using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using XAct;

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
        private readonly IAtencionesService _atencionesService;
        private readonly IUsuarioContextService _usuarioContext;

        public AtencionesController(IAtencionRepositorio atencionRepo, 
                IValidacionDocumentosObligatoriosService validacionDocumentosService,
                ICambioEstadoService cambioEstadoService,
                IVisualizacionEstadoService visualizacionEstadoService,
                IAtencionesService atencionesService,
                IUsuarioContextService usuarioContext,
                IMapper mapper
           )
        {
            _atencionRepo = atencionRepo;
            _mapper = mapper;
            _validacionDocumentosService = validacionDocumentosService;
            _cambioEstadoService = cambioEstadoService;
            _visualizacionEstadoService = visualizacionEstadoService;
            _atencionesService = atencionesService;
            _usuarioContext = usuarioContext;
        }

    

        [HttpGet("visibles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AtencionDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAtencionesVisibles()
        {
            var estados = _visualizacionEstadoService.ObtenerEstadosVisiblesPorRol();

            var atenciones = await _atencionRepo.GetAtencionesPorEstadoAsync(estados);

            var atencionesDto = _mapper.Map<IEnumerable<AtencionDto>>(atenciones);
            
            
            string[] idsPacientesEstadoIngreso = atencionesDto
                .Where(a => a.EstadoAtencionId == 3)
                .Select(a => a.PacienteId)
                .ToArray();

            var ubicaciones = await _atencionRepo.GetUltimaUbicacionPacientesAsync(idsPacientesEstadoIngreso);

        
            foreach (var at in atencionesDto.Where(a => a.EstadoAtencionId == 3))
            {
                at.UbicacionPaciente = ubicaciones.FirstOrDefault(u => u.PacienteId == at.PacienteId);
            }


            return Ok(new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = atencionesDto
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
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Datos inválidos para crear la atención." }
                });
            }
            
            var atencion = _mapper.Map<Atencion>(crearAtencionDto);

            atencion.Fecha = DateTime.Now;
            atencion.EstadoAtencionId = 1;
            atencion.UsuarioId = _usuarioContext.ObtenerUsuarioId();

            await _atencionRepo.CrearAtencionAsync(atencion);

            atencion = await  _atencionRepo.ObtenerAtencionPorIdAsync(atencion.Id);


            return CreatedAtAction(nameof(CrearAtencion), new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.Created,
                Result = atencion
            });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditarAtencion(int id,[FromBody] AtencionEditarDto atencionDto)
        {
            if (!ModelState.IsValid || atencionDto == null)
            {
                return BadRequest(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Datos inválidos para editar la atención." }
                });
            }

            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(id);

            if (atencion == null)
            {
                return NotFound(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Atención no encontrada" }
                });
            }

            // Actualiza los campos permitidos
            atencion.TerceroId = !string.IsNullOrEmpty(atencionDto.TerceroId) 
                                    ? atencionDto.TerceroId 
                                    : atencion.TerceroId;

            atencion.TipoAtencionId = atencionDto.TipoAtencionId>0
                                   ? atencionDto.TipoAtencionId
                                   : atencion.TipoAtencionId;

            await _atencionRepo.EditarAtencionAsync(atencion);

            return Ok(new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Atención {atencionDto.AtencionId} actualizada correctamente",
                Result = atencion,
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

            return StatusCode((int)resultado.StatusCode, resultado);
        }

        [HttpPost("cerrar")]
        [Authorize(Roles = "Medico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> CerrarAtencion([FromBody] AtencionCambioEstadoDto dto)
        {
            var resultado = await _cambioEstadoService.CerrarAtencionAsync(dto);

            return StatusCode((int)resultado.StatusCode, resultado);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAtencionesFiltradas([FromQuery] AtencionFiltroDto filtro)
        {
            var respuesta = await _atencionesService.ObteneAtencionesPorFiltroAsync(filtro);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> AnularAtencion([FromBody] AnulacionAtencionCrearDto dto)
        {
            var respuesta = await _atencionesService.AnularAtencionAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }
    }
}
