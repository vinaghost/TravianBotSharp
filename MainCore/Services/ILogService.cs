﻿using Serilog.Events;

namespace MainCore.Services
{
    public interface ILogService
    {
        LinkedList<LogEvent> GetLog(AccountId accountId);

        ILogger GetLogger(AccountId accountId);

        void Shutdown();
    }
}