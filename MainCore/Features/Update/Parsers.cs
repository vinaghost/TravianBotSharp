namespace MainCore.Features.Update;

public static class Parsers
{
    public record struct RawHeroItemDto(int ItemId, string? DataTier, int Amount);

    public static async Task<List<HeroItemDto>> GetHeroItems(this IPage page)
    {
        var rawItemsData = await page.EvaluateAsync<RawHeroItemDto[]>(@"() => {
            const cells = document.querySelectorAll('div.heroItems div.heroItem:not(.empty)');
            const result = [];
            const itemRegex = /item(\d+)/;

            cells.forEach(cell => {
                const itemSlot = cell.querySelector('.item');
                const countSlot = cell.querySelector('.count');
                const classes = itemSlot ? (itemSlot.getAttribute('class') || '') : '';
                const itemMatch = classes.match(itemRegex);

                if (!itemMatch) return;
                const itemId = parseInt(itemMatch[1], 10);
                if (isNaN(itemId)) return;

                const dataTier = cell.getAttribute('data-tier') || '';
                const isConsumable = dataTier.includes('consumable');

                let amount = 1;
                if (isConsumable && countSlot) {
                    const parsedAmt = parseInt(countSlot.innerText.trim(), 10);
                    if (!isNaN(parsedAmt) && parsedAmt > 0) {
                        amount = parsedAmt;
                    }
                }

                result.push({
                    ItemId: itemId,
                    DataTier: dataTier,
                    Amount: amount
                });
            });

            return result;
        }");

        var extractedItems = new List<HeroItemDto>();
        foreach (var raw in rawItemsData)
        {
            var item = Enum.IsDefined(typeof(HeroItemEnums), raw.ItemId) ? (HeroItemEnums)raw.ItemId : HeroItemEnums.None;
            if (item == HeroItemEnums.None) continue;

            extractedItems.Add(new HeroItemDto
            {
                Type = item,
                Amount = raw.Amount
            });
        }

        return extractedItems;
    }

    // The sidebar script embeds the page data as a JSON-compatible object literal after "viewData:".
    // The object is cut out by matching braces (skipping string contents) and parsed.
    private const string ReadViewDataScript = @"
            const readViewData = () => {
                const scripts = document.querySelectorAll('div#sidebarAfterContent div.sidebarBoxWrapper script');
                for (const script of scripts) {
                    const source = script.textContent || '';
                    const keyIndex = source.indexOf('viewData:');
                    if (keyIndex < 0) continue;

                    const start = source.indexOf('{', keyIndex);
                    if (start < 0) continue;

                    let depth = 0;
                    let inString = false;
                    for (let i = start; i < source.length; i++) {
                        const c = source[i];
                        if (inString) {
                            if (c === '\\') i++;
                            else if (c === '""') inString = false;
                        } else if (c === '""') {
                            inString = true;
                        } else if (c === '{') {
                            depth++;
                        } else if (c === '}' && --depth === 0) {
                            try {
                                return JSON.parse(source.substring(start, i + 1));
                            } catch {
                                return null;
                            }
                        }
                    }
                }
                return null;
            };
";

    public record struct AccountDataDto(AccountInfoDto AccountInfo, VillageDto[] Villages);

    public static async Task<(AccountInfoDto AccountInfo, List<VillageDto> Villages)> GetAccountData(this IPage page)
    {
        var data = await page.EvaluateAsync<AccountDataDto>("() => {" + ReadViewDataScript + @"
            const goldEl = document.querySelector('div.ajaxReplaceableGoldAmount');
            const silverEl = document.querySelector('div.ajaxReplaceableSilverAmount');
            const player = readViewData()?.ownPlayer;

            const parseDomInt = (el) => {
                if (!el) return 0;
                // Strip out non-numeric characters except minus signs (like localized thousand separators)
                const text = (el.innerText || el.textContent || '').replace(/[^\d-]/g, '');
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 0 : parsed;
            };

            const accountInfo = {
                Gold: parseDomInt(goldEl),
                Silver: parseDomInt(silverEl),
                HasPlusAccount: player?.goldFeatures?.travianPlus?.isActive === true,
                Tribe: player?.tribeId ?? 0
            };

            // names arrive HTML-encoded (e.g. &#39;)
            const decoder = document.createElement('textarea');
            const decode = (text) => {
                decoder.innerHTML = text ?? '';
                return decoder.value.trim();
            };

            const currentVillageId = player?.village?.id ?? 0;
            const villageById = new Map();

            // the list mixes groups (with nested villages) and ungrouped villages
            (player?.villageList ?? []).forEach(entry => {
                const items = entry && Array.isArray(entry.villages) ? entry.villages : [entry];
                items.forEach(village => {
                    if (!village || !village.id || villageById.has(village.id)) return;

                    villageById.set(village.id, {
                        Id: village.id,
                        Name: decode(village.name),
                        X: village.x ?? 0,
                        Y: village.y ?? 0,
                        IsActive: village.id === currentVillageId,
                        IsUnderAttack: (village.incomingAttacksAmount ?? 0) > 0
                    });
                });
            });

            return { AccountInfo: accountInfo, Villages: [...villageById.values()] };
        }");

        return (data.AccountInfo, [.. data.Villages]);
    }

