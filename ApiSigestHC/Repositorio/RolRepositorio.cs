using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class RolRepositorio : IRolRepositorio
    {
        private readonly ApplicationDbContext _db;

        public RolRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
        {
            return await _db.Roles.ToListAsync();
        }
    }
}
