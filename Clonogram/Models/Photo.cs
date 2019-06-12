using System;
using NpgsqlTypes;

namespace Clonogram.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public NpgsqlPoint Geo { get; set; }
        public string ImagePath { get; set; }
        public int ImageSize { get; set; }
    }
}
