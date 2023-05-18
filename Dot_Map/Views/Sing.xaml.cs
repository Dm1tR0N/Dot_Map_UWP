using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Dot_Map.Models;
using Dot_Map.Servises;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dot_Map.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Sing : Page
    {
        public Sing()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Метод для аутентификации пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>Объект User, представляющий аутентифицированного пользователя. Если аутентификация не удалась, возвращает null.</returns>
        private async Task<User> AuthenticateUser(string username, string password)
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
                ErrorMessageTextBlock.Text = "Авторизация не удалась!";
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
    }
}
