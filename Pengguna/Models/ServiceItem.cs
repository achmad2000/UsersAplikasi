using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class ServiceItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama item/jasa wajib diisi")]
        [Display(Name = "Nama Barang / Jasa")]
        public string NamaItem { get; set; }

        [Required(ErrorMessage = "Harga wajib diisi")]
        [Column(TypeName = "decimal(18, 2)")] 
        [Display(Name = "Harga Satuan")]
        [Range(0, double.MaxValue, ErrorMessage = "Harga tidak boleh negatif")]
        public decimal Harga { get; set; }
    }
}