namespace reportCore.Entities
{
    public class PaginatorData
    {
        public int Count { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string filterDataSt { get; set; }
        public string orderField { get; set; }
        public bool descending { get; set; }

        public PaginatorData()
        {
            Count = 0;
            PageNumber = 1;
            PageSize = 100;
        }
    }
}
