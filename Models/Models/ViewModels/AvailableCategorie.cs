using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models.ViewModels
{
    public class AvailableCategorie
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}
