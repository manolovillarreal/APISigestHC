using ApiSigestHC.Modelos;

namespace ApiSigestHC.Repositorio.IRepositorio
{
    public interface IAnulacionAtencionRepositorio
    {
        Task GuardarAsync(AnulacionAtencion anulacion);
        Task<bool> AtencionExisteAsync(int atencionId);
        Task<bool> MotivoExisteAsync(int motivoId);
    }
}
