using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.EntityFrameworkCore;
using XAct.Library.Settings;

namespace ApiSigestHC.Servicios
{
    public class ValidacionDocumentosObligatoriosService:IValidacionDocumentosObligatoriosService
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly IDocumentoRepositorio _documentoRepo;

        public ValidacionDocumentosObligatoriosService(IDocumentoRequeridoRepositorio documentoRequeridoRepositorio,
            IDocumentoRepositorio documentoRepo,
            ITipoDocumentoRepositorio tipoDocumentoRepo)
        {
            _documentoRequeridoRepo = documentoRequeridoRepositorio;
            _documentoRepo = documentoRepo;
            _tipoDocumentoRepo = tipoDocumentoRepo;
        }

        public async Task<ResultadoValidacionDto> ValidarDocumentosObligatoriosAsync(Atencion atencion)
        {
            // Obtener documentos requeridos para el estado destino
            var requeridos = await _documentoRequeridoRepo.ObtenerPorEstadoAsync(atencion.Id);
            var requeridosIds = requeridos.Select(r => r.TipoDocumentoId).ToList();

            // Obtener tipos de documento ya cargados para esta atención
            var cargados = await _documentoRepo.ObtenerPorAtencionIdAsync(atencion.Id);
            var cargadosIds = cargados.Select(d => d.TipoDocumentoId).ToList();

            var faltantesIds = requeridosIds.Except(cargadosIds).ToList();

            if (!faltantesIds.Any())
            {
                return new ResultadoValidacionDto { EsValido = true };
            }

            // Obtener nombres de los documentos faltantes
            var tiposFaltantes = await _tipoDocumentoRepo.ObtenerPorIdsAsync(faltantesIds);
            var nombresFaltantes = tiposFaltantes.Select(t => t.Nombre).ToList();

            return new ResultadoValidacionDto
            {
                EsValido = false,
                DocumentosFaltantes = nombresFaltantes
            };
        }

    }
}
