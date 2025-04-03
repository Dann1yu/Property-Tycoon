using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PotLuckRandomiser : MonoBehaviour
{
    public Sprite[] cardFaces; // Pot Luck card images
    public Image cardUI; // UI Image for displaying the card
    public Sprite cardBack; // Default back sprite

    public void RandomiseCard()
    {
        cardUI.gameObject.SetActive(true); // Show the UI
        cardUI.sprite = cardBack; // Set back of card first

        StartCoroutine(ShowRandomCard());
    }

    private IEnumerator ShowRandomCard()
    {
        yield return new WaitForSeconds(1f); // Small delay for effect

        int randomIndex = Random.Range(0, cardFaces.Length);
        cardUI.sprite = cardFaces[randomIndex]; // Reveal the random card
    }
}
