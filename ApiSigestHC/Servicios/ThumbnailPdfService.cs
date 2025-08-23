using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PdfiumViewer;
using ApiSigestHC.Servicios.IServicios;
using System.Drawing;

namespace ApiSigestHC.Servicios
{
    public class ThumbnailPdfService : IThumbnailPdfService
    {
        public async Task<List<string>> GenerarThumbnailsAsync(Stream pdfStream, int width = 150, int height = 150)
        {
            var thumbnails = new List<string>();
            using (var pdfDocument = PdfDocument.Load(pdfStream))
            {
                for (int i = 0; i < pdfDocument.PageCount; i++)
                {
                    using (var image = pdfDocument.Render(i, width, height, 96, 96, true))
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        thumbnails.Add(Convert.ToBase64String(ms.ToArray()));
                    }
                    break;
                }
            }
            return thumbnails;
        }
    }
}
