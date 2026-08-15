namespace DairyManagementSystem.Helpers
{
    public static class FormBindingHelpers
    {
        public static bool ParseBoolFormValue(string? value) =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
