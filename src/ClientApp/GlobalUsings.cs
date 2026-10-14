global using BasketGrpcClient = eShop.Basket.API.Grpc.Basket.BasketClient;
global using eShop.Basket.API.Grpc;
// eShop.Basket.API.Grpc (imported above for BasketGrpcClient) has its own protobuf-generated
// BasketItem, colliding with the domain model of the same name -- same fix pattern as the
// BasketGrpcClient alias above, so every file can use the bare name for the model type.
global using BasketItem = eShop.ClientApp.Models.Basket.BasketItem;
