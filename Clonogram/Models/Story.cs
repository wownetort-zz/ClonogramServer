using System;

namespace Clonogram.Models
{
    public class Story
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ImagePath { get; set; }
        public int ImageSize { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
