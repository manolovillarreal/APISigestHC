using ApiSigestHC.Data;
using Microsoft.EntityFrameworkCore;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;

namespace ApiSigestHC.Repositorio
{
    public class TipoDocumentoRolRepositorio : ITipoDocumentoRolRepositorio
    {
        private readonly ApplicationDbContext _db;

        public TipoDocumentoRolRepositorio(ApplicationDbContext context)
        {
            _db = context;
        }
        public async Task<IEnumerable<TipoDocumentoRol>> GetByRolAsync(int rolId)
        {
            return await _db.TipoDocumentoRoles
               .Where(tdr => tdr.RolId == rolId )
               .Include(tdr => tdr.Rol)
               .Include(tdr => tdr.TipoDocumento)
               .ToListAsync();
        }
        public async Task<IEnumerable<TipoDocumentoRol>> GetByRolParaCargaAsync(int rolId)
        {
            return await _db.TipoDocumentoRoles
               .Where(tdr => tdr.RolId == rolId && tdr.PuedeCargar)
               .Include(tdr => tdr.Rol)
               .Include(tdr => tdr.TipoDocumento)
               .ToListAsync();
        }

        public async Task<IEnumerable<TipoDocumentoRol>> GetPorTipoDocumentoAsync(int tipoDocumentoId)
        {
            return await _db.TipoDocumentoRoles
                .Where(x => x.TipoDocumentoId == tipoDocumentoId)
                .Include(x => x.Rol)
                .Include(x => x.TipoDocumento)
                .ToListAsync();
        }


        public async Task<TipoDocumentoRol> GetPorIdsAsync(int tipoDocumentoId, int rolId)
        {
            return await _db.TipoDocumentoRoles
                .FirstOrDefaultAsync(x => x.TipoDocumentoId == tipoDocumentoId && x.RolId == rolId);
        }

        public async Task CrearAsync(TipoDocumentoRol entidad)
        {
            _db.TipoDocumentoRoles.Add(entidad);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarAsync(TipoDocumentoRol entidad)
        {
            _db.TipoDocumentoRoles.Update(entidad);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarAsync(TipoDocumentoRol entidad)
        {
            _db.TipoDocumentoRoles.Remove(entidad);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> PuedeCargarTipoDocumento(int rolId,int tipoDocumentoId)
        {
            return await _db.TipoDocumentoRoles
            .AnyAsync(x => x.RolId == rolId
                    && x.TipoDocumentoId == tipoDocumentoId
                    && x.PuedeCargar);
        }

      
    }

}
