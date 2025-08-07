using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class TipoDocumentoRolService : ITipoDocumentoRolService
    {
        private readonly ITipoDocumentoRolRepositorio _repo;
        private readonly IMapper _mapper;


        public TipoDocumentoRolService(ITipoDocumentoRolRepositorio repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> ObtenerPorTipoDocumentoAsync(int tipoDocumentoId)
        {
            try
            {
                var relaciones = await _repo.GetPorTipoDocumentoAsync(tipoDocumentoId);
                var dto = _mapper.Map<IEnumerable<TipoDocumentoRolDto>>(relaciones);
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
                    ErrorMessages = ["Error al obtener relaciones", ex.Message]
                };
            }
        }
        public async Task<RespuestaAPI> ObtenerPorRolAsync(int rolId)
        {
             try
            {
                var relaciones = await _repo.GetByRolAsync(rolId);
                var dto = _mapper.Map<IEnumerable<TipoDocumentoRolDto>>(relaciones);
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
                    ErrorMessages = ["Error al obtener relaciones", ex.Message]
                };
            }
        }
        public async Task<RespuestaAPI> CrearAsync(TipoDocumentoRolCrearDto dto)
        {
            try
            {
                var entidad = _mapper.Map<TipoDocumentoRol>(dto);
                await _repo.CrearAsync(entidad);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = "Relación creada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al crear relación", ex.Message]
                };
            }
        }

        public async Task<RespuestaAPI> ActualizarAsync(TipoDocumentoRolCrearDto dto)
        {
            try
            {
                var entidad = await _repo.GetPorIdsAsync(dto.TipoDocumentoId, dto.RolId);
                if (entidad == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = ["Relación no encontrada"]
                    };
                }

                _mapper.Map(dto, entidad);
                await _repo.ActualizarAsync(entidad);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.NoContent
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al actualizar relación", ex.Message]
                };
            }
        }

        public async Task<RespuestaAPI> EliminarAsync(int tipoDocumentoId, int rolId)
        {
            try
            {
                var entidad = await _repo.GetPorIdsAsync(tipoDocumentoId, rolId);
                if (entidad == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = ["Relación no encontrada"]
                    };
                }

                await _repo.EliminarAsync(entidad);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.NoContent
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al eliminar relación", ex.Message]
                };
            }
        }

       
    }

}
