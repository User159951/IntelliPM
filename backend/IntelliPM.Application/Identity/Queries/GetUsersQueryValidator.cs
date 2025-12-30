using FluentValidation;

namespace IntelliPM.Application.Identity.Queries;

/// <summary>
/// Validator for GetUsersQuery.
/// </summary>
public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    private static readonly string[] ValidSortFields = 
    {
        "Username",
        "Email",
        "CreatedAt",
        "LastLoginAt",
        "Role",
        "IsActive"
    };

    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.SortField)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortField))
            .WithMessage($"SortField must be one of: {string.Join(", ", ValidSortFields)}");
    }

    private static bool BeValidSortField(string? sortField)
    {
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return true; // Optional field
        }

        var normalizedField = sortField.Trim();
        
        // Check exact match (case-insensitive)
        return ValidSortFields.Any(field => 
            string.Equals(field, normalizedField, StringComparison.OrdinalIgnoreCase) ||
            string.Equals("CreatedAt", normalizedField, StringComparison.OrdinalIgnoreCase) ||
            string.Equals("LastLoginAt", normalizedField, StringComparison.OrdinalIgnoreCase) ||
            string.Equals("GlobalRole", normalizedField, StringComparison.OrdinalIgnoreCase));
    }
}

