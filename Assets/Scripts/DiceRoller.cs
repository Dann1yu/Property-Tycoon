using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for pointer events

public class DiceRoller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image diceImage1;
    public Sprite[] diceFaces1;
    public Sprite rollDiceSprite1;
    public Image diceImage2;
    public Sprite[] diceFaces2;
    public Sprite rollDiceSprite2;
    public bool isRolling = false;

    public GameObject dice1ImagesParent;
    public GameObject dice2ImagesParent;

    [SerializeField] private CanvasGroup diceCanvasGroup;

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

    public (int, bool) RollDice()
    {
        int firstRoll = 0;
        int secondRoll = 0;
        firstRoll = Random.Range(0, diceFaces1.Length);
        secondRoll = Random.Range(0, diceFaces2.Length);
        StartCoroutine(RollAnimation(firstRoll, secondRoll));

        var roll = firstRoll + secondRoll + 2;
        var rolledDouble = (firstRoll == secondRoll);
        return (roll, rolledDouble);

    }

    private IEnumerator RollAnimation(int firstRoll, int secondRoll)
    {
        isRolling = true;

        for (int i = 0; i < 10; i++)
        {
            diceImage1.sprite = diceFaces1[Random.Range(0, diceFaces1.Length)];
            diceImage2.sprite = diceFaces2[Random.Range(0, diceFaces2.Length)];
            yield return new WaitForSeconds(0.1f);
        }

        diceImage1.sprite = diceFaces1[firstRoll];
        diceImage2.sprite = diceFaces2[secondRoll];
        yield return new WaitForSeconds(1f);
        ShowDice(false);
        diceImage1.sprite = rollDiceSprite1;
        diceImage2.sprite = rollDiceSprite2;
        isRolling = false;
    }

    public bool ShowDice(bool show)
    {
        foreach (Transform child in dice1ImagesParent.transform)
        {
            child.gameObject.SetActive(show);
        }

        foreach (Transform child in dice2ImagesParent.transform)
        {
            child.gameObject.SetActive(show);
        }

        return show;
    }
}
