using Cysharp.Threading.Tasks;
using SteamQueries.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CheeseMods.CSA3
{
    public static class DependancyLoader
    {
        public static bool Load(ulong id)
        {
            if (ModLoader.ModLoader.Instance._loadedItems.Any(i => i.Value.Item.PublishFieldId == id))
            {
                Debug.Log($"CSA3: Mod {id} already loaded!");
                return true;
            }

            Debug.Log($"CSA3: Checking local mods for {id}");
            foreach (SteamItem item2 in ModLoader.ModLoader.Instance.FindLocalItems())
            {
                Debug.Log($"{item2.Title} - {item2.PublishFieldId}");
            }

            SteamItem item = ModLoader.ModLoader.Instance.FindLocalItems().ToArray().FirstOrDefault(i => i.PublishFieldId == id);
            if (item == null)
            {
                Debug.Log($"CSA3: Checking steam mods for {id}");
                item = FindSteamItems().FirstOrDefault(i => i.PublishFieldId == id);
            }
            if (item == null)
            {
                Debug.Log($"CSA3: Couldn't find dependancy {id}, probably gonna cause problems...");
                return false;
            }

            Debug.Log($"CSA3: Loading dependancy {id}");
            ModLoader.ModLoader.Instance.LoadSteamItem(item);
            return true;
        }

        public static IReadOnlyCollection<SteamItem> FindSteamItems()
        {
            Debug.Log("CSA3: finding steam items...");

            int currentPage = 1;
            const int maxPages = 100;
            List<SteamItem> returnValue = new List<SteamItem>();

            while (currentPage < maxPages)
            {
                UniTask<GetSubscribedItemsResponse> pageResults = ModLoader.SteamQuery.SteamQueries.Instance.GetSubscribedItems(currentPage);
                if (pageResults.result == null)
                {
                    Debug.Log("pageResults were null");
                    break;
                }

                if (!pageResults.result.HasValues)
                {
                    Debug.Log("Get Subscribed Items didn't have any values");
                    break;
                }

                List<SteamItem> visibleItems = pageResults.result.Items;

                if (!visibleItems.Any())
                {
                    Debug.Log("Finished searching pages");
                    break;
                }

                returnValue.AddRange(visibleItems);
                Debug.Log($"Found {visibleItems.Count} mods on page {currentPage}");

                currentPage++;
            }

            return returnValue;
        }
    }
}
