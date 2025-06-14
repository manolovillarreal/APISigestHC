using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IValidacionDocumentosObligatoriosService
    {
        Task<ResultadoValidacionDto> ValidarDocumentosObligatoriosAsync(Atencion atencion);
    }
}
