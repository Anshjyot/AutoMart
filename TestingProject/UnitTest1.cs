using Microsoft.AspNetCore.Mvc;
using Moq;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsAViewResult_WithAListOfItems()
    {
        // Arrange
        var mockRepo = new Mock<ISomeRepository>();
        mockRepo.Setup(repo => repo.GetAll()).Returns(GetTestItems());
        var controller = new HomeController(mockRepo.Object);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Item>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
    }

    private List<Item> GetTestItems()
    {
        return new List<Item>
        {
            new Item{ /* ... properties ... */ },
            new Item{ /* ... properties ... */ }
        };
    }
}
