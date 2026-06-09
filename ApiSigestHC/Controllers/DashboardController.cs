using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> ObtenerDashboard()
        {
            var resultado = await _service.ObtenerDashboardAsync();
            return StatusCode((int)resultado.StatusCode, resultado);
        }
    }
}
