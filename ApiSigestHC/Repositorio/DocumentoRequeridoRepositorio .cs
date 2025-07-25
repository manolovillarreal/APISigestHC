﻿using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiSigestHC.Repositorio
{
    public class DocumentoRequeridoRepositorio : IDocumentoRequeridoRepositorio
    {
        private readonly ApplicationDbContext _db;

        public DocumentoRequeridoRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DocumentoRequerido>> ObtenerTodosAsync()
        {
            return await _db.DocumentosRequeridos
                .Include(d => d.TipoDocumento)
                .Include(d => d.EstadoAtencion)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentoRequerido>> ObtenerPorEstadoAsync(int estadoAtencionId)
        {
            return await _db.DocumentosRequeridos
                .Include(d => d.TipoDocumento)
                .Where(d => d.EstadoAtencionId == estadoAtencionId)
                .ToListAsync();
        }

        public async Task AgregarAsync(DocumentoRequerido doc)
        {
            _db.DocumentosRequeridos.Add(doc);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarAsync(int estadoAtencionId, int tipoDocumentoId)
        {
            var doc = await _db.DocumentosRequeridos.FindAsync(estadoAtencionId, tipoDocumentoId);
            if (doc != null)
            {
                _db.DocumentosRequeridos.Remove(doc);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(int estadoAtencionId, int tipoDocumentoId)
        {
            return await _db.DocumentosRequeridos
                .AnyAsync(x => x.EstadoAtencionId == estadoAtencionId && x.TipoDocumentoId == tipoDocumentoId);
        }
    }

}
