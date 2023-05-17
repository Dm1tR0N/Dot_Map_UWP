using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dot_Map.Models;
using Newtonsoft.Json;


namespace Dot_Map.Servises
{
    public class UserService
    {
        private HttpClient httpClient;

        public UserService()
        {
            httpClient = new HttpClient();
        }

        public async Task<List<User>> GetUsers()
        {
            var response = await httpClient.GetAsync("http://localhost:5071/api/users");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(responseBody);

            return users;
        }
    }

}
