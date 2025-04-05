using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
public class UI : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public void OnMyButtonClick()
    {
        GameObject button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var playerMovement = GameObject.Find("GameController").GetComponent<PlayerMovement>();
        playerMovement.DecidedToClick(button);
    }
}
