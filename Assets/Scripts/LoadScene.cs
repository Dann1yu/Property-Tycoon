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
    void Start()
    {

        
    }

    public void StartGamePressed()
    {
        int menuIndex = DropdownPlayer.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions = DropdownPlayer.GetComponent<Dropdown>().options;
        string Pvalue = menuOptions[menuIndex].text;
        Debug.Log(Pvalue);
        int menuIndex2 = DropdownAI.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions2 = DropdownAI.GetComponent<Dropdown>().options;
        string Avalue = menuOptions2[menuIndex2].text;
        Debug.Log(Avalue);
        int Gvalue = DropdownGame.GetComponent<Dropdown>().value;
        Debug.Log(Gvalue);

        SceneManager.LoadScene("Property-Tycoon");
        Debug.Log("SWITCH");
        var playerMovement = GameObject.Find("GameController").GetComponent<PlayerMovement>();
        playerMovement.PlayerStartedTheGame(int.Parse(Pvalue), int.Parse(Avalue), Gvalue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
