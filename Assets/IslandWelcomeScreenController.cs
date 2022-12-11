using Core;
using UnityEngine;

public class IslandWelcomeScreenController : MonoSingleton<IslandWelcomeScreenController>
{
    public GameObject welcomeScreen;
    private IslandWelcomeScreen _welcomeScreen;
    private float _sameIslandSignCooldownUntil;
    private IslandInfo _lastVisitedIsland;

    void Start()
    {
        _welcomeScreen = welcomeScreen.GetComponent<IslandWelcomeScreen>();
        _welcomeScreen.Done += HideWelcomeScreen;

        welcomeScreen.SetActive(false);
    }

    private void HideWelcomeScreen()
    {
        welcomeScreen.SetActive(false);
    }

    public void Show(IslandInfo island)
    {
        if (Time.time < 5) return;
        if (welcomeScreen.activeSelf) return;
        if (island == _lastVisitedIsland && Time.time < _sameIslandSignCooldownUntil) return;

        _lastVisitedIsland = island;
        _sameIslandSignCooldownUntil = Time.time + 60;

        welcomeScreen.SetActive(true);

        var joint = GetComponentInChildren<SpringJoint2D>();
        joint.distance = Screen.height - 80f;

        _welcomeScreen.RefreshAnchor();
        _welcomeScreen.Play(island);
    }
}