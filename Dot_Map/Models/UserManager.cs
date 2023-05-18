using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dot_Map.Models
{
    /// <summary>
    /// Менеджер пользователей.
    /// </summary>
    internal static class UserManager
    {
        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public static User CurrentUser { get; set; }
    }

}
