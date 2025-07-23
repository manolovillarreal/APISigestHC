using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class AnulacionAtencionService : IAnulacionAtencionService
    {
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IAnulacionAtencionRepositorio _anulacionRepo;
        private readonly IUsuarioContextService _usuarioContextService;

        public AnulacionAtencionService(
        IAnulacionAtencionRepositorio anulacionRepo,
        IAtencionRepositorio atencionRepo,
        IUsuarioContextService usuarioContextService)
        {
            _anulacionRepo = anulacionRepo;
            _atencionRepo = atencionRepo;
            _usuarioContextService = usuarioContextService;
        }
        public async Task<RespuestaAPI> AnularAtencionAsync(AnulacionAtencionCrearDto dto)
        {
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.atencion_id);
            if (atencion == null)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "La atención no existe." }
                };
            }

            var motivoExiste = await _anulacionRepo.MotivoExisteAsync(dto.motivoAnulacionAtencion_id);
            if (!motivoExiste)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Motivo de anulación inválido." }
                };
            }

            var rol = _usuarioContextService.ObtenerRolNombre();
            if (atencion.EstadoAtencionId != 1 || !rol.Equals("admisiones", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    ErrorMessages = new List<string> { "Solo se pueden anular atenciones en estado 'admisión' y con el rol 'admisiones'." }
                };
            }

            var usuarioId = _usuarioContextService.ObtenerUsuarioId();

            var anulacion = new AnulacionAtencion
            {
                AtencionId = dto.atencion_id,
                MotivoAnulacionAtencionId = dto.motivoAnulacionAtencion_id,
                UsuarioId = usuarioId,
                Observacion = dto.observacion,
                Fecha = DateTime.Now
            };

            await _anulacionRepo.GuardarAsync(anulacion);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.Created
            };
        }
    }
}
