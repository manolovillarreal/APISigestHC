using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using System.Threading.Tasks;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IAnulacionAtencionService
    {
        Task<RespuestaAPI> AnularAtencionAsync(AnulacionAtencionCrearDto dto);
    }
}
