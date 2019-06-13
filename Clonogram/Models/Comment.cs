using System;

namespace Clonogram.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PhotoId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
