using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IDocumentoRequeridoRepositorio
    {
        Task<IEnumerable<DocumentoRequerido>> ObtenerTodosAsync();
        Task<IEnumerable<DocumentoRequerido>> ObtenerPorEstadoAsync(int estadoAtencionId);
        Task AgregarAsync(DocumentoRequerido doc);
        Task EliminarAsync(int estadoAtencionId, int tipoDocumentoId);
        Task<bool> ExisteAsync(int estadoAtencionId, int tipoDocumentoId);
    }
}
