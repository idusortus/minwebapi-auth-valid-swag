using FluentValidation;
using MinimalApi_Auth_Valid.Models;

namespace MinimalApi_Auth_Valid.Validation;

public class CustomerValidator : AbstractValidator<Customer>
{
	public CustomerValidator()
	{
		RuleFor(x => x.FullName).NotEmpty().MinimumLength(2);
	}
}
