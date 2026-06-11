namespace ECommercePoc.Application.Dtos;

public sealed class OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "USD";
    public List<OrderItemDto> Items { get; init; } = [];
}
