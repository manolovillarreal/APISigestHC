using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using System.Net;
using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Servicios
{
    public class AtencionesService : IAtencionesService
    {
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IMapper _mapper;
        private readonly IMotivoAnulacionAtencionRepositorio _motivoAnulacionRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IVisualizacionEstadoService _visualizacionEstadoService;

        public AtencionesService(IAtencionRepositorio atencionRepo, 
                                    IMapper mapper, 
                                    IMotivoAnulacionAtencionRepositorio motivoAnulacionRepo, 
                                    IDocumentoRepositorio documentoRepo, 
                                    IUsuarioContextService usuarioContextService,
                                    IVisualizacionEstadoService visualizacionEstadoService)
        {
            _atencionRepo = atencionRepo;
            _mapper = mapper;
            _motivoAnulacionRepo = motivoAnulacionRepo;
            _documentoRepo = documentoRepo;
            _usuarioContextService = usuarioContextService;
            _visualizacionEstadoService = visualizacionEstadoService;
        }

        public async Task<RespuestaAPI> ObteneAtencionesPorFiltroAsync(AtencionFiltroDto filtro)
        {
            if (filtro.Page <= 0) filtro.Page = 1;
            if (filtro.PageSize <= 0) filtro.PageSize = 50;

            if (filtro.FechaInicio.HasValue && filtro.FechaFin.HasValue && filtro.FechaInicio > filtro.FechaFin)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "La fecha inicial no puede ser mayor que la final" }
                };
            }

            filtro.EstadosPermitidos = _visualizacionEstadoService.ObtenerEstadosPermitidosPorRol();

            // El repositorio devuelve el conjunto completo (ya filtrado por visibilidad del rol);
            // aquí se recorta la página y se calcula el total real.
            var atenciones = await _atencionRepo.ObtenerAtencionesPorFiltroAsync(filtro);
            var pagina = Helpers.Paginacion.Paginar(atenciones, filtro.Page, filtro.PageSize);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = pagina
            };
        }

        public async Task<RespuestaAPI> AnularAtencionAsync(AnulacionAtencionCrearDto dto)
        {
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            if (atencion == null)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "La atenci�n no existe." }
                };
            }

            // Validar que el estado sea 1
            if (atencion.EstadoAtencionId != 1)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Solo se pueden anular atenciones en estado 'Admisi�n'." }
                };
            }

            // Validar que no est� ya anulada
            if (atencion.FechaAnulacion.HasValue)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "La atenci�n ya ha sido anulada." }
                };
            }

            var motivoExiste = await _motivoAnulacionRepo.ExisteAsync(dto.MotivoAnulacionAtencionId);
            if (!motivoExiste)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Motivo de anulaci�n inv�lido." }
                };
            }

            var usuarioId = _usuarioContextService.ObtenerUsuarioId();

            // Actualizar campos de anulaci�n directamente en la atenci�n
            atencion.MotivoAnulacionAtencionId = dto.MotivoAnulacionAtencionId;
            atencion.FechaAnulacion = DateTime.Now;
            atencion.UsuarioAnulaId = usuarioId;
            atencion.ObservacionAnulacion = dto.Observacion;

            await _atencionRepo.EditarAtencionAsync(atencion);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Atenci�n anulada correctamente"
            };
        }
    }
}
