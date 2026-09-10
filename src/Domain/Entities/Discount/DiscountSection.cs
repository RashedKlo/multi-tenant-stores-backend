using Domain.Common;

namespace Domain.Entities;

public sealed class DiscountSection
{
    public Guid DiscountId { get; private set; }
    public Guid SectionId { get; private set; }

    private DiscountSection()
    {
    }


}