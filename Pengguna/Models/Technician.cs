using System.ComponentModel.DataAnnotations;

namespace Pengguna.Models
{
    public class Technician
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }
    }
}
