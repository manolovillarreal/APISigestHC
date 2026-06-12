namespace ApiSigestHC.Modelos.Dtos
{
    /// <summary>
    /// Envoltorio estándar para respuestas paginadas.
    /// Se serializa en camelCase: { data, page, pageSize, total, totalPages }.
    /// </summary>
    public class ResultadoPaginadoDto<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
}
