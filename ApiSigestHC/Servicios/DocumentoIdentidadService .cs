using System;
using System.Threading.Tasks;

namespace ApiSigestHC.Servicios
{
    //public class DocumentoIdentidadService : IDocumentoIdentidadService
    //{
    //    private readonly ApplicationDbContext _db;
    //    private readonly ILogger<DocumentoIdentidadService> _logger;

    //    public DocumentoIdentidadService(ApplicationDbContext db, ILogger<DocumentoIdentidadService> logger)
    //    {
    //        _db = db;
    //        _logger = logger;
    //    }

    //    public async Task<Documento?> ObtenerDocumentoIdentidadAnteriorValidoAsync(int atencionId)
    //    {
    //        var atencion = await _db.Atenciones
    //            .Include(a => a.Paciente)
    //            .FirstOrDefaultAsync(a => a.Id == atencionId);

    //        if (atencion == null || atencion.Paciente == null)
    //            return null;

    //        var paciente = atencion.Paciente;

    //        var tipoDocumento = await _db.TipoDocumentos
    //            .FirstOrDefaultAsync(t => t.Codigo == "ID");

    //        if (tipoDocumento == null)
    //        {
    //            _logger.LogWarning("Tipo de documento 'ID' no está configurado.");
    //            return null;
    //        }

    //        var fechaActual = DateTime.UtcNow;

    //        var documento = await _db.Documentos
    //            .Where(d =>
    //                d.TipoDocumentoId == tipoDocumento.Id &&
    //                d.Atencion.PacienteId == paciente.Id &&
    //                d.AtencionId != atencionId &&
    //                d.Fecha != null &&
    //                d.Fecha.Value.AddYears(5) >= fechaActual)
    //            .OrderByDescending(d => d.Fecha)
    //            .FirstOrDefaultAsync();

    //        if (documento == null || paciente.FechaNacimiento == null)
    //            return null;

    //        int edadAlMomento = CalcularEdad(paciente.FechaNacimiento.Value, documento.Fecha.Value);
    //        int edadActual = CalcularEdad(paciente.FechaNacimiento.Value, fechaActual);

    //        bool cambioDeRango = (edadAlMomento < 7 && edadActual >= 7) || (edadAlMomento < 18 && edadActual >= 18);

    //        if (cambioDeRango)
    //            return null;

    //        return documento;
    //    }

    //    private int CalcularEdad(DateTime nacimiento, DateTime referencia)
    //    {
    //        int edad = referencia.Year - nacimiento.Year;
    //        if (referencia < nacimiento.AddYears(edad))
    //            edad--;
    //        return edad;
    //    }
    //}


}
