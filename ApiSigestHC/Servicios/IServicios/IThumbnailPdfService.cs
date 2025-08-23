using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface IThumbnailPdfService
    {
        Task<List<string>> GenerarThumbnailsAsync(Stream pdfStream, int width = 150, int height = 150);
    }
}
