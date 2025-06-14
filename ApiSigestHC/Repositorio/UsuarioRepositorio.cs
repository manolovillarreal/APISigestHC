using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace ApiSigestHC.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db;
        private string claveSecreta;

        public UsuarioRepositorio(ApplicationDbContext db,IConfiguration config)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secret");
        }


        public Usuario GetUsuario(int usuarioId)
        {
           return _db.Usuarios
                .Include(u=>u.Rol)
                .FirstOrDefault(u=>u.Id == usuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            return _db.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u=>u.NombreUsuario).ToList();
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuarioBd = _db.Usuarios.FirstOrDefault(u=>u.NombreUsuario == usuario);

            return usuarioBd == null;
        }

        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            var passwordEcriptado = obtenerMD5(usuarioLoginDto.Password);
            var usuario = _db.Usuarios
                 .Include(u => u.Rol) // Incluye la tabla de rol
                .FirstOrDefault(
                u => u.NombreUsuario.ToLower() == usuarioLoginDto.NombreUsuario.ToLower()
                && u.Contraseña == passwordEcriptado
                );
            
            if (usuario == null) {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };              
            }
            //Aqui existe el usuario entonces podemos procesar el login
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name,usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role,usuario.RolNombre),
                    new Claim("rol_id", usuario.RolId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new (new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = usuario
            };

            return usuarioLoginRespuestaDto;


        }

        public async Task<Usuario> CrearUsuario(UsuarioCrearDto usuarioRegistroDto)
        {
            var passwordEcriptado = obtenerMD5(usuarioRegistroDto.Password);

            Usuario usuario = new Usuario()
            {
                NombreUsuario = usuarioRegistroDto.NombreUsuario,
                Correo = usuarioRegistroDto.Correo,
                Contraseña = passwordEcriptado,
                Nombre = usuarioRegistroDto.Nombre,
                Apellidos = usuarioRegistroDto.Apellidos,
                RolId = usuarioRegistroDto.RoleId,
                Estado = true
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return usuario;
        }
        private string obtenerMD5(string valor)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
            data = x.ComputeHash(data);
            string resp = "";
            for (int i = 0; i < data.Length; i++)
            {
                resp += data[i].ToString("x2").ToLower();
            }
            return resp;    

        }
    }
}
