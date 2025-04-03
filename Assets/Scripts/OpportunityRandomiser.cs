using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OpportunityRandomiser : MonoBehaviour
{
    public Sprite[] cardFaces; // Opportunity Knocks card images
    public Image cardUI; // What is shown
    public Sprite cardBack; // Default back sprite

    public void RandomiseCard()
    {
        cardUI.gameObject.SetActive(true); // Show the UI but active by default
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
