using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class MotivoAnulacionAtencionRepositorio : IMotivoAnulacionAtencionRepositorio
    {
        private readonly ApplicationDbContext _context;

        public MotivoAnulacionAtencionRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MotivoAnulacionAtencion>> ObtenerTodosAsync()
        {
            return await _context.MotivosAnulacionAtencion
                .OrderBy(m => m.Descripcion).ToListAsync();
        }

        public async Task<MotivoAnulacionAtencion?> ObtenerPorIdAsync(int id)
        {
            return await _context.MotivosAnulacionAtencion
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.MotivosAnulacionAtencion.AnyAsync(m => m.Id == id);
        }
    }

}
