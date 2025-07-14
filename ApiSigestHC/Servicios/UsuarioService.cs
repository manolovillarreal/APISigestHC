using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;
using System.Security.Claims;

namespace ApiSigestHC.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IMapper _mapper;

        public UsuarioService(IUsuarioRepositorio usuarioRepo,
                    IUsuarioContextService usuarioContextService,
                    IMapper mapper)
        {
            _usuarioRepo = usuarioRepo;
            _usuarioContextService = usuarioContextService;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> ObtenerUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepo.GetUsuariosAsync();
                var usuariosDto = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);

                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuariosDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener los usuarios.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> ObtenerUsuarioPorIdAsync(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepo.GetUsuarioAsync(usuarioId);
                if (usuario == null)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Usuario no encontrado." }
                    };
                }

                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener el usuario.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> CrearUsuarioAsync(UsuarioCrearDto dto)
        {
            try
            {
                var esUnico = await _usuarioRepo.IsUniqueUser(dto.NombreUsuario);
                if (!esUnico)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El nombre de usuario ya existe." }
                    };
                }

                var usuario = await _usuarioRepo.CrearUsuario(dto);
                if (usuario == null)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Error al registrar el usuario." }
                    };
                }
                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al crear el usuario.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> LoginAsync(UsuarioLoginDto dto)
        {
            try
            {
                var resultadoLogin = await _usuarioRepo.Login(dto);

                if (resultadoLogin.Usuario == null || string.IsNullOrEmpty(resultadoLogin.Token))
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Nombre de usuario o contraseña incorrectos." }
                    };
                }

                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = resultadoLogin
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error durante el inicio de sesión.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> ObtenerPerfilAsync()
        {
            try
            {               
                int userId = _usuarioContextService.ObtenerUsuarioId();
                var usuario = await  _usuarioRepo.GetUsuarioAsync(userId);

                if (usuario == null)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Usuario no encontrado" }
                    };
                }

                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);

                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener perfil", ex.Message }
                };
            }
        }
    }
}
