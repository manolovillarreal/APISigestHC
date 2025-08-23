using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IAtencionRepositorio
    {
        Task<Atencion> ObtenerAtencionPorIdAsync(int id);
        Task<IEnumerable<Atencion>> GetAtencionesPorEstadoAsync(List<int> estados);
        
        Task CrearAtencionAsync(Atencion atencion);
        Task EditarAtencionAsync(Atencion atencion);
        Task EliminarAtencionAsync(int id);

        Task<IEnumerable<UbicacionPacienteDto>> GetUltimaUbicacionPacientesAsync(string[] pacienteIds);
        Task<ICollection<Atencion>> ObtenerAtencionesPorFiltroAsync(AtencionFiltroDto filtro);
    }
}