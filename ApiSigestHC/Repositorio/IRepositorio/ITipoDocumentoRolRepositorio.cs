using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ITipoDocumentoRolRepositorio
    {
        Task<IEnumerable<TipoDocumentoRol>> GetPorTipoDocumentoAsync(int tipoDocumentoId);
        Task<TipoDocumentoRol> GetPorIdsAsync(int tipoDocumentoId, int rolId);
        Task CrearAsync(TipoDocumentoRol entidad);
        Task ActualizarAsync(TipoDocumentoRol entidad);
        Task EliminarAsync(TipoDocumentoRol entidad);
        Task<IEnumerable<TipoDocumentoRol>> ObtenerPorRolAsync(int rolId);
        Task <bool> PuedeCargarTipoDocumento(int rolId, int tipoDocumentoId);
    }
}
