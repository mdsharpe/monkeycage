using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MonkeyCage.MonkeyBusiness
{
    public class Monkey
    {
        private static readonly int BufMaxLength = 10000;

        private readonly ILogger<Monkey> _logger;
        private readonly ImmutableArray<char> _knownCharacters;

        public Monkey(
            ILogger<Monkey> logger,
            string name,
            IEnumerable<char> knownCharacters)
        {
            _logger = logger;
            Name = name;
            _knownCharacters = knownCharacters.ToImmutableArray();
        }

        public string Name { get; }

        public KeyHittingResult HitKeys(string targetText, CancellationToken cancellationToken)
        {
            targetText = new string(
                targetText
                    .Where(o => _knownCharacters.Contains(o, CaseInsensitiveCharComparer.Shared))
                    .ToArray());

            _logger.LogDebug("{MonkeyName} hitting keys until '{NormalizedTargetText}' found.",
                Name,
                targetText);

            var buf = new StringBuilder();
            var lcs = string.Empty;
            var lcsBest = string.Empty;
            var targetFound = false;
            var totalCharsTypedCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                buf.Append(_knownCharacters[Random.Shared.Next(_knownCharacters.Length)]);

                if (buf.Length >= BufMaxLength)
                {
                    var charsTyped = buf.ToString();
                    totalCharsTypedCount += buf.Length;
                    buf.Length = 0;

                    lcs = GetLongestCommonSubstring(targetText, charsTyped);
                    lcsBest = lcs.Length > lcsBest.Length ? lcs : lcsBest;

                    if (lcs.Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("{MonkeyName} found the target text!", Name);
                        targetFound = true;
                        break;
                    }

                    var lcsIndex = charsTyped.LastIndexOf(lcs, StringComparison.OrdinalIgnoreCase);

                    if (lcsIndex == charsTyped.Length - lcs.Length)
                    {
                        buf.Append(lcs);
                    }
                }
            }
            
            totalCharsTypedCount += buf.Length;

            if (!targetFound)
            {
                lcs = GetLongestCommonSubstring(targetText, buf.ToString());
                lcsBest = lcs.Length > lcsBest.Length ? lcs : lcsBest;
            }

            _logger.LogDebug("{MonkeyName} found '{FoundText}' of '{NormalizedTargetText}' after {KeyPressCount} key presses.",
                Name,
                lcsBest,
                targetText,
                totalCharsTypedCount);

            return new KeyHittingResult(targetText, lcsBest, totalCharsTypedCount);
        }

        /// <summary>
        /// Adapted from https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_substring#Retrieve_the_Longest_Substring
        /// </summary>
        private string GetLongestCommonSubstring(string targetText, string charsTyped)
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
