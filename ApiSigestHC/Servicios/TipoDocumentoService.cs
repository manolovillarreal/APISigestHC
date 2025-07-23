using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class TipoDocumentoService : ITipoDocumentoService
    {
        private readonly ITipoDocumentoRepositorio _repository;
        private readonly ITipoDocumentoRolRepositorio _tipoDocumentoRolRepositorio;
        private readonly IDocumentoRepositorio _documentoRepositorio;
        private readonly IUsuarioContextService _usuarioContextService;

        private readonly IMapper _mapper;

        public TipoDocumentoService(
            ITipoDocumentoRepositorio repository,
            ITipoDocumentoRolRepositorio rolRepositorio,
            IUsuarioContextService usuarioContextService,
            IDocumentoRepositorio documentoRepositorio,
            IMapper mapper)
        {
            _repository = repository;
            _tipoDocumentoRolRepositorio = rolRepositorio;
            _usuarioContextService = usuarioContextService;
            _documentoRepositorio = documentoRepositorio;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> ObtenerTiposPermitidosPorRolAsync()
        {
            try
            {
                var rolId = _usuarioContextService.ObtenerRolId();
                var relaciones = await _tipoDocumentoRolRepositorio.ObtenerPorRolAsync(rolId);
                var tipos = relaciones.Select(r => r.TipoDocumento).Distinct();
                var dto = _mapper.Map<IEnumerable<TipoDocumentoDto>>(tipos);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = dto };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener tipos autorizados.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> ObtenerTodosAsync()
        {
            try
            {
                var tipos = await _repository.GetTiposDocumentoAsync();
                var dto = _mapper.Map<IEnumerable<TipoDocumentoDto>>(tipos);
                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = dto };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener todos los tipos.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> ObtenerPorIdAsync(int id)
        {
            try
            {
                var tipo = await _repository.GetTipoDocumentoPorIdAsync(id);
                if (tipo == null)
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = ["No encontrado"] };

                var dto = _mapper.Map<TipoDocumentoDto>(tipo);
                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.OK, Result = dto };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al obtener tipo por id", ex.Message]
                };
            }
        }

        public async Task<RespuestaAPI> CrearAsync(TipoDocumentoCrearDto dto)
        {
            try
            {
                if (await _repository.ExisteTipoDocumentoPorCodigoAsync(dto.Codigo))
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.Conflict, ErrorMessages = ["Código duplicado"] };

                var tipo = _mapper.Map<TipoDocumento>(dto);
                await _repository.CrearTipoDocumentoAsync(tipo);
                var resultDto = _mapper.Map<TipoDocumentoDto>(tipo);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.Created, Result = resultDto };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al crear tipo", ex.Message]
                };
            }
        }

        public async Task<RespuestaAPI> EditarAsync(int id, TipoDocumentoCrearDto dto)
        {
            try
            {
                var existente = await _repository.GetTipoDocumentoPorIdAsync(id);
                if (existente == null)
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = ["No encontrado"] };

                _mapper.Map(dto, existente); 

                await _repository.ActualizarAsync(existente);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.NoContent };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al editar tipo", ex.Message]
                };
            }
        }
        public async Task<RespuestaAPI> EliminarAsync(int id)
        {
            try
            {
                var existente = await _repository.GetTipoDocumentoPorIdAsync(id);
                if (existente == null)
                    return new RespuestaAPI { Ok = false, StatusCode = HttpStatusCode.NotFound, ErrorMessages = ["No encontrado"] };
                string message = "";
                if((await _documentoRepositorio.ExistenDelTipoAsync(id)))
                {
                    return new RespuestaAPI { 
                        Ok = false, 
                        StatusCode = HttpStatusCode.BadRequest, 
                        ErrorMessages = new List<string> {
                            "No se puede eliminar el Tipo de Documento ya que existen documentos asociados a este" } 
                    };
                    
                }
                await _repository.EliminarAsync(existente);

                return new RespuestaAPI { Ok = true, StatusCode = HttpStatusCode.NoContent, Message=message };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Error al editar tipo", ex.Message]
                };
            }
        }



    }
}

