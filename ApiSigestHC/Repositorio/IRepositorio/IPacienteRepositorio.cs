using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IPacienteRepositorio
    {
        Task<Paciente> ObtenerPacientePorIdAsync(string id);
    }
}