    public record struct BuildingNameDto(string Name, int Type);
    public record struct BuildingDataDto(BuildingDto[] Buildings, QueueBuildingDto[] Queue, int Dorf, int Tribe);

    public static async Task<BuildingDataDto> GetBuildingData(this IPage page, IReadOnlyDictionary<string, int> buildingNames)
    {
        await page.WaitForSelectorAsync("#questmasterButton");
        var data = await page.EvaluateAsync<BuildingDataDto>("() => {" + ReadViewDataScript + @"
            const isDorf1 = document.querySelector('.village1') !== null;
            const isDorf2 = document.querySelector('.village2') !== null;
            const buildingsResult = [];
            const queueResult = [];

            const tribeId = readViewData()?.ownPlayer?.village?.tribeId ?? 0;

            const getWallType = (tId) => {
                switch(tId) {
                    case 1: return 31; // Romans: City Wall
                    case 2: return 32; // Teutons: Earth Wall
                    case 3: return 33; // Gauls: Palisade
                    case 6: return 42; // Egyptians: Stone Wall
                    case 7: return 43; // Huns: Makeshift Wall
                    case 8: return 47; // Spartan: Defensive Wall
                    default: return 0;
                }
            };

            if (isDorf1) {
                const elements = document.querySelectorAll('#resourceFieldContainer a.level');
                const locRegex = /buildingSlot(\d+)/;
                const gidRegex = /gid(\d+)/;
                const levelRegex = /\blevel(\d+)/;

                elements.forEach(field => {
                    const classAttr = field.getAttribute('class') || '';
                    const locMatch = classAttr.match(locRegex);
                    const gidMatch = classAttr.match(gidRegex);
                    const levelMatch = classAttr.match(levelRegex);
                    const gid = gidMatch ? parseInt(gidMatch[1], 10) : 0;

                    buildingsResult.push({
                        Location: locMatch ? parseInt(locMatch[1], 10) : -1,
                        Type: gid,
                        Level: levelMatch ? parseInt(levelMatch[1], 10) : -1,
                        IsUnderConstruction: classAttr.includes('underConstruction')
                    });
                });
            } else if (isDorf2) {
                const elements = document.querySelectorAll('#villageContent .buildingSlot');
                elements.forEach((building, i) => {
                    if (i === 22) return;

                    const link = building.querySelector('a');
                    const rawLoc = parseInt(building.getAttribute('data-did') || building.getAttribute('data-aid'), 10);
                    const location = isNaN(rawLoc) ? -1 : rawLoc;

                    const rawType = parseInt(building.getAttribute('data-gid'), 10);
                    let typeId = isNaN(rawType) ? -1 : rawType;

                    if (location === 26) typeId = 15;
                    else if (location === 39) typeId = 16;
                    else if (location === 40) typeId = getWallType(tribeId);

                    const rawLvl = link ? parseInt(link.getAttribute('data-level'), 10) : -1;

                    buildingsResult.push({
                        Location: location,
                        Type: typeId,
                        Level: isNaN(rawLvl) ? -1 : rawLvl,
                        IsUnderConstruction: link ? link.classList.contains('underConstruction') : false
                    });
                });
            }

            const queueElements = document.querySelectorAll('.buildingList li');
            queueElements.forEach((building, position) => {
                const nameEl = building.querySelector('.name');
                const lvlEl = building.querySelector('.lvl');
                const timerEl = building.querySelector('.timer');

                const rawText = nameEl && nameEl.childNodes.length > 0 ? nameEl.childNodes[0].textContent : '';
                const lvlText = lvlEl ? (lvlEl.innerText || lvlEl.textContent || '') : '';
                const lvlMatch = lvlText.match(/\d+/);
                const level = lvlMatch ? parseInt(lvlMatch[0], 10) : -1;
                const durationText = timerEl?.getAttribute('value');
                const durationSeconds = durationText && /^\d+$/.test(durationText) ? Number(durationText) : -1;

                queueResult.push({
                    Name: (rawText || '').trim(),
                    Position: position,
                    Level: level,
                    DurationSeconds: durationSeconds
                });
            });

            return {
                Buildings: buildingsResult,
                Queue: queueResult,
                Dorf: isDorf1 === isDorf2 ? 0 : (isDorf1 ? 1 : 2),
                Tribe: tribeId
            };
        }");

        var now = DateTime.Now;
        foreach (var item in data.Queue)
        {
            if (item.DurationSeconds >= 0 && item.DurationSeconds <= (DateTime.MaxValue - now).TotalSeconds)
                item.CompleteTime = now.AddSeconds(item.DurationSeconds);

            if (!buildingNames.TryGetValue(item.Name, out var type))
                throw new InvalidOperationException($"Unknown building name in queue: {item.Name}");

            item.Type = (BuildingEnums)type;
        }

        return data;
    }
}