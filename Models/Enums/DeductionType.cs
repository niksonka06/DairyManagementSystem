namespace DairyManagementSystem.Models.Enums
{
    // Predefined list, per business decision — not free-form text. Keeps
    // "Other Deductions" reportable/groupable rather than an arbitrary string.
    public enum DeductionType
    {
        Loan,
        Insurance,
        SocietyFee,
        Other
    }
}
