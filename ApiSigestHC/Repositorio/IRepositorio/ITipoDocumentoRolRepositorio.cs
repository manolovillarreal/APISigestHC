using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ITipoDocumentoRolRepositorio
    {
        Task<IEnumerable<TipoDocumentoRol>> GetByRolAsync(int rolId);
        Task<IEnumerable<TipoDocumentoRol>> GetPorTipoDocumentoAsync(int tipoDocumentoId);
        Task<TipoDocumentoRol> GetPorIdsAsync(int tipoDocumentoId, int rolId);
        Task CrearAsync(TipoDocumentoRol entidad);
        Task ActualizarAsync(TipoDocumentoRol entidad);
        Task EliminarAsync(TipoDocumentoRol entidad);
       
        Task <bool> PuedeCargarTipoDocumento(int rolId, int tipoDocumentoId);
    }
}
