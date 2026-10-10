using BookMVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BooksMVC.Tests
{
    public class BooksControllerTests
    {
        private BooksController CreateControllerWithRepo(Mock<IBookRepository> mockRepo)
        {
            return new BooksController(mockRepo.Object);
        }

        [Fact]
        public void GetAll_ReturnsListOfBooks()
        {
            var books = new List<Book>
        {
            new Book{ Id = 1, Title = "A", Author = "X", Year = 2000 },
            new Book{ Id = 2, Title = "B", Author = "Y", Year = 2010 }
        };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetAll()).Returns(books);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<Book>>(ok.Value);
            Assert.Equal(2, returned.Count());
        }

        [Fact]
        public void GetAll_ReturnsEmptyList_WhenNoBooks()
        {
            var empty = new List<Book>();
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetAll()).Returns(empty);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<Book>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public void GetById_ReturnsBook_WhenExists()
        {
            var book = new Book { Id = 1, Title = "Exists", Author = "A", Year = 1999 };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(1)).Returns(book);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.GetById(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Book>(ok.Value);
            Assert.Equal(book.Id, returned.Id);
            Assert.Equal(book.Title, returned.Title);
        }

        [Fact]
        public void GetById_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(42)).Returns((Book?)null);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_AddsNewBook_WhenValid()
        {
            var newBook = new Book { Title = "New Book", Author = "Auth", Year = 2021 };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.Add(It.IsAny<Book>()))
                .Callback<Book>(b => { b.Id = 123; }); // simulate repository assigning Id
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Create(newBook);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(BooksController.GetById), created.ActionName);
            var returned = Assert.IsType<Book>(created.Value);
            Assert.Equal(123, returned.Id);
            mock.Verify(r => r.Add(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public void Create_ReturnsBadRequest_WhenTitleEmpty()
        {
            var badBook = new Book { Title = " ", Author = "Auth", Year = 2020 };
            var mock = new Mock<IBookRepository>();
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Create(badBook);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            mock.Verify(r => r.Add(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public void Update_UpdatesExistingBook()
        {
            var existing = new Book { Id = 5, Title = "Old", Author = "A", Year = 2000 };
            var updated = new Book { Id = 5, Title = "Updated", Author = "B", Year = 2022 };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(5)).Returns(existing);
            mock.Setup(r => r.Update(It.IsAny<Book>()));
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Update(5, updated);
            Assert.IsType<NoContentResult>(result);
            mock.Verify(r => r.Update(It.Is<Book>(b => b.Id == 5 && b.Title == "Updated" && b.Author == "B" && b.Year == 2022)), Times.Once);
        }

        [Fact]
        public void Update_ReturnsNotFound_WhenMissing()
        {
            var updated = new Book { Id = 99, Title = "X", Author = "Y", Year = 2000 };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(99)).Returns((Book?)null);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Update(99, updated);
            Assert.IsType<NotFoundResult>(result);
            mock.Verify(r => r.Update(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public void Delete_RemovesExistingBook()
        {
            var existing = new Book { Id = 8, Title = "ToDelete", Author = "Z", Year = 2015 };
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(8)).Returns(existing);
            mock.Setup(r => r.Delete(8));
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Delete(8);
            Assert.IsType<NoContentResult>(result);
            mock.Verify(r => r.Delete(8), Times.Once);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenMissing()
        {
            var mock = new Mock<IBookRepository>();
            mock.Setup(r => r.GetById(7)).Returns((Book?)null);
            var controller = CreateControllerWithRepo(mock);
            var result = controller.Delete(7);
            Assert.IsType<NotFoundResult>(result);
            mock.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }
    }
}
