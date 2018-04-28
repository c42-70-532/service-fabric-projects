using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductCatalog.Model;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace ECommerce.ProductCatalog.Repository
{
    class ServiceFabricProductRepository : IProductRepository
    {
        private readonly IReliableStateManager reliableStateManager;

        public ServiceFabricProductRepository(IReliableStateManager reliableStateManager)
        {
            this.reliableStateManager = reliableStateManager;
        }

        public async Task AddProduct(Product product)
        {
            var products = await reliableStateManager.GetOrAddAsync<IReliableDictionary<Guid, Product>>("products");
            
            using (var tx = reliableStateManager.CreateTransaction())
            {
                await products.AddOrUpdateAsync(tx, product.Id, product, (id, value) => product);
                await tx.CommitAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            var nullableProductDictionary = await reliableStateManager.TryGetAsync<IReliableDictionary<Guid, Product>>("products");
            var result = new List<Product>();

            if (!nullableProductDictionary.HasValue) return result;

            var productDictionary = nullableProductDictionary.Value;

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var enumerator = (await productDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    result.Add(enumerator.Current.Value);
                }
            }

            return result;
        }

        public async Task<Product> GetProduct(Guid productId)
        {
            var nullableProductDictionary = await reliableStateManager.TryGetAsync<IReliableDictionary<Guid, Product>>("products");

            if (!nullableProductDictionary.HasValue) return null;

            var productDictionary = nullableProductDictionary.Value;

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var product = await productDictionary.TryGetValueAsync(tx, productId);

                return product.Value;
            }
        }
    }
}
