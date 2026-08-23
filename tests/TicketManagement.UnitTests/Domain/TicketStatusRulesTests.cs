using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Rules;

namespace TicketManagement.UnitTests.Domain;

public class TicketStatusRulesTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Closed, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved, false)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Open, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Closed, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Closed, TicketStatus.InProgress, false)]
    [InlineData(TicketStatus.Closed, TicketStatus.Resolved, false)]
    public void CanTransition_EnforcesStateMachine(TicketStatus from, TicketStatus to, bool expected)
    {
        TicketStatusRules.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void CanTransition_SameStatus_IsAlwaysAllowed()
    {
        foreach (TicketStatus status in Enum.GetValues<TicketStatus>())
        {
            TicketStatusRules.CanTransition(status, status).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed, true)]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Closed, false)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, false)]
    public void CanCustomerTransition_OnlyAllowsClosingAResolvedTicket(TicketStatus from, TicketStatus to, bool expected)
    {
        TicketStatusRules.CanCustomerTransition(from, to).Should().Be(expected);
    }
}
