namespace DMF_Services.DTOs.Common
{
    public class PagedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}
