using ECommerce.API.Model;
using ECommerce.UserActor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    public class BasketController : Controller
    {
        [HttpGet("{userId}")]
        public async Task<ApiBasket> Get(string userId)
        {
         
            IUserActor userActor = GetActor(userId);

            Dictionary<Guid, int> basketItems = await userActor.GetBasket();

            ApiBasket apiBasket = new ApiBasket() {
                UserId = userId,
                Items = basketItems.Select(i => new ApiBasketItem { ProductId = i.Key.ToString(), Quantity = i.Value}).ToArray()
            };

            return apiBasket;
        }

        [HttpPost("{userId}")]
        public async Task Add(string userId, [FromBody] ApiBasketAddRequest request)
        {
            IUserActor userActor = GetActor(userId);

            await userActor.AddToBasket(new Guid(request.ProductId), request.Quantity);
        }

        [HttpDelete("{userId}")]
        public async Task Delete(string userId)
        {
            IUserActor userActor = GetActor(userId);

            await userActor.ClearBasket();
        }

        private IUserActor GetActor(string userId)
        {
            return ActorProxy.Create<IUserActor>(
                new ActorId(userId),
                new Uri("fabric:/ECommerce/UserActorService"));
        }
    }
}
