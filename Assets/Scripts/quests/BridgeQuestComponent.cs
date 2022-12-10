using UnityEngine;

namespace quests
{
    [RequireComponent(typeof(IslandQuests))]
    public class BridgeQuestComponent : MonoBehaviour
    {
        public int woodNeeded = 9;
        public string introText = "Thank you for regrowing the forrest. Now I need help gathering wood for my bridge!";
        public string progressMadeText= "Thanks these will help. Just need a few more!";
        public string completedText = "Wow, thanks a bunch! Take these logs to the bridge down the hill, and let's build that bridge.";
        
        private IslandQuests _islandQuests;
        private bool _completed;

        private void Awake()
        {
            _islandQuests = GetComponent<IslandQuests>();
        }

        private void Start()
        {
            _islandQuests.RegisterQuest(new BridgeQuest(woodNeeded, MarkCompleted, introText, progressMadeText, completedText));
        }

        public bool Completed()
        {
            return _completed;
        }

        private void MarkCompleted()
        {
            _completed = true;
        }

        public int WoodNeeded()
        {
            return woodNeeded;
        }
    }
}