using Application.Common.Interfaces;
using Application.Features.Support.Commands.StartConversation;
using Application.Features.Support.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests;

public class SupportConversationHandlerTests
{
    [Fact]
    public async Task StartConversation_WhenOpenConversationExists_ReturnsConversationDtoWithoutCreatingDuplicate()
    {
        var customerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var existing = SupportConversation.Create(tenantId, customerId).Value!;

        var conversations = Substitute.For<ISupportConversationRepository>();
        conversations.GetOpenByCustomerAndTenantAsync(customerId, tenantId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var user = Substitute.For<ICurrentUserService>();
        user.IsAuthenticated.Returns(true);
        user.CustomerId.Returns(customerId);

        var handler = new StartConversationHandler(conversations, user);

        var result = await handler.Handle(new StartConversationCommand(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(existing.Id);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Status.Should().Be("Open");

        await conversations.DidNotReceive().AddAsync(Arg.Any<SupportConversation>(), Arg.Any<CancellationToken>());
    }
}
