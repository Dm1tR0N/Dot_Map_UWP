using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dot_Map.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public object Reviews { get; set; } // или измените тип на соответствующий модели отзывов
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Photo { get; set; }
    }
}
