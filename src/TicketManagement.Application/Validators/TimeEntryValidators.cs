using FluentValidation;
using TicketManagement.Application.TimeEntries.Dtos;

namespace TicketManagement.Application.Validators;

public class CreateTimeEntryRequestValidator : AbstractValidator<CreateTimeEntryRequest>
{
    public CreateTimeEntryRequestValidator()
    {
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 24 * 60);
        RuleFor(x => x.WorkDate).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Work date cannot be in the future.");
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
