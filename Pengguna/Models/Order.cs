using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        // Relasi ke Customer
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        [InverseProperty("CustomerOrders")]
        public UserModel? Customer { get; set; }

        // Relasi ke Technician
        public int? TechnicianId { get; set; }

        [ForeignKey("TechnicianId")]
        [InverseProperty("TechnicianOrders")]
        public UserModel? Technician { get; set; }

        public string Status { get; set; } // Pending, InProgress, Done
    }
}
