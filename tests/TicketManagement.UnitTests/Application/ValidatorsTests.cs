using TicketManagement.Application.TimeEntries.Dtos;
using TicketManagement.Application.Tickets.Dtos;
using TicketManagement.Application.Validators;

namespace TicketManagement.UnitTests.Application;

public class ValidatorsTests
{
    [Fact]
    public void CreateTicketRequestValidator_RejectsTooShortTitle()
    {
        var validator = new CreateTicketRequestValidator();
        var result = validator.Validate(new CreateTicketRequest("ab", "A valid description here"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateTicketRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateTicketRequestValidator();
        var result = validator.Validate(new CreateTicketRequest("Valid title", "A valid description here"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTimeEntryRequestValidator_RejectsFutureWorkDate()
    {
        var validator = new CreateTimeEntryRequestValidator();
        var request = new CreateTimeEntryRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 30, "desc");

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryRequest.WorkDate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1441)]
    public void CreateTimeEntryRequestValidator_RejectsOutOfRangeDuration(int minutes)
    {
        var validator = new CreateTimeEntryRequestValidator();
        var request = new CreateTimeEntryRequest(DateOnly.FromDateTime(DateTime.UtcNow), minutes, "desc");

        validator.Validate(request).IsValid.Should().BeFalse();
    }
}
