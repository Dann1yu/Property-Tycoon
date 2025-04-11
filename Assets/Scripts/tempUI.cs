using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class tempUI : MonoBehaviour
{
    public Dropdown humanDropdown;
    public Dropdown aiDropdown;

    private const int maxTotalPlayers = 6;

    void Start()
    {
        PopulateDropdown(humanDropdown, 1, 5);
        PopulateDropdown(aiDropdown, 1, 5);

        humanDropdown.onValueChanged.AddListener(OnHumanDropdownChanged);
        aiDropdown.onValueChanged.AddListener(OnAIDropdownChanged);
    }

    void PopulateDropdown(Dropdown dropdown, int min, int max)
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = min; i <= max; i++)
        {
            options.Add(i.ToString());
        }
        dropdown.AddOptions(options);
    }

    void OnHumanDropdownChanged(int index)
    {
        int selectedHumans = index + 1;
        int maxAIs = Mathf.Clamp(maxTotalPlayers - selectedHumans, 1, 5);
        int currentAI = aiDropdown.value + 1;

        aiDropdown.onValueChanged.RemoveListener(OnAIDropdownChanged);
        PopulateDropdown(aiDropdown, 1, maxAIs);
        aiDropdown.value = Mathf.Clamp(currentAI - 1, 0, maxAIs - 1);
        aiDropdown.onValueChanged.AddListener(OnAIDropdownChanged);
    }

    void OnAIDropdownChanged(int index)
    {
        int selectedAIs = index + 1;
        int maxHumans = Mathf.Clamp(maxTotalPlayers - selectedAIs, 1, 5);
        int currentHumans = humanDropdown.value + 1;

        humanDropdown.onValueChanged.RemoveListener(OnHumanDropdownChanged);
        PopulateDropdown(humanDropdown, 1, maxHumans);
        humanDropdown.value = Mathf.Clamp(currentHumans - 1, 0, maxHumans - 1);
        humanDropdown.onValueChanged.AddListener(OnHumanDropdownChanged);
    }

}
