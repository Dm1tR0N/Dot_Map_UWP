using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dot_Map.Models
{
    /// <summary>
    /// Класс, представляющий информацию о месте.
    /// </summary>
    public class Place
    {
        /// <summary>
        /// Идентификатор места.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название места.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание места.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Коллекция отзывов о месте.
        /// </summary>
        public object Reviews { get; set; }

        /// <summary>
        /// Широта координаты места.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Долгота координаты места.
        /// </summary>
        public double Longitude { get; set; }
    }

}
