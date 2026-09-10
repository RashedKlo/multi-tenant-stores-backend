using Domain.Common;

namespace Domain.Entities;

public sealed class DiscountProduct
{
    public Guid DiscountId { get; private set; }
    public Guid ProductId { get; private set; }

    private DiscountProduct()
    {
    }

 
}