using NUnit.Framework;
using System.Threading.Tasks;
using Dot_Map.Models;
using Dot_Map.Views;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        //[Test]
        //public async Task SearchCity_WithValidQueryAndTypeSimple_ShouldSetMapCenterAndShowNotification()
        //{
        //    // Arrange
        //    var mapSearch = new Dot_Map.Views.MainPage();
        //    var searchQuery = "London";
        //    var typeSearch = "simple";

        //    // Act
        //    await mapSearch.SearchCity(searchQuery, typeSearch);

        //    // Assert
        //    Assert.AreEqual(10, mapSearch.MapControl.ZoomLevel);
        //    // Add assertions for other expected changes to mapControl and notificationManager
        //}

        //[Test]
        //public async Task SearchCity_WithValidQueryAndTypeStartPoint_ShouldSetStartPoint()
        //{
        //    // Arrange
        //    var mapSearch = new MapSearch();
        //    var searchQuery = "New York";
        //    var typeSearch = "startPoint";

        //    // Act
        //    await mapSearch.SearchCity(searchQuery, typeSearch);

        //    // Assert
        //    // Add assertions to verify that startPoint is set correctly
        //}

        //[Test]
        //public async Task SearchCity_WithValidQueryAndTypeEndPoint_ShouldSetEndPoint()
        //{
        //    // Arrange
        //    var mapSearch = new MapSearch();
        //    var searchQuery = "Paris";
        //    var typeSearch = "endPoint";

        //    // Act
        //    await mapSearch.SearchCity(searchQuery, typeSearch);

        //    // Assert
        //    // Add assertions to verify that endPoint is set correctly
        //}

        //[Test]
        //public async Task SearchCity_WithInvalidQuery_ShouldShowErrorNotification()
        //{
        //    // Arrange
        //    var mapSearch = new MapSearch();
        //    var searchQuery = "InvalidCity";
        //    var typeSearch = "simple";

        //    // Act
        //    await mapSearch.SearchCity(searchQuery, typeSearch);

        //    // Assert
        //    // Add assertions to verify that the error notification is shown and searchTextBox is cleared
        //}

        //[Test]
        //public async Task SearchCity_WithEmptyQuery_ShouldShowEmptyQueryNotification()
        //{
        //    // Arrange
        //    var mapSearch = new MapSearch();
        //    var searchQuery = "";
        //    var typeSearch = "simple";

        //    // Act
        //    await mapSearch.SearchCity(searchQuery, typeSearch);

        //    // Assert
        //    // Add assertions to verify that the empty query notification is shown
        //}
    }
}