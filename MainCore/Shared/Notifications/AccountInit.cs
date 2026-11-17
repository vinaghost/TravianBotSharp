namespace MainCore.Shared.Notifications;

public record AccountInit(AccountId AccountId) : IAccountNotification;