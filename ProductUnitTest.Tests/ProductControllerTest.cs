using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductUnitTest.Test.Controllers;
using ProductUnitTest.Test.Models;
using ProductUnitTest.Test.Repository;

namespace ProductUnitTest.Tests
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsController _controller;

        private List<Product> products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            products = new List<Product>()
            {
                new Product() { Id = 1, Name = "Ceket", Price = 1250, Stock = 25, Color = "Lacivert" },
                new Product() { Id = 2, Name = "Jean", Price = 1000, Stock = 30, Color = "Gri" }
            };
        }
        [Fact]
        public async Task Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Index_ActionExecutes_ReturnProductList()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);
            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(2, productList.Count());
        }

        [Fact]
        public async Task Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

        }

        [Fact]
        public async Task Details_InValid_ReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);
            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);

        }

        [Theory]
        [InlineData(1)]
        public async Task Details_ValidId_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(product.Id)).ReturnsAsync(product);
            var result = await _controller.Details(product.Id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsType<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_InvalidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name is required");
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async Task Create_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_ValidModelState_CreateMethodExecutes()
        {
            Product newProduct = null;
            _mockRepo.Setup(x => x.Create(It.IsAny<Product>()))
                .Callback<Product>(x => newProduct = x);
            await _controller.Create(products.First());
            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Once);
            Assert.Equal(products.First().Id, newProduct.Id);
            Assert.Equal(products.First().Name, newProduct.Name);
        }

        [Fact]
        public async Task CreatePOST_InvalidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "");
            var result = await _controller.Create(products.First());
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(3)]
        public async void Edit_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ValidId_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsType<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<NotFoundResult>(result);

        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "");
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {
            Product updateProduct = null;
            _mockRepo.Setup(x => x.Update(It.IsAny<Product>()))
                .Callback<Product>(x => updateProduct = x);
            _controller.Edit(productId, products.First(x => x.Id == productId));
            _mockRepo.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
            Assert.Equal(products.First().Id, updateProduct.Id);
            Assert.Equal(products.First().Name, updateProduct.Name);
        }

        [Fact]
        public async Task Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _controller.Delete(null);
            
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsType<Product>(viewResult.Model);
            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteConfirmed_ActionExecutes_ReturnRedirectToIndexAction(int productId)
        {
           
            var product = new Product { Id = productId, Name = "Test Product", Price = 100, Stock = 10, Color = "Red" };
            _mockRepo.Setup(r => r.GetById(productId)).ReturnsAsync(product);

            
            var result = await _controller.DeleteConfirmed(productId);

           
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int productId)
        {
            
            var product = new Product { Id = productId, Name = "Test Product", Price = 100, Stock = 10, Color = "Red" };
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product); 

            
            await _controller.DeleteConfirmed(productId);

            
            _mockRepo.Verify(repo => repo.Delete(productId), Times.Once);
        }






    }
}
