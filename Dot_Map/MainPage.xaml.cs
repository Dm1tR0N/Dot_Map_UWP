using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dot_Map.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace Dot_Map
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Geolocator geolocator;
        private MapIcon locationIcon;

        // Для построения маршрута
        private Geopoint startPoint;
        private Geopoint endPoint;

        // Контроллер для уведомлений пользователю
        private NotificationManager notificationManager;

        // Цвета
        public SolidColorBrush RED_Notification     = new SolidColorBrush( Color.FromArgb(125, 6, 16, 1) );
        public SolidColorBrush GREEN_Notification   = new SolidColorBrush( Color.FromArgb(255, 48, 92, 63) );
        public SolidColorBrush BLUE_Notification    = new SolidColorBrush( Color.FromArgb(44, 68, 100, 1) );

        // Хранение текущего местоположения
        public Geopoint myGeopoint;

        // Для хранения данных об авторезированном пользователе
        public int userId;

        public MainPage()
        {
            this.InitializeComponent();
            mapControl.MapServiceToken = "35wISN8sOtCWorow7xE8~kFqkQNroOLGF4n0qIdTLfA~AqSmv4QThH7uxnbScEHHguCNdvVVsHlvfRiZzMqgtPJAAGrIlKaJn0SEKAMizS9q";
            SetCurrentLocation();
            mapControl.ZoomLevel = 12; // Установите желаемый уровень масштабирования карты
            mapControl.Style = MapStyle.Road;
            this.Loaded += MainPage_Loaded;

            geolocator = new Geolocator();
            locationIcon = new MapIcon();

            // Настройка метки
            locationIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/LocationIcon.png")); // Измените путь к изображению на свое
            locationIcon.NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 0.5);

            // Добавление метки на карту
            mapControl.MapElements.Add(locationIcon);

            // Подписка на событие изменения местоположения
            geolocator.PositionChanged += Geolocator_PositionChanged;


            // Запуск получения местоположения
            GetLocation();
            // menuGrid.SizeChanged += MenuGrid_SizeChanged;
            //MenuGrid_SizeChanged();
            LoadPlaces();
        }

        /// <summary>
        /// Метод для загрузки меток на карту.
        /// </summary>
        public async void LoadPlaces()
        {
            HttpClient client = new HttpClient();
            string url = "http://localhost:5071/api/places";
            string imageUrl = @"ms-appx:///Assets/mapIcon.png";
            RandomAccessStreamReference imageReference = RandomAccessStreamReference.CreateFromUri(new Uri(imageUrl));

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // Десериализация полученного JSON-ответа в список мест
                List<Place> places = JsonConvert.DeserializeObject<List<Place>>(responseBody);

                // Очистка карты от существующих меток
                //mapControl.MapElements.Clear();

                // Расстановка меток на карту
                foreach (Place place in places)
                {
                    BasicGeoposition location = new BasicGeoposition
                    {
                        Latitude = place.Latitude,
                        Longitude = place.Longitude
                    };

                    Geopoint point = new Geopoint(location);

                    // Создание метки на основе данных места
                    MapIcon mapIcon = new MapIcon
                    {
                        Tag = place.Id,
                        Location = point,
                        NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 0.5),
                        Title = $"{place.Name}",
                        Image = imageReference                  
                    };

                    // Добавление метки на карту
                    mapControl.MapElements.Add(mapIcon);
                }
            }
            catch (Exception ex)
            {
                notificationManager.ShowNotification("Ошибка", $"Не удалось отобразить метку на карте.\n{ex.Message}", RED_Notification);
            }
        }

        /// <summary>
        /// Обработчик события клика по элементу карты.
        /// </summary>
        private async void MapControl_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            // Проверка, является ли нажатый элемент меткой
            if (args.MapElements.FirstOrDefault() is MapIcon mapIcon)
            {
                // Получение идентификатора места из тега метки
                if (mapIcon.Tag is int placeId)
                {
                    // Формирование URL-адреса для запроса отзывов
                    string reviewsUrl = $"http://localhost:5071/api/places/{placeId}/reviews";

                    // Выполнение запроса к API для получения отзывов
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                    {
                        try
                        {
                            // Отправка GET-запроса и получение ответа
                            System.Net.Http.HttpResponseMessage response = await client.GetAsync(reviewsUrl);

                            // Проверка статуса ответа
                            if (response.IsSuccessStatusCode)
                            {
                                // Чтение содержимого ответа в виде строки
                                string responseContent = await response.Content.ReadAsStringAsync();

                                // Десериализация полученного JSON-ответа в объекты C#
                                List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(responseContent);

                                // Вывод информации о полученных отзывах или выполнение другой логики
                                // Например, можно отобразить информацию в диалоговом окне или на другом элементе пользовательского интерфейса
                                ShowReviews(reviews);

                                // Построение маршрута
                                await BuildRoute(mapIcon.Location);
                            }
                            else
                            {
                                HandleError("Кажется отзывы отсутсвтуют.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Обработка ошибки при выполнении запроса
                            HandleError(ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Построение маршрута на карте до указанного местоположения.
        /// </summary>
        /// <param name="destination">Местоположение назначения</param>
        private async Task BuildRoute(Geopoint destination)
        {
            // Получение координат начальной точки (можно использовать SearchCity или другой метод для ее получения)
            Geopoint startPoint = myGeopoint;

            if (startPoint != null && destination != null)
            {
                // Создаем объект маршрута
                MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(startPoint, destination);

                // Проверяем, удалось ли построить маршрут
                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    // Очищаем предыдущие маршруты на карте
                    mapControl.Routes.Clear();

                    // Отображаем маршрут на карте
                    MapRouteView routeView = new MapRouteView(routeResult.Route);
                    mapControl.Routes.Add(routeView);

                    // Задаем область отображения карты, чтобы охватить весь маршрут
                    await mapControl.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);


                    // Не актуально, т.к выводятся отзывы

                    // Показываем длину маршрута и примерное время езды
                    //double lengthInKilometers = routeResult.Route.LengthInMeters / 1000.0;
                    //TimeSpan estimatedDuration = routeResult.Route.EstimatedDuration;            
                    //ShowRouteInformation(lengthInKilometers, estimatedDuration);
                }
            }
        }

        /// <summary>
        /// Отображение отзывов о месте.
        /// </summary>
        /// <param name="reviews">Список отзывов</param>
        private void ShowReviews(List<Review> reviews)
        {
            string ListRewiev = "";
            int countReview = 0;
            double rating = 0;
            // Вывод информации о полученных отзывах
            foreach (Review review in reviews)
            {
                // Обработка отзыва
                ListRewiev += $"{countReview+1}: {review.Comment}\n";
                countReview++;
                rating += review.Rating;
            }      
            if (countReview > 1) { notificationManager.ShowNotification("Отзывы", $"{ListRewiev}Рейтинг места: {CalculateAverage(rating, countReview)}\n" , GREEN_Notification); }
            else if (countReview == 1) { notificationManager.ShowNotification("Отзыв", ListRewiev, GREEN_Notification); }
        }

        /// <summary>
        /// Вычисление среднего значения на основе суммы и количества элементов.
        /// </summary>
        /// <param name="sum">Сумма элементов</param>
        /// <param name="count">Количество элементов</param>
        /// <returns>Среднее значение</returns>
        public double CalculateAverage(double sum, int count)
        {
            if (count == 0)
                throw new ArgumentException("Count cannot be zero.");

            double average = (double)sum / count;
            return average;
        }

        /// <summary>
        /// Обработка ошибки и отображение уведомления.
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        private void HandleError(string errorMessage)
        {
            notificationManager.ShowNotification("Ошибка отзыва", $"Сообщение: {errorMessage}", RED_Notification);
        }

        /// <summary>
        /// Получение текущего местоположения.
        /// </summary>
        private async void GetLocation()
        {
            Geoposition geoposition = await geolocator.GetGeopositionAsync();
            UpdateLocation(geoposition.Coordinate.Point.Position);
        }

        /// <summary>
        /// Обработчик события изменения позиции геолокатора.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="args">Аргументы события</param>
        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            UpdateLocation(args.Position.Coordinate.Point.Position);
        }

        /// <summary>
        /// Обновление местоположения.
        /// </summary>
        /// <param name="position">Координаты местоположения</param>
        private async void UpdateLocation(BasicGeoposition position)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // mapControl.Center = new Geopoint(position);
                locationIcon.Location = new Geopoint(position);
                locationIcon.Title = $"Моё местоположение\n{position.Latitude}°, {position.Longitude}°";
                myGeopoint = new Geopoint(position);
            });
        }

        /// <summary>
        /// Установка текущего местоположения на центр карты.
        /// </summary>
        public async void SetCurrentLocation()
        {
            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = await geolocator.GetGeopositionAsync();

            double latitude = geoposition.Coordinate.Point.Position.Latitude;
            double longitude = geoposition.Coordinate.Point.Position.Longitude;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Установка центра карты на текущее местоположение
                mapControl.Center = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });
            });
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку поиска.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = searchTextBox.Text;
            SearchCity(searchQuery, "simple");
        }

        /// <summary>
        /// Поиск города.
        /// </summary>
        /// <param name="searchQuery">Запрос поиска</param>
        /// <param name="typeSearch">Тип поиска</param>
        /// <returns>Задача, представляющая операцию поиска города</returns>
        public async Task SearchCity(string searchQuery, string typeSearch)
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(searchQuery, null);

                if (result.Status == MapLocationFinderStatus.Success && result.Locations.Count > 0)
                {
                    BasicGeoposition location = new BasicGeoposition
                    {
                        Latitude = result.Locations[0].Point.Position.Latitude,
                        Longitude = result.Locations[0].Point.Position.Longitude
                    };

                    if (typeSearch == "simple") // Обычный поиск
                    {
                        mapControl.Center = new Geopoint(location);
                        mapControl.ZoomLevel = 10;
                        notificationManager.ShowNotification("Результат поиска.", $"Город {searchQuery} найден!\nКоординаты: {location.Latitude}, {location.Longitude}", GREEN_Notification);

                    }
                    else if (typeSearch == "startPoint") // Начальная точка
                    {
                        startPoint = new Geopoint(location);
                    }
                    else if (typeSearch == "endPoint") // Конечная точка
                    {
                        endPoint = new Geopoint(location);
                    }
                }
                else
                {
                    notificationManager.ShowNotification("Ошибка поиска!", "Кажется вы ввели неправильное название, или у вас нет интернет соединения!", RED_Notification);
                    searchTextBox.Text = "";
                }
            }
            else
            {
                // Пустой запрос поиска
                notificationManager.ShowNotification("Поиск невозможен.", "Введите название города!", RED_Notification);
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Применить".
        /// Применяет выбранный стиль к элементу управления картой.
        /// </summary>
        /// <param name="sender">Отправитель события</param>
        /// <param name="e">Аргументы события</param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedStyle = (ComboBoxItem)mapStyleComboBox.SelectedItem;
            if (selectedStyle != null)
            {
                string style = selectedStyle.Content.ToString();
                ChangeMapStyle(mapControl, style);
            }
        }

        /// <summary>
        /// Изменяет стиль карты.
        /// </summary>
        /// <param name="mapControl">Элемент управления картой.</param>
        /// <param name="mapStyle">Новый стиль карты.</param>
        public void ChangeMapStyle(MapControl mapControl, string mapStyle)
        {
            switch (mapStyle)
            {
                case "Aerial":
                    mapControl.Style = MapStyle.Aerial;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль Aerial установлен.", GREEN_Notification);
                    break;
                case "AerialWithRoads":
                    mapControl.Style = MapStyle.AerialWithRoads;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль AerialWithRoads установлен.", GREEN_Notification);
                    break;
                case "Road":
                    mapControl.Style = MapStyle.Road;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль Road установлен.", GREEN_Notification);
                    break;
                case "Terrain":
                    mapControl.Style = MapStyle.Terrain;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль Terrain установлен.", GREEN_Notification);
                    break;
                case "Aerial3D":
                    mapControl.Style = MapStyle.Aerial3D;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль Aerial3D установлен.", GREEN_Notification);
                    break;
                case "Aerial3DWithRoads":
                    mapControl.Style = MapStyle.Aerial3DWithRoads;
                    notificationManager.ShowNotification("Обновление карты.", "Стиль Aerial3DWithRoads установлен.", GREEN_Notification);
                    break;
                default:
                    // По умолчанию устанавливаем стиль "Road"
                    mapControl.Style = MapStyle.Road;
                    break;
            }
        }

        /// <summary>
        /// Построение маршрута между начальной и конечной точками.
        /// </summary>
        /// <returns>Асинхронная задача</returns>
        private async Task BuildRoute()
        {
            await SearchCity(startRoutePoint.Text, "startPoint");
            await SearchCity(endRoutePoint.Text, "endPoint");

            if (startPoint != null && endPoint != null)
            {
                // Создаем объект маршрута
                MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(startPoint, endPoint);

                // Проверяем, удалось ли построить маршрут
                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    // Очищаем предыдущие маршруты на карте
                    mapControl.Routes.Clear();

                    // Отображаем маршрут на карте
                    MapRouteView routeView = new MapRouteView(routeResult.Route);
                    mapControl.Routes.Add(routeView);

                    // Задаем область отображения карты, чтобы охватить весь маршрут
                    await mapControl.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);

                    // Показываем длину маршрута и примерное время езды
                    double lengthInKilometers = routeResult.Route.LengthInMeters / 1000.0;
                    TimeSpan estimatedDuration = routeResult.Route.EstimatedDuration;

                    ShowRouteInformation(lengthInKilometers, estimatedDuration);
                }
            }
        }

        /// <summary>
        /// Выводит информацию о маршруте, такую как длина маршрута и примерное время пути.
        /// </summary>
        /// <param name="lengthInKilometers">Длина маршрута в километрах</param>
        /// <param name="estimatedDuration">Примерное время пути</param>
        private void ShowRouteInformation(double lengthInKilometers, TimeSpan estimatedDuration)
        {
            try
            {
                string lengthString = string.Format("Длина маршрута: {0} км", lengthInKilometers.ToString("0.00"));
                string durationString = string.Format("Примерное время езды: {0} день, {1} часов, {2} минут ", estimatedDuration.ToString(@"dd"), estimatedDuration.ToString(@"hh"), estimatedDuration.ToString(@"mm"));

                // Выводим информацию о маршруте
                notificationManager.ShowNotification("Маршрут построен.", $"{lengthString}\n{durationString}", GREEN_Notification);
            }
            catch (Exception ex)
            {
                notificationManager.ShowNotification("Ошибка!", $"Непредвиденная ошибка!\n ERROR({ex.Message})", RED_Notification);
            }
        }

        /// <summary>
        /// Выполняется при загрузке главной страницы. Создает экземпляр NotificationManager 
        /// и передает пользовательский контрол для отображения уведомлений.
        /// </summary>
        /// <param name="sender">Инициатор события</param>
        /// <param name="e">Аргументы события</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Создание экземпляра NotificationManager и передача пользовательского контрола
            notificationManager = new NotificationManager(notificationControl);

            // Метод MainPage_Loaded вызывается при загрузке главной страницы.
            // Внутри метода создается экземпляр NotificationManager и передается 
            // пользовательский контрол notificationControl, который будет 
            // использоваться для отображения уведомлений.Документирующий комментарий<summary>
            // предоставляет описание метода и его функциональности для дальнейшего понимания и использования.
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку создания маршрута. Выполняет проверку заполненности полей начальной и конечной точек маршрута.
        /// Если поля заполнены, вызывает методы BuildRoute() и UpdateMap() для построения маршрута и обновления карты.
        /// Если поля не заполнены, выводит уведомление об ошибке.
        /// </summary>
        /// <param name="sender">Инициатор события</param>
        /// <param name="e">Аргументы события</param>
        private async void CreateRoute_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(startRoutePoint.Text) && !string.IsNullOrEmpty(endRoutePoint.Text))
            {
                await BuildRoute();
                await UpdateMap();
            }
            else
            {
                notificationManager.ShowNotification("Ошибка.", $"Заполните все поля!", RED_Notification);
            }
        }

        /// <summary>
        /// Метод для обновления карты. Получает текущий маршрут, если он доступен, и задает область отображения карты,
        /// чтобы охватить маршрут.
        /// </summary>
        /// <returns>Асинхронная задача</returns>
        private async Task UpdateMap()
        {
            // Получаем текущий маршрут, если он доступен
            if (mapControl.Routes.Count > 0)
            {
                MapRouteView routeView = mapControl.Routes[0];

                // Получаем границы маршрута
                GeoboundingBox routeBounds = routeView.Route.BoundingBox;

                // Задаем область отображения карты, чтобы охватить маршрут
                await mapControl.TrySetViewBoundsAsync(routeBounds, null, MapAnimationKind.None);
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Авторизация".
        /// </summary>
        /// <param name="sender">Отправитель события</param>
        /// <param name="e">Аргументы события</param>
        private void signIn_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = new Frame();
            Sing sing= new Sing();

            frame.Navigate(typeof(Sing));

            var currentWindow = Window.Current.Content as Frame;

            Window.Current.Content = frame;

            Window.Current.Activate();
        }

        /// <summary>
        /// Метод, вызываемый при переходе на эту страницу (Page).
        /// </summary>
        /// <param name="e">Аргументы перехода</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (UserManager.CurrentUser != null)
                userId = UserManager.CurrentUser.Id;           
        }
    }
}
