using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.API.Model;
using ECommerce.ProductCatalog.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductCatalogService catalogService;

        public ProductController()
        {
            catalogService = ServiceProxy.Create<IProductCatalogService>(new Uri("fabric:/ECommerce/ECommerce.ProductCatalog"), new ServicePartitionKey(0));
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<ApiProduct>> Get()
        {

            //return new [] { new ApiProduct { Id = Guid.NewGuid(), Description = "fake" } };

            var products = await catalogService.GetAllProducts();

            return products.Select(p => new ApiProduct
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.Availablity > 0
            });
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]ApiProduct apiProduct)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = apiProduct.Name,
                Description = apiProduct.Description,
                Price = apiProduct.Price
            };

            await catalogService.AddProduct(product);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
