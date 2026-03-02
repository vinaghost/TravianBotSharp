using MainCore.Commands.Features;
using MainCore.Enums;
using MainCore.Services;
using MainCore.Entities;
using Xunit;

namespace MainCore.Test.Services
{
    public class SleepCommandTests
    {
        private class FakeSettings : ISettingService
        {
            private readonly Dictionary<object, int> _settings = new();

            public void Set(AccountSettingEnums setting, int value) => _settings[$"A_{setting}"] = value;

            public bool BooleanByName(AccountId accountId, AccountSettingEnums setting) => throw new NotImplementedException();
            public bool BooleanByName(VillageId villageId, VillageSettingEnums setting) => throw new NotImplementedException();
            public int ByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1)
            {
                return _settings.TryGetValue($"A_{settingMin}", out var v) ? v : 0;
            }
            public int ByName(AccountId accountId, AccountSettingEnums setting)
            {
                return _settings.TryGetValue($"A_{setting}", out var v) ? v : 0;
            }
            public Dictionary<VillageSettingEnums, int> ByName(VillageId villageId, List<VillageSettingEnums> settings) => throw new NotImplementedException();
            public int ByName(VillageId villageId, VillageSettingEnums setting) => throw new NotImplementedException();
            public int ByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1) => throw new NotImplementedException();
        }

        [Fact]
        public void CalculateSleepDuration_DoesNotExceedNextStart()
        {
            var fake = new FakeSettings();
            var account = new AccountId(1);

            // choose a work start a couple minutes in the future
            var now = DateTime.Now;
            var future = now.AddMinutes(3);
            fake.Set(AccountSettingEnums.WorkStartHour, future.Hour);
            fake.Set(AccountSettingEnums.WorkStartMinute, future.Minute);

            // choose a very large sleep value so it would normally overshoot
            fake.Set(AccountSettingEnums.SleepTimeMin, 1000);
            fake.Set(AccountSettingEnums.SleepTimeMax, 1000);

            int result = SleepCommand.CalculateSleepDurationMinutes(fake, account);
            // should be no more than about 3 minutes
            Assert.InRange(result, 0, 5);
        }

        [Fact]
        public void CalculateSleepDuration_UsesRandomWithinBounds()
        {
            var fake = new FakeSettings();
            var account = new AccountId(1);

            // arbitrary work start far in future so it doesn't cap
            fake.Set(AccountSettingEnums.WorkStartHour, 23);
            fake.Set(AccountSettingEnums.WorkStartMinute, 59);

            fake.Set(AccountSettingEnums.SleepTimeMin, 5);
            fake.Set(AccountSettingEnums.SleepTimeMax, 5);

            int result = SleepCommand.CalculateSleepDurationMinutes(fake, account);
            Assert.Equal(5, result);
        }
    }
}