using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
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
    [SerializeField] private int playerAmount = 2;
    [SerializeField] private GameObject PlayerObject;

    public TextMeshProUGUI displayName1;
    public TextMeshProUGUI displayName2;

    // Default values
    public GameObject CurrentPlayer;
    public int playerTurn = -1;
    private Bank_ bank;

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

        CurrentPlayer = GameObject.Find("Player0");
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
            
            NextTurn();
            moveForward(DiceRoll(), CurrentPlayer.transform.position);

            //BankTrans(100);
            //BankTrans(-150);
            //PlayerTrans(0, 1, 10);
            //PlayerTrans(1, 0, 15);

        }
    }
    void moveForward(int distance, Vector3 currentpos)
    {
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

    public void PlayerTrans(Player_ sender, Player_ receiver, int amount)
    {
        sender.PayPlayer(receiver, amount);
    }

    void positionHandling(Player_ player)
    {
        // handling
        var position = player.pos;
        var location = bank.Properties[position];
        
        // bank.info(position);

        //Debug.Log($"{position}");
        //Debug.Log($"{location.Position}");

        // Landed on property that can be purchased
        if (location.CanBeBought && bank.BankOwnedProperties.Contains(position))
        {
            Debug.Log("Property for sale");
            purchaseProperty(player, location);
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
            _Teleport(player, 10);
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
        //end turn function
    }


    void purchaseProperty(Player_ player, Property location)
    {
        player.addProperty(player.pos);
        BankTrans(-location.Cost);
        location.Owner = player;
        //Debug.Log($"{player.properties}");
        //Debug.Log($"{bank.BankOwnedProperties}");
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

    public void _ReceiveMoneyFromBank(Player_ player, int amount)
    {
        Debug.Log("_ReceiveMoneyFromBank");
    }
    public void _PayBank(Player_ player, int amount)
    {
        Debug.Log("_PayBank");
    }
    public void _OppKnocksOption(Player_ player, int amount)
    {
        Debug.Log("_OppKnocksOption");
    }
    public void _DepositToFreeParking(Player_ player, int amount)
    {
        Debug.Log("_DepositToFreeParking");
    }
    public void _GoToJail(Player_ player, int amount)
    {
        Debug.Log("_GoToJail");
    }
    public void _ReceiveMoneyFromAll(Player_ player, int amount)
    {
        Debug.Log("_ReceiveMoneyFromAll");
    }
    public void _JailFreeCard(Player_ player, int amount)
    {
        Debug.Log("_JailFreeCard");
    }
    public void _CardMove(Player_ player, int amount)
    {
        Debug.Log("_CardMove");

    }
    public void _Repairs(Player_ player, int amount)
    {
        Debug.Log("_Repairs");

    }



}
