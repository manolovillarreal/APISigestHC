
using Microsoft.EntityFrameworkCore;
using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;

namespace ApiSigestHC.Repositorio
{
    public class TipoDocumentoRepositorio : ITipoDocumentoRepositorio
    {
        private readonly ApplicationDbContext _db;

        public TipoDocumentoRepositorio(ApplicationDbContext contexto)
        {
            _db = contexto;
        }

        public async Task CrearTipoDocumentoAsync(TipoDocumento tipo)
        {
            _db.TiposDocumento.Add(tipo);
            await _db.SaveChangesAsync();
        }

         public async Task ActualizarTipoDocumentoAsync(TipoDocumento tipo)
        {
            _db.TiposDocumento.Update(tipo);
            await _db.SaveChangesAsync();
        }
      

        public async Task<IEnumerable<TipoDocumento>> GetTiposDocumentoAsync()
        {
            return await _db.TiposDocumento.ToListAsync();
        }

        public async Task<TipoDocumento> GetTipoDocumentoPorIdAsync(int id)
        {
            return await _db.TiposDocumento.FindAsync(id);
        }
        public async Task<IEnumerable<TipoDocumento>> ObtenerPorIdsAsync(IEnumerable<int> ids)
        {
            return await _db.TiposDocumento
                .Where(td => ids.Contains(td.Id))
                .ToListAsync();
        }

        public async Task<bool> ExisteTipoDocumentoPorCodigoAsync(string codigo)
        {
            return await _db.TiposDocumento
                .AnyAsync(td => td.Codigo.ToLower() == codigo.ToLower());
        }



    }
}
