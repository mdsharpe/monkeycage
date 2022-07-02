using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace MonkeyCage.MonkeyBusiness
{
    public class MonkeyFactory
    {
        private static readonly ImmutableArray<char> KnownCharacters = "abcdefghijklmnopqrstuvwxyz".ToImmutableArray();

        private static readonly ImmutableArray<string> Names = new[]
        {
            "Romeo",
            "Juliet",
            "Ophelia",
            "Desdemona",
            "Claudius",
            "Polonius",
            "Mercutio",
            "Hamlet",
            "Macbeth",
            "Shylock",
            "Antony",
            "Cleopatra",
            "Beatrice",
            "Iago",
            "Falstaff",
            "Prospero",
            "Edmund",
            "Viola",
            "Titania",
            "Portia",
            "Cordelia",
            "Caliban",
            "Horatio",
            "Yorick",
            "Othello",
        }.ToImmutableArray();

        private static int _nextNameIndex = 0;
        private static object _nextNameLock = new object();

        private readonly IServiceProvider _serviceProvider;

        public MonkeyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Monkey Create()
        {
            return new Monkey(
                _serviceProvider.GetService<ILogger<Monkey>>(),
                GetNextName(),
                KnownCharacters,
                _serviceProvider.GetService<ISystemClock>());
        }

        private static string GetNextName()
        {
            string name;

            lock (_nextNameLock)
            {
                name = Names[_nextNameIndex];
                _nextNameIndex++;
                if (_nextNameIndex > Names.Length - 1)
                {
                    _nextNameIndex = 0;
                }
            }

            return name;
        }
    }
}
