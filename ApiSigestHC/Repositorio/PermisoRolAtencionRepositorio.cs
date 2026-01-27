using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class PermisoRolAtencionRepositorio : IPermisoRolAtencionRepositorio
    {
        private readonly ApplicationDbContext _db;

        public PermisoRolAtencionRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<int>> ObtenerEstadosVisiblesPorRolAsync(int rolId)
        {
            return await _db.PermisosRolAtencion
                .Where(p => p.RolId == rolId && p.EsVisible)
                .Select(p => p.EstadoAtencionId)
                .ToListAsync();
        }

        public async Task<List<int>> ObtenerEstadosPermitidosPorRolAsync(int rolId)
        {
            return await _db.PermisosRolAtencion
                .Where(p => p.RolId == rolId && p.EsPermitido)
                .Select(p => p.EstadoAtencionId)
                .ToListAsync();
        }

        public async Task<bool> TienePermisoAsync(int rolId, int estadoAtencionId)
        {
            return await _db.PermisosRolAtencion
                .AnyAsync(p => p.RolId == rolId && p.EstadoAtencionId == estadoAtencionId && p.EsPermitido);
        }

        public async Task<bool> EsVisibleAsync(int rolId, int estadoAtencionId)
        {
            return await _db.PermisosRolAtencion
                .AnyAsync(p => p.RolId == rolId && p.EstadoAtencionId == estadoAtencionId && p.EsVisible);
        }
    }
}
