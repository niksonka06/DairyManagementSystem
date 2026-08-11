namespace DairyManagementSystem.Helpers
{
    // Thrown for business-rule violations a service detects (e.g. "registration
    // number already in use") that aren't simple [Required]/[StringLength] model
    // validation — those are already caught by ModelState before the service
    // is even called. This lets controllers show a friendly, specific message
    // instead of a generic 500, without leaking exception internals to the user.
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}
