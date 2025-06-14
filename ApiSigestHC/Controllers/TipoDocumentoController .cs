namespace ApiSigestHC.Controllers
{
    using ApiSigestHC.Modelos;
    using ApiSigestHC.Repositorio.IRepositorio;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ApiSigestHC.Modelos; // Reemplaza con el namespace real
    using ApiSigestHC.Repositorio; // Reemplaza con el namespace real
    using ApiSigestHC.Modelos.Dtos;
    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using ApiSigestHC.Servicios.IServicios;
    using System.Net;

    [ApiController]
    [Route("api/[controller]")]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly ITipoDocumentoRepositorio _repository;
        private readonly ITipoDocumentoRolRepositorio _tipoDocumentoRolRepositorio;
        private readonly IUsuarioContextService _usuarioContextService;

        private readonly IMapper _mapper;

        public TipoDocumentoController(ITipoDocumentoRepositorio repository, IMapper mapper, ITipoDocumentoRolRepositorio tipoDocumentoRolRepositorio, IUsuarioContextService usuarioContextService)
        {
            _repository = repository;
            _mapper = mapper;
            _tipoDocumentoRolRepositorio = tipoDocumentoRolRepositorio;
            _usuarioContextService = usuarioContextService;
        }

        [Authorize]
        [HttpGet("Autorizados")]
        public async Task<IActionResult> ObtenerTiposDocumentoQuePuedeCargar()
        {
            var respuesta = new RespuestaAPI();
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();

                var relaciones = await _tipoDocumentoRolRepositorio.ObtenerPorRolAsync(rolId);

                var tiposDocumento = relaciones.Select(r => r.TipoDocumento).Distinct();

                var dtos = _mapper.Map<IEnumerable<TipoDocumentoDto>>(tiposDocumento);

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                respuesta.IsSuccess = false;
                respuesta.StatusCode = HttpStatusCode.InternalServerError;
                respuesta.ErrorMessages.Add("Error");
                respuesta.ErrorMessages.Add(ex.Message);
                return StatusCode((int)respuesta.StatusCode, respuesta);
            }
           
        }


        // GET: api/TipoDocumento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoDocumento>>> GetTiposDocumento()
        {
            var tipos = await _repository.GetTiposDocumentoAsync();
            return Ok(tipos);
        }

        // GET: api/TipoDocumento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoDocumento>> GetTipoDocumento(int id)
        {
            var tipo = await _repository.GetTipoDocumentoPorIdAsync(id);
            if (tipo == null)
                return NotFound();

            return Ok(tipo);
        }

        // POST: api/TipoDocumento
        [HttpPost]
        public async Task<ActionResult<TipoDocumento>> CrearTipoDocumento(TipoDocumentoCrearDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var yaExiste = await _repository.ExisteTipoDocumentoPorCodigoAsync(dto.Codigo);

            if (yaExiste)
                return Conflict(new { mensaje = "Ya existe un tipo de documento con ese código." });

            var entidad = _mapper.Map<TipoDocumento>(dto);
            await _repository.CrearTipoDocumentoAsync(entidad);

            return CreatedAtAction(nameof(GetTipoDocumento), new { id = entidad.Id }, entidad);
        }


        // PUT: api/TipoDocumento/5
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarTipoDocumento(int id, TipoDocumentoCrearDto dto)
        {
            var existente = await _repository.GetTipoDocumentoPorIdAsync(id);
            if (existente == null)
                return NotFound();

            existente.Codigo = dto.Codigo;
            existente.Nombre = dto.Nombre;
            existente.Descripcion = dto.Descripcion;

            await _repository.ActualizarTipoDocumentoAsync(existente);
            return NoContent();
        }

    }

}
