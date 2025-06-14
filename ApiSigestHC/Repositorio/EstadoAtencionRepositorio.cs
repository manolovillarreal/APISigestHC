using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class EstadoAtencionRepositorio : IEstadoAtencionRepositorio
    {
        private readonly ApplicationDbContext _db;

        public EstadoAtencionRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<EstadoAtencion>> ObtenerTodosAsync()
        {
            return await _db.EstadosAtencion.OrderBy(e => e.Orden).ToListAsync();
        }

        public async Task<EstadoAtencion?> ObtenerPorIdAsync(int id)
        {
            return await _db.EstadosAtencion.FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
