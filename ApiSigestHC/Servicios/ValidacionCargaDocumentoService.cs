using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;
using ApiSigestHC.Repositorio;
using PdfSharp.Pdf.IO;

namespace ApiSigestHC.Servicios
{
    public class ValidacionCargaDocumentoService : IValidacionCargaArchivoService
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
            var rolId = _usuarioContextService.ObtenerRolId();
            var atencion = await _atencionRepo.ObtenerAtencionPorIdAsync(dto.AtencionId);
            var tipoDoc = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);

            if (tipoDoc == null)
                return Error("Tipo de documento inválido.", HttpStatusCode.BadRequest);

            if (atencion == null)
                return Error("Atencion no encontrada.", HttpStatusCode.NotFound);

            if (!PuedeCargarSegunEstado(atencion.EstadoAtencionId))
                return Error("No puede cargar documentos en el estado actual de la atención.", HttpStatusCode.Forbidden);
            
            var puedeCargar = await _documentoRepo.PuedeCargarDocumento(rolId, tipoDoc.Id);
            if (!puedeCargar)
                return Error("No tiene permiso para cargar este tipo de documento.", HttpStatusCode.Forbidden);

            var validacionArchivo = await ValidarArchivoAsync(dto.Archivo, tipoDoc);
            if (!validacionArchivo.Ok)
                return validacionArchivo;

            if (!tipoDoc.PermiteMultiples)
            {
                var existe = await _documentoRepo.ExisteDocumentoAsync(dto.AtencionId, dto.TipoDocumentoId);
                if (existe)
                    return Error("Ya existe un documento de este tipo para esta atención.", HttpStatusCode.BadRequest);
            }

            if (tipoDoc.RequiereNumeroRelacion && string.IsNullOrWhiteSpace(dto.NumeroRelacion))
                return Error("Este tipo de documento requiere un número de relación.", HttpStatusCode.BadRequest);

            return new RespuestaAPI { Ok = true };
        }

        public async Task<RespuestaAPI> ValidarReemplazoDocumentoAsync(DocumentoReemplazarDto dto)
        {
            var documento = await _documentoRepo.ObtenerPorIdAsync(dto.Id);
            if (documento == null)
                return Error("Documento no encontrado.", HttpStatusCode.NotFound);

            var tipoDoc = documento.TipoDocumento;

            return await ValidarArchivoAsync(dto.Archivo, tipoDoc);
        }
        public async Task<RespuestaAPI> ValidarCargaArchivoCorreccionAsync(IFormFile archivo, TipoDocumento tipoDoc)
        {        
            var rolId = _usuarioContextService.ObtenerRolId();

            var puedeCargar = await _documentoRepo.PuedeCargarDocumento(rolId, tipoDoc.Id);
            if (!puedeCargar)
                return Error("No tiene permiso para cargar este tipo de documento.", HttpStatusCode.Forbidden);

            return await ValidarArchivoAsync(archivo, tipoDoc);
        }

        public async Task<RespuestaAPI> ValidarArchivoAsync(IFormFile archivo, TipoDocumento tipoDoc)
        {          

            if (archivo == null || archivo.Length == 0)
                return Error("Archivo no proporcionado o vacío.", HttpStatusCode.BadRequest);     

            var ext = Path.GetExtension(archivo.FileName).TrimStart('.').ToLowerInvariant();
            var permitidas = tipoDoc.ExtensionPermitida.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!permitidas.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                return Error($"Solo se permiten archivos con extensión: {tipoDoc.ExtensionPermitida}.", HttpStatusCode.UnsupportedMediaType);

         
            if (ext == "pdf")
            {
                int numeroPaginas;

                try
                {
                    using var stream = archivo.OpenReadStream();
                    var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    numeroPaginas = document.PageCount;
                }
                catch
                {
                    return Error("No se pudo leer el archivo PDF. Verifique que el archivo no esté dañado.", HttpStatusCode.BadRequest);
                }

                // Validar límite de hojas si está definido
                if (tipoDoc.LimiteDePaginas > 0 && numeroPaginas > tipoDoc.LimiteDePaginas)
                {
                    return Error($"El documento tiene {numeroPaginas} páginas, excede el límite permitido de {tipoDoc.LimiteDePaginas} para {tipoDoc.Nombre}.", HttpStatusCode.BadRequest);
                }

                // Validar tamaño máximo por página
                var maxTamanioPermitido = tipoDoc.PesoPorPagina * 1024 * numeroPaginas;
                if (archivo.Length > maxTamanioPermitido)
                {
                    return Error($"El archivo excede el límite de tamaño permitido: {tipoDoc.PesoPorPagina}Kb  por pagina.", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                // Validación clásica para otros formatos
                if (archivo.Length > 2 * 1024 * 1024)
                    return Error("El archivo excede el tamaño máximo permitido (2 MB).", HttpStatusCode.BadRequest);
            }



            return new RespuestaAPI { Ok = true };
        }

  
        private RespuestaAPI Error(string mensaje, HttpStatusCode code)
        {
            return new RespuestaAPI
            {
                Ok = false,
                StatusCode = code,
                ErrorMessages = new List<string> { mensaje },
            };
        }

        private bool PuedeCargarSegunEstado(int estadoAtencion)
        {
            var rolNombre = _usuarioContextService.ObtenerRolNombre();

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
