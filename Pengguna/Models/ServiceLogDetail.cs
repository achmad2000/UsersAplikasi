using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models // Pastikan namespace Anda benar
{
    public class ServiceLogDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceLogId { get; set; }
        [ForeignKey("ServiceLogId")]
        public virtual ServiceLog ServiceLog { get; set; }

        // [BARU] Tambahkan kolom ini
        [Required]
        [Display(Name = "Jenis Service")]
        public string JenisService { get; set; }

        [Required]
        [Display(Name = "Nama Barang/Jasa")]
        public string NamaBarang { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Jumlah minimal 1")]
        public int Jumlah { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Harga Satuan")]
        public decimal HargaSatuan { get; set; }

        [NotMapped]
        public decimal SubTotal => Jumlah * HargaSatuan;
    }
}