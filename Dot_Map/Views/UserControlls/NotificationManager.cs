using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Dot_Map
{
    public class NotificationManager
    {
        private UserControl notificationControl;

        public NotificationManager(UserControl control)
        {
            notificationControl = control;
        }

        /// <summary>
        /// Отображает уведомление.
        /// </summary>
        /// <param name="title">Заголовок уведомления</param>
        /// <param name="content">Содержимое уведомления</param>
        /// <param name="color">Цвет фона уведомления</param>
        public void ShowNotification(string title, string content, SolidColorBrush color)
        {
            // Установка текстовых значений в контрол уведомления
            TextBlock titleTextBlock = notificationControl.FindName("titleTextBlock") as TextBlock;
            TextBlock contentTextBlock = notificationControl.FindName("contentTextBlock") as TextBlock;
            Grid grid = notificationControl.FindName("BoxNotification") as Grid;

            titleTextBlock.Text = title;
            contentTextBlock.Text = content;
            grid.Background = color;

            // Получаем кнопку "Скрыть"
            Button closeButton = notificationControl.FindName("Close") as Button;

            // Устанавливаем обработчик события Click для кнопки "Скрыть"
            closeButton.Click += (sender, e) =>
            {
                // Скрываем контрол уведомления
                notificationControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            };

            // Изменение цвета кнопки closeButton
            Color darkerColor = Color.FromArgb((byte)(color.Color.A * 0.7), (byte)(color.Color.R * 0.7), (byte)(color.Color.G * 0.7), (byte)(color.Color.B * 0.7));
            closeButton.Background = new SolidColorBrush(darkerColor);

            // Отображение контрола уведомления
            notificationControl.Visibility = Windows.UI.Xaml.Visibility.Visible;

            // Здесь можно добавить анимацию или другие эффекты для плавного отображения уведомления

            // Задержка на отображение уведомления
            Task.Delay(10000).ContinueWith(t =>
            {
                // Скрытие уведомления
                _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    notificationControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                });
            });
        }
    }
}
