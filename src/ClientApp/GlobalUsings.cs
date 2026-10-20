// Every ViewModel needs these -- matches the old MAUI GlobalUsings.cs, which globally imported
// both rather than repeating the using in all 14 ViewModel files.
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using eShop.Basket.API.Grpc;
global using BasketGrpcClient = eShop.Basket.API.Grpc.Basket.BasketClient;
// eShop.Basket.API.Grpc (imported above for BasketGrpcClient) has its own protobuf-generated
// BasketItem, colliding with the domain model of the same name -- same fix pattern as the
// BasketGrpcClient alias above, so every file can use the bare name for the model type.
global using BasketItem = eShop.ClientApp.Models.Basket.BasketItem;
