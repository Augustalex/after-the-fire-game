using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class IslandSign : MonoBehaviour
{
    private TMP_Text _text;
    private GameObject[] _toggles;

    void Start()
    {
        _text = GetComponentInChildren<TMP_Text>();
        _toggles = GetComponentsInChildren<BeetleToggle>().Select(b => b.gameObject).ToArray();
    }

    public void Refresh(IslandInfo island)
    {
        _text.text = island.GetIslandNameFormatted();

        var questIsland = island.GetComponent<Island>();
        if (questIsland)
        {
            var completedQuests = questIsland.GetNumberOfReceivedBeetles();
            for (int i = 0; i < _toggles.Length; i++)
            {
                var toggleGameObject = _toggles[i];
                toggleGameObject.SetActive(true);
                
                var toggle = toggleGameObject.GetComponent<BeetleToggle>();
                if (i < completedQuests)
                {
                    toggle.IsDone();
                }
                else
                {
                    toggle.IsNotDone();
                }
            }
        }
        else
        {
            foreach (var beetleToggle in _toggles)
            {
                beetleToggle.SetActive(false);
            }
        }
    }
}