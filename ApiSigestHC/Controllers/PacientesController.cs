using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XAct.Security;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController:ControllerBase
    {
        private readonly IPacienteRepositorio _repositorio;
        private readonly IMapper _mapper;

       public PacientesController(IPacienteRepositorio repositorio, IMapper mapper)
        {
            _repositorio = repositorio;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaciente(string pacienteId)
        {
            if (string.IsNullOrWhiteSpace(pacienteId))
            {
                return BadRequest(new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "El ID del paciente es obligatorio." }
                });
            }

            try
            {
                var paciente = await _repositorio.ObtenerPacientePorIdAsync(pacienteId);
                if (paciente == null)
                {
                    return NotFound(new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Paciente con ID {pacienteId} no encontrado." }
                    });
                }

                return Ok(new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = paciente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Ocurrió un error al consultar el paciente.", ex.Message }
                });
            }
        }



    }
}
