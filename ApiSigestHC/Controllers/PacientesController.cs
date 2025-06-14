using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Controllers
{

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
        public async Task<IActionResult> GetPaciente(string pacienteId)
        {
            var atenciones = await _repositorio.ObtenerPacientePorIdAsync(pacienteId);
            return Ok(atenciones);
        }

    }
}
