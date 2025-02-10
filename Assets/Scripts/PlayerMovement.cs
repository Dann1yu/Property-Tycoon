using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject Player1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 moveForward(int distance)
    {
        if ((int)Player1.transform.position.x < 100)
        {
            Player1.transform.position = new Vector3 (0, 0, distance);
        }
        return new Vector3(0,0,0);
    }
}
