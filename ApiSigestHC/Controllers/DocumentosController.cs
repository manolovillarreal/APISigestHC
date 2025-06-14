using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using XAct.Library.Settings;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;

namespace ApiSigestHC.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentosController : ControllerBase
    {
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly ISolicitudCorreccionRepositorio _solicitudCorreccionRepo;

        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IValidacionCargaDocumentoService _validacionCargaDocumentoService;




        private readonly IMapper _mapper;
        private IAlmacenamientoArchivoService _almacenamientoArchivoService;

        public DocumentosController(IAtencionRepositorio atencionRepo,
                                    IDocumentoRepositorio documentoRepo, 
                                    ISolicitudCorreccionRepositorio solicitudCorreccionRepo,
                                    IAlmacenamientoArchivoService almacenamientoArchivoService,
                                    IUsuarioContextService usuarioContextService,
                                    IMapper mapper)
        {
            _atencionRepo = atencionRepo;
            _documentoRepo = documentoRepo;
            _solicitudCorreccionRepo = solicitudCorreccionRepo;
            _almacenamientoArchivoService = almacenamientoArchivoService;
            _usuarioContextService = usuarioContextService;
            _mapper = mapper;
        }
        //[HttpGet("{id}")]
        //public async Task<IActionResult> ObtenerDocumentoPorId(int id)
        //{
        //    try
        //    {
        //        int rolId = _usuarioContextService.ObtenerRolId(); // del servicio creado anteriormente

        //        var documento = await _documentoRepo.ObtenerPorIdAsync(id);

        //        var documentoDto = _mapper.Map<DocumentoDto>(documento);

        //        return Ok(documentoDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new RespuestaAPI { IsSuccess = false, ErrorMessages = new List<string> { ex.Message } });
        //    }
        //}

        [HttpGet("por-atencion/{atencionId}")]
        public async Task<IActionResult> ObtenerDocumentosPorAtencion(int atencionId)
        {
            try
            {
                int rolId = _usuarioContextService.ObtenerRolId(); // del servicio creado anteriormente

                var documentos = await _documentoRepo.ObtenerPermitidosParaCargar(atencionId, rolId);

                var documentosDto = _mapper.Map<IEnumerable<DocumentoDto>>(documentos);

                return Ok(documentosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new RespuestaAPI { IsSuccess = false, ErrorMessages = new List<string> { ex.Message } });
            }
        }

        [HttpPost("cargar")]
        public async Task<IActionResult> CargarDocumento([FromForm] DocumentoCargarDto dto)
        {

            var validacion = await _validacionCargaDocumentoService.ValidarCargaDocumentoAsync(dto);

            if (!validacion.IsSuccess)
                return StatusCode((int)validacion.StatusCode, validacion);

            try
            {
                
                var resultado = await _almacenamientoArchivoService.GuardarArchivoAsync(dto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Error al cargar el documento.", ex.Message }
                });
            }
        }


        [HttpPost("reemplazar")]
        public async Task<IActionResult> ReemplazarDocumento([FromForm] DocumentoReemplazarDto dto)
        {
            var validacion = await _validacionCargaDocumentoService.ValidarReemplazoDocumentoAsync(dto);

            if (!validacion.IsSuccess)
                return StatusCode((int)validacion.StatusCode, validacion);

            try
            {
                var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
                await _almacenamientoArchivoService.ReemplazarArchivoAsync(documento, dto.Archivo);
                documento.FechaCarga = DateTime.Now;
                documento.UsuarioId = _usuarioContextService.ObtenerUsuarioId();
                await _documentoRepo.ActualizarDocumentoAsync(documento);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Error al reemplazar el documento.", ex.Message }
                });
            }
        }

        [HttpPost("corregir")]
        public async Task<IActionResult> CorregirDocumento([FromForm] DocumentoReemplazarDto dto)
        {
            // 1. Verificar si hay una corrección pendiente para este documento
            var correccion = await _solicitudCorreccionRepo.ObtenerPendientePorDocumentoIdAsync(dto.Id);
            if (correccion == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Este documento no tiene una solicitud de corrección pendiente." }
                });
            }

            // 2. Validar reemplazo (permite reemplazar aunque el estado sea auditoría, si hay corrección)
            var validacion = await _validacionCargaDocumentoService.ValidarReemplazoDocumentoAsync(dto);
            if (!validacion.IsSuccess)
                return StatusCode((int)validacion.StatusCode, validacion);

            try
            {
                // 3. Obtener documento original
                var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);

                // 4. Reemplazar archivo físico
                await _almacenamientoArchivoService.ReemplazarArchivoAsync(documento, dto.Archivo);

                // 5. Actualizar datos
                documento.FechaCarga = DateTime.Now;
                documento.UsuarioId = _usuarioContextService.ObtenerUsuarioId();
                await _documentoRepo.ActualizarDocumentoAsync(documento);

                // 6. Marcar corrección como aplicada
                correccion.Pendiente = false;
                correccion.FechaCorrige = DateTime.Now;
                correccion.UsuarioCorrigeId = _usuarioContextService.ObtenerUsuarioId();
                await _solicitudCorreccionRepo.ActualizarAsync(correccion);

                return Ok(new RespuestaAPI { IsSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Ocurrió un error al corregir el documento.", ex.Message }
                });
            }
        }


        [HttpGet("descargar/{documentoId}")]
        public async  Task<IActionResult> DescargarDocumento(int documentoId)
        {
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();

                var doc = await _documentoRepo.ObtenerPorIdAsync(documentoId);

                if (doc == null) return NotFound();

                var puededescargarTipo = await _documentoRepo.PuedeVerDocumento(rolId, doc.TipoDocumentoId);

                if (!puededescargarTipo) return Forbid();


                FileStreamResult archivo = await _almacenamientoArchivoService.DescargarDocumentoAsync(doc);
                if (archivo == null) return NotFound();

                return archivo;
            }
            catch (Exception ex)
            {

                return BadRequest(new RespuestaAPI { IsSuccess = false, ErrorMessages = new List<string> { ex.Message } });
            }
            
        }

        [HttpGet("ver/{documentoId}")]
        public async Task<IActionResult> VerDocumento(int documentoId)
        {
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();

                var doc = await _documentoRepo.ObtenerPorIdAsync(documentoId);
                if (doc == null) return NotFound();

                var puedeVerTipo = await _documentoRepo.PuedeVerDocumento(rolId, doc.TipoDocumentoId);
                if (!puedeVerTipo) return Forbid();

                var resultado = await _almacenamientoArchivoService.ObtenerArchivoParaVisualizacionAsync(doc);
                if (resultado == null) return NotFound();

                return resultado;
            }
            catch (Exception ex)
            {
                return BadRequest(new RespuestaAPI
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

       
    }
}
