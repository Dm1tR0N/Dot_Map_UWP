﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Dot_Map.Models;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;

namespace Dot_Map.Servises
{
    /// <summary>
    /// Сервис пользователей.
    /// </summary>
    public class UserService
    {
        private HttpClient httpClient;

        /// <summary>
        /// Конструктор класса UserService.
        /// </summary>
        public UserService()
        {
            httpClient = new HttpClient();         
        }

        /// <summary>
        /// Получает список пользователей.
        /// </summary>
        /// <returns>Список пользователей.</returns>
        public async Task<List<User>> GetUsers()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5071/api/users");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(responseBody);

                return users;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
