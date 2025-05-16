using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MainCore.Parsers
{
    public class BrokenParserException : Exception
    {
        private BrokenParserException(string message) : base(message)
        {
        }

        public static BrokenParserException NotFound(string? name) => new($"Element '{name}' not found.");

        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw BrokenParserException.NotFound(paramName);
            }
        }

        public static void ThrowIfEmpty(IEnumerable<object?> argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (!argument.Any())
            {
                throw BrokenParserException.NotFound(paramName);
            }
        }
    }
}