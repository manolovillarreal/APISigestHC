using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ApiSigestHC.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IUsuarioContextService _usuarioContextService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public UsuarioService(IUsuarioRepositorio usuarioRepo,
                    IUsuarioContextService usuarioContextService,
                    IConfiguration config,
                    IMapper mapper)
        {
            _usuarioRepo = usuarioRepo;
            _usuarioContextService = usuarioContextService;
            _config = config;
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
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuariosDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
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
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Usuario no encontrado." }
                    };
                }

                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
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
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El nombre de usuario ya existe." }
                    };
                }

                var usuario = await _usuarioRepo.CrearUsuario(dto);
                if (usuario == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Error al registrar el usuario." }
                    };
                }
                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error interno al crear el usuario.", ex.Message }
                };
            }
        }

        public async Task<RespuestaAPI> LoginAsync(UsuarioLoginDto dto)
        {
            try
            {
                var usuario = await _usuarioRepo.ObtenerPorCredencialesAsync(dto.NombreUsuario, dto.Password);

                if (usuario == null)
                {

                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Nombre de usuario o contraseña incorrectos." }
                    };

                }

                var token = GenerarToken(usuario); // método privado
                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = new UsuarioLoginRespuestaDto
                    {
                        Token = token,
                        Usuario = _mapper.Map<UsuarioDto>(usuario)
                    }
                };
            }
            catch (Exception e)
            {

                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                   ErrorMessages = new List<string> { "Error inesperado:  "+e.Message}
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
                        Ok = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Usuario no encontrado" }
                    };
                }

                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener perfil", ex.Message }
                };
            }
        }

        private string GenerarToken(Usuario usuario)
        {
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("ApiSettings:Secret"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Role, usuario.RolNombre),
            new Claim("rol_id", usuario.RolId.ToString())
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);
            return manejadorToken.WriteToken(token);
        }

    }
}
