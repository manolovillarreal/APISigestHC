using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ApiSigestHC.Repositorio
{
    public class DocumentoRepositorio : IDocumentoRepositorio
    {
        private readonly ApplicationDbContext _db;
        private readonly IUsuarioContextService _usuarioContext;

        public DocumentoRepositorio(ApplicationDbContext contexto)
        {
            _db = contexto;
        }

        public async Task<IEnumerable<Documento>> ObtenerPorAtencionIdAsync(int atencionId)
        {
            return await _db.Documentos
                .Where(d => d.AtencionId == atencionId)
                .Include(d => d.Usuario)
                    .ThenInclude(u=>u.Rol)
                .ToListAsync();
        }
        public async Task<IEnumerable<Documento>> ObtenerPermitidosParaCargar(int atencionId, int rolId)
        {
            return await _db.Documentos
                .Include(d => d.TipoDocumento)
                .Include(d => d.Usuario)
                    .ThenInclude(u => u.Rol)
                .Where(d => d.AtencionId == atencionId &&
                            _db.TipoDocumentoRoles.Any(tdr =>
                                tdr.TipoDocumentoId == d.TipoDocumentoId &&
                                tdr.RolId == rolId &&
                                tdr.PuedeCargar))
                .ToListAsync();
        }
        public async Task<IEnumerable<Documento>> ObtenerPermitidosParaVer(int atencionId, int rolId)
        {
            return await _db.Documentos
                .Include(d => d.TipoDocumento)
                .Include(d => d.Usuario)
                    .ThenInclude(u => u.Rol)
                .Where(d => d.AtencionId == atencionId &&
                            _db.TipoDocumentoRoles.Any(tdr =>
                                tdr.TipoDocumentoId == d.TipoDocumentoId &&
                                tdr.RolId == rolId &&
                                tdr.PuedeVer))
                .ToListAsync();
        }

        public async Task<Documento> ObtenerPorIdAsync(int id)
        {
            return await _db.Documentos
                .Include(d => d.TipoDocumento)
                .Include(d=>d.Atencion)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d=> d.Id == id);
        }

        public async Task GuardarAsync(Documento documento)
        {
            await _db.Documentos.AddAsync(documento);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Documento documento)
        {
            _db.Documentos.Update(documento);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarAsync(Documento documento)
        {
            if (documento == null)
                throw new ArgumentNullException(nameof(documento));

            _db.Documentos.Remove(documento);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> PuedeCargarDocumento(int rolId, int tipoDocumentoId)
        {
                return await _db.TipoDocumentoRoles
            .AnyAsync(td =>
                td.RolId == rolId &&
                td.TipoDocumentoId == tipoDocumentoId &&
                td.PuedeCargar);
        }
        public async Task<bool> PuedeVerDocumento(int rolId, int tipoDocumentoId)
        {
            return await _db.TipoDocumentoRoles
            .AnyAsync(td =>
                td.RolId == rolId &&
                td.TipoDocumentoId == tipoDocumentoId &&
                td.PuedeVer);
        }
        public async Task<bool> ExisteDocumentoAsync(int atencionId, int tipoDocumentoId)
        {
            return await _db.Documentos.AnyAsync(d =>
                d.AtencionId == atencionId &&
                d.TipoDocumentoId == tipoDocumentoId);
        }
        public async Task<bool> ExistenDelTipoAsync(int tipoDocumentoId)
        {
            return await _db.Documentos.AnyAsync(d =>
                d.TipoDocumentoId == tipoDocumentoId);
        }
        public async Task<int> ContarPorTipoYAtencionAsync(int atencionId, int tipoDocumentoId)
        {
            return await _db.Documentos
                .CountAsync(d => d.AtencionId == atencionId && d.TipoDocumentoId == tipoDocumentoId);
        }

      
    }
}
