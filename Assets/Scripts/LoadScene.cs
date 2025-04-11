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
    [SerializeField] private Dropdown DropdownGame;

    // Input value storage
    private static int Pvalue;
    private static int Avalue;
    private static int Gvalue;

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

        // Get gamemode dropdown value
        Gvalue = DropdownGame.GetComponent<Dropdown>().value;

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
        return Gvalue;
    }
}