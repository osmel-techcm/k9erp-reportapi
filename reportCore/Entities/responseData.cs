namespace reportCore.Entities
{
    public class responseData
    {
        public bool error { get; set; }
        public int errorValue { get; set; }
        public string description { get; set; }
        public object data { get; set; }
        public object othersValidations { get; set; }

        public responseData()
        {
            error = false;
            description = "Saved";
        }
    }
}
