using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductCatalog.Model;
using ECommerce.ProductCatalog.Repository;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ECommerce.ProductCatalog
{

    internal sealed class ProductCatalog : StatefulService, IProductCatalogService
    {
        private IProductRepository productRepository;

        public ProductCatalog(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddProduct(Product product)
        {
            await productRepository.AddProduct(product);
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await productRepository.GetAllProducts();
        }

        public async Task<Product> GetProduct(Guid productId)
        {
            return await productRepository.GetProduct(productId);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
            };
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            productRepository = new ServiceFabricProductRepository(this.StateManager);

            // fake products

            var product1 = new Product { Id = Guid.NewGuid(), Name = "Product 1", Description = "Product 1 Desc", Price = 34.87, Availablity = 2 };
            var product2 = new Product { Id = Guid.NewGuid(), Name = "Product 2", Description = "Product 2 Desc", Price = 44.87, Availablity = 4 };
            var product3 = new Product { Id = Guid.NewGuid(), Name = "Product 3", Description = "Product 3 Desc", Price = 54.87, Availablity = 5 };

            await productRepository.AddProduct(product1);
            await productRepository.AddProduct(product2);
            await productRepository.AddProduct(product3);

            var savedProducts = await productRepository.GetAllProducts();
        }
    }
}
