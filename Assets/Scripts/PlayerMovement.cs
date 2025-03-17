using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    // In unity objects / vars
    [SerializeField] private int playerAmount = 2;
    [SerializeField] private GameObject PlayerObject;

    // Default values
    public GameObject CurrentPlayer;
    public int playerTurn = -1;

    private Bank_ bank;

    // Empty values to be set
    List<Player_> playerlist = new List<Player_>();
    Vector3[] boardPosition = new Vector3[40];

    // Creates the board as 40 vectors as 4 sides
    public void CreateBoard()
    {
        for (int i = 0; i < 11; i++)
        {
            boardPosition[i] = new Vector3(10 - i, 0.5f, 0); 
        }

        for (int i = 11; i < 21; i++)
        {
            boardPosition[i] = new Vector3(0, 0.5f, i - 10);
        }

        for (int i = 21; i < 31; i++)
        {
            boardPosition[i] = new Vector3(i - 20, 0.5f, 10); 
        }

        for (int i = 31; i < 40; i++)
        {
            boardPosition[i] = new Vector3(10, 0.5f, 40 - i); 
        }
    }

    // Emulation of two dice rolls (2x 1->6)
    public int DiceRoll()
    {
        int dice1 = Random.Range(1, 7);
        int dice2 = Random.Range(1, 7);
        Debug.Log("you rolled a: " + (dice1 + dice2));
        return (dice1 + dice2);
    }

    // Ends current term and starts next player's go
    public void NextTurn()
    {
        playerTurn++;

        if (playerTurn >= playerlist.Count) // Loops back to first player
        {
            playerTurn = 0;
        }

        CurrentPlayer = playerlist[playerTurn].gameObject; // Get GameObject of current player
    }

    // Spawns x players with attached scripts "Player_" to hold required variables
    public void spawnPlayers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var spawnedPlayer = Instantiate(PlayerObject, new Vector3(10, 0.5f, 0), Quaternion.identity);
            spawnedPlayer.name = $"Player {i}";

            // Attach and initialize the Player_ script
            Player_ playerComponent = spawnedPlayer.AddComponent<Player_>();
            playerComponent.Initialize($"Player {i}", 1500);

            // Add to player list for tracking
            playerlist.Add(playerComponent);
        }
    }

    // Creates the board, spawns the player and sets current player to player 0
    void Start()
    {
        CreateBoard();

        bank = FindObjectOfType<Bank_>(); // Finds the Bank_ instance in the scene


        spawnPlayers(playerAmount);

        CurrentPlayer = GameObject.Find("Player0");
    }

    // Updates on next players turn
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //for now to test movement up arrow presses in order to simulate players turn
            NextTurn();
            moveForward(DiceRoll(), CurrentPlayer.transform.position);

            BankTrans(100);
            BankTrans(-150);
            PlayerTrans(0, 1, 10);
            PlayerTrans(1, 0, 15);

        }
    }
    void moveForward(int distance, Vector3 currentpos)
    {
        Player_ currentPlayerScript = CurrentPlayer.GetComponent<Player_>();
        int oldposition=0;
        for (int i = 0; i < boardPosition.Length; i++)
        {
            if (currentpos == boardPosition[i])
            {
                oldposition = i;
                //finds the square that the current player is standing on
            }
        }

        //github test


        int new_position =  distance + oldposition;

        if (new_position >= 40)
        {
            new_position = new_position - 40;
            //for the loop back to start of game (GO!)
        }
        Debug.Log("newpos: " + new_position);
        CurrentPlayer.transform.position = boardPosition[new_position];
        currentPlayerScript.pos = new_position;
        //potential to add animation herre
        bank.info(new_position);
    }

    public void BankTrans(int amount)
    {
        Player_ currentPlayerScript = CurrentPlayer.GetComponent<Player_>();

        if (amount > 0)
        {
            currentPlayerScript.ReceiveMoney(amount);
        }
        else
        {
            currentPlayerScript.PayBank(-amount);
        }
    }

    public void PlayerTrans(int playerFromIdx, int playerToIdx, int amount)
    {
        Player_ sender = playerlist[playerFromIdx];
        Player_ receiver = playerlist[playerToIdx];

        sender.PayPlayer(receiver, amount);
    }

}
