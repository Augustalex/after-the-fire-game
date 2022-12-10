using System.Collections.Generic;
using UnityEngine;

namespace quests
{
    public class IslandQuests : MonoBehaviour
    {
        private readonly List<IslandQuest> _uncompletedQuests = new List<IslandQuest>();
        
        public void RegisterQuest(IslandQuest islandQuest)
        {
            _uncompletedQuests.Add(islandQuest);

            islandQuest.ShowText += ShowText;
            islandQuest.Completed += OnCompletion;

            void OnCompletion()
            {
                _uncompletedQuests.Remove(islandQuest);
                islandQuest.Completed -= OnCompletion;
            }
        }

        void ShowText(string text)
        {
            UIManager.Instance.SetSubtitle(text);
        }
        
        public bool HasQuest()
        {
            return _uncompletedQuests.Count > 0;
        }

        public void InteractWithOngoingQuest(PlayerInventory playerInventory)
        {
            if (!HasQuest()) return;
            
            _uncompletedQuests[0].Interact(playerInventory);
        }
    }
}