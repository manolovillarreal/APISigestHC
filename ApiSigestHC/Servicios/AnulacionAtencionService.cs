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
        private readonly IMotivoAnulacionAtencionRepositorio _motivoAnulacionRepo;
        private readonly IUsuarioContextService _usuarioContextService;

        public AnulacionAtencionService(
        IMotivoAnulacionAtencionRepositorio motivoAnulacionRepo,
        IAtencionRepositorio atencionRepo,
        IUsuarioContextService usuarioContextService)
        {
            _motivoAnulacionRepo = motivoAnulacionRepo;
            _atencionRepo = atencionRepo;
            _usuarioContextService = usuarioContextService;
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

            // Validar que no esté ya anulada
            if (atencion.FechaAnulacion.HasValue)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "La atención ya ha sido anulada." }
                };
            }

            var motivoExiste = await _motivoAnulacionRepo.ExisteAsync(dto.MotivoAnulacionAtencionId);
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

            // Actualizar campos de anulación directamente en la atención
            atencion.MotivoAnulacionAtencionId = dto.MotivoAnulacionAtencionId;
            atencion.FechaAnulacion = DateTime.Now;
            atencion.UsuarioAnulaId = usuarioId;
            atencion.ObservacionAnulacion = dto.Observacion;

            await _atencionRepo.EditarAtencionAsync(atencion);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Atención anulada correctamente"
            };
        }
    }
}
