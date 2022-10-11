﻿using MainCore.Models.Runtime;
using System;

namespace MainCore.Services
{
    public class EventManager : IEventManager
    {
        public event Action AccountsTableUpdate;

        public void OnAccountsTableUpdate() => AccountsTableUpdate?.Invoke();

        public event Action<int> AccountStatusUpdate;

        public void OnAccountStatusUpdate(int accountId) => AccountStatusUpdate?.Invoke(accountId);

        public event Action<int, LogMessage> LogUpdated;

        public void OnLogUpdated(int accountId, LogMessage logMessage) => LogUpdated?.Invoke(accountId, logMessage);

        public event Action<int> TaskUpdated;

        public void OnTaskUpdated(int accountId) => TaskUpdated?.Invoke(accountId);

        public event Action<int> TaskExecuted;

        public void OnTaskExecuted(int accountId) => TaskExecuted?.Invoke(accountId);

        public event Action<int> VillagesUpdated;

        public void OnVillagesUpdated(int accountId) => VillagesUpdated?.Invoke(accountId);

        public event Action<int> HeroInfoUpdate;

        public void OnHeroInfoUpdate(int accountId) => HeroInfoUpdate?.Invoke(accountId);

        public event Action<int> HeroInventoryUpdate;

        public void OnHeroInventoryUpdate(int accountId) => HeroInventoryUpdate?.Invoke(accountId);

        public event Action<int> HeroAdventuresUpdate;

        public void OnHeroAdventuresUpdate(int accountId) => HeroAdventuresUpdate?.Invoke(accountId);

        public event Action<int> FarmListUpdated;

        public void OnFarmListUpdated(int accountId) => FarmListUpdated?.Invoke(accountId);
    }
}