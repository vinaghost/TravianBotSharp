using Serilog.Events;

namespace MainCore.Infrastructure
{
    /// <summary>
    /// Configuration for command logging behavior.
    /// Controls which commands are logged and at which log level.
    /// </summary>
    public sealed class CommandLoggingConfig
    {
        /// <summary>
        /// Commands excluded entirely from logging (e.g., "Update", "Delay").
        /// </summary>
        public string[] ExcludedCommands { get; set; } = new[]
        {
            "Update",
            "Delay",
            "NextExecute"
        };

        /// <summary>
        /// Commands logged at Debug level instead of Information.
        /// Empty by default (logs everything at Information level).
        /// Add command name patterns here to reduce noise.
        /// Examples: "ToBuildingByType", "SwitchVillage", "Navigate"
        /// </summary>
        public string[] DebugLevelCommands { get; set; } = new[]
        {
            "ToBuildingByType",
            "SwitchVillage",
            "Navigate",
            "ToNpcResourcePage",
            "GetTrainTroopBuilding",
            "GetBuildPlanCommand"
        };

        /// <summary>
        /// Default log level when no specific rule applies.
        /// </summary>
        public LogEventLevel DefaultLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        /// Enable/disable command logging entirely.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Determines the log level for a given command name.
        /// </summary>
        public LogEventLevel GetLogLevel(string commandFullName)
        {
            if (!Enabled) return LogEventLevel.Debug; // Will be filtered out by Serilog

            var simpleName = commandFullName
                .Replace("MainCore.", "")
                .Replace("+Command", "");

            if (DebugLevelCommands.Any(pattern => simpleName.Contains(pattern)))
                return LogEventLevel.Debug;

            return DefaultLevel;
        }

        /// <summary>
        /// Determines if a command should be logged at all.
        /// </summary>
        public bool ShouldLog(string commandFullName)
        {
            if (!Enabled) return false;

            var simpleName = commandFullName
                .Replace("MainCore.", "")
                .Replace("+Command", "");

            return !ExcludedCommands.Any(pattern => simpleName.Contains(pattern));
        }
    }
}
