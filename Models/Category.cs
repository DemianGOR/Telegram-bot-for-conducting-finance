using System;
using System.Collections.Generic;
using System.Text;

namespace SerGOFinance.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public List<Outcomes> Outcomes { get; set; }
        public List<Incomes> Incomes { get; set; }
    }
}
