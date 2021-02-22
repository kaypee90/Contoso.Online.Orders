using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ContosoOnlineOrders.Abstractions;
using ContosoOnlineOrders.Abstractions.Models;
using ContosoOnlineOrders.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoOnlineOrders.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiVersion("1.2")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AdminController : ControllerBase
    {
        public IStoreDataService StoreServices { get; }

        public AdminController(IStoreDataService storeServices)
        {
            StoreServices = storeServices;
        }

        [HttpGet("/orders", Name = nameof(GetOrders))]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await Task.FromResult(Ok(StoreServices.GetOrders()));
        }

        [HttpGet("/orders/{id}", Name = nameof(GetOrder))]
        public async Task <ActionResult<Order>> GetOrder([FromRoute] Guid id)
        {
            var order = StoreServices.GetOrder(id);
            ActionResult<Order> result = NotFound();

            if(order != null)
            {
                result = Ok(order);
            }

            return await Task.FromResult(result);
        }

        [HttpGet("/orders/{id}/checkInventory", Name = nameof(CheckInventory))]
        public async Task<ActionResult> CheckInventory([FromRoute] Guid id)
        {
            ActionResult result = NotFound();

            try
            {
                var inventory = StoreServices.CheckOrderInventory(id);
                if (inventory)
                {
                    result = Ok();
                }
            }
            catch
            {
                result = Conflict();
            }

            return await Task.FromResult(result);
        }

        [HttpGet("/orders/{id}/ship", Name = nameof(ShipOrder))]
        public async Task<ActionResult> ShipOrder([FromRoute] Guid id)
        {
            var shipResult = StoreServices.ShipOrder(id);
            ActionResult result = NotFound();

            if(shipResult)
            {
                result = Ok();
            }

            return await Task.FromResult(result);
        }

        [HttpPut("/products/{id}/checkInventory", Name = nameof(UpdateProductInventory))]
        public async Task<ActionResult> UpdateProductInventory([FromRoute] int id, 
            [FromBody] InventoryUpdateRequest request)
        {
            ActionResult result = NotFound();

            try
            {
                StoreServices.UpdateProductInventory(id, request.countToAdd);
                result = Ok();
            }
            catch
            {
                result = NotFound();
            }

            return await Task.FromResult(result);
        }

[HttpPost("/products", Name = nameof(CreateProduct))]
        public async Task<ActionResult<Product>> CreateProduct(
            [FromBody] CreateProductRequest request)
        {
            ActionResult<Product> result = NotFound();

            try
            {
                var newProduct = new Product(request.Id, request.Name, request.InventoryCount);
                StoreServices.CreateProduct(newProduct);
                result = Created($"/products/{request.Id}", newProduct);
            }
            catch
            {
                result = Conflict();
            }

            return await Task.FromResult(result);
        }

        [HttpGet("/low-inventory", Name = nameof(GetLowInventoryProducts))]
        [MapToApiVersion("1.2")]
        public async Task<ActionResult<Product>> GetLowInventoryProducts()
        {
            var products = StoreServices.GetProducts().Where(_ => _.InventoryCount <= 5);
            return await Task.FromResult(Ok(products));
        }
    }
}