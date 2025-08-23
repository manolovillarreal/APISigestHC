using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using System.Threading.Tasks;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IAtencionesService
    {
        Task<RespuestaAPI> ObteneAtencionesPorFiltroAsync(AtencionFiltroDto filtro);
        Task<RespuestaAPI> AnularAtencionAsync(AnulacionAtencionCrearDto dto);
    }
}
