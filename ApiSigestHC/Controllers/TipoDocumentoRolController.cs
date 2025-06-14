using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ApiSigestHC.Repositorio.IRepositorio;

namespace ApiSigestHC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoDocumentoRolController : ControllerBase
    {
        private readonly ITipoDocumentoRolRepositorio _repo;
        private readonly IMapper _mapper;

        public TipoDocumentoRolController(ITipoDocumentoRolRepositorio repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }        

        // GET: api/TipoDocumentoRol?tipoDocumentoId=1
        [HttpGet]
        public async Task<IActionResult> GetPorTipoDocumento(int tipoDocumentoId)
        {
            var relaciones = await _repo.GetPorTipoDocumentoAsync(tipoDocumentoId);
            var dto = _mapper.Map<IEnumerable<TipoDocumentoRolDto>>(relaciones);
            return Ok(dto);
        }

        // POST: api/TipoDocumentoRol
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] TipoDocumentoRolDto dto)
        {
            var entidad = _mapper.Map<TipoDocumentoRol>(dto);
            await _repo.CrearAsync(entidad);
            return Ok();
        }

        // PUT: api/TipoDocumentoRol
        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] TipoDocumentoRolDto dto)
        {
            var entidad = await _repo.GetPorIdsAsync(dto.TipoDocumentoId, dto.RolId);
            if (entidad == null) return NotFound();

            _mapper.Map(dto, entidad); // reverse map
            await _repo.ActualizarAsync(entidad);

            return NoContent();
        }

        // DELETE: api/TipoDocumentoRol?tipoDocumentoId=1&rolId=2
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int tipoDocumentoId, int rolId)
        {
            var entidad = await _repo.GetPorIdsAsync(tipoDocumentoId, rolId);
            if (entidad == null) return NotFound();

            await _repo.EliminarAsync(entidad);
            return NoContent();
        }
    }

}
