using eShop.EventBus.Events;

namespace eShop.OrderProcessor.Events
{
    public record GracePeriodConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
}
