using System.Security.Cryptography;

namespace DairyManagementSystem.Helpers
{
    // Generates a one-time temporary password for newly created farmer
    // accounts. Guarantees the result satisfies Identity's policy configured
    // in Program.cs (8+ chars, at least one uppercase, at least one digit) —
    // if that policy ever changes, this must be updated to match, or new
    // farmer accounts will fail to create with a confusing Identity error.
    public static class TemporaryPasswordGenerator
    {
        private const string Lowercase = "abcdefghijkmnpqrstuvwxyz"; // no 'l','o' — avoid confusion with 1/0
        private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no 'I','O'
        private const string Digits = "23456789"; // no 0/1 — avoid confusion with O/l

        public static string Generate()
        {
            // One guaranteed char from each required category, then fill the
            // rest randomly, then shuffle — guarantees policy compliance
            // regardless of where in the string each character landed.
            var chars = new List<char>
            {
                PickRandom(Uppercase),
                PickRandom(Digits),
                PickRandom(Lowercase),
                PickRandom(Lowercase)
            };

            const string allAllowed = Lowercase + Uppercase + Digits;
            for (var i = 0; i < 4; i++)
            {
                chars.Add(PickRandom(allAllowed));
            }

            // Fisher-Yates shuffle using a cryptographically secure RNG —
            // this is a real credential, not a cosmetic ID.
            for (var i = chars.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars.ToArray());
        }

        private static char PickRandom(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}
