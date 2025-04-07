using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Dropdown DropdownPlayer;
    public Dropdown DropdownAI;
    public Dropdown DropdownGame;

    public static int Pvalue;
    public static int Avalue;
    public static int Gvalue;
    void Start()
    {


    }

    public void StartGamePressed()
    {
        int menuIndex = DropdownPlayer.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions = DropdownPlayer.GetComponent<Dropdown>().options;
        Pvalue = int.Parse(menuOptions[menuIndex].text);
        Debug.Log(Pvalue);
        int menuIndex2 = DropdownAI.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions2 = DropdownAI.GetComponent<Dropdown>().options;
        Avalue = int.Parse(menuOptions2[menuIndex2].text);
        Debug.Log(Avalue);
        Gvalue = DropdownGame.GetComponent<Dropdown>().value;
        Debug.Log(Gvalue);

        SceneManager.LoadScene("Property-Tycoon");
    }

    public int UpdateGameSettingsPlayers()
    {
        return Pvalue;
    }
    public int UpdateGameSettingsAI()
    {
        return Avalue;
    }
    public int UpdateGameSettingsGame()
    {
        return Gvalue;
    }

    // Update is called once per frame
    void Update()
    {

    }
}