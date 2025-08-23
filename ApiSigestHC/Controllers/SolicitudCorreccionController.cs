using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudCorreccionController : ControllerBase
    {
        private readonly ISolicitudCorreccionService _service;

        public SolicitudCorreccionController(ISolicitudCorreccionService service)
        {
            _service = service;
        }

        // Crear solicitud de corrección
        [HttpPost]
        public async Task<IActionResult> CrearSolicitudCorreccion([FromBody] SolicitudCorreccionCrearDto solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var respuesta = await _service.CrearAsync(solicitud);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // Obtener solicitudes por documento
        [HttpGet("documento")]
        public async Task<IActionResult> ObtenerPorDocumento(int documentoId)
        {
            var respuesta = await _service.ObtenerPorDocumentoAsync(documentoId);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        // Obtener solicitudes de corrección según el rol del usuario autenticado
        [HttpGet("por-rol")]
        public async Task<IActionResult> ObtenerPorRolUsuario()
        {
            var respuesta = await _service.ObtenerPorRolUsuarioAsync();
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [HttpPost("{solicitudId}/responder")]
        public async Task<IActionResult> ResponderSolicitud(int solicitudId, [FromForm] SolicitudCorreccionRespuestaDto dto)
        {
            var respuesta = await _service.ResponderSolicitudAsync(solicitudId, dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [HttpPost("{solicitudId}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int solicitudId, [FromBody] SolicitudCorreccionAprobarDto dto)
        {
            var respuesta = await _service.AprobarSolicitudAsync(solicitudId,dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

        [HttpGet("{solicitudId}/visualizar")]
        public async Task<IActionResult> VerDocumentoCorreccion(int solicitudId)
        {
            return  await _service.VerDocumentoCorreccion(solicitudId);
        }

    }
}


