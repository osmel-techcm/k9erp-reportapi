namespace reportCore.Entities
{
    public class FilterReport
    {
        public string dataSourceField { get; set; }
        public string primaryValue { get; set; }
        public string secondaryValue { get; set; }
        public int valueComparison { get; set; }
        public int primaryValueId { get; set; }
        public int secondaryValueId { get; set; }
        public string inputType { get; set; }
    }
}
