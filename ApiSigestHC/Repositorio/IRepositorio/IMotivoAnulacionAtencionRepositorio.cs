using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IMotivoAnulacionAtencionRepositorio
    {
        Task<List<MotivoAnulacionAtencion>> ObtenerTodosAsync();
        Task<MotivoAnulacionAtencion?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
    }

}
