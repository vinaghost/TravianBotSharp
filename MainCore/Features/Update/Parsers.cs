using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Features.Update;

public static class Parsers
{
    public record struct RawHeroItemDto(int ItemId, string? DataTier, int Amount);

    public static async Task<List<HeroItemDto>> GetHeroItems(this IPage page)
    {
        var rawItemsData = await page.EvaluateAsync<List<RawHeroItemDto>>(@"() => {
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

    public record struct AccountDataDto(AccountInfoDto AccountInfo, List<VillageDto> Villages);

    public static async Task<(AccountInfoDto AccountInfo, List<VillageDto> Villages)> GetAccountData(this IPage page)
    {
        var data = await page.EvaluateAsync<AccountDataDto>(@"() => {
            const goldEl = document.querySelector('div.ajaxReplaceableGoldAmount');
            const silverEl = document.querySelector('div.ajaxReplaceableSilverAmount');
            const editButton = document.querySelector('#sidebarBoxLinklist a.edit.round');
            const questmasterBtn = document.querySelector('#questmasterButton');

            const parseDomInt = (el) => {
                if (!el) return 0;
                // Strip out non-numeric characters except minus signs (like localized thousand separators)
                const text = (el.innerText || el.textContent || '').replace(/[^\d-]/g, '');
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 0 : parsed;
            };

            const plusAttr = editButton ? (editButton.getAttribute('class') || '') : '';
            const tribeAttr = questmasterBtn ? (questmasterBtn.getAttribute('class') || '') : '';

            const tribeMatch = tribeAttr.match(/vid_(\d+)/);
            const tribeId = tribeMatch ? parseInt(tribeMatch[1], 10) : 0;

            const accountInfo = {
                Gold: parseDomInt(goldEl),
                Silver: parseDomInt(silverEl),
                HasPlusAccount: plusAttr.includes('green'),
                Tribe: tribeId
            };

            const villageElements = document.querySelectorAll('#sidebarBoxVillageList div.listEntry.village');
            const villages = [];

            villageElements.forEach(node => {
                const nameEl = node.querySelector('a span.name');
                const xEl = node.querySelector('span.coordinateX');
                const yEl = node.querySelector('span.coordinateY');

                const rawId = parseInt(node.getAttribute('data-did'), 10);
                const id = isNaN(rawId) ? 0 : rawId;

                const parseCoord = (el) => {
                    if (!el) return 0;
                    const text = (el.innerText || el.textContent || '').replace(/[^\d-]/g, '');
                    const parsed = parseInt(text, 10);
                    return isNaN(parsed) ? 0 : parsed;
                };

                villages.push({
                    Id: { Value: id },
                    Name: nameEl ? (nameEl.innerText || nameEl.textContent).trim() : '',
                    X: parseCoord(xEl),
                    Y: parseCoord(yEl),
                    IsActive: node.classList.contains('active'),
                    IsUnderAttack: node.classList.contains('attack')
                });
            });

            return { AccountInfo: accountInfo, Villages: villages };
        }");

        return (data.AccountInfo, data.Villages);
    }

    public record struct BuildingDataDto(List<BuildingDto> Buildings, List<QueueBuildingDto> Queue, TribeEnums Tribe);

    public static async Task<(List<BuildingDto> Buildings, List<QueueBuildingDto> Queue, TribeEnums Tribe)> GetBuildingData(this IPage page)
    {
        var data = await page.EvaluateAsync<BuildingDataDto>(@"() => {
            const url = window.location.href;
            const isDorf1 = url.includes('dorf1');
            const buildingsResult = [];
            const queueResult = [];

            const questMaster = document.querySelector('#questmasterButton');
            const tribeAttr = questMaster ? (questMaster.getAttribute('class') || '') : '';
            const tribeMatch = tribeAttr.match(/vid_(\d+)/);
            const tribeId = tribeMatch ? parseInt(tribeMatch[1], 10) : 0; // 0 maps to TribeEnums.Any

            const getWallType = (tId) => {
                switch(tId) {
                    case 1: return ; // Romans: City Wall
                    case 2: return 32; // Teutons: Earth Wall
                    case 3: return 33; // Gauls: Palisade
                    case 6: return 42; // Egyptians: Stone Wall
                    case 7: return 43; // Huns: Makeshift Wall
                    case 8: return 47; // Spartan: Defensive Wall
                    default: return 0;  // Unknown/None
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

                    buildingsResult.push({
                        Location: locMatch ? parseInt(locMatch[1], 10) : -1,
                        Type: gidMatch ? parseInt(gidMatch[1], 10) : 0, // BuildingEnums.Unknown
                        Level: levelMatch ? parseInt(levelMatch[1], 10) : -1,
                        IsUnderConstruction: classAttr.includes('underConstruction')
                    });
                });
            } else {
                const elements = document.querySelectorAll('#villageContent .buildingSlot');
                elements.forEach((building, i) => {
                    if (i === 22) return; // Skip rally point visual slot spacer if present

                    const link = building.querySelector('a');
                    const rawLoc = parseInt(building.getAttribute('data-did') || building.getAttribute('data-aid'), 10);
                    const location = isNaN(rawLoc) ? -1 : rawLoc;

                    const rawType = parseInt(building.getAttribute('data-gid'), 10);
                    let typeId = isNaN(rawType) ? -1 : rawType;

                    if (location === 26) typeId = 15;      // MainBuilding
                    else if (location === 39) typeId = 16; // RallyPoint
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
            queueElements.forEach(building => {
                const nameEl = building.querySelector('.name');
                const lvlEl = building.querySelector('.lvl');
                const timerEl = building.querySelector('.timer');

                const rawText = nameEl && nameEl.childNodes.length > 0 ? nameEl.childNodes[0].textContent : '';
                const cleanedType = rawText.replace(/[^a-zA-Z0-9]/g, '');

                const lvlText = lvlEl ? (lvlEl.innerText || lvlEl.textContent || '') : '';
                const lvlMatch = lvlText.match(/\d+/);
                const level = lvlMatch ? parseInt(lvlMatch[0], 10) : 0;

                const durationRaw = timerEl ? parseInt(timerEl.getAttribute('value'), 10) : 0;
                const durationSeconds = isNaN(durationRaw) ? 0 : durationRaw;

                queueResult.push({
                    Type: cleanedType,
                    Level: level,
                    Location: durationSeconds
                });
            });

            return {
                Buildings: buildingsResult,
                Queue: queueResult,
                Tribe: tribeId
            };
        }");

        var now = DateTime.Now;
        var processedQueue = data.Queue.Select(q => new QueueBuildingDto
        {
            Type = q.Type,
            Level = q.Level,
            Location = -1,
            CompleteTime = now.AddSeconds(q.Location)
        }).ToList();

        return (data.Buildings, processedQueue, data.Tribe);
    }
}