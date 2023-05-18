using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dot_Map.Models
{
    /// <summary>
    /// Класс, представляющий отзыв о месте.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Идентификатор отзыва.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Комментарий в отзыве.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Рейтинг, оценка отзыва.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Идентификатор пользователя, оставившего отзыв.
        /// </summary>
        public int User { get; set; }

        /// <summary>
        /// Идентификатор места, к которому относится отзыв.
        /// </summary>
        public int PlaceId { get; set; }
    }
}
