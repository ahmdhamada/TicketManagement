using FluentValidation;
using TicketManagement.Application.Tickets.Dtos;

namespace TicketManagement.Application.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).NotEmpty().Length(5, 4000);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateTicketDetailsRequestValidator : AbstractValidator<UpdateTicketDetailsRequest>
{
    public UpdateTicketDetailsRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).NotEmpty().Length(5, 4000);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
