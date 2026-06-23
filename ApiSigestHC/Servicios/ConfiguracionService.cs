using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IConfiguracionRepositorio _configuracionRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IFileSystemService _fileSystem;

        public ConfiguracionService(
            IConfiguracionRepositorio configuracionRepo,
            IUsuarioContextService usuarioContextService,
            IFileSystemService fileSystem)
        {
            _configuracionRepo = configuracionRepo;
            _usuarioContextService = usuarioContextService;
            _fileSystem = fileSystem;
        }

        public async Task<string?> ObtenerValorAsync(string clave)
        {
            var config = await _configuracionRepo.ObtenerPorClaveAsync(clave);
            return config?.Valor;
        }

        public async Task<string> ObtenerRutaBaseDocumentosAsync()
        {
            var valor = await ObtenerValorAsync("ruta_base_documentos");
            return valor ?? string.Empty;
        }

        public async Task<RespuestaAPI> ObtenerTodasAsync()
        {
            var configuraciones = await _configuracionRepo.ObtenerTodasAsync();
            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = configuraciones
            };
        }

        public async Task<RespuestaAPI> ActualizarAsync(string clave, string valor)
        {
            var config = await _configuracionRepo.ObtenerPorClaveAsync(clave);
            if (config == null)
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { $"No existe configuración con clave '{clave}'." }
                };

            config.Valor = valor;
            config.FechaActualizacion = DateTime.UtcNow;
            config.UsuarioActualizacion = _usuarioContextService.ObtenerUsuarioId();

            await _configuracionRepo.ActualizarAsync(config);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = config
            };
        }

        public async Task<RespuestaAPI> ActualizarRutaBaseDocumentosAsync(string valor)
        {
            if (!_fileSystem.DirectoryExists(valor))
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { $"La ruta '{valor}' no existe en el sistema de archivos." }
                };

            var config = await _configuracionRepo.ObtenerPorClaveAsync("ruta_base_documentos");
            if (config == null)
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "No existe configuración con clave 'ruta_base_documentos'." }
                };

            config.Valor = valor;
            config.FechaActualizacion = DateTime.UtcNow;
            config.UsuarioActualizacion = _usuarioContextService.ObtenerUsuarioId();

            await _configuracionRepo.ActualizarAsync(config);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = config
            };
        }
    }
}
