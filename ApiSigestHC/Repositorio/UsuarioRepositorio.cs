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

        public UsuarioRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<Usuario> GetUsuarioAsync(int usuarioId)
        {
            return await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);
        }

        public async Task<ICollection<Usuario>> GetUsuariosAsync()
        {
            return await _db.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u=>u.NombreUsuario).ToListAsync();
        }

        public async Task<bool> IsUniqueUser(string usuario)
        {
            var usuarioBd = await _db.Usuarios.FirstOrDefaultAsync(u=>u.NombreUsuario == usuario);

            return usuarioBd == null;
        }


        public async Task<Usuario?> ObtenerPorCredencialesAsync(string username, string password)
        {
            var passwordEncriptado = obtenerMD5(password);
            return await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == username.ToLower()
                                       && u.Contraseña == passwordEncriptado);
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
