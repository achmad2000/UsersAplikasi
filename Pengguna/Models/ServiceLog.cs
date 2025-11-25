using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class ServiceLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int WaitingResponOrderId { get; set; }

        [ForeignKey("WaitingResponOrderId")]
        public virtual WaitingResponOrder WaitingResponOrder { get; set; }

        [Display(Name = "Waktu Mulai Service")]
        public DateTime? TimeStart { get; set; }

        [Display(Name = "Waktu Selesai Service")]
        public DateTime? TimeStop { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Tagihan")]
        public decimal TotalHarga { get; set; }

        [Display(Name = "Status Pembayaran")]
        public string? StatusPembayaran { get; set; } 
        public virtual ICollection<ServiceLogDetail> ServiceLogDetails { get; set; }
        public string? BuktiPembayaranPath { get; set; }

        public ServiceLog()
        {
            ServiceLogDetails = new HashSet<ServiceLogDetail>();
        }
    }
}