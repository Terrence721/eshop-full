namespace eShop.Basket.API.IntegrationEvents.Events;

// Integration Events notes:
// An Event is "something that has happened in the past", therefore its name has to be
// An Integration Event is an event that can cause side effects to other microservices, Bounded-Contexts or external systems.
/// <summary>
/// Integration event raised when an order is started; carries the ordering user's id so the
/// basket can be cleared.
/// </summary>
public record OrderStartedIntegrationEvent(string UserId) : IntegrationEvent;
