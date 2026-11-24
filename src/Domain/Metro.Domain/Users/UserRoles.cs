namespace Metro.Domain.Users
{
    public static class UserRoles
    {
        public const string Passenger = "Passenger";
        public const string Staff = "Staff";

        public const string ValidationMessage = "Role must be Passenger or Staff.";

        public static bool IsValid(string? role)
            => role is Passenger or Staff;
    }
}
