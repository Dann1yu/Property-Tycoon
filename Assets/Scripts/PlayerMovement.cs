using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject Player;

    [SerializeField] private int playerAmount = 2;
    [SerializeField] private GameObject Player1;

    public GameObject CurrentPlayer;
    public int playerTurn = -1;

    List<Player> playerlist = new List<Player>();

    Vector3[] boardPosition = new Vector3[40];

    public void CreateBoard()
    {
        for (int i = 0; i < 11; i++)
        {
            boardPosition[i] = new Vector3 (i, 0.5f, 0);
        }
        for (int i = 11; i < 21; i++)
        {
            boardPosition[i] = new Vector3(10, 0.5f, i-10);
        }
        for (int i = 21; i < 31; i++)
        {
            boardPosition[i] = new Vector3(30-i, 0.5f, 10);
        }
        for (int i = 31; i < 40; i++)
        {
            boardPosition[i] = new Vector3(0, 0.5f, 40-i);
        }
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

    public void NextTurn()
    {
        playerTurn++;

        if (GameObject.Find($"Player {playerTurn}") ==  null) //makes sure that only works for amount of players that are playing
        {
            playerTurn = 0;
        }

        CurrentPlayer = GameObject.Find($"Player {playerTurn}");
    }

    void spawnPlayers(int amount)
    {

        //will potentially be changed in order to accomodate choosing how many players and names etc...
        for (int i = 0; i < amount; i++)
        {

            var spawnedPlayer = Instantiate(Player1, new Vector3(0, 0.5f, 0), Quaternion.identity);
            spawnedPlayer.name = $"Player {i}";

            //Player player = new Player($"Player {i}", 1500); //check initaly starting money
            //Debug.Log(player.name + player.Balance);
            //playerlist.Add(player);



        }

    }


    void Start()
    {
        
        
        
        CreateBoard();

        spawnPlayers(playerAmount);

        CurrentPlayer = GameObject.Find("Player0");//will always have 1 player minimum so they start playing first
                                                   //instantiates the boards coordinates
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //for now to test movement up arrow presses in order to simulate players turn
            NextTurn();
            moveForward(DiceRoll(), CurrentPlayer.transform.position);
        }
    }
    void moveForward(int distance, Vector3 currentpos)
    {
        int oldposition=0;
        for (int i = 0; i < boardPosition.Length; i++)
        {
            if (currentpos == boardPosition[i])
            {
                oldposition = i;
                //finds the square that the current player is standing on
            }
        }


        int new_position =  distance + oldposition;

        if (new_position >= 40)
        {
            new_position = new_position - 40;
            //for the loop back to start of game (GO!)
        }
        Debug.Log("newpos: " + new_position);
        CurrentPlayer.transform.position = boardPosition[new_position];
            
        //potential to add animation herre
           
        
    }
}
