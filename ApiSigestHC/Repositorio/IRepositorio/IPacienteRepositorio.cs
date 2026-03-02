using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IPacienteRepositorio
    {
        Task<Paciente> ObtenerPacientePorIdAsync(string id);
        Task<IngresoUrgencias> ObtenerUltimoPacienteIngresadoIdAsync();
        Task<List<IngresoUrgencias>> ObtenerUltimosPacientesIngresadosAsync(int limit);
    }

}
