using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using NUnit.Framework;

namespace MVCTests
{
    public class Tests
    {
        private static List<Person> _data = new List<Person>{
            new Person{
                Id = 1,
                FirstName = "Long",
                LastName = "Bao",
                Gender = "Male",
                DateOfBirth = new DateTime(1994,1,16),
                BirthPlace = "Bac Ninh",
                PhoneNumber = "0946616194",
                IsGraduated = false,
            },
            new Person{
                Id = 2,
                FirstName = "Hung",
                LastName = "Ngo Quoc",
                Gender = "Male",
                DateOfBirth = new DateTime(1991,3,7),
                BirthPlace = "Hai Phong",
                PhoneNumber = "0946616194",
                IsGraduated = false,
            }
        };

        private ILogger<RookiesController> _loggerMock;
        private Mock<IPersonService> _serviceMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = Mock.Of<ILogger<RookiesController>>();

            _serviceMock = new Mock<IPersonService>();
            _serviceMock.Setup(s => s.GetAll()).Returns(_data);
        }

        [Test]
        public void Index_ReturnAViewResult_WithAListOfPeople()
        {
            // Arrange
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var view = (ViewResult)result;
            Assert.IsAssignableFrom<List<Person>>(view.ViewData.Model);

            var list = (List<Person>)view.ViewData.Model;
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void Detail_ReturnsHttpNotFound_ForInputInvalid()
        {
            const int personId = 3;
            _serviceMock.Setup(s => s.GetOne(personId)).Returns((Person)null);
            // Arrange
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);

            // Act
            var result = controller.Detail(personId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void Detail_ReturnsAPerson()
        {
            // Arrange
            const int personId = 1;
            _serviceMock.Setup(service => service.GetOne(personId)).Returns(_data.First());
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);
            const string expectedFullName = "Bao Long";

            // Act
            var result = controller.Detail(personId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            Assert.IsAssignableFrom<Person>(((ViewResult)result).ViewData.Model);

            Assert.AreEqual(expectedFullName, ((Person)((ViewResult)result).ViewData.Model).FullName);
        }

        [Test]
        public void Create_ReturnsBadRequest_GivenInvalidModel()
        {
            // Arrange
            const string message = "some error";

            var controller = new RookiesController(_loggerMock, _serviceMock.Object);

            controller.ModelState.AddModelError("error", message);

            // Act
            var result = controller.Create(model: null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            Assert.IsAssignableFrom<SerializableError>(((BadRequestObjectResult)result).Value);

            var error = (SerializableError)((BadRequestObjectResult)result).Value;

            Assert.AreEqual(1, error.Count);

            error.TryGetValue("error", out var msg);
            Assert.IsNotNull(msg);
            Assert.AreEqual(message, ((string[])msg).First());
        }

        [Test]
        public void Create_ReturnRedirect_GivenValidModel()
        {
            // Arrange
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);
            Person newPerson = new Person
            {
                Id = 3,
                FirstName = "Nhien",
                LastName = "Hao",
                Gender = "Male",
                DateOfBirth = new DateTime(1995, 1, 16),
                BirthPlace = "Bac Ninh",
                PhoneNumber = "0946616194",
                IsGraduated = false,
            };

            // Act
            var result = controller.Create(newPerson);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Index", ((RedirectToActionResult)result).ActionName);
        }

        [Test]
        public void Edit_ReturnsBadRequest_GivenInValidId()
        {
            // Arrange
            const string message = "some error";

            var controller = new RookiesController(_loggerMock, _serviceMock.Object);

            controller.ModelState.AddModelError("error", message);

            // Act
            var result = controller.Edit(model: null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            Assert.IsAssignableFrom<SerializableError>(((BadRequestObjectResult)result).Value);

            var error = (SerializableError)((BadRequestObjectResult)result).Value;

            Assert.AreEqual(1, error.Count);

            error.TryGetValue("error", out var msg);
            Assert.IsNotNull(msg);
            Assert.AreEqual(message, ((string[])msg).First());
        }

        [Test]
        public void Edit_ReturnsRedirect_GivenInValidModel()
        {
            // Arrange
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);
            int personId = 9999;
            _serviceMock.Setup(service => service.GetOne(personId)).Returns((Person)_data.First());
            // Act
            var result = controller.Edit(personId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }


        [Test]
        public void Delete_ReturnsRedirect_GivenValidId()
        {
            // Arrange
            int personId = 1;
            _serviceMock.Setup(service => service.GetOne(personId)).Returns((Person)_data.First());
            _serviceMock.Setup(service => service.Delete(personId)).Returns(true);
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);

            // Act
            var result = controller.Delete(personId);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Result", ((RedirectToActionResult)result).ActionName);
        }

        [Test]
        public void Delete_ReturnsHttpNotFound_GivenInvalidId()
        {
            // Arrange
            var controller = new RookiesController(_loggerMock, _serviceMock.Object);
            int personId = 9999;
            _serviceMock.Setup(service => service.GetOne(personId)).Returns((Person)null);
            // Act
            var result = controller.Delete(personId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

    }
}