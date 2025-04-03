using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for pointer events

public class DiceRoller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image diceImage;
    public Sprite[] diceFaces;
    public Sprite rollDiceSprite;
    public bool isRolling = false;

    public Texture2D handCursor; // Assign a hand cursor texture in the Inspector
    private Texture2D defaultCursor;

    // public DiceRoller diceRoller = new DiceRoller();

    void Start()
    {
        defaultCursor = null; // Default cursor (can assign your own)
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(handCursor, Vector2.zero, CursorMode.Auto); // Change to hand cursor
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto); // Revert to default
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        // RollDice();
    }

    public int RollDice()
    {
        if (!isRolling)
        {
            int finalRoll = 0;
            finalRoll = Random.Range(0, diceFaces.Length);
            StartCoroutine(RollAnimation(finalRoll));
            finalRoll += Random.Range(0, diceFaces.Length) + 2;
            return finalRoll;
        }
        return 0;

    }

    private IEnumerator RollAnimation(int finalRoll)
    {
        isRolling = true;

        for (int i = 0; i < 10; i++)
        {
            diceImage.sprite = diceFaces[Random.Range(0, diceFaces.Length)];
            yield return new WaitForSeconds(0.1f);
        }

        diceImage.sprite = diceFaces[finalRoll];
        yield return new WaitForSeconds(2f);

        diceImage.sprite = rollDiceSprite;
        isRolling = false;
    }
}
