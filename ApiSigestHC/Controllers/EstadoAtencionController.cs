using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoAtencionController : ControllerBase
    {
        private readonly IEstadoAtencionRepositorio _estadoRepo;
        private readonly IMapper _mapper;

        public EstadoAtencionController(IEstadoAtencionRepositorio estadoRepo, IMapper mapper)
        {
            _estadoRepo = estadoRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAPI))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(RespuestaAPI))]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var estados = await _estadoRepo.ObtenerTodosAsync();
                var estadosDto = _mapper.Map<IEnumerable<EstadoAtencionDto>>(estados);

                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = estadosDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al obtener los estados de atención.", ex.Message }
                });
            }
        }
    }
}
