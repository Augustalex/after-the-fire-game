using System;

public abstract class IslandQuest
{
    protected string introText = "This is the quest I need you to do.";
    protected string progressMadeText = "This is the quest I need you to do.";
    protected string completedText = "Thank you for completed this quest.";

    public event Action Completed;
    public event Action<string> ShowText;

    public enum QuestState
    {
        Begun,
        ProgressMade,
        Completed
    }
    
    public void Interact(PlayerInventory playerInventory)
    {
        var questState = TrySatisfyQuest(playerInventory);
        switch (questState)
        {
            case QuestState.Begun:
                ShowQuestIntroText();
                break;
            case QuestState.ProgressMade:
                ShowQuestProgressMadeText();
                break;
            case QuestState.Completed:
                ShowQuestCompletedText();
                Complete();
                break;
            default:
                ShowQuestIntroText();
                break;
        }
    }

    public abstract QuestState TrySatisfyQuest(PlayerInventory playerInventory);

    protected void Complete()
    {
        Completed?.Invoke();
    }

    private void ShowQuestCompletedText()
    {
        ShowText?.Invoke(completedText);
    }

    private void ShowQuestIntroText()
    {
        ShowText?.Invoke(introText);
    }

    private void ShowQuestProgressMadeText()
    {
        ShowText?.Invoke(progressMadeText);
    }
}
