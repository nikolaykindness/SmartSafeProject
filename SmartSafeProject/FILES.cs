namespace FBFormAppExample
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Firebird.FILES")]
    public partial class FILES
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FILES()
        {
            //INVOICES = new HashSet<INVOICE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Required]

        [StringLength(255)]
        public string NAME { get; set; }


        [StringLength(50)]
        public string FILETYPE { get; set; }


        public byte[] FILEDATA { get; set; }


        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<INVOICE> INVOICES { get; set; }
    }
}
