using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSafeProject
{
    internal class FilesModel
    {
        public int ID { get; set; }

        [Required]

        [StringLength(255)]
        public string NAME { get; set; }


        [StringLength(50)]
        public string FILETYPE { get; set; }


        public byte[] FILEDATA { get; set; }
    }
}
