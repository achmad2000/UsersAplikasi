using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class WaitingResponOrder
    {
        public int Id { get; set; }
        public string NamaCustomer { get; set; }
        public string ItemService { get; set; }
        public DateTime JadwalService { get; set; }
        public string NoWA { get; set; }
        public string Alamat { get; set; }
        public string DeskripsiProblem { get; set; }
        public DateTime TanggalOrder { get; set; } = DateTime.Now;
        public bool IsTaken { get; set; } = false;
        public string Status { get; set; } = "Menunggu Teknisi";
        public int? TechnicianId { get; set; }
        public string? NamaTeknisi { get; set; }
        public DateTime? CancelRequestedAt { get; set; }
    }

}
