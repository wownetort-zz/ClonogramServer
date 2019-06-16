using System;

namespace Clonogram.ViewModels
{
    public class PhotoView
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImagePath { get; set; } = "";
        public int ImageSize { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
