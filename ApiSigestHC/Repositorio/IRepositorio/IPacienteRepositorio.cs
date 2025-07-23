using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IPacienteRepositorio
    {
        Task<Paciente> ObtenerPacientePorIdAsync(string id);
        Task<Paciente> ObtenerUltimoPacienteIngresadoIdAsync();
        Task<List<Paciente>> ObtenerUltimosPacientesIngresadosAsync(int limit);
    }

}
