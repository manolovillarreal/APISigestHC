using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IDocumentoRequeridoRepositorio
    {
        Task<IEnumerable<DocumentoRequerido>> ObtenerTodosAsync();
        Task<IEnumerable<DocumentoRequerido>> ObtenerPorEstadoAsync(int estadoAtencionId);
        Task<DocumentoRequerido> ObtenerPorTipoAsync(int tipoDocumentoId);
        Task AgregarAsync(DocumentoRequerido doc);
        Task Actualizar(DocumentoRequerido doc);
        Task EliminarAsync(int tipoDocumentoId);
        Task<bool> ExisteAsync(int estadoAtencionId, int tipoDocumentoId);
        Task<bool> ExistePorTipoAsync(int tipoDocumentoId);
    }
}
