namespace eShop.WebBFF.Services
{
    public record BasketQuantity(int ProductId, int Quantity);

    public class BasketService(BasketGrpcClient basketClient)
    {
        public async Task<IReadOnlyCollection<BasketQuantity>> GetBasketAsync(string accessToken)
        {
            var response = await basketClient.GetBasketAsync(new GetBasketRequest(), BuildCallOptions(accessToken));
            return MapItems(response);
        }

        public async Task<IReadOnlyCollection<BasketQuantity>> UpdateBasketAsync(string accessToken, IReadOnlyCollection<BasketQuantity> items)
        {
            var request = new UpdateBasketRequest();
            foreach (var item in items)
            {
                request.Items.Add(new BasketItem { ProductId = item.ProductId, Quantity = item.Quantity });
            }

            var response = await basketClient.UpdateBasketAsync(request, BuildCallOptions(accessToken));
            return MapItems(response);
        }

        public async Task DeleteBasketAsync(string accessToken)
        {
            await basketClient.DeleteBasketAsync(new DeleteBasketRequest(), BuildCallOptions(accessToken));
        }

        private static IReadOnlyCollection<BasketQuantity> MapItems(CustomerBasketResponse response) =>
            response.Items.Select(item => new BasketQuantity(item.ProductId, item.Quantity)).ToList();

        private static CallOptions BuildCallOptions(string accessToken) =>
            new(headers: new Metadata { { "Authorization", $"Bearer {accessToken}" } });
    }
}
