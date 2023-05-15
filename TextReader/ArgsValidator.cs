using FluentValidation;

namespace TextReader;

public class ArgsValidator : AbstractValidator<string[]>
{
    public ArgsValidator()
    {
        RuleFor(x => x).Must(x => x.Length == 2).WithMessage("Incorrect number of arguments entered");

        When(x => x.Length > 0, () =>
        {
            RuleFor(x => x[0]).NotEmpty().WithMessage("No file path entered please enter a file path");
        });

        When(x => x.Length > 1, () =>
        {
            RuleFor(x => x[1]).Must(BeValidFilterOption).WithMessage("No arguments detected please enter a filtration option");
        });
    }

    private bool BeValidFilterOption(string arg)
    {
        return arg.All(c => new[] { 'v', 's', 't' }.Contains(c));
    }
}