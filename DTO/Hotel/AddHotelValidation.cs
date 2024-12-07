using FluentValidation;

namespace Hotel_Backend_API.DTO.Hotel
{
    public class AddHotelValidation : AbstractValidator<AddHotel>
    {
        public AddHotelValidation()
        {
            RuleFor(hotel => hotel.Name).NotNull().WithMessage("the name should not empty");
            RuleFor(hotel => hotel.Phone).NotNull().WithMessage("the Phone should not empty");
            RuleFor(hotel => hotel.Email).NotNull().WithMessage("the Email should not empty");
            RuleFor(hotel => hotel.Stars).NotNull().WithMessage("the Stars should not empty");
            RuleFor(hotel => hotel.Address).NotNull().WithMessage("the Address should not empty");

            RuleFor(x => x.Stars)
             .InclusiveBetween(1, 4);

            RuleFor(hotel => hotel.Phone).MinimumLength(9).WithMessage("the length of phone should be 9");
            RuleFor(hotel => hotel.Phone).MaximumLength(9).WithMessage("the length of phone should be 9");


        }

    }
}
