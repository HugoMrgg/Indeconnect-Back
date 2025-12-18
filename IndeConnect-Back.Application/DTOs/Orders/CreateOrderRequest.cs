namespace IndeConnect_Back.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public long ShippingAddressId { get; set; }
    public List<DeliveryChoiceDto> DeliveryChoices { get; set; } = new();
}