using Domain.Common;

namespace Domain.Entities;

public sealed class StoreCategory
{
    public Guid StoreId { get; private set; }
    public Guid CategoryId { get; private set; }

    private StoreCategory()
    {
    }

}