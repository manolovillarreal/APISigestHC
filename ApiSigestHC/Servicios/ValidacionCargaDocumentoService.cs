using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;
using ApiSigestHC.Repositorio;

namespace ApiSigestHC.Servicios
{
    public class ValidacionCargaDocumentoService : IValidacionCargaDocumentoService
    {
        private readonly IAtencionRepositorio _atencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly IDocumentoRepositorio _documentoRepo;
        private readonly IUsuarioContextService _usuarioContextService;

        public ValidacionCargaDocumentoService(
            IAtencionRepositorio atencionRepo,
            ITipoDocumentoRepositorio tipoDocumentoRepo,
            IDocumentoRepositorio documentoRepo,
            IUsuarioContextService usuarioContextService)
        {
            _atencionRepo = atencionRepo;
            _tipoDocumentoRepo = tipoDocumentoRepo;
            _documentoRepo = documentoRepo;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<RespuestaAPI> ValidarCargaDocumentoAsync(DocumentoCargarDto dto)
        {
            var respuesta = new RespuestaAPI();

            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            var tipoDoc = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);

            if (tipoDoc == null)
                return Error("Tipo de documento inválido.", HttpStatusCode.BadRequest);

            if (atencion == null)
                return Error("Atencion no encontrada.", HttpStatusCode.NotFound);

            var validacionArchivo = await ValidarArchivoAsync(dto.Archivo, atencion, tipoDoc);
            if (!validacionArchivo.IsSuccess)
                return validacionArchivo;

            if (!tipoDoc.PermiteMultiples)
            {
                var existe = await _documentoRepo.ExisteDocumentoAsync(dto.AtencionId, dto.TipoDocumentoId);
                if (existe)
                    return Error("Ya existe un documento de este tipo para esta atención.", HttpStatusCode.BadRequest);
            }

            if (tipoDoc.RequiereNumeroRelacion && string.IsNullOrWhiteSpace(dto.NumeroRelacion))
                return Error("Este tipo de documento requiere un número de relación.", HttpStatusCode.BadRequest);

            return new RespuestaAPI { IsSuccess = true };
        }

        public async Task<RespuestaAPI> ValidarReemplazoDocumentoAsync(DocumentoReemplazarDto dto)
        {
            var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
            if (documento == null)
                return Error("Documento no encontrado.", HttpStatusCode.NotFound);

            var atencion = documento.Atencion;
            var tipoDoc = documento.TipoDocumento;

            return await ValidarArchivoAsync(dto.Archivo, atencion, tipoDoc);
        }

        private async Task<RespuestaAPI> ValidarArchivoAsync(IFormFile archivo, Atencion atencion, TipoDocumento tipoDoc)
        {
            var rolNombre = _usuarioContextService.ObtenerRolNombre();
            var rolId = _usuarioContextService.ObtenerRolId();

            if (archivo == null || archivo.Length == 0)
                return Error("Archivo no proporcionado o vacío.", HttpStatusCode.BadRequest);

            var puedeCargar = await _documentoRepo.PuedeCargarDocumento(rolId, tipoDoc.Id);
            if (!puedeCargar)
                return Error("No tiene permiso para cargar este tipo de documento.", HttpStatusCode.Forbidden);

            if (!PuedeCargarSegunEstado(rolNombre, atencion.EstadoAtencionId))
                return Error("No puede cargar documentos en el estado actual de la atención.", HttpStatusCode.Forbidden);

            var ext = Path.GetExtension(archivo.FileName).TrimStart('.').ToLowerInvariant();
            var permitidas = tipoDoc.ExtensionPermitida.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!permitidas.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                return Error($"Solo se permiten archivos con extensión: {tipoDoc.ExtensionPermitida}.", HttpStatusCode.UnsupportedMediaType);

            if (archivo.Length > 10 * 1024 * 1024)
                return Error("El archivo excede el tamaño máximo permitido (10 MB).", HttpStatusCode.BadRequest);

            return new RespuestaAPI { IsSuccess = true };
        }

        private RespuestaAPI Error(string mensaje, HttpStatusCode code)
        {
            return new RespuestaAPI
            {
                IsSuccess = false,
                StatusCode = code,
                ErrorMessages = new List<string> { mensaje },
            };
        }

        private bool PuedeCargarSegunEstado(string rolNombre, int estadoAtencion)
        {
            return rolNombre switch
            {
                "Admisiones" => estadoAtencion >= 1 && estadoAtencion <= 4,
                "Medico" => estadoAtencion >= 2 && estadoAtencion <= 3,
                "Enfermeria" => estadoAtencion == 3,
                "Laboratorio" => estadoAtencion == 3,
                "Auditoria" => estadoAtencion >= 1 && estadoAtencion <= 5,
                "facturacion" => estadoAtencion == 6 || estadoAtencion == 7,
                _ => false
            };
        }
    }

}
