using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MotivosAnulacionAtencionController : ControllerBase
    {
        private readonly IMotivoAnulacionAtencionRepositorio _motivoRepo;

        public MotivosAnulacionAtencionController(IMotivoAnulacionAtencionRepositorio motivoRepo)
        {
            _motivoRepo = motivoRepo;
        }

        /// <summary>
        /// Obtiene todos los motivos de anulación de atención activos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var motivos = await _motivoRepo.ObtenerTodosAsync();
                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = motivos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener los motivos de anulación.", ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene un motivo de anulación por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var motivo = await _motivoRepo.ObtenerPorIdAsync(id);
                
                if (motivo == null)
                {
                    return NotFound(new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { $"Motivo de anulación con id {id} no encontrado." }
                    });
                }

                return Ok(new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = motivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener el motivo de anulación.", ex.Message }
                });
            }
        }
    }
}
