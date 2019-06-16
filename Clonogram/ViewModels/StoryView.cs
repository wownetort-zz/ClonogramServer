using System;

namespace Clonogram.ViewModels
{
    public class StoryView
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ImagePath { get; set; } = "";
        public int ImageSize { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
