using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Validations
{
    public class CustomPriceAttribute : ValidationAttribute
    {
        private readonly int _minPrice;
        private readonly int _maxPrice;

        public CustomPriceAttribute(int minPrice, int maxPrice)
        {
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return false;

            if (value is int price)
            {
                if (price >= _minPrice && price <= _maxPrice)
                    return true;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be between {_minPrice} and {_maxPrice}.";
        }

    }
}
