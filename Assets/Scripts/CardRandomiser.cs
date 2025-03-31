using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardRandomiser : MonoBehaviour
{
    public GameObject cardUI;  // Assign the UI Panel
    public Image cardImage;    // Assign the UI Image
    public Sprite cardBack;    // Assign the back of the card sprite
    public Sprite[] cardFaces; // Assign card face images
    public float revealDelay = 1f; // Time before revealing the card

    private void OnMouseDown()
    {
        Debug.Log("Object Clicked!"); // Check if this logs in the Console
        cardUI.SetActive(true);
        cardImage.sprite = cardBack;
        StartCoroutine(RevealCard());
    }


    private IEnumerator RevealCard()
    {
        yield return new WaitForSeconds(revealDelay); // Wait for the delay
        cardImage.sprite = cardFaces[Random.Range(0, cardFaces.Length)]; // Reveal a random card
    }
}
