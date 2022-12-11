using Core;

public class TutorialManager : MonoSingleton<TutorialManager>
{
    private int _hitTree = 0;
    private const int MaxHitTree = 2;
    private bool _approachedNpc;
    private bool _pickedUpPineCone;
    private bool _collectedLogs;
    private bool _shookTree;
    private bool _noMorePineCones;

    public void HitTree()
    {
        if (_hitTree >= MaxHitTree || _shookTree) return;
        
        if (_hitTree == 0)
        {
            UIManager.Instance.ShowTemporarySubtitle("~ Didn't hit it hard enough ~");
        }
        else if (_hitTree == 1)
        {
            UIManager.Instance.ShowTemporarySubtitle("~ Perhaps not enough snow ~");
        }
        
        _hitTree += 1;
    }
    
    public void ShookTree()
    {
        if (_shookTree) return;
        _shookTree = true;
        UIManager.Instance.ShowTemporarySubtitle("~ The tree dropped a pine cone ~");
    }
    
    public void AlreadyCollectedPineCone()
    {
        if (_noMorePineCones) return;
        _noMorePineCones = true;
        UIManager.Instance.ShowTemporarySubtitle("~ No more pine cones in this tree ~");
    }

    public void CollectedLogs()
    {
        if (_collectedLogs) return;
        _collectedLogs = true;
        UIManager.Instance.ShowTemporarySubtitle("~ These logs looks useful ~");
    }

    public void PickedUpPineCone()
    {
        if (_pickedUpPineCone) return;
        _pickedUpPineCone = true;
        UIManager.Instance.ShowTemporarySubtitle("~ Press Q/X to plant on grass ~");
    }

    public void ApproachedNpc()
    {
        if (_approachedNpc) return;
        _approachedNpc = true;
        UIManager.Instance.ShowTemporarySubtitle("~ Press E/B to talk ~");
    }
}