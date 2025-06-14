using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ITipoDocumentoRepositorio
    {
        Task CrearTipoDocumentoAsync(TipoDocumento tipo);
        Task ActualizarTipoDocumentoAsync(TipoDocumento tipo);

        Task<IEnumerable<TipoDocumento>> GetTiposDocumentoAsync();
        Task<TipoDocumento> GetTipoDocumentoPorIdAsync(int id);
        Task<IEnumerable<TipoDocumento>> ObtenerPorIdsAsync(IEnumerable<int> ids);

        Task<bool> ExisteTipoDocumentoPorCodigoAsync(string codigo);

    }
}
