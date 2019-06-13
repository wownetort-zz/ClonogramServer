using System;

namespace Clonogram.ViewModels
{
    public class CommentView
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PhotoId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
