using System;
using UnityEditor.Compilation;
using UnityEngine;

namespace quests
{
    public class BridgeQuest : IslandQuest
    {
        private readonly int _neededLogCount = 9;
        private int _consumedLogs = 0;
        private readonly Action _buildBridge;

        public BridgeQuest(int woodNeeded, Action buildBridge, string started, string progressMade, string completed)
        {
            _neededLogCount = woodNeeded;
            _buildBridge = buildBridge;

            introText = started;
            progressMadeText = progressMade;
            completedText = completed;
        }
        
        public override QuestState TrySatisfyQuest(PlayerInventory playerInventory)
        {
            var taken = playerInventory.ConsumeWoodUpToAmount(_neededLogCount);
            _consumedLogs += taken;

            if (_consumedLogs >= _neededLogCount)
            {
                _buildBridge();
                return QuestState.Completed;
            }
            else if (_consumedLogs > 0)
            {
                return QuestState.ProgressMade;
            }
            else
            {
                return QuestState.Begun;
            }
        }
    }
}