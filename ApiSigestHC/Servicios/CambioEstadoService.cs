using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class CambioEstadoService : ICambioEstadoService
    {
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly IEstadoAtencionRepositorio _estadoRepo;
        private readonly ICambioEstadoRepositorio _cambioEstadoRepo;
        private readonly IValidacionDocumentosObligatoriosService _validacionDocService;
        private readonly IUsuarioContextService _usuarioContext;

        public CambioEstadoService(
             IAtencionRepositorio atencionRepo,
             IEstadoAtencionRepositorio estadoRepo,
             ICambioEstadoRepositorio cambioEstadoRepo,
             IValidacionDocumentosObligatoriosService validacionDocService,
             IUsuarioContextService usuarioContext)
        {
            _atencionRepo = atencionRepo;
            _estadoRepo = estadoRepo;
            _cambioEstadoRepo = cambioEstadoRepo;
            _validacionDocService = validacionDocService;
            _usuarioContext = usuarioContext;
        }

        public async Task<RespuestaAPI> CambiarEstadoAsync(AtencionCambioEstadoDto dto)
        {
            var respuesta = new RespuestaAPI();
            var usuarioId = _usuarioContext.ObtenerUsuarioId();
            var rolNombre = _usuarioContext.ObtenerRolNombre();

            //Validamos que la atencion exista
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            if (atencion == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.NotFound;
                respuesta.ErrorMessages.Add("La atención no existe");
                return respuesta;
            }

            //validamos que el rol pueda avanzar al siguiente estado
            var nuevoEstado = ObtenerNuevoEstado(atencion.EstadoAtencionId, rolNombre);
            if (nuevoEstado == null)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.Forbidden;
                respuesta.ErrorMessages.Add($"El rol '{rolNombre}' no puede cambiar desde el estado {atencion.EstadoAtencionId}");
                return respuesta;
            }

            //Validamos si existen documento requerido faltantes para avanzzar
            var validacion = await _validacionDocService.ValidarDocumentosObligatoriosAsync(atencion);
            if (!validacion.EsValido)
            {
                respuesta.Ok = false;
                respuesta.StatusCode = HttpStatusCode.BadRequest;
                respuesta.ErrorMessages.Add("Faltan documentos requeridos para el cambio:");
                respuesta.ErrorMessages.AddRange(validacion.DocumentosFaltantes);
                return respuesta;
            }

           
            await _cambioEstadoRepo.RegistrarCambioAsync(new CambioEstado
            {
                AtencionId = dto.AtencionId,
                EstadoInicial = atencion.EstadoAtencionId,
                EstadoNuevo = nuevoEstado.Value,
                Fecha = DateTime.UtcNow,
                UsuarioId = usuarioId,
                Observacion = dto.Obervacion,
            });
            atencion.EstadoAtencionId = nuevoEstado.Value;
            atencion.EstadoAtencion = await _estadoRepo.ObtenerPorIdAsync(nuevoEstado.Value);
            await _atencionRepo.EditarAtencionAsync(atencion);

            respuesta.Ok = true;
            respuesta.StatusCode = HttpStatusCode.OK;
            respuesta.Message =  $"Atención actualizada al estado {nuevoEstado.Value}";
            respuesta.Result = atencion;
            return respuesta;
        }




        private int? ObtenerNuevoEstado(int estadoActual, string rol)
        {
            var transiciones = new Dictionary<(int, string), int>
                {
                    // (estadoActual, rol) => estadoNuevo
                    {(1, "Admisiones"), 2}, //Admisiones pasa a Consulta
                    {(2, "Medico"), 3},     //Medico pasa a Ingreso
                    {(3, "Enfermeria"), 4}, //Enfermeria pasas a Salida
                    {(4, "Admisiones"), 5}, //Admisiones pasa a Auditoria
                    {(5, "Auditoria"), 6},  //Auditoria pasa a Facturacion
                    {(6, "Facturacion"), 7},//Facturacion pasa a Radicacion
                    {(7, "Facturacion"), 8},//Facturacion pasas a Archivado
                    // puedes agregar más reglas aquí
                };
            /*Estados de Atencion
                1	Admision
                2	Consulta
                3	Ingreso
                4	Salida
                5	Auditoria
                6	Facturacion
                7	Radicacion
                8	Archivado
            */
            if (transiciones.TryGetValue((estadoActual, rol), out var nuevoEstado))
                return nuevoEstado;

            return null;
        }
    }

}
