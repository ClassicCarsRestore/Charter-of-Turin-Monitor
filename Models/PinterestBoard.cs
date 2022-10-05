namespace tasklist.Models
{
    public class PinterestBoard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PinterestUser Owner { get; set; }
        public string Privacy { get; set; }
    }
}
