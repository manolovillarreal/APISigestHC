using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IConfiguracionService
    {
        Task<string?> ObtenerValorAsync(string clave);
        Task<RespuestaAPI> ObtenerTodasAsync();
        Task<RespuestaAPI> ActualizarAsync(string clave, string valor);
        Task<string> ObtenerRutaBaseDocumentosAsync();
        Task<RespuestaAPI> ActualizarRutaBaseDocumentosAsync(string valor);
    }
}
