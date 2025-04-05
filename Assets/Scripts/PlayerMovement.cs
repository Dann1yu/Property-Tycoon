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
using System.Reflection;

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
    [SerializeField] private int playerAmount = 6;
    [SerializeField] private GameObject PlayerObject;

    public TextMeshProUGUI displayName1;
    public TextMeshProUGUI displayName2;

    public GameObject ButtonObject;
    public TextMeshProUGUI txt;
    public Transform WhereYouWantButtonsParented;

    public GameObject optionsButton;
    public GameObject endButton;
    public GameObject propertyButton;


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
    public int roll = -1;
    public int rolledDouble = 0;

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
    public (int, bool) DiceRoll()
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

        UpdateBalanceUI();
    }

    public void UpdateBalanceUI()
    {
        string current = playerlist[playerTurn].ToString();
        current = current.Remove(current.Length - 9);
        displayName1.text = current;
        displayName2.text = "Balance: $" + playerlist[playerTurn].balance.ToString();
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

        optionsButton = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);
        endButton = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);
        propertyButton = Instantiate<GameObject>(ButtonObject.gameObject, WhereYouWantButtonsParented);

        initiateEndTurn();
        initiateBuyProperty();
        initiateHaveOptions();

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
            if (rolledDouble == 0) NextTurn();

            (int roll, bool boolDouble) = DiceRoll();
            onRoll(roll, boolDouble);

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

    public void onRoll(int roll, bool boolDouble)
    {
        if (boolDouble)
        {
            rolledDouble += 1;
            Debug.Log($"You rolled a double! {rolledDouble}");
            if (rolledDouble == 3)
            {
                rolledDouble = 0;
                _GoToJail(CurrentPlayer.GetComponent<Player_>());
                return;
            }
        }        else rolledDouble = 0;

        moveForward(roll, CurrentPlayer.transform.position);
    }

    void moveForward(int distance, Vector3 currentpos)
    {
        Debug.Log($"DiceRoll: {distance}");
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        if (player.inJail == 2)
        {
            player.inJail = -1;
            return;
        }
        else if (player.inJail > -1)
        {
            player.inJail += 1;
            Debug.Log($"YOU ARE IN JAIL LOSER NO GO FOR YOU {player.inJail}");
            return;
        }

        int oldposition = player.pos;

        //github test


        int new_position =  distance + oldposition;

        if (new_position >= 40)
        {
            new_position = new_position - 40;
            //for the loop back to start of game (GO!)
        }

        if (new_position < oldposition)
        {
            Debug.Log("You passed go!");
            BankTrans(200);
        }

        Debug.Log("newpos: " + new_position);
        CurrentPlayer.transform.position = boardPosition[new_position];
        player.pos = new_position;
        //potential to add animation herre

        positionHandling(player);
    }

    public void BankTrans(int amount)
    {
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        if (amount > 0)
        {
            player.ReceiveMoneyFromBank(amount);
        }
        else
        {
            player.PayBank(-amount);
        }
        UpdateBalanceUI();
    }

    public bool waitForDecision()
    {
        return true;
    }

    public void PlayerTrans(Player_ sender, Player_ receiver, int amount)
    {
        sender.PayPlayer(receiver, amount);
        UpdateBalanceUI();
    }

    void positionHandling(Player_ player)
    {
        // handling
        var position = player.pos;
        var location = bank.Properties[position];
        canEndTurn(true);
        canHaveOptions(true);
        // bank.info(position);

        //Debug.Log($"{position}");
        //Debug.Log($"{location.Position}");

        // Landed on property that can be purchased
        if (location.CanBeBought && bank.BankOwnedProperties.Contains(position))
        {
            
            Debug.Log("Property for sale");
            if (location.Cost < player.balance)
            {
                canBuyProperty(true);
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
        if (position == 7 | position==22 | position == 36)
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
            _DepositToFreeParking(player, 200);
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
           _GoToJail(player);
        }

        // Landed on super tax
        if (position == 38)
        {
            Debug.Log("Landed on Super Tax and charged $100");
            Debug.Log(player.balance);
            _DepositToFreeParking(player, 100);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go
        if (position == 0)
        {
            // should not need any logic if there is pass go logic implemented in move forward
            Debug.Log("Landed on GO");
        }
    }


    void purchaseProperty(Player_ player, Property location)
    {
        player.addProperty(player.pos);
        BankTrans(-location.Cost);
        location.Owner = player;
        //Debug.Log($"{player.properties}");
        //Debug.Log($"{bank.BankOwnedProperties}");
        UpdateBalanceUI();
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
        UpdateBalanceUI();
    }
    
    // works but only for pay (not go to jail)
    public void potLuck(Player_ player) {
        Card card = bank.PLCards[0];
        bank.PLCards.RemoveAt(0);

        Debug.Log($"{card.Description}");
        
        var action = card.Action;
        var amount = card.Integer;

        runMethod(player, action, amount);

        bank.PLCards.Add(card);
    }

    public void oppKnock(Player_ player) {
        Card card = bank.OKCards[0];
        bank.OKCards.RemoveAt(0);

        Debug.Log($"{card.Description}");

        var action = card.Action;
        var amount = card.Integer;

        runMethod(player, action, amount);

        bank.OKCards.Add(card);

    }

    public void runMethod(Player_ player, string action, int Integer)
    {
        var method = this.GetType().GetMethod(action, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (method != null)
        {
            method.Invoke(this, new object[] { player, Integer });
        }
    }


    // ALL FOR CARD USAGE
    public void _Teleport(Player_ player, int newPosition)
    {
        player.pos = newPosition;
        CurrentPlayer.transform.position = boardPosition[newPosition];
    }

    //ui section tried to make a script for it didnt work


    public void initiateBuyProperty()
    {

        propertyButton.name = "propertyBtn";
        propertyButton.transform.position = new Vector3(Screen.width - 380, Screen.height - 50, 0);
        propertyButton.SetActive(false);

    }

    public void initiateEndTurn()
    {
        endButton.name = "EndBtn";
        GameObject.Find("EndBtn").GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
        endButton.transform.position = new Vector3(Screen.width -60, Screen.height - 50, 0);
        endButton.SetActive(false);
    }

    public void initiateHaveOptions()
    {
        optionsButton.name = "optionsBtn";
        GameObject.Find("optionsBtn").GetComponentInChildren<TextMeshProUGUI>().text = "Options";
        optionsButton.transform.position = new Vector3(Screen.width - 220, Screen.height - 50, 0);
        optionsButton.SetActive(false);
    }

    public void canBuyProperty(bool boolean)
    {
        propertyButton.SetActive(boolean);
    }
    public void canEndTurn(bool boolean)
    {
        endButton.SetActive(boolean);
    }
    public void canHaveOptions(bool boolean)
    {
        optionsButton.SetActive(boolean);
    }

    public void DecidedToClick(GameObject button)
    {
        string buttonName = button.name;

        Debug.Log("button name " + buttonName);
        if (buttonName == "propertyBtn")
        {
            Debug.Log($"{playerTurn}");
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();
            canBuyProperty(false);
            purchaseProperty(player, bank.Properties[player.pos]);

        }
        if (buttonName == "EndBtn")
        {
            Debug.Log("EndTurn");
        }
        if (buttonName == "optionsBtn")
        {
            Debug.Log("Options");
        }

    }   
 
    public void _ReceiveMoneyFromBank(Player_ player, int amount)
    {
        BankTrans(amount);
    }
    public void _PayBank(Player_ player, int amount)
    {
        BankTrans(-amount);
    }
    public void _OppKnocksOption(Player_ player, int amount)
    {
        Debug.Log("Pay a $10 fine or take opportunity knocks");

        // TODO add ui for choice but for now is random
        bool randomBool = Random.value > 0.5f;

        if (randomBool)
        {
            _DepositToFreeParking(player, amount);
        } else
        {
            oppKnock(player);
        }
    }
    public void _DepositToFreeParking(Player_ player, int amount)
    {
        Debug.Log("_DepositToFreeParking");
        player.DepositToFreeParking(amount);
    }
    public void _GoToJail(Player_ player, int amount = 0)
    {
        Debug.Log("_GoToJail");
        _Teleport(player, 10);
        player.inJail = 0;
    }
    public void _ReceiveMoneyFromAll(Player_ player, int amount)
    {
        Debug.Log("_ReceiveMoneyFromAll");
    }
    public void _JailFreeCard(Player_ player, int amount)
    {
        Debug.Log("_JailFreeCard");
        player.JailFreeCards += 1;
    }
    public void _CardMove(Player_ player, int amount)
    {
        var old_pos = player.pos;
        var distance = amount - old_pos;

        if (distance < 0)
        {
            distance += 40;
        }

        moveForward(distance, CurrentPlayer.transform.position);
        Debug.Log("_CardMove");

    }
    public void _Repairs(Player_ player, int amount)
    {
        Debug.Log("_Repairs");

    }

    public void Jail(Player_ player)
    {

    }


}
