using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

/// <summary>
/// Controls the dice rolling UI and backend
/// </summary>
public class DiceRoller : MonoBehaviour
{
    // Canvas
    [SerializeField] private CanvasGroup diceCanvasGroup;

    // Dice objects
    // Sprites
    public Sprite[] diceFaces1;
    public Sprite[] diceFaces2;
    public Sprite rollDiceSprite1;
    public Sprite rollDiceSprite2;

    // Images
    public Image diceImage1;
    public Image diceImage2;

    // Game objects
    public GameObject dice1ImagesParent;
    public GameObject dice2ImagesParent;

    // Pointers
    public bool isRolling = false;

    /// <summary>
    /// Rolls dice and starts animation of dice roll, changing isRolling to true
    /// </summary>
    /// <returns>(roll, rolledDouble) where roll is the sum of two dice and rolledDouble is a boolean regarding if a double has been rolled</returns>
    public (int, bool) RollDice()
    {
        // Randomises two rolls
        int firstRoll = Random.Range(0, diceFaces1.Length);
        int secondRoll = Random.Range(0, diceFaces2.Length);

        // Starts animation
        StartCoroutine(RollAnimation(firstRoll, secondRoll));

        // Prepares values to return to GameLoop
        var roll = firstRoll + secondRoll + 2;
        var rolledDouble = (firstRoll == secondRoll);
        return (roll, rolledDouble);

    }

    /// <summary>
    /// Animation control for dice UI
    /// </summary>
    /// <param name="firstRoll">Dice 1 image index</param>
    /// <param name="secondRoll">Dice 2 image index</param>
    /// <returns>yield return to wait before continuing with the script</returns>
    private IEnumerator RollAnimation(int firstRoll, int secondRoll)
    {
        isRolling = true;

        // Every 0.1s randomise dice sprite to look like the dice are rolling
        for (int i = 0; i < 10; i++)
        {
            diceImage1.sprite = diceFaces1[Random.Range(0, diceFaces1.Length)];
            diceImage2.sprite = diceFaces2[Random.Range(0, diceFaces2.Length)];
            yield return new WaitForSeconds(0.1f);
        }

        // Set dice to actual roll for 1s
        diceImage1.sprite = diceFaces1[firstRoll];
        diceImage2.sprite = diceFaces2[secondRoll];
        yield return new WaitForSeconds(1f);

        // Disable dice and reset to preroll sprite
        ShowDice(false);
        diceImage1.sprite = rollDiceSprite1;
        diceImage2.sprite = rollDiceSprite2;
        isRolling = false;
    }

    /// <summary>
    /// Shows UI depending boolean
    /// </summary>
    /// <param name="show">Boolean whether to show dice or not</param>
    /// <returns></returns>
    public void ShowDice(bool show)
    {
        foreach (Transform child in dice1ImagesParent.transform)
        {
            child.gameObject.SetActive(show);
        }

        foreach (Transform child in dice2ImagesParent.transform)
        {
            child.gameObject.SetActive(show);
        }
    }
}
