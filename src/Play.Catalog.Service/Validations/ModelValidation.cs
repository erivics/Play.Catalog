using FluentValidation;
using Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Validations
{
  public class CreateItemValidation : AbstractValidator<CreateItemDto>
  {
    public CreateItemValidation()
    {
        RuleFor(itemDto => itemDto.Name)
                .NotEmpty();
        

        RuleFor(itemDto => itemDto.Price)
                .GreaterThan(0);
                    
    }
  }

  public class UpdateItemValidation : AbstractValidator<UpdateItemDto>
  {
    public UpdateItemValidation()
    {
        RuleFor(itemDto => itemDto.Name)
                    .NotEmpty();

        RuleFor(itemDto => itemDto.Price)
                .GreaterThan(0);
                    
    }
  }
}