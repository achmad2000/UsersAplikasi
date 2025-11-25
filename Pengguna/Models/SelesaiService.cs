using System.ComponentModel.DataAnnotations;

namespace Pengguna.Models
{
    public class SelesaiService
    {
        [Key]
        public int Id { get; set; } // Primary Key baru untuk arsip

        public string NomorService { get; set; }
        public string NamaCustomer { get; set; }
        public string ItemService { get; set; }
        public DateTime JadwalService { get; set; }
        public string NoWA { get; set; }
        public string Alamat { get; set; }
        public string DeskripsiProblem { get; set; }
        public DateTime TanggalOrder { get; set; }

        // Data dari ServiceLog (Hasil Pengerjaan)
        public string StatusPembayaran { get; set; } // Cash / Transfer
        public int? TechnicianId { get; set; }
        public string NamaTeknisi { get; set; }

        // Opsional: Tambahan info waktu selesai
        public DateTime? WaktuSelesai { get; set; }
        public decimal TotalBiaya { get; set; }
    }
}