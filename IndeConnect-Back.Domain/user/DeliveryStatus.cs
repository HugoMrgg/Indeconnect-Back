namespace IndeConnect_Back.Domain.user;

public enum DeliveryStatus
{
    Pending,
    Preparing,
    Shipped,
    InTransit,
    OutForDelivery,
    Delivered,
    Cancelled
}