using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using XAct.Library.Settings;

namespace ApiSigestHC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosRequeridosController : ControllerBase
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly IEstadoAtencionRepositorio _estadoAtencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;

        private readonly IMapper _mapper;

        public DocumentosRequeridosController(IDocumentoRequeridoRepositorio documentoRequeridoRepo,
                                              IEstadoAtencionRepositorio estadoAtencionRepositorio,
                                              ITipoDocumentoRepositorio  tipoDocumentoRepositorio,
                                                IMapper mapper)
        {
            _documentoRequeridoRepo = documentoRequeridoRepo;
            _estadoAtencionRepo = estadoAtencionRepositorio;
            _tipoDocumentoRepo = tipoDocumentoRepositorio;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var docs = await _documentoRequeridoRepo.ObtenerTodosAsync();
            return Ok(_mapper.Map<IEnumerable<DocumentoRequeridoDto>>(docs));
        }

        [HttpGet("{estadoAtencionId}")]
        public async Task<IActionResult> ObtenerPorEstado(int estadoAtencionId)
        {
            var docs = await _documentoRequeridoRepo.ObtenerPorEstadoAsync(estadoAtencionId);
            return Ok(_mapper.Map<IEnumerable<DocumentoRequeridoDto>>(docs));
        }


        /// <summary>
        /// Crea un nuevo documento requerido para una transición de estado.
        /// </summary>
        /// <param name="dto">DTO con los datos del estado y tipo de documento requerido.</param>
        /// <returns>Código 201 si se crea exitosamente, 400 si hay errores de validación, 500 si ocurre un error inesperado en el servidor.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DocumentoRequeridoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] DocumentoRequeridoDto dto)
        {
            try
            {
                // Validar existencia del EstadoAtencion
                var estado = await _estadoAtencionRepo.ObtenerPorIdAsync(dto.EstadoAtencionId);
                if (estado == null)
                    return BadRequest("El estado de atención no existe.");

                // Validar que el orden del estado sea > 1
                if (estado.Orden <= 1)
                    return BadRequest("No se pueden registrar documentos requeridos para el estado inicial.");

                // Validar existencia del TipoDocumento
                var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);
                if (tipoDocumento == null)
                    return BadRequest("El tipo de documento no existe.");

                // Verificar si ya existe ese registro
                if (await _documentoRequeridoRepo.ExisteAsync(dto.EstadoAtencionId, dto.TipoDocumentoId))
                    return BadRequest("Este documento ya está registrado como requerido.");

                // Crear y guardar
                var nuevo = _mapper.Map<DocumentoRequerido>(dto);
                await _documentoRequeridoRepo.AgregarAsync(nuevo);

                var resultDto = _mapper.Map<DocumentoRequeridoDto>(nuevo);

                return CreatedAtAction(nameof(ObtenerPorEstado), new { estadoAtencionId = resultDto.EstadoAtencionId }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Ocurrió un error inesperado al registrar el documento requerido.", ex.Message }
                });
            }
            
        }

    }

}
