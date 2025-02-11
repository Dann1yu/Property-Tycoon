using UnityEngine;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject Player1;

    public int position =0;
    // the players current position on the board, will change in future as this does not allow multiple players

    int[,] boardposition = new int[41, 2];


    public void CreateBoardCoords()
    {
        for (int i = 0; i < 11; i++)
        {
            boardposition[i, 0] = i;
            boardposition[i, 1] = 0;
        }
        for (int i = 11; i < 21; i++)
        {
            boardposition[i, 0] = 10;
            boardposition[i, 1] = i-10;
        }
        //for the positive axis board movement

        for (int i = 21; i < 31; i++)
        {
            boardposition[i, 0] = 30-i;
            boardposition[i, 1] = 10;
        }
        for (int i = 31; i < 40; i++)
        {
            boardposition[i, 0] = 0;
            boardposition[i, 1] = 40-i;
        }
        Debug.Log("board made");
        //for the negative corrdinate movemebt board coords
    }

    public int DiceRoll()
    {
        int dice1 = Random.Range(1, 7);
        int dice2 = Random.Range(1, 7);
        Debug.Log("you rolled a: " + (dice1 + dice2));
        return (dice1 + dice2);
        //returns random number that is calculate through 2 virtual dice
        // will be upgraded in future
    }
    void Start()
    {
        CreateBoardCoords();
       //instantiates the boards coordinates
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //for now to test movement up arrow presses in order to simulate players turn
            moveForward(DiceRoll(), position);
        }
    }
    void moveForward(int distance, int currentpos)
    {
        int new_position = distance + currentpos;
        
        if (new_position >= 40)
        {
            new_position = new_position - 40;
            position = new_position;
            //this accounts for the board looping overitself and resets the arrary
        }
        else
        {
            position = position + distance;
        }

        Debug.Log("newpos: " + new_position);
        Player1.transform.position = new Vector3(boardposition[new_position, 0], 0.5f, boardposition[new_position, 1]);
            
        //potential to add animation herre
           
        
    }
}
