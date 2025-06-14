using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class CambioEstadoRepositorio: ICambioEstadoRepositorio
    {
        private readonly ApplicationDbContext _db;

        public CambioEstadoRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RegistrarCambioAsync(CambioEstado cambio)
        {
            _db.CambiosEstado.Add(cambio);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CambioEstado>> ObtenerCambiosPorAtencionAsync(int atencionId)
        {
            return await _db.CambiosEstado
                            .Where(c => c.AtencionId == atencionId)
                            .OrderByDescending(c => c.Fecha)
                            .ToListAsync();
        }
    }
}
