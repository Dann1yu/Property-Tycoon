using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using System.Collections;
public class UI : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public void OnMyButtonClick()
    {
        GameObject button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var playerMovement = GameObject.Find("GameController").GetComponent<PlayerMovement>();
        playerMovement.DecidedToClick(button);
    }



    public void propertyDropdownChange()
    {
        GameObject dropdown = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var playerMovement = GameObject.Find("GameController").GetComponent<PlayerMovement>();
        playerMovement.propertyDropdownChange();
    }

    public void setDropdownChange()
    {
        GameObject dropdown = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var playerMovement = GameObject.Find("GameController").GetComponent<PlayerMovement>();
        playerMovement.setDropdownChange();
    }


}
