using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static System.Net.Mime.MediaTypeNames;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{


    /*
    -   detect whether going to jail versu just visinng, also going to have to telelport players to different spots onthe ajil tile depending
        on why they are there
    -   ergo add a teleprotation fucntion
    -   checking at start of each turn if you are in jaile (will use global int for each player) in next turn function probably
    -   add doubles fucntionalitly
    */


    // In unity objects / vars
    [SerializeField] private int playerAmount = 2;
    [SerializeField] private GameObject PlayerObject;

    public TextMeshProUGUI displayName1;
    public TextMeshProUGUI displayName2;

    public GameObject ButtonObject;
    public TextMeshProUGUI txt;
    public Transform WhereYouWantButtonsParented;



    private bool bought = false;
    private bool endedTurn = true;
    private bool options = false;
    private bool chose = false;
    // Default values
    public GameObject CurrentPlayer;
    public int playerTurn = -1;
    private Bank_ bank;

    public Player_ PlayerInControl;
    private UI UserInterface;

    // Empty values to be set
    List<Player_> playerlist = new List<Player_>();
    Vector3[] boardPosition = new Vector3[40];

    private DiceRoller diceRoller;

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
        return diceRoller.RollDice();
        //int dice1 = Random.Range(1, 7);
        //int dice2 = Random.Range(1, 7);
        //Debug.Log("you rolled a: " + (dice1 + dice2));
        //return (dice1 + dice2);
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

        string current = playerlist[playerTurn].ToString();
        current = current.Remove(current.Length - 9);
        displayName1.text = current;
        displayName2.text = "Balance: $" +playerlist[playerTurn].balance.ToString();
        
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
            Debug.Log("player count" + playerlist.Count);
        }
    }

    public string findPlayerName()
    {
        if (playerTurn >= playerlist.Count) // Loops back to first player
        {
            playerTurn = 0;
        }

        //Debug.Log("ttest" + playerlist[0]);
        return playerlist[1].ToString();
    }

    // Creates the board, spawns the player and sets current player to player 0
    void Start()
    {
        Debug.Log("started");
        CreateBoard();

        bank = FindFirstObjectByType<Bank_>(); // Finds the Bank_ instance in the scene


        spawnPlayers(playerAmount);

        diceRoller = FindFirstObjectByType<DiceRoller>();

        CurrentPlayer = GameObject.Find("Player 0");
        //Debug.Log("current" + CurrentPlayer);
        //Player_ thisPlayer = CurrentPlayer.GetComponent<Player_>();
        //Debug.Log("name" + thisPlayer.playerName);
    }

    // Updates on next players turn
    public void Update()
    {
  
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
           
            //for now to test movement up arrow presses in order to simulate players turn
            // if the dice is rolling do nothing
            if (diceRoller.isRolling)
            {
                return;
            }
            endedTurn = false;
            NextTurn();
            moveForward(DiceRoll(), CurrentPlayer.transform.position);

            bool firstroll = true;
            if (firstroll)
            {
                
                firstroll = false;
            }

            //all decision making after roll:

            //BankTrans(100);
            //BankTrans(-150);
            //PlayerTrans(0, 1, 10);
            //PlayerTrans(1, 0, 15);

        }
    }
    void moveForward(int distance, Vector3 currentpos)
    {

        Debug.Log("ran moveforward");
        Player_ currentPlayerScript = CurrentPlayer.GetComponent<Player_>();
        int oldposition=1;
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

        positionHandling(currentPlayerScript);
    }

    public void BankTrans(int amount)
    {
        Player_ currentPlayerScript = CurrentPlayer.GetComponent<Player_>();

        if (amount > 0)
        {
            currentPlayerScript.ReceiveMoneyFromBank(amount);
        }
        else
        {
            currentPlayerScript.PayBank(-amount);
        }
    }

    public bool waitForDecision()
    {
        return true;
    }

    public void PlayerTrans(Player_ sender, Player_ receiver, int amount)
    {
        sender.PayPlayer(receiver, amount);
    }

    void positionHandling(Player_ player)
    {
        // handling
        var position = player.pos;
        var location = bank.Properties[position];
        canEndTurn();
        canHaveOptions();
        // bank.info(position);

        //Debug.Log($"{position}");
        //Debug.Log($"{location.Position}");

        // Landed on property that can be purchased
        if (location.CanBeBought && bank.BankOwnedProperties.Contains(position))
        {
            
            Debug.Log("Property for sale");
            if (location.Cost < player.balance)
            {
                canBuyProperty();
            }
           
            if (bought)
            {
                purchaseProperty(player, location);
                bought = false;
                chose = true;
                Destroy(ButtonObject);
            } 
                
            
        }
        // Landed on property that is owned by a player
        else if (location.CanBeBought && !bank.BankOwnedProperties.Contains(position))
        {
            if (player.properties.Contains(position)) {
                Debug.Log("Property owned by the same player");
            }
            else {
                Debug.Log("Property owned by another player");
                payRent(player, location);
            }
        }

        // Landed on oppurtunity knocks 8, 37
        if (position == 7 | position == 36)
        {
            Debug.Log("Landed on oppurtunity knocks");
            oppKnock(player);
        }

        // Landed on pot luck
        if (position == 17 | position == 33)
        {
            Debug.Log("Landed on pot luck");
            potLuck(player);
        }

        // Landed on income tax
        if (position == 4)
        {
            Debug.Log("Landed on Income Tax and charged $200");
            player.DepositToFreeParking(200);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on free parking
        if (position == 20)
        {
            Debug.Log($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            player.balance += bank.FreeParkingBalance;
            bank.FreeParkingBalance = 0;
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go to jail
        if (position == 30)
        {
            Debug.Log("Go to jail");
            player.inJail=1;
            teleport(player, 10);
        }

        // Landed on super tax
        if (position == 38)
        {
            Debug.Log("Landed on Super Tax and charged $100");
            Debug.Log(player.balance);
            player.DepositToFreeParking(100);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go
        if (position == 0)
        {
            // should not need any logic if there is pass go logic implemented in move forward
            Debug.Log("Landed on GO");
        }
        //end turn functionff
        if (endedTurn)
        {
            Destroy(ButtonObject);
        }
    }


    void purchaseProperty(Player_ player, Property location)
    {
        player.addProperty(player.pos);
        BankTrans(-location.Cost);
        location.Owner = player;
        //Debug.Log($"{player.properties}");
        //Debug.Log($"{bank.BankOwnedProperties}");
        // needs checking if there isnt enough money
    }

    void payRent(Player_ player, Property location)
    {
        if (location.NumberOfHouses == 0) {
            PlayerTrans(player, location.Owner, location.RentUnimproved);
        } else if (location.NumberOfHouses == 1)
        {
            PlayerTrans(player, location.Owner, location.Rent1House);
        } else if (location.NumberOfHouses == 2)
        {
            PlayerTrans(player, location.Owner, location.Rent2Houses);
        } else if (location.NumberOfHouses == 3)
        {
            PlayerTrans(player, location.Owner, location.Rent3Houses);
        } else if (location.NumberOfHouses == 4)
        {
            PlayerTrans(player, location.Owner, location.Rent4Houses);
        } else if (location.NumberOfHouses == 5)
        {
            PlayerTrans(player, location.Owner, location.RentHotel);
        }
    }
        
    public void potLuck(Player_ player) {
        Card card = bank.PLCards[0];
        bank.PLCards.RemoveAt(0);

        Debug.Log($"{card.Description}");

        bank.PLCards.Add(card);
    }

    public void oppKnock(Player_ player) {
        Card card = bank.OKCards[0];
        bank.PLCards.RemoveAt(0);

        Debug.Log($"{card.Description}");

        bank.OKCards.Add(card);

    }

    public void teleport(Player_ player, int newPosition)
    {
        player.pos = newPosition;
        CurrentPlayer.transform.position = boardPosition[newPosition];
    }

    //ui section tried to make a script for it didnt work


    public void canBuyProperty()
    {

        var spawnedButtonP = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);
        spawnedButtonP.name = "propertyBtn";
        spawnedButtonP.transform.position = new Vector3(Screen.width - 380, Screen.height - 50, 0);
        Debug.Log("ui worked");
        
    }

    public void canEndTurn()
    {
        var spawnedButtonE = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);
        spawnedButtonE.name = "EndBtn";
        GameObject.Find("EndBtn").GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
        spawnedButtonE.transform.position = new Vector3(Screen.width -60, Screen.height - 50, 0);
    }

    public void canHaveOptions()
    {
        var spawnedButtonO = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);
        spawnedButtonO.name = "optionsBtn";
        GameObject.Find("optionsBtn").GetComponentInChildren<TextMeshProUGUI>().text = "Options";
        spawnedButtonO.transform.position = new Vector3(Screen.width - 220, Screen.height - 50, 0);
    }

    public void DecidedToClick(string buttonName)
    {
        Debug.Log("button name " + buttonName);
        if (buttonName == "propertyBtn")
        {
           
            bought = true;
            chose = true;
        }
        if (buttonName == "EndBtn")
        {
            endedTurn = true;
            chose = true;
        }
        if (buttonName == "optionsBtn")
        {
            options = true;
            chose = true;
        }

    }

    public void EndedTurn()
    {
        endedTurn = true;
    }

 

}
