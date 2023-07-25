using System.Collections.Generic;

namespace tasklist.Models
{
    public class Image
    {
        public string Width { get; set; }
        public string Height { get; set; }
        public string Url { get; set; }
    }
    public class Images
    {
        public Image Originals { get; set; }
    }
    public class PinMedia
    {
        //public Images Images { get; set; }
        public string Cover_Image_Url { get; set; }
        public double Duration { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Media_type { get; set; }
        public Dictionary<string, Image> Images { get; set; }
    }
    public class BoardOwner
    {
        public string Username { get; set; }
    }
    public class PinterestPin
    {
        public string Id { get; set; }
        public string Created_At { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Dominant_Color { get; set; }
        public string Alt_Text { get; set; }
        public string Board_Id { get; set; }
        public string Board_Section_Id { get; set; }
        public BoardOwner Board_Owner { get; set; }
        public PinMedia Media { get; set; }
        public string Parent_Pin_Id { get; set; }
        public string Note {get; set; }
    }
}
