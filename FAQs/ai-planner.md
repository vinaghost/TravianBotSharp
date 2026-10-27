# AI village planner
Open a village's **Build** tab and choose **AI planner**. Log in through the bot first, then pause the account and let its current task finish.

1. Choose **OpenRouter** or **OpenAI**, enter your API key, and click **Test key / refresh models**. OpenRouter defaults to `openrouter/free` with **Free models only** enabled; no automatic paid fallback is used. Free model availability and quotas depend on OpenRouter. OpenAI calls use your API balance.
2. Choose **New village**, **Offensive Army**, **Defensive Army**, **Conqueror**, or **Production**. New village prepares the **current village to settle another village**. Conqueror prepares buildings for catapults and chiefs; research, training, and settlement are listed as follow-up actions.
3. Set server speed, capital status, upgrade limit, and planning horizon. Add relevant details such as culture points, occupied expansion slots, settlers, existing troops, research, or resource support. These are not inferred when unknown.
4. Select **Append after existing jobs** or **Replace pending jobs**, then **Analyze village**. Review the ordered upgrades, costs, reasons, validation notes, and sources. Analysis does not apply the proposed queue.
5. Click **Apply queue** to refresh and validate the village again, then write the approved jobs. Changed or expired previews must be analyzed again. Active construction is preserved. Resume the account when ready. **Undo apply** restores the previous pending queue while this window remains open, provided those jobs have not been executed or edited.

The request includes village buildings, resources, production when available, queues, relevant execution settings, and excerpts from the [second-village guides](https://unofficialtravian.com/2025/01/guides-fast-second-village/) and [Travian Manual](https://travianmanual.wordpress.com/). Sources are cached for seven days; unavailable sources use an explicitly labelled cached excerpt or bundled summary. The historical manual is background context, not authoritative current rules.

API keys stay in memory unless **Remember key on this Windows account** is selected, which stores them in Windows Credential Manager. Non-secret preferences and guide caches are under `%LOCALAPPDATA%\TravianBotSharp\AiPlanner`. Keys are excluded from plan records. The selected provider receives the gameplay context and your additional notes.

The planner checks supported building IDs, slots, prerequisites, tribe/capital restrictions, individual levels, and storage capacity. Costs and funding estimates use the bot's rule tables and current production; they are not a complete simulation or a guarantee of optimal timing. Culture points, used expansion slots, research, training, construction durations, and special server bonuses need manual review. Existing gold, NPC, hero-resource, and troop-training settings still govern execution.

Developer checks: run `dotnet run --project MainCore.Test/MainCore.Test.csproj -c Release` (the test project uses Microsoft Testing Platform). Provider tests use mocked HTTP responses; no API key or paid completion is needed.
