using System.ComponentModel.DataAnnotations;

namespace Pengguna.Models
{
    public class Garansi
    {
        [Key]
        public int Id { get; set; }

        // Menghubungkan ke Nomor Service Asli (misal: HS2052)
        public string NomorServiceRef { get; set; }

        public string NamaCustomer { get; set; }
        public string ItemService { get; set; } // Info barang (AC/Kulkas)

        [Required]
        public string DeskripsiKeluhan { get; set; } // Keluhan Customer

        public DateTime TanggalKlaim { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Menunggu Review";

        public string? CatatanAdmin { get; set; } // Alasan tolak/terima
        public string? BuktiFotoPath { get; set; }
    }
}