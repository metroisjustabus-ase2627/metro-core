namespace Metro.Domain.Auth
{
    public static class PasswordPolicy
    {
        public const string RequirementsMessage =
            "Password must be at least 8 characters long and include uppercase, lowercase, and numeric characters.";

        public static bool IsValid(string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return false;
            }

            var hasUpper = false;
            var hasLower = false;
            var hasDigit = false;

            foreach (var character in password)
            {
                if (char.IsUpper(character))
                {
                    hasUpper = true;
                }
                else if (char.IsLower(character))
                {
                    hasLower = true;
                }
                else if (char.IsDigit(character))
                {
                    hasDigit = true;
                }

                if (hasUpper && hasLower && hasDigit)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
