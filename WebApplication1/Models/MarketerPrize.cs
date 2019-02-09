using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class MarketerPrize
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(40)]
        public string Title { get; set; }
        public string Text { get; set; }
        [MaxLength(300)]
        public string PicAddress { get; set; }
    }
}