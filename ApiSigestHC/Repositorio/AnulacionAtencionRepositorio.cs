using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class AnulacionAtencionRepositorio : IAnulacionAtencionRepositorio
    {
        private readonly ApplicationDbContext _context;

        public AnulacionAtencionRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GuardarAsync(AnulacionAtencion anulacion)
        {
            _context.Add(anulacion);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AtencionExisteAsync(int atencionId)
        {
            return await _context.Atenciones.AnyAsync(a=> a.Id == atencionId);
        }

        public async Task<bool> MotivoExisteAsync(int motivoId)
        {
            return await _context.MotivosAnulacionAtencion.AnyAsync(m => m.Id == motivoId);
        }
    }

}
