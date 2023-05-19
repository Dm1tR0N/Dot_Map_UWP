using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Dot_Map.Models;
using Dot_Map.Servises;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using Windows.UI.Xaml.Media;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dot_Map.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Sing : Page
    {
        private NotificationManager notificationManager;
        public Sing()
        {
            this.InitializeComponent();
            notificationManager = new NotificationManager(notificationControl);
        }

        /// <summary>
        /// Метод для аутентификации пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>Объект User, представляющий аутентифицированного пользователя. Если аутентификация не удалась, возвращает null.</returns>
        private async Task<User> AuthenticateUser(string username, string password)
        {
            try
            {
                var userService = new UserService();
                var users = await userService.GetUsers();

                // Поиск пользователя с заданным именем пользователя
                var user = users.FirstOrDefault(u => u.Username == username);

                // Проверка пароля пользователя
                if (user != null && user.Password == password)
                {
                    return user;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки входа.
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            User authenticatedUser = await AuthenticateUser(username, password);

            if (authenticatedUser != null)
            {
                // Авторизация успешна
                UserManager.CurrentUser = authenticatedUser;

                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                notificationManager.ShowNotification("Ошибка", "Возможно сервер недоступен.", new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 174, 16, 49)));
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Вернутся".
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private void back_Click(object sender, RoutedEventArgs e)
        {
            var frame = new Frame();

            frame.Navigate(typeof(MainPage));

            var currentWindow = Window.Current.Content as Frame;

            Window.Current.Content = frame;

            Window.Current.Activate();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Регистрация.
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private async void RegButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationStackPanel.Visibility = Visibility.Visible;
            LoginStackPanel.Visibility = Visibility.Collapsed;
            EmailTextBox.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Регистрирует нового пользователя путем отправки POST-запроса к API.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="email">Адрес электронной почты</param>
        /// <returns>True, если регистрация прошла успешно; False, если пользователь с таким
        private async Task<bool> RegisterUser(string username, string password, string email)
        {
            try
            {
                // Создание объекта HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // URL адрес для отправки POST-запроса
                    string url = "http://localhost:5071/api/users";

                    // Создание объекта User для отправки
                    User newUser = new User()
                    {
                        Username = username,
                        Password = password,
                        Email = email
                    };

                    // Преобразование объекта User в JSON-строку
                    string json = JsonConvert.SerializeObject(newUser);

                    // Создание контент-объекта с JSON-строкой
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Отправка POST-запроса
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Проверка статуса ответа
                    if (response.IsSuccessStatusCode)
                    {
                        return true; // Регистрация успешна
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        return false; // Пользователь с таким логином уже существует
                    }
                    else
                    {
                        return false; // Регистрация не удалась по другой причине
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки регистрации.
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text;

            bool isRegistered = await RegisterUser(username, password, email);

            if (isRegistered)
            {
                RegistrationStackPanel.Visibility = Visibility.Collapsed;
                LoginStackPanel.Visibility = Visibility.Visible;
                EmailTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                notificationManager.ShowNotification("Ошибка", "Регистрация не удалась!", new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 174, 16, 49)));
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Авторизироватся".
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private void LoginUpButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationStackPanel.Visibility = Visibility.Collapsed;
            LoginStackPanel.Visibility = Visibility.Visible;
            EmailTextBox.Visibility= Visibility.Collapsed;
        }
    }
}
