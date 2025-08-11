using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IAtencionRepositorio
    {
        Task<ICollection<Atencion>> ObtenerAtencionesAsync();
        Task<Atencion> ObtenerAtencionPorIdAsync(int id);
        Task<IEnumerable<Atencion>> GetAtencionesPorEstadoAsync(List<int> estados);
        Task<IEnumerable<Atencion>> ObtenerPorFechasAsync(DateTime fechaInicio, DateTime fechaFin, int page, int pageSize);

        Task CrearAtencionAsync(Atencion atencion);
        Task EditarAtencionAsync(Atencion atencion);
        Task EliminarAtencionAsync(int id);

        Task<IEnumerable<UbicacionPacienteDto>> GetUltimaUbicacionPacientesAsync(string[] pacienteIds);
    }
}
