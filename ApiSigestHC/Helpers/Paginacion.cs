using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Helpers
{
    /// <summary>
    /// Utilidades de paginación. El tamaño de página por defecto y el máximo
    /// permitido se definen aquí, en un único lugar del backend (el frontend
    /// envía su propio PAGE_SIZE; estos valores son el fallback/limite de seguridad).
    /// </summary>
    public static class Paginacion
    {
        public const int PageSizeDefault = 50;
        public const int PageSizeMax = 200;

        public static (int page, int pageSize) Normalizar(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = PageSizeDefault;
            if (pageSize > PageSizeMax) pageSize = PageSizeMax;
            return (page, pageSize);
        }

        /// <summary>
        /// Toma una colección ya filtrada/ordenada en memoria y devuelve la página solicitada
        /// junto con los metadatos (total, totalPages).
        /// </summary>
        public static ResultadoPaginadoDto<T> Paginar<T>(IEnumerable<T> fuente, int page, int pageSize)
        {
            (page, pageSize) = Normalizar(page, pageSize);

            var lista = fuente as IList<T> ?? fuente.ToList();
            var total = lista.Count;
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            var data = lista
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new ResultadoPaginadoDto<T>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = totalPages
            };
        }
    }
}
