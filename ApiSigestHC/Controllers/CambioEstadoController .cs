using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Controllers
{
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
        [Authorize] // O puedes restringir por rol si es necesario
        public async Task<IActionResult> ObtenerHistorial(int atencionId)
        {
            var cambios = await _cambioEstadoRepo.ObtenerCambiosPorAtencionAsync(atencionId);

            var cambiosDto = _mapper.Map<List<CambioEstado>>(cambios);

            return Ok(cambiosDto);
        }
    }
}
