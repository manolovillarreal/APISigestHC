using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ITipoDocumentoRepositorio
    {
        Task CrearTipoDocumentoAsync(TipoDocumento tipo);
        Task ActualizarAsync(TipoDocumento tipo);
        Task EliminarAsync(TipoDocumento tipo);
        Task<IEnumerable<TipoDocumento>> GetTiposDocumentoAsync();
        Task<TipoDocumento> GetTipoDocumentoPorIdAsync(int id);
        Task<IEnumerable<TipoDocumento>> ObtenerPorIdsAsync(IEnumerable<int> ids);

        Task<bool> ExisteTipoDocumentoPorCodigoAsync(string codigo);

    }
}
