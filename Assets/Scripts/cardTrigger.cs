using UnityEngine;

public class CardTrigger : MonoBehaviour
{
    public OpportunityRandomiser opportunityKnocks; // Assign if it's a Community Chest card
    public PotLuckRandomiser potLuck; // Assign if it's a Pot Luck card

    private void OnMouseDown()
    {
        if (opportunityKnocks != null)
        {
            opportunityKnocks.RandomiseCard();
        }
        else if (potLuck != null)
        {
            potLuck.RandomiseCard();
        }
        else
        {
            Debug.LogWarning("No card randomizer assigned to this object!");
        }
    }
}
