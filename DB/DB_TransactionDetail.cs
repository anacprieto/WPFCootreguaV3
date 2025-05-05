using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB
{
    public class DB_TransactionDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TranDetailId { get; set; }
        public int IdApi { get; set; }
        public int IdTransaction { get; set; }
        public int IdCurrencyDenomination { get; set; }
        public int CurrencyDenomination { get; set; }
        public int IdTypeOperation { get; set; }
        public string TypeOperation { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set;}
    }
}
