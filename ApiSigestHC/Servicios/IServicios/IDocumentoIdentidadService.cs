using System.Threading.Tasks;
using ApiSigestHC.Modelos;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IDocumentoIdentidadService
    {
        Task<Documento> ObtenerDocumentoIdentidadAnteriorValidoAsync(int atencionId);
    }
}
