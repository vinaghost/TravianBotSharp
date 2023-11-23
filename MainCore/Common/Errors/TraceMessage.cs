using FluentResults;
using System.Runtime.CompilerServices;

namespace MainCore.Common.Errors
{
    public class TraceMessage : Error
    {
        public TraceMessage(string message) : base(message)
        {
        }

        public static string Line(
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            return $"from file: {sourceFilePath} [{sourceLineNumber - 1}]";
        }
    }
}