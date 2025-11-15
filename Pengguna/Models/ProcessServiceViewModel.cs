using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pengguna.Models
{
    public class ProcessServiceViewModel
    {
        // 1. Data service log yang sedang diproses
        public ServiceLog ServiceLog { get; set; }

        // 2. Daftar dropdown (diisi dari ServiceItems)
        public IEnumerable<SelectListItem> AvailableItems { get; set; }

        // 3. Properti untuk form "Tambah Item"
        [Display(Name = "Barang / Jasa")]
        public int NewItemId { get; set; } // Akan diisi oleh ID dari dropdown

        [Display(Name = "Jumlah")]
        [Range(1, 100, ErrorMessage = "Jumlah minimal 1")]
        public int NewItemQuantity { get; set; } = 1;
    }
}