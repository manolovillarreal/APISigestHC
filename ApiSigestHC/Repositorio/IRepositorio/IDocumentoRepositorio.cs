using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IDocumentoRepositorio
    {
        Task<IEnumerable<Documento>> ObtenerPorAtencionIdAsync(int atencionId);
        Task<IEnumerable<Documento>> ObtenerPermitidosParaCargar(int atencionId, int rolId);
        Task<Documento> ObtenerPorIdAsync(int id);
        Task GuardarAsync(Documento documento);
        Task ActualizarDocumentoAsync(Documento documento);
        Task EliminarDocumentoAsync(int id);
        Task<bool> PuedeCargarDocumento(int rol, int tipoDocumentoId);
        Task<bool> PuedeVerDocumento(int rol, int tipoDocumentoId);
        Task<bool> ExisteDocumentoAsync(int atencionId, int tipoDocumentoId);
        Task<int> ContarPorTipoYAtencionAsync(int atencionId, int tipoDocumentoId);
    }
}
