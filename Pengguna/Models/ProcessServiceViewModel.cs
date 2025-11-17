using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Pastikan ini ada

namespace Pengguna.Models
{
    public class ProcessServiceViewModel
    {
        public ServiceLog ServiceLog { get; set; }

        // [DIGANTI] Kita tidak lagi pakai AvailableItems
        // public IEnumerable<SelectListItem> AvailableItems { get; set; }

        // [BARU] Ini untuk Dropdown 1 (Jenis Service)
        public IEnumerable<SelectListItem> JenisServiceList { get; set; }

        // [BARU] Ini untuk data Dropdown 2 (Tindakan), dikirim sebagai JSON
        public string AllItemsJson { get; set; }

        // Properti untuk form "Tambah Item"
        [Display(Name = "Tindakan / Perbaikan")]
        [Required(ErrorMessage = "Pilih tindakan")]
        public int NewItemId { get; set; } // Akan diisi oleh ID dari dropdown 2

        [Display(Name = "Jumlah")]
        [Range(1, 100, ErrorMessage = "Jumlah minimal 1")]
        public int NewItemQuantity { get; set; } = 1;
    }
}