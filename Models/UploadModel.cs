using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace testOcr2.Models
{
    public class UploadModel
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
        public string ImageCaption { get; set; }
        public string ImageDescription { get; set; }

    }
}
