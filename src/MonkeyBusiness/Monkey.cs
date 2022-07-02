using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace MonkeyCage.MonkeyBusiness
{
    public class Monkey
    {
        private readonly ILogger<Monkey> _logger;
        private readonly ImmutableArray<char> _knownCharacters;
        private readonly ISystemClock _clock;

        public Monkey(
            ILogger<Monkey> logger,
            string name,
            IEnumerable<char> knownCharacters,
            ISystemClock clock)
        {
            _logger = logger;
            Name = name;
            _knownCharacters = knownCharacters.ToImmutableArray();
            _clock = clock;
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
            var lastCheckTime = _clock.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                buf.Append(_knownCharacters[Random.Shared.Next(_knownCharacters.Length)]);

                if (_clock.UtcNow.Subtract(lastCheckTime) >= TimeSpan.FromSeconds(5))
                {
                    lcs = GetLongestCommonSubstring(targetText, buf);

                    if (lcs.Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("{MonkeyName} found the target text!", Name);
                        break;
                    }

                    lastCheckTime = _clock.UtcNow;
                }
            }

            lcs = GetLongestCommonSubstring(targetText, buf);

            _logger.LogDebug("{MonkeyName} found '{FoundText}' of '{NormalizedTargetText}' after {KeyPressCount} key presses.",
                Name,
                lcs,
                targetText,
                buf.Length);

            return new KeyHittingResult(targetText, lcs, buf.Length);
        }

        // TODO This takes way too long when charsTyped gets large
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
