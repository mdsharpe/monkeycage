using System.Diagnostics.CodeAnalysis;

namespace MonkeyCage.MonkeyBusiness
{
    internal class CaseInsensitiveCharComparer : IEqualityComparer<char>
    {
        public static CaseInsensitiveCharComparer Shared = new();

        public bool Equals(char x, char y)
        {
            return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        }

        public int GetHashCode([DisallowNull] char obj)
        {
            return char.ToUpperInvariant(obj).GetHashCode();
        }
    }
}
