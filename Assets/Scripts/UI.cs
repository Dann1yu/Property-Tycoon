using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the UI interaction, sending info to gameLoop
/// </summary>
public class UI : MonoBehaviour
{
    public GameLoop gameLoop;

    /// <summary>
    /// Attached to every UI button that interacts with gameLoop,on click: sends the gameobject to gameLoop
    /// </summary>
    public void OnMyButtonClick()
    {
        GameObject button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var gameLoop = GameObject.Find("GameController").GetComponent<GameLoop>();
        gameLoop.DecidedToClick(button);
    }

    /// <summary>
    /// Attached to property dropdown and activates on dropdown option change: sends the gameobject to gameLoop
    /// </summary>
    public void propertyDropdownChange()
    {
        GameObject dropdown = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var gameLoop = GameObject.Find("GameController").GetComponent<GameLoop>();
        gameLoop.propertyDropdownChange();
    }

    /// <summary>
    /// Attached to sets dropdown and activates on dropdown option change: sends the gameobject to gameLoop
    /// </summary>
    public void setDropdownChange()
    {
        GameObject dropdown = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var gameLoop = GameObject.Find("GameController").GetComponent<GameLoop>();
        gameLoop.setDropdownChange();
    }
}
