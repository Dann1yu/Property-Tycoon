using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary> 
/// Script for loading screen interactions and starting the game
/// </summary>
public class LoadScene : MonoBehaviour
{
    // Dropdowns
    [SerializeField] private Dropdown DropdownPlayer;
    [SerializeField] private Dropdown DropdownAI;
    [SerializeField] private UnityEngine.UI.Toggle gameCheckBox;
    [SerializeField] private Scrollbar gameLengthScrollbar;

    [SerializeField] private GameObject shortGameToggle;
    [SerializeField] private TextMeshProUGUI lengthText;

    // Input value storage
    private static int Pvalue;
    private static int Avalue;

    private int Length;

    private static int finalLength;

    public List<int> gameLengths = new List<int> { 5, 10, 20, 30, 45, 60, 75, 90, 120, 150, 180 };



    /// <summary>
    /// When start button is pressed checks dropdowns and stores their values for future access
    /// </summary>
    public void StartGamePressed()
    {
        // Get player count dropdowm value
        int menuIndex = DropdownPlayer.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions = DropdownPlayer.GetComponent<Dropdown>().options;
        Pvalue = int.Parse(menuOptions[menuIndex].text);

        // Get AI player count dropdown value
        int menuIndex2 = DropdownAI.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions2 = DropdownAI.GetComponent<Dropdown>().options;
        Avalue = int.Parse(menuOptions2[menuIndex2].text);

        finalLength = Length;

        // Start game
        SceneManager.LoadScene("Property-Tycoon");
    }

    /// <summary>
    /// Returns number of players to be spawned
    /// </summary>
    /// <returns>Pvalue: Number of players</returns>
    public int UpdateGameSettingsPlayers()
    {
        return Pvalue;
    }

    /// <summary>
    /// Returns number of AI players to be spawned
    /// </summary>
    /// <returns>Avalue: Number of AI players</returns>
    public int UpdateGameSettingsAI()
    {
        return Avalue;
    }

    /// <summary>
    /// Returns gametype int
    /// </summary>
    /// <returns>Gvalue: Game type vakue</returns>
    public int UpdateGameSettingsGame()
    {
        return finalLength;
    }

    public void shortGameClicked()
    {
        shortGameToggle.SetActive(gameCheckBox.isOn);

        if (!gameCheckBox.isOn)
        {
            Length = -1;
        }


    }

    public void gameLengthChange()
    {
        float floatidx = gameLengthScrollbar.value * 10;
        int idx = Mathf.RoundToInt(floatidx);

        if (Length != gameLengths[idx])
        {
            Length = gameLengths[idx];
            lengthText.text = $"Game Length: {Length} minutes";

        }
        
        Debug.Log(gameLengths[idx]);
    }
}