# Telegram notifications
Open **Account settings → Configure Telegram notifications**. Settings are separate for each account and local database location.

1. Create a Telegram bot with [@BotFather](https://t.me/BotFather), open your bot, and send `/start`. For a group, add the bot and send it a command there.
2. Paste the bot token. Use **Find chats**, explicitly select your destination, or enter its numeric chat ID. Discovery reads recent updates without acknowledging them; if the bot already uses a webhook or another updates consumer, enter the ID manually or use a dedicated bot.
3. Click **Send test** to send one message to that destination. Select **Enable Telegram notifications**, check the event types you want, and **Save**. Saving these settings is separate from the account settings Save button.

Available checkboxes:

| Notification | Trigger |
| --- | --- |
| Incoming attack | First observed attack flag; resets once a refresh sees no attack. It does not identify individual waves while the flag remains active. |
| Completed upgrades | An observed building level increases after an initial baseline; initial village loading sends no completion history. |
| Pending build queue dispatched or satisfied | The builder removes the last completed/already-started job. Active in-game construction can still be running. Manual queue deletion does not trigger this alert. |
| Storage nearly full | Any resource reaches 90% capacity; re-arms when all resources fall to 80% or less. |
| Low crop stocks | Crop falls to 10% of granary capacity or less; re-arms at 20%. This measures stocks, not starvation risk. |
| Account status | Account becomes online, paused, or offline; intermediate transitions and repeated states are suppressed. |
| Task failures and errors | A task stops after failure, or an Error/Fatal log is emitted. Only a summary is sent; repeated error summaries are limited to one per account per ten minutes. |
| AI plan applied | An AI plan was successfully written to the pending build queue, when the AI planner feature is installed. |

Notifications follow normal game-page refreshes while the bot is running; this feature does not add independent game polling and cannot monitor while the app is closed. Attack and resource alerts may fire on the first refresh after enabling or restarting. Paused accounts receive only events that are still observed or generated locally.

Tokens are stored in **Windows Credential Manager**, not account exports or JSON. Non-secret preferences are in `%LOCALAPPDATA%\TravianBotSharp\Telegram`. Messages include account name, server, relevant village and observation time. **Send silently** disables Telegram notification sound. **Disable / forget token** stops future queued alerts and removes the saved token; an already-sending message cannot be recalled.

Delivery uses a bounded in-memory queue, plain text, no paid broadcasts, and pacing of at least 3.1 seconds between attempts. Telegram rate-limit responses may be retried once using `retry_after` (up to five minutes), with settings checked again first. Authentication and ambiguous network failures are not retried automatically. Alerts older than ten minutes, over capacity, or from changed settings are skipped. Pending alerts do not survive app shutdown. Use **Delivery status** to see the latest delivery result; the **Send test** button verifies real delivery with your own token.

Protocol reference: [Telegram Bot API](https://core.telegram.org/bots/api#sendmessage), [Telegram message limits](https://core.telegram.org/bots/faq#my-bot-is-hitting-limits-how-do-i-avoid-this).
