using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SerGOFinance.Models
{
   public  class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int Balance { get; set; }
     
        

        public virtual ICollection<Incomes> Income { get; set; }
        public virtual ICollection<Outcomes> Outcome { get; set; }
    }
}
