using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class RolService : IRolService
    {
        private readonly IRolRepositorio _rolRepo;
        private readonly IMapper _mapper;

        public RolService(IRolRepositorio rolRepo, IMapper mapper)
        {
            _rolRepo = rolRepo;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> ObtenerTodosAsync()
        {
            try
            {
                var roles = await _rolRepo.ObtenerTodosAsync();
                var dto = _mapper.Map<IEnumerable<RolDto>>(roles);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = dto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener los roles.", ex.Message }
                };
            }
        }
    }
}
