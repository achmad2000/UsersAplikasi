using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } 

        [Phone]
        public string PhoneNumber { get; set; }
        public string? ProfileImagePath { get; set; }

        // Relasi dengan Order
        [InverseProperty("Customer")]
        public ICollection<OrderModel>? CustomerOrders { get; set; }

        [InverseProperty("Technician")]
        public ICollection<OrderModel>? TechnicianOrders { get; set; }
    }
}
