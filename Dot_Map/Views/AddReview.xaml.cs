using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dot_Map.Models;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dot_Map.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AddReview : Page
    {
        public int userId;
        public int pointTag;

        // Контроллер для уведомлений пользователю
        private NotificationManager notificationManager;
        public AddReview()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Вызывается при переходе на страницу.
        /// </summary>
        /// <param name="e">Данные события, описывающие, как была достигнута эта страница.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Проверяем, что переданы параметры
            if (e.Parameter is Dictionary<string, object> parameters)
            {
                // Извлекаем значения переменных
                if (parameters.TryGetValue("userId", out object userIdValue))
                {
                    userId = (int)userIdValue;
                }

                if (parameters.TryGetValue("pointTag", out object pointTagValue))
                {
                    pointTag = (int)pointTagValue;
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Добавить отзыв".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (rating.SelectedItem as ComboBoxItem != null && reviewBox.Text != "")
            {
                ComboBoxItem selectedItem = rating.SelectedItem as ComboBoxItem;
                string selectedValue = selectedItem?.Tag?.ToString();
                _ = AddReview_Func(userId, pointTag, reviewBox.Text, Convert.ToInt32(selectedValue));
            }
            else
            {
                notificationManager.ShowNotification("Ошибка при добавлении отзыва.", "Не все поля были заполнены.", new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 174, 16, 49)));
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Вернутся".
        /// </summary>
        /// <param name="sender">Объект, инициирующий событие</param>
        /// <param name="e">Аргументы события</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = new Frame();

            frame.Navigate(typeof(MainPage));

            var currentWindow = Window.Current.Content as Frame;

            Window.Current.Content = frame;

            Window.Current.Activate();
        }

        /// <summary>
        /// Обработчик события изменения текста в поле отзыва.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void reviewBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int characterCount = reviewBox.Text.Length;

            if (characterCount >= 500)
            {
                messageLenght.Text = "Ошибка! Превышен лимит символов (500).";
            }
            else
            {
                messageLenght.Text = $"Ваш отзыв содержит {reviewBox.Text.Length} символов.";
            }
        }

        /// <summary>
        /// Метод для добавления отзыва о месте.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="pointTag">Идентификатор места.</param>
        /// <param name="reviewBox">Текст отзыва.</param>
        /// <param name="rating">Рейтинг.</param>
        public async Task AddReview_Func(int userId, int pointTag, string reviewBox, int rating)
        {
            // Создание объекта HttpClient
            using (HttpClient client = new HttpClient())
            {
                // URL адрес для отправки POST-запроса
                string url = "http://localhost:5071/api/places/reviews";

                // Создание объекта Review для отправки
                Review review = new Review()
                {
                    Id = 0,
                    Comment = reviewBox,
                    Rating = rating,
                    User = userId,
                    PlaceId = pointTag
                };

                // Преобразование объекта Review в JSON-строку
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(review);

                // Создание контент-объекта с JSON-строкой
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправка POST-запроса
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Обработка ответа
                if (response.IsSuccessStatusCode)
                {
                    var frame = new Frame();

                    frame.Navigate(typeof(MainPage));

                    var currentWindow = Window.Current.Content as Frame;

                    Window.Current.Content = frame;

                    Window.Current.Activate();
                }
                else
                {
                    //notificationManager.ShowNotification("Ошибка при добавлении отзыва.", "Код ошибки: " + response.StatusCode, new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 174, 16, 49)));
                }
            }
        }
    }
}
