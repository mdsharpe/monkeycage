using System.Text;

namespace MonkeyCage.MonkeyBusiness
{
    internal class Monkey
    {
        private static readonly char[] KnownCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        public KeyHittingResult HitKeys(string targetText, CancellationToken cancellationToken)
        {
            targetText = new string(
                targetText
                    .Where(o => KnownCharacters.Contains(o, CaseInsensitiveCharComparer.Shared))
                    .ToArray());

            var buf = new StringBuilder();
            var lcs = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                buf.Append(KnownCharacters[Random.Shared.Next(KnownCharacters.Length)]);

                if (buf.Length % 1000 == 0)
                {
                    lcs = GetLongestCommonSubstring(targetText, buf);

                    if (lcs.Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }

            lcs = GetLongestCommonSubstring(targetText, buf);
            return new KeyHittingResult(targetText, lcs, buf.Length);
        }

        /// <summary>
        /// Adapted from https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_substring#Retrieve_the_Longest_Substring
        /// </summary>
        private string GetLongestCommonSubstring(string targetText, StringBuilder charsTyped)
        {
            if (string.IsNullOrEmpty(targetText) || charsTyped.Length == 0)
            {
                return string.Empty;
            }

            var num = new int[targetText.Length, charsTyped.Length];
            var maxlen = 0;
            var lastSubsBegin = 0;
            var result = new StringBuilder();

            for (var i = 0; i < targetText.Length; i++)
            {
                for (var j = 0; j < charsTyped.Length; j++)
                {
                    if (!CaseInsensitiveCharComparer.Shared.Equals(targetText[i], charsTyped[j]))
                    {
                        num[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                        {
                            num[i, j] = 1;
                        }
                        else
                        {
                            num[i, j] = 1 + num[i - 1, j - 1];
                        }

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            var thisSubsBegin = i - num[i, j] + 1;

                            if (lastSubsBegin == thisSubsBegin)
                            {
                                result.Append(targetText[i]);
                            }
                            else
                            {
                                lastSubsBegin = thisSubsBegin;
                                result.Length = 0;
                                result.Append(targetText.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}
