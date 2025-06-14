using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using System.Net;
using Microsoft.AspNetCore.Http;
using ApiSigestHC.Servicios.IServicios;

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
                .ToListAsync();
        }
        public async Task<IEnumerable<Documento>> ObtenerPermitidosParaCargar(int atencionId, int rolId)
        {
            return await _db.Documentos
                .Include(d => d.TipoDocumento)
                .Where(d => d.AtencionId == atencionId &&
                            _db.TipoDocumentoRoles.Any(tdr =>
                                tdr.TipoDocumentoId == d.TipoDocumentoId &&
                                tdr.RolId == rolId &&
                                tdr.PuedeCargar))
                .ToListAsync();
        }

        public async Task<Documento> ObtenerPorIdAsync(int id)
        {
            return await _db.Documentos
                .Include(d => d.TipoDocumento)
                .Include(d=>d.Atencion)
                .FirstOrDefaultAsync(d=> d.Id == id);
        }

        public async Task GuardarAsync(Documento documento)
        {
            await _db.Documentos.AddAsync(documento);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarDocumentoAsync(Documento documento)
        {
            _db.Documentos.Update(documento);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarDocumentoAsync(int id)
        {
            var documento = await ObtenerPorIdAsync(id);
            if (documento != null)
            {
                _db.Documentos.Remove(documento);
                await _db.SaveChangesAsync();
            }
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
        public async Task<int> ContarPorTipoYAtencionAsync(int atencionId, int tipoDocumentoId)
        {
            return await _db.Documentos
                .CountAsync(d => d.AtencionId == atencionId && d.TipoDocumentoId == tipoDocumentoId);
        }

    }
}
