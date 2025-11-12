using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pengguna.Models
{
    public class ActiveOrder
    {
        public int Id { get; set; }
        public string NamaCustomer { get; set; }
        public string ItemService { get; set; }
        public string NoWA { get; set; }
        public string Alamat { get; set; }
        public string DeskripsiProblem { get; set; }
        public string TeknisiNama { get; set; }
        public DateTime TanggalAmbil { get; set; } = DateTime.Now;
    }

}
