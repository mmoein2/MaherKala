using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class MarketerChat
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(300,ErrorMessage ="پیام بیش از حد بزرگ است")]
        public string Text { get; set; }
        public long Timestamp { get; set; }
        public MarketerUser User { get; set; }
        public MarketerChat()
        {
            
            Timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        }

    }
}