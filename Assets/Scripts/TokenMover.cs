using UnityEngine;
using System.Collections;

public class TokenMover : MonoBehaviour
{
    public Transform[] boardSpaces; // Assigned from PlayerMovement
    public float moveSpeed = 5f;

    //Moves token from startPos by steps 
    public IEnumerator MoveToken(int startPos, int steps, System.Action onComplete = null)
    {
        int pos = startPos;
        for (int i = 0; i < steps; i++)
        {
            int next = (pos + 1) % boardSpaces.Length;
            yield return StartCoroutine(MoveTo(boardSpaces[next].position));
            pos = next;
        }

        onComplete?.Invoke();
    }
    //Moves token smoothly towards target position
    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}
