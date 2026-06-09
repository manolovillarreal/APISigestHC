using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface ISolicitudCorreccionRepositorio
    {
        Task AgregarAsync(SolicitudCorreccion entidad);
        Task<IEnumerable<SolicitudCorreccion>> ObtenerPorDocumentoAsync(int documentoId);
        Task<SolicitudCorreccion?> ObtenerPendientePorDocumentoIdAsync(int documentoId);

        Task ActualizarAsync(SolicitudCorreccion solicitud);

        Task<SolicitudCorreccion> ObtenerPorIdAsync(int id);
        Task<IEnumerable<SolicitudCorreccion>> ObtenerSolicitudesPorRolAsync(int rolId, FiltroCorreccionesDto filtro);
        Task<IEnumerable<SolicitudCorreccion>> ObtenerSolicitudesEnviadasPorRolAsync(int rolId);
    }

}
