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
        private readonly IAnulacionAtencionRepositorio _anulacionAtencionRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IUsuarioContextService _usuarioContextService;

        public AtencionesService(IAtencionRepositorio atencionRepo, IMapper mapper, IAnulacionAtencionRepositorio anulacionAtencionRepo, IDocumentoRepositorio documentoRepo, IUsuarioContextService usuarioContextService)
        {
            _atencionRepo = atencionRepo;
            _mapper = mapper;
            _anulacionAtencionRepo = anulacionAtencionRepo;
            _documentoRepo = documentoRepo;
            _usuarioContextService = usuarioContextService;
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

            var atenciones = await _atencionRepo.ObtenerAtencionesPorFiltroAsync(filtro);
            var total = atenciones.Count;

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = new
                {
                    Total = total,
                    Page = filtro.Page,
                    PageSize = filtro.PageSize,
                    Data = atenciones
                }
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
                    ErrorMessages = new List<string> { "La atención no existe." }
                };
            }

            // Validar que el estado sea 1
            if (atencion.EstadoAtencionId != 1)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Solo se pueden anular atenciones en estado 'Admisión'." }
                };
            }


            var motivoExiste = await _anulacionAtencionRepo.MotivoExisteAsync(dto.MotivoAnulacionAtencionId);
            if (!motivoExiste)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Motivo de anulación inválido." }
                };
            }

            var usuarioId = _usuarioContextService.ObtenerUsuarioId();

            var anulacion = new AnulacionAtencion
            {
                AtencionId = dto.AtencionId,
                MotivoAnulacionAtencionId = dto.MotivoAnulacionAtencionId,
                UsuarioId = usuarioId,
                Observacion = dto.Observacion,
                Fecha = DateTime.Now
            };

            atencion.EstaAnulada = true;
            await _atencionRepo.EditarAtencionAsync(atencion);
            await _anulacionAtencionRepo.GuardarAsync(anulacion);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.Created,
                Message = "Atención anulada correctamente"
            };
        }
    }
}
