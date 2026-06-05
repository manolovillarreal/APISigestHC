using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class ConfiguracionRepositorio : IConfiguracionRepositorio
    {
        private readonly ApplicationDbContext _db;

        public ConfiguracionRepositorio(ApplicationDbContext contexto)
        {
            _db = contexto;
        }

        public async Task<Configuracion?> ObtenerPorClaveAsync(string clave)
        {
            return await _db.Configuraciones.FirstOrDefaultAsync(c => c.Clave == clave);
        }

        public async Task<IEnumerable<Configuracion>> ObtenerTodasAsync()
        {
            return await _db.Configuraciones.ToListAsync();
        }

        public async Task ActualizarAsync(Configuracion configuracion)
        {
            _db.Configuraciones.Update(configuracion);
            await _db.SaveChangesAsync();
        }
    }
}
