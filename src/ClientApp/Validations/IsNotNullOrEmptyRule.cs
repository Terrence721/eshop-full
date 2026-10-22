namespace eShop.ClientApp.Validations
{
    public class IsNotNullOrEmptyRule<T> : IValidationRule<T>
    {
        public string ValidationMessage { get; set; } = null!;

        public bool Check(T value)
        {
            return value is string str &&
                   !string.IsNullOrWhiteSpace(str);
        }
    }
}
