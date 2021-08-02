using System;
using System.Collections.Generic;
using System.Text;

namespace SerGOFinance.Models
{
    public class Incomes
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
        public Category Category { get; set; }
    }
}
