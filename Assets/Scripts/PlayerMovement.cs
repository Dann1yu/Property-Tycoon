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
using static UnityEngine.Rendering.DebugUI;
using static System.Math;
using System.Threading;
using System.Collections;

/// <summary>
/// Controls the main game loop
/// Including Human and AI logic
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // private variables that will be changed by load screen
    private int playerAmount;
    private int AIplayerAmount;
    private bool abridgedGamemode;
    public float startTime;
    public float endTime;
    private bool testing = false;

    // all pointers used through out script
    private int lowestHouses = 5;
    private int highestHouses = 0;
    
    private int roll = -1;
    public int rolledDouble = 0;

    public bool next = true;
    public bool showing = true;
    public bool running = false;
    public bool gameStarted = false;

    private Player_ highestBidder;
    private int nextBidder = 0;
    private int highestBid = 0;
    private int totalpassed = 0;

    // player variables
    public GameObject CurrentPlayer;
    public int playerTurn = -1;
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();

    // display elements
    [SerializeField] private TextMeshProUGUI displayName1;
    [SerializeField] private TextMeshProUGUI displayName2;
    [SerializeField] private TextMeshProUGUI displayName3;
    [SerializeField] private TextMeshProUGUI displaydouble;

    // UI assignment
    // Base player options
    [SerializeField] private GameObject auctionButton;
    [SerializeField] private GameObject endButton;
    [SerializeField] private GameObject propertyButton;
    [SerializeField] private GameObject manageButton;

    // Managing property options
    [SerializeField] private GameObject managePanel;
    [SerializeField] private GameObject setsPanel;
    [SerializeField] private GameObject propertiesPanel;

    [SerializeField] private TMP_Dropdown propertiesDropdown;
    [SerializeField] private TMP_Dropdown setsDropdown;
    [SerializeField] private TextMeshProUGUI mortgageText;

    [SerializeField] private GameObject sellHouseButton;
    [SerializeField] private GameObject upgradeHouseButton;
    [SerializeField] private GameObject closeButton;

    [SerializeField] private GameObject jailButton;

    // Auction UI
    [SerializeField] private GameObject playerBidPanel;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TMP_InputField bidInputField;

    // Choices UI
    [SerializeField] private GameObject oppKnocksOption;
    [SerializeField] private GameObject jailOption;

    // Winner panel
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TextMeshProUGUI winningText;
    [SerializeField] private TextMeshProUGUI winningBalance;

    // Import scripts
    private Bank_ bank;
    private DiceRoller diceRoller;

    // Empty values to be set
    List<Player_> playerlist = new List<Player_>();
    Vector3[] boardPosition = new Vector3[40];
    public List<Player_> bidders = new List<Player_>();
    public List<string> cpuStack = new List<string>(); // can be replaced by past actions
    public Dictionary<string, bool> pastActions = new Dictionary<string, bool>();

    /// <summary>
    /// Creates the board as 40 vectors across 4 sides
    /// By creating a new vector for each vector in boardPosition for each position
    /// </summary>
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

    /// <summary>
    /// Emulation of two dice rolls
    /// </summary>
    /// <returns>
    /// (int, bool) 
    /// int: Value of the dice roll (2 to 12)
    /// bool: Boolean value representative of if a double is rolled
    /// </returns>
    public (int, bool) DiceRoll()
    {
        canRoll(false);
        return diceRoller.RollDice();
    }

    /// <summary>
    /// Prepares the game loop for the next players turn.
    /// Updates UI
    /// </summary>
    public void NextTurn()
    {
        cpuStack.Clear();
        playerTurn++;

        if (playerTurn >= playerlist.Count) // Loops back to first player if past playerlist count
        {
            playerTurn = 0;
        }

        UpdateBalanceUI();
    }

    /// <summary>
    /// Updates the UI representing: 
    /// * Current player
    /// * Player balance
    /// * If a double has rolled
    /// 
    /// Also checks if balance is negative and makes the player perform actions until balance is positive.
    /// </summary>
    public void UpdateBalanceUI()
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        string current = player.ToString();
        current = current.Remove(current.Length - 9);
        displayName1.text = current;
        displayName2.text = "Balance: $" + playerlist[playerTurn].balance.ToString();
        displaydouble.text = "";
        displayName3.text = "";

        // If action is required to go above negative balance
        if (pastActions["canManage"] && !pastActions["canEndTurn"] && (player.balance >= 0)) {
            canEndTurn(true);
            if (player.AI)
            {
                CPULogic(player);
            }
        } else if (pastActions["canManage"] && !pastActions["canEndTurn"])
        {
            canEndTurn(false);
        }
    }

    /// <summary>
    /// Spawns players with attached scripts "Player_" to hold required variables
    /// Aswell as if they are or are not AI players
    /// </summary>
    /// <param name="amount">Total number of players</param>
    /// <param name="AI">Number of those players whcih will be AI players</param>
    public void spawnPlayers(int amount, int AI)
    {
        for (int i = 0; i < amount; i++)
        {
            // Create each unique player
            var PlayerObject = characterPrefabs[i];
            var spawnedPlayer = Instantiate(PlayerObject, new Vector3(10, 0.5f, 0), Quaternion.identity);
            spawnedPlayer.name = $"Player {i}";

            // Attach and initialize the Player_ script
            Player_ playerComponent = spawnedPlayer.AddComponent<Player_>();
            playerComponent.Initialize($"Player {i}", 1500);
            playerlist.Add(playerComponent);

            // CPU LOGIC
            playerComponent.AI = (i >= (amount - AI));

            Debug.Log($"{playerComponent.playerName} : AI={playerComponent.AI}");   
        }
    }

    /// <summary>
    /// Creates the board, spawns the players, sets current player to player 0,
    /// Makes all unneeded UI components invisible
    /// </summary>
    void Start()
    {
        startTime = Time.time;
        diceRoller = FindFirstObjectByType<DiceRoller>();
        CreateBoard();

        // Sets all panels to invisible
        playerBidPanel.SetActive(false);
        managePanel.SetActive(false);
        setsPanel.SetActive(false);
        propertiesPanel.SetActive(false);
        winnerPanel.SetActive(false);
        closeButton.SetActive(false);
        oppKnocksOption.SetActive(false);
        jailOption.SetActive(false);

        pastActions["canEndTurn"] = false;
        pastActions["canBuyProperty"] = false;
        pastActions["canStartAuction"] = false;
        pastActions["canRoll"] = false;
        pastActions["canManage"] = false;

        // Calling the info from the loading scene
        var PlayerAmounts = GameObject.Find("GameController").GetComponent<LoadScene>();
        playerAmount = PlayerAmounts.UpdateGameSettingsPlayers();
        AIplayerAmount = PlayerAmounts.UpdateGameSettingsAI();

        // TODO get gamemode and therefore length of game

        // Checks if script is ran from Property-Tycoon scene and puts into testing mode
        if (playerAmount == 0)
        {
            playerAmount = 3;
            AIplayerAmount = 3;
            endTime = 120f;
            abridgedGamemode = true;
            testing = true;
            Debug.Log("TESTING MODE");

        }

        bank = FindFirstObjectByType<Bank_>(); // Finds the Bank_ instance in the scene

        spawnPlayers(playerAmount, AIplayerAmount);
        NextTurn();

        canBuyProperty(false);
        canEndTurn(false);
        canStartAuction(false);

        gameStarted = true;

        canRoll(true);

        // If first player is an AI start the CPU Logic
        if (playerlist[0].AI)
        {
            CPULogic(playerlist[0]);
        }
    }

    /// <summary>
    /// Checks every frame for specified requirements
    /// </summary>
    public void Update()
    {
        // If start() hasn't finished do not check update
        if (!gameStarted)
        {
            return;
        }

        // If player can roll the dice and if it isn't showing
        if (!diceRoller.isRolling && next && !showing && !propertyButton.activeSelf)
        {
            diceRoller.ShowDice(true);
            canRoll(true);
        }

        // If UpArrow pressed OR player is an AI and logic isn't running
        if (Input.GetKeyDown(KeyCode.UpArrow) | (playerlist[playerTurn].AI && !running))
        {
            // If first roll of the go
            if (next && rolledDouble == 0)
            {
                canRoll(false);

                if (testing)
                {
                    test();
                    testing = false;
                }
            }
            else if (!next)
            {
                return;
            }

            // If the dice is rolling do nothing
            if (diceRoller.isRolling)
            {
                return;
            }

            // Else starts dice roll
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            running = true;
            StartCoroutine(diceRollRoutine(player));
        }
    }

    /// <summary>
    /// Starts dice roll with time waits for animations to complete.
    /// Runs onRoll(roll, boolDouble) after dice roll.
    /// If player is a cpu run their logic.
    /// </summary>
    /// <param name="player">Player that is rolling the dice</param>
    /// <returns></returns>
    IEnumerator diceRollRoutine(Player_ player)
    {
        // If player is AI wait a second to mimic a human
        if (player.AI)
        {
            yield return new WaitForSeconds(1f);
        }

        // Roll the dice and wait 1.5s for it to finish
        (int roll, bool boolDouble) = DiceRoll();
        yield return new WaitForSeconds(1.5f);

        // Run roll logic
        onRoll(roll, boolDouble);

        // If player is AI run cpu logic
        if (player.AI) {
            Debug.Log("CPU LOGIC");
            StartCoroutine(CPULogic(player));
        }
    }

    /// <summary>
    /// Function for testing code, only runs if script ran from Property-Tycoon scene
    /// </summary>
    public void test()
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        player.balance = 100000;

        purchaseProperty(player, bank.Properties[1]);
        purchaseProperty(player, bank.Properties[3]);

        purchaseProperty(player, bank.Properties[5]);
        purchaseProperty(player, bank.Properties[15]);

        purchaseProperty(player, bank.Properties[6]);
        purchaseProperty(player, bank.Properties[8]);
        purchaseProperty(player, bank.Properties[9]);

        //purchaseProperty(player, bank.Properties[12]);
        //purchaseProperty(player, bank.Properties[28]);

        //player.upgradeHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[3]);
        //player.upgradeHouse(bank.Properties[3]);
        //player.upgradeHouse(bank.Properties[3]);
        //player.upgradeHouse(bank.Properties[3]);

        mortgageProperty(player, bank.Properties[15]);

        player.balance = 2000;

        //purchaseProperty(playerlist[1], bank.Properties[37]);
        //purchaseProperty(playerlist[1], bank.Properties[39]);
        playerlist[1].balance = 10;
        playerlist[2].balance = 10;

        //player = playerlist[1].gameObject.GetComponent<Player_>();
        //player.balance = 10;
        //player = playerlist[2].gameObject.GetComponent<Player_>();
        //player.balance = 10;
        //player = playerlist[3].gameObject.GetComponent<Player_>();
        //player.balance = 10;
        //player = playerlist[4].gameObject.GetComponent<Player_>();
        //player.balance = 10;
        //player = playerlist[5].gameObject.GetComponent<Player_>();
        //player.balance = 10;

        //Debug.Log(player.checkLiquidation());

        //_Repairs(player, 0);
        //_Repairs(player, 1);
    }

    /// <summary>
    /// Logic for dealing with post dice roll
    /// </summary>
    /// <param name="rollValue">Value of the double dice roll</param>
    /// <param name="boolDouble">Boolean for whether player rolled a double</param>
    public void onRoll(int rollValue, bool boolDouble)
    {
        roll = rollValue;

        // If rolled a double increase rolledDouble
        if (boolDouble)
        {
            rolledDouble += 1;
            Debug.Log($"You rolled a double! {rolledDouble}");
            displaydouble.text = ($"You rolled a double!");

            // If player has rolled 3 doubles go to jail and stop logic
            if (rolledDouble == 3)
            {
                rolledDouble = 0;
                _GoToJail(CurrentPlayer.GetComponent<Player_>());
                return;
            }
        } else rolledDouble = 0;

        // Move forward roll
        moveForward(roll, CurrentPlayer.transform.position);
    }

    /// <summary>
    /// Move distance spaces forward (if distance is -ve then |x| backwards)
    /// Then run positionHandling(player)
    /// </summary>
    /// <param name="distance">Number of spaces to be moved</param>
    /// <param name="currentpos">Current position (vector3) of player</param>
    void moveForward(int distance, Vector3 currentpos)
    {
        Debug.Log($"DiceRoll: {distance}");
        displayName3.text = ($"DiceRoll: {distance}");

        Player_ player = CurrentPlayer.GetComponent<Player_>();

        // If player has been in jail for 2 rounds then let free
        if (player.inJail == 2)
        {
            player.inJail = -1;
        }
        // Else if player is in jail but not for 2 rounds stay in jail, can end turn and stop logic
        else if (player.inJail > -1)
        {
            player.inJail += 1;
            Debug.Log($"YOU ARE IN JAIL LOSER NO GO FOR YOU {player.inJail}");
            displayName3.text = ($"YOU ARE IN JAIL NO GO FOR YOU! {player.inJail}");
            canEndTurn(true);
            return;
        }

        // Calculate new distance, if larger than possible positions player has wrapped board and is treated accordingly
        int oldposition = player.pos;
        int new_position = distance + oldposition;
        
        // If position is negative, make positive
        if (new_position < 0)
        {
            new_position = new_position + 40;
        }
        // Else if position is larger than 39, mod back down and pass GO
        else if (new_position >= 40)
        {
            new_position = new_position - 40;

            Debug.Log("You passed go!");
            displayName3.text = ("You passed go!");
            BankTrans(200);

            if (!player.passedGo)
            {
                player.passedGo = true;
                totalpassed++;
            }
        }

        // Move the player to new position
        // TODO add animation here
        Debug.Log("newpos: " + new_position);
        CurrentPlayer.transform.position = boardPosition[new_position];
        player.pos = new_position;

        // Handle new position of player
        positionHandling(player);
    }

    /// <summary>
    /// Transaction to or from bank 
    /// +ve amount is from the bank, -ve amount is to the bank
    /// </summary>
    /// <param name="amount">Amount to receive or send (+ve : receive, -ve : send)</param>
    /// <param name="player">Player receiving or sending</param>
    public void BankTrans(int amount, Player_ player = null)
    {
        // If player is null, find player
        if (player == null)
        {
            player = CurrentPlayer.GetComponent<Player_>();
        }

        // If amount is positive, bank sends money to the player
        if (amount > 0)
        {
            player.ReceiveMoneyFromBank(amount);
        }
        // Else paybank amount
        else
        {
            player.PayBank(checkBalance(player, -amount));
        }
        // Update UI
        UpdateBalanceUI();
    }

    /// <summary>
    /// Transaction from sender to receiver of amount
    /// </summary>
    /// <param name="sender">Player_ that is sending the amount</param>
    /// <param name="receiver">Player_ that is receiving the amount</param>
    /// <param name="amount">Amount that is being sent / received</param>
    public void PlayerTrans(Player_ sender, Player_ receiver, int amount)
    {
        sender.PayPlayer(receiver, checkBalance(sender, amount));
        UpdateBalanceUI();
    }

    /// <summary>
    /// Checks where the player landed and performs the correct action
    /// </summary>
    /// <param name="player">Player_ who's position is being handled</param>
    void positionHandling(Player_ player)
    {
        // Basic values and UI changes
        var position = player.pos;
        var location = bank.Properties[position];
        canEndTurn(false)

        // If landed on property that can be purchased
        if (location.CanBeBought && bank.BankOwnedProperties.Contains(position))
        {
            canEndTurn(false);
            Debug.Log("Property for sale");
            displayName3.text = ("The Property is for sale!");

            // If player has not passed go no action is to occur
            if (!player.passedGo)
            {
                if (rolledDouble == 0)
                {
                    canEndTurn(true);
                } else canRoll(true);

                Debug.Log("not passed go yet!");
                return;
            }

            // If player has the balance to purchase give them the option
            if (location.Cost < player.balance)
            {
                canBuyProperty(true);
            }

            // If total number of people passed go that aren't player is 2 or more auction can start
            if ((totalpassed - (player.passedGo ? 1 : 0)) >= 2)
            {
                canStartAuction(true); //checks to make sure at least 2 can auction before auctioning
            }
            // Else if not enough players can join the auction, allow end turn
            else 
            { 
                canEndTurn(true); 
            }
        }
        // Else if landed on property that is owned by a player
        else if (location.CanBeBought && !bank.BankOwnedProperties.Contains(position))
        {
            // If player owns the property do nothing
            if (player.properties.Contains(position)) {
                Debug.Log("Property owned by the same player");
                displayName3.text = ("Property owned by the same player");
            }
            // Else payrent
            else {
                Debug.Log("Property owned by another player");
                displayName3.text = ("Property owned by another player");
                if (!location.mortgaged && (location.Owner.inJail == -1))
                {
                    payRent(player, location);
                }

                // If rolled a double give chance to roll again
                if (rolledDouble > 0)
                {
                    canRoll(true);
                }
            }
        }

        // Landed on oppurtunity knocks 7, 22, 36
        else if (position == 7 | position == 22 | position == 36)
        {
            Debug.Log("Landed on oppurtunity knocks");
            displayName3.text = ("Landed on oppurtunity knocks");
            oppKnock(player);
        }

        // Landed on pot luck 2, 17, 33
        else if (position == 2 | position == 17 | position == 33)
        {
            Debug.Log("Landed on pot luck");
            displayName3.text = ("Landed on pot luck");
            potLuck(player);
        }

        // Landed on income tax 4
        else if (position == 4)
        {
            Debug.Log("Landed on Income Tax and charged $200");
            displayName3.text = ("Landed on Income Tax and charged $200");
            _DepositToFreeParking(player, 200);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on free parking 20
        else if (position == 20)
        {
            Debug.Log($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            displayName3.text = ($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            player.balance += bank.FreeParkingBalance;
            bank.FreeParkingBalance = 0;
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go to jail 30
        else if (position == 30)
        {
            Debug.Log("Go to jail");
            displayName3.text = ("Go to jail");
            _GoToJail(player);
        }

        // Landed on super tax 38
        else if (position == 38)
        {
            Debug.Log("Landed on Super Tax and charged $100");
            Debug.Log(player.balance);
            _DepositToFreeParking(player, 100);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go 0
        else if (position == 0)
        {
            // Doesn't need any logic because pass go logic implemented in moveForward()
            Debug.Log("Landed on GO");
            displayName3.text = ("Landed on GO");
        }

        // If no actions are available and rolled double, next roll
        Debug.Log($"{location.CanBeBought}, {!bank.BankOwnedProperties.Contains(position)}, {rolledDouble}");
        if ((location.Group == "" && rolledDouble > 0) | ((location.CanBeBought && !bank.BankOwnedProperties.Contains(position) && (rolledDouble > 0))))
        {
            Debug.Log("SPECIALX");
            displaydouble.text = "you rolled a double!";
            canRoll(true);
        }
        // Else can not roll
        else canRoll(false);

        // If nothing for player to do, allow end turn
        if ((rolledDouble == 0) && (!jailOption.activeSelf) && (!propertyButton.activeSelf))
        {
            canEndTurn(true);
        }
    }

    /// <summary>
    /// Purchase property logic
    /// </summary>
    /// <param name="player">Player_ purchasing</param>
    /// <param name="location">Property being purchased</param>
    /// <param name="amount">How much the property is for, if no amount set will default to a -ve value to flag</param>
    void purchaseProperty(Player_ player, Property location, int amount = -1)
    {
        // If amount not set, amount is location default cost
        if (amount == -1)
        {
            amount = location.Cost;
        }

        player.addProperty(location);
        BankTrans(-amount, player);
        location.Owner = player;
        UpdateBalanceUI();
    }

    /// <summary>
    /// Mortgage property logic
    /// </summary>
    /// <param name="player">Player mortgaging the property</param>
    /// <param name="location">Location being mortgaged</param>
    void mortgageProperty(Player_ player, Property location)
    {
        BankTrans(location.Cost / 2);
        location.mortgaged = true;
        UpdateBalanceUI();

    }

    /// <summary>
    /// Unmortgage property logic
    /// </summary>
    /// <param name="player">Player unmortgaging the property</param>
    /// <param name="location">Location being unmortgaged</param>
    void unMortgageProperty(Player_ player, Property location)
    {
        // If player can't afford return
        if (player.balance < (location.Cost / 2))
        {
            return;
        }

        BankTrans(-(location.Cost / 2));
        location.mortgaged = false;
        UpdateBalanceUI();
    }
    
    /// <summary>
    /// Sell property logic
    /// </summary>
    /// <param name="player">Player selling the property</param>
    /// <param name="location">Property being sold</param>
    void sellProperty(Player_ player, Property location)
    {
        // If location is mortgaged only reward 1/2 the price
        if (location.mortgaged)
        {
            BankTrans(location.Cost / 2);
        } else BankTrans(location.Cost);

        player.removeProperty(location);
        location.Owner = null;
        bank.BankOwnedProperties.Add(player.pos);
        UpdateBalanceUI();
    }

    void payRent(Player_ player, Property location)
    {
        Debug.Log($"{location.Name} {location.NumberOfHouses} {location.RentUnimproved} {location.Rent1House} {location.Rent2Houses} {location.Rent3Houses} {location.Rent4Houses} {location.RentHotel}");

        if (location.Group == "Station")
        {
            int amount = 25 * System.Convert.ToInt32(System.Math.Pow(2, (location.Owner.Sets["Station"] - 1)));
            PlayerTrans(player, location.Owner, amount);
            return;
        }
        else if (location.Group == "Utilities")
        {
            int amount;
            if (location.Owner.Sets["Utilities"] == 2)
            {
                amount = 10;
            }
            else amount = 4;

            amount = roll * amount;
            PlayerTrans(player, location.Owner, amount);
            return;
        }

        if (location.NumberOfHouses == 0)
        {
            if (location.Owner.OwnedSets.Contains(location.Group))
            {
                PlayerTrans(player, location.Owner, (location.RentUnimproved * 2));
                return;
            }
            PlayerTrans(player, location.Owner, location.RentUnimproved);
        }
        else if (location.NumberOfHouses == 1)
        {
            PlayerTrans(player, location.Owner, location.Rent1House);
        }
        else if (location.NumberOfHouses == 2)
        {
            PlayerTrans(player, location.Owner, location.Rent2Houses);
        }
        else if (location.NumberOfHouses == 3)
        {
            PlayerTrans(player, location.Owner, location.Rent3Houses);
        }
        else if (location.NumberOfHouses == 4)
        {
            PlayerTrans(player, location.Owner, location.Rent4Houses);
        }
        else if (location.NumberOfHouses == 5)
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
        displayName3.text = ($"{card.Description}");

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
        positionHandling(player);

        if (newPosition == 0)
        {
            BankTrans(200);
        }

    }

    //ui section tried to make a script for it didnt work

    public void canBuyProperty(bool boolean)
    {
        // CPU LOGIC
        CPUPush($"canBuyProperty {boolean}");

        auctionButton.SetActive(boolean);
        propertyButton.SetActive(boolean);
        canEndTurn(false);
    }
    public void canEndTurn(bool boolean)
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        if ((player.balance < 0) && boolean)
        {
            canEndTurn(false);
            return;
        }

        CPUPush($"canEndTurn {boolean}");
        endButton.SetActive(boolean);

        if (!boolean) // if false disable manage button
        {
            if (player.balance < 0)
            {
                canManage(true);
            }
            else canManage(false);
            return;
        }

        canRoll(false);

        if (player.properties.Count() > 0)
        {
            canManage(boolean);
        }

        if (player.inJail > -1)
        {
            canManage(false);
        }


    }

    public void canStartAuction(bool boolean)
    {
        if (!boolean)
        {
            CPUPush("canStartAuction false");
            auctionButton.SetActive(false);

            if (rolledDouble > 0)
            {
                canRoll(true);
                canEndTurn(false);
            }
            else
            {
                canRoll(false);
            }
            return;
        }

        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        bidders.Clear();
        foreach (var item in playerlist)
        {
            if ((item != player) && item.passedGo && (item.inJail == -1)) //checks to make sure players that have passed go added
            {
                bidders.Add(item);
            }
        }

        if (bidders.Count() <= 1)
        {
            canStartAuction(false);
            return;
        }

        CPUPush("canStartAuction true");
        auctionButton.SetActive(boolean);
    }

    public void DecidedToClick(GameObject button)
    {
        if (diceRoller.isRolling)
        {
            return;
        }

        string buttonName = button.name;

        Debug.Log("button name " + buttonName);
        if (buttonName == "buyPropertyButton")
        {
            buyProperty();
        }
        if (buttonName == "endTurnButton")
        {
            endTurn();
        }
        if (buttonName == "startAuctionButton")
        {
            startAuction();
        }

        if (buttonName == "bidButton")
        {
            string input = bidInputField.text;
            if (highestBidder == null)
            {
                highestBidder = CurrentPlayer.GetComponent<Player_>();
            }

            if (int.TryParse(input, out int bid) && (bid <= highestBidder.balance) && (bid > highestBid))
            {
                Debug.Log("Bid entered: " + bid);
                displayName3.text = ("latest bid is " + bid);
                nextBid(bid);
            }
            else
            {
                Debug.LogWarning("Invalid bid value.");
                displayName3.text = "invalid bid value please try again";
            }
        }

        if (buttonName == "skipButton")
        {
            nextBid(-1);
        }

        if (buttonName == "manageButton")
        {
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            canEndTurn(false);
            if (player.OwnedSets.Count() > 0)
            {
                managePanel.SetActive(true);
                endButton.SetActive(true);
            }
            else manageProperty(true);
        }

        if (buttonName == "setsButton")
        {
            managePanel.SetActive(false);
            manageSets(true);
            setDropdownChange();
        }

        if (buttonName == "propertiesButton")
        {
            managePanel.SetActive(false);
            manageProperty(true);
        }

        if (buttonName == "closeButton")
        {
            closeOptions();
        }

        if (buttonName == "propertiesDropdown")
        {
            manageProperty(true);
        }

        if (buttonName == "sellPropertyButton")
        {
            var (loc, player) = returnPropertyOnShow("Properties");

            sellProperty(player, loc);

            if (player.properties.Count() == 0)
            {
                closeOptions();
            }
            else manageProperty(true);
        }

        if (buttonName == "mortgageButton")
        {
            var (loc, player) = returnPropertyOnShow("Properties");

            if (loc.mortgaged)
            {
                unMortgageProperty(player, loc);
            } else mortgageProperty(player, loc);

            manageProperty(true);
        }

        if (buttonName == "sellHouseButton")
        {
            var (loc, player) = returnPropertyOnShow("Sets");

            player.sellHouse(loc);



            setDropdownChange();
        }

        if (buttonName == "upgradeHouseButton")
        {
            var (loc, player) = returnPropertyOnShow("Sets");

            player.upgradeHouse(loc);
            setDropdownChange();
        }

        if (buttonName == "pay50")
        {
            pay50();
        }

        if (buttonName == "jailFree")
        {
            usedJailFree();
        }

        if (buttonName == "stayInJail")
        {
            jailOption.SetActive(false);
            canEndTurn(true);
            canManage(false);
        }

        if (buttonName == "pay10")
        {
            oppKnocksOption.SetActive(false);
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            _DepositToFreeParking(player, 10);

            if (rolledDouble > 0)
            {
                canRoll(true);
            }
            else
            {
                canEndTurn(true);
            }

        }
        if (buttonName == "oppKnock")
        {
            oppKnocksOption.SetActive(false);
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            oppKnock(player);
        }
    }

    public void usedJailFree()
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        player.JailFreeCards--;
        player.inJail = -1;
        jailOption.SetActive(false);
        canEndTurn(true);
        canManage(false);
    }

    public void buyProperty()
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        canBuyProperty(false);
        canStartAuction(false);
        purchaseProperty(player, bank.Properties[player.pos]);

        if (rolledDouble == 0)
        {
            canRoll(false);
            canEndTurn(true);
        }
        else canRoll(true);
    }

    public void endTurn()
    {
        canEndTurn(false);
        if (abridgedGamemode && (playerTurn == (playerlist.Count() - 1)))
        {
            if ((Time.time - startTime) > endTime)
            {
                endAbridgedGame();
            }
        }

        running = false;
        NextTurn();
        UpdateBalanceUI();
        canRoll(true);
        Debug.Log("ended turn");
    }

    public void pay50()
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        _DepositToFreeParking(player, -50);
        player.inJail = -1;
        jailOption.SetActive(false);
        canEndTurn(true);
        canManage(false);
    }


    public void propertyDropdownChange()
    {
        editMortgageButton();
    }

    public void setDropdownChange()
    {
        Property loc;
        Player_ player;

        (loc, player) = returnPropertyOnShow("Sets");
        Debug.Log($"Name: {loc.Name}");

        housesInGroup(player, loc.Group);

        var (buy, sell) = canChangeHouses(loc);

        upgradeHouseButton.SetActive(buy);
        sellHouseButton.SetActive(sell);
    }

    public (bool, bool) canChangeHouses(Property loc) {
        bool sell;
        bool buy;
        if (loc.NumberOfHouses > 0 && loc.NumberOfHouses == highestHouses)
        {
            sell = true;
        }
        else sell = false;

        if (loc.NumberOfHouses == 5 | ((loc.NumberOfHouses != lowestHouses) && (lowestHouses != highestHouses)))
        {
            buy = false;
        }
        else buy = true;

        return (buy, sell);
    }

    public void housesInGroup(Player_ player, string group)
    {
        lowestHouses = 5;
        highestHouses = 0;
        foreach (var index in player.properties)
        {
            Property location = bank.Properties[index];
            if (location.Group == group)
            {
                if (lowestHouses > location.NumberOfHouses)
                {
                    lowestHouses = location.NumberOfHouses;
                }
                if (highestHouses < location.NumberOfHouses)
                {
                    highestHouses = location.NumberOfHouses;
                }
            }
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
        displayName3.text = ("Pay a $10 fine or take opportunity knocks");

        // CPU LOGIC
        if (player.AI)
        {
            if (player.balance >= 10)
            {
                _DepositToFreeParking(player, 10);
            } else
            {
                oppKnock(player);
            }

            return;
        }

        oppKnocksOption.SetActive(true);
    }
    public void _DepositToFreeParking(Player_ player, int amount)
    {
        Debug.Log("_DepositToFreeParking");
        displayName3.text = ($"Deposited: ${amount} to Free Parking");
        player.DepositToFreeParking(checkBalance(player, amount));

        canEndTurn(true);
    }
    public void _GoToJail(Player_ player, int amount = 0)
    {
        Debug.Log("_GoToJail");
        _Teleport(player, 10);
        player.inJail = 0;
        canManage(false);
        canEndTurn(false);
        jailOptions(player);
    }
    public void _ReceiveMoneyFromAll(Player_ player, int amount)
    {
        Debug.Log("_ReceiveMoneyFromAll");
        foreach (var sender in playerlist)
        {
            if (sender != player)
            {
                PlayerTrans(sender, player, amount);
            }
        }
    }
    public void _JailFreeCard(Player_ player, int amount)
    {
        Debug.Log("_JailFreeCard");
        displayName3.text = "You got a Get out of Jail free card!";
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
        canEndTurn(false);
        Debug.Log("_CardMove");

    }
    public void _Repairs(Player_ player, int version)
    {
        int noHouses = 0;
        int noHotels = 0;

        int amount = 0;

        foreach (var location in player.properties)
        {
            var temp = bank.Properties[location].NumberOfHouses;

            Debug.Log($"Houses {temp}");

            if (temp == 5)
            {
                noHotels++;
            }
            else noHouses += temp;
        }

        if (version == 0)
        {
            amount += 115 * noHotels;
            amount += 40 * noHouses;
        } else
        {
            amount += 100 * noHotels;
            amount += 25 * noHouses;
        }
        BankTrans(-amount);

    }

    public void canRoll(bool canRoll)
    {
        // CPU LOGIC
        CPUPush($"canRoll {canRoll}");

        next = canRoll;
        showing = !canRoll;
    }

    public void canManage(bool boolean)
    {
        CPUPush($"canManage {boolean}");
        manageButton.SetActive(boolean);
    }

    public void startAuction()
    {
        canBuyProperty(false);
        canStartAuction(false);

        highestBid = 0;
        highestBidder = null;

        playerBidPanel.SetActive(true);

        if (bidders[0].AI)
        {
            CPUAuction();
        }


        playerNameText.text = $"{bidders[0].playerName}, please enter your bid or skip";

        CPUPush($"canStartAuction false");
    }

    public void nextBid(int bid)
    {
        Player_ bidder = bidders[nextBidder];
        Debug.Log($"{bidder.playerName} bid {bid}");

        if (bid > highestBid)
        {
            highestBid = bid;
            highestBidder = bidder;
            Debug.Log("Highest bidder");
        }
        else
        {
            //remove this bidder from the bidding
            bidders.Remove(bidder);
            nextBidder--;
            Debug.Log($"Bidder removed, now only {bidders.Count()} people bidding");
        }

        if (bidders.Count() == 1)
        {
            if (highestBid != 0)
            {
                Player_ player = playerlist[playerTurn];

                purchaseProperty(highestBidder, bank.Properties[player.pos], highestBid);
            }

            endAuction();
            return;
        }


        if (nextBidder == bidders.Count - 1)
        {
            nextBidder = -1;
        } 

        nextBidder++;

        Debug.Log(bidders.Count());
        Debug.Log(nextBidder);
        if (bidders[nextBidder].AI)
        {
            CPUAuction();
        }

        playerNameText.text = $"{bidders[nextBidder].playerName}, please enter your bid or skip";
    }

    public void CPUAuction()
    {
        if (bidders[nextBidder].balance > 5)
        {
            nextBid(5);
        }
        else nextBid(-1);

    }

    public void endAuction()
    {
        canStartAuction(false);

        if (rolledDouble == 0)
        {
            canRoll(false);
            canEndTurn(true);
        }
        else
        {
            canRoll(true);
        }
        playerBidPanel.SetActive(false);

        Player_ player = playerlist[playerTurn];
        if (player.AI)
        {
            StartCoroutine(CPULogic(player));
        }
    }

    public void manageProperty(bool boolean)
    {
        if (boolean)
        {
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            List<string> showProps = new List<string>();
            
            foreach (int loc in player.properties)
            {
                showProps.Add(bank.Properties[loc].Name);
            }

            propertiesDropdown.ClearOptions();
            propertiesDropdown.AddOptions(showProps);
        }

        propertiesPanel.SetActive(boolean);
        endButton.SetActive(boolean);
        closeButton.SetActive(boolean);
        canEndTurn(!boolean);
        editMortgageButton();
    }

    public void manageSets(bool boolean)
    {
        if (boolean)
        {
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            List<string> showProps = new List<string>();

            foreach (int loc in player.properties)
            {
                if (player.OwnedSets.Contains(bank.Properties[loc].Group))
                {
                    Debug.Log($"{loc}");
                    showProps.Add(bank.Properties[loc].Name);
                }
            }

            setsDropdown.ClearOptions();
            setsDropdown.AddOptions(showProps);
        }

        setsPanel.SetActive(boolean);
        endButton.SetActive(boolean);
        closeButton.SetActive(boolean);
        editMortgageButton();
    }

    public void closeOptions()
    {
        closeButton.SetActive(false);
        setsPanel.SetActive(false);
        propertiesPanel.SetActive(false);
        canManage(true);

        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();
        if (player.balance >= 0)
        {
            canEndTurn(true);
        }
    }

    // Returns the property on show on either the "Sets" or "Properties" dropdown
    public (Property, Player_) returnPropertyOnShow(string type)
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        string propertyName = "";

        if (type == "Sets")
        {
            propertyName = setsDropdown.options[setsDropdown.value].text;
        }
        else if (type == "Properties")
        {
            propertyName = propertiesDropdown.options[propertiesDropdown.value].text; ;
        }

        foreach (var locIdx in player.properties)
        {
            var loc = bank.Properties[locIdx];
            if (loc.Name == propertyName)
            {
                return (loc, player);
            }
        }

        Debug.Log("SENDING NULL");
        return (null, null);
    }

    // When called it checks whether the location is mortgaged and changes the button text
    public void editMortgageButton()
    {
        var (loc, player) = returnPropertyOnShow("Properties");

        if (loc.mortgaged)
        {
            mortgageText.text = "Unmortgage";
        }
        else mortgageText.text = "Mortgage";
    }

    // If bankrupt, remove player and start next turn
    // If not return false
    public int bankrupt(Player_ player)
    {
        playerlist[playerTurn].gameObject.SetActive(false);

        playerlist.Remove(playerlist[playerTurn]);
        playerTurn--;

        if (playerTurn == -1)
        {
            playerTurn = playerlist.Count();
        }

        if (playerlist.Count == 1)
        {
            endLongGame();
            return 0;
        }

        int amount = player.checkLiquidation();

        foreach (int propertyIdx in player.properties)
        {
            var property = bank.Properties[propertyIdx];
            property.mortgaged = false;
            property.NumberOfHouses = 0;
            property.Owner = null;

            player.removeProperty(property);
        }

        Debug.Log($"Player turn: {playerTurn}");
        endTurn();
        return amount;
    }

    // When the time limit is reached, calculate every player's worth and crown the winner
    // End the game
    public void endAbridgedGame()
    {
        Player_ winner = null;
        int winnerLiquid = 0;
        foreach (var player in playerlist)
        {
            int playerLiquid = player.checkLiquidation();
            if (winnerLiquid < playerLiquid)
            {
                winnerLiquid = playerLiquid;
                winner = player;
            }
        }
        Debug.Log($"Winner is {winner} with a liquid value of {winnerLiquid}");
        endGame(winner);
    }

    // If player is the last remaning player this should be called to end the game
    public void endLongGame()
    {
        Debug.Log($"Winner: {playerlist[0]}");
        endGame(playerlist[0]);
    }

    // Logic to end the game
    public void endGame(Player_ winner)
    {
        // new scene with player wins and there value
        // player.checkLiquidation();
        winnerPanel.SetActive(true);
        winningText.text = $"You won {winner.playerName}!";
        winningBalance.text = $"End NetWorth: {winner.checkLiquidation()}";
    }

    // Code for which buttons appear when a player first enters jail
    public void jailOptions(Player_ player)
    {
        // CPU LOGIC
        if (player.AI)
        {
            if (player.JailFreeCards > 0)
            {
                Debug.Log("Used Jail Free Card");
                usedJailFree();
            } else if (player.balance > 50)
            {
                Debug.Log("Paid 50");
                pay50();
            } else
            {
                Debug.Log("Stayed in jail");
                jailOption.SetActive(false);
                canEndTurn(true);
                canManage(false);
            }
            return;
        }

        jailOption.SetActive(true);
        if (player.JailFreeCards == 0)
        {
            jailButton.SetActive(false);
        }
        else jailButton.SetActive(true);
    }

    public (string, bool) stackSplit(string input)
    {
        string[] parts = input.Split(' ');
        return (parts[0], bool.Parse(parts[1]));
    }

    public void CPUPush(string action)
    {
        if (cpuStack.Count() > 0)
        {
            if (cpuStack[cpuStack.Count() - 1] != action)
            {
                cpuStack.Add(action);
            }
        } else cpuStack.Add(action);
    }

    IEnumerator CPULogic(Player_ player)
    {
        // CPU LOGIC
        // Rolling works
        // Buying property works
        // Jail works
        // Purchasing hotels works
        // Opp knock option works
        UpdateBalanceUI();
        yield return new WaitForSeconds(1f);

        int stackIndex = cpuStack.Count() - 1;
        string tempAction;
        bool tempBool;

        Property location = bank.Properties[player.pos];

        for (int i = 0; i < cpuStack.Count; i++)
        {
            (tempAction, tempBool) = stackSplit(cpuStack[i]);

            pastActions[tempAction] = tempBool;
        }

        Debug.Log("--------------------");
        foreach (KeyValuePair<string, bool> pair in pastActions)
        {
            Debug.Log($"Key: {pair.Key}, Value: {pair.Value}");
        }
        Debug.Log("--------------------");

        if (pastActions["canBuyProperty"]) // if can buy property
        {
            if (location.Cost <= player.balance) // Logic whether to purchase
            {
                yield return new WaitForSeconds(1f);
                Debug.Log($"COST: {location.Cost}");
                buyProperty();
                Debug.Log("BOT PURCHASED");
            }
            else if (pastActions["canStartAuction"])
            {
                yield return new WaitForSeconds(1f);
                startAuction();
                Debug.Log("STARTED AUCTION");
            }

            StartCoroutine(CPULogic(player));
        }
        else if (pastActions["canStartAuction"])
        {
            yield return new WaitForSeconds(1f);
            startAuction();
            Debug.Log("STARTED AUCTION");
        }

        else if (pastActions["canRoll"]) // if can roll, roll
        {
            running = false;
        }

        else if (pastActions["canManage"] && pastActions["canEndTurn"]) // if can manage or end turn
        {
            // if player owns sets check if they are upgradeable
            if (player.OwnedSets.Count() > 0)
            {
                List<Property> upgradeableProperties = new List<Property>();

                foreach (string set in player.OwnedSets)
                {
                    // check whether upgrade is possible and decide
                    housesInGroup(player, set);

                    if (player.balance > (player.checkColourPrice(set) + 400))
                    {
                        foreach (int position in player.properties)
                        {
                            Property property = bank.Properties[position];

                            if ((property.Group == set))
                            {
                                var (buy, sell) = canChangeHouses(location);

                                if (buy)
                                {
                                    upgradeableProperties.Add(property);
                                }
                            }
                        }
                    }
                }

                // sort upgradeablePositions by property.position
                upgradeableProperties = upgradeableProperties
                    .OrderByDescending(p => p.Position)
                    .ToList();

                foreach (Property loc in upgradeableProperties)
                {
                    if (player.balance > (player.checkColourPrice(loc.Group) + 400))
                    {
                        player.upgradeHouse(loc);
                    }
                }

                if (player.balance < 1000)
                {
                    canManage(false);

                }
                else
                {
                    canManage(true);
                }
                StartCoroutine(CPULogic(player));
            }
            else
            {
                canManage(false);
                StartCoroutine(CPULogic(player));
            }

        }

        else if (pastActions["canManage"] && !pastActions["canEndTurn"]) // if can manage but not end turn
        {
            List<int> sortedProperties = player.properties.OrderBy(p => p).ToList();

            List<Property> noSetProperties = new List<Property>();
            List<Property> setProperties = new List<Property>();

            // sort properties
            foreach (int property in sortedProperties)
            {
                if (player.OwnedSets.Contains(bank.Properties[property].Group))
                {
                    setProperties.Add(bank.Properties[property]);
                }
                else noSetProperties.Add(bank.Properties[property]);
            }

            // mortgage all unfinished sets first
            foreach (Property property in noSetProperties)
            {
                if (!property.mortgaged)
                {
                    mortgageProperty(player, property);
                    if (player.balance >= 0) goto endOfNestedLoops;
                }
            }

            // sell all unfinished sets second
            foreach (Property property in noSetProperties)
            {
                sellProperty(player, property);
                if (player.balance >= 0) goto endOfNestedLoops;
            }

            List<Property> housedProperties = new List<Property>();
            foreach (Property property in setProperties)
            {
                if (property.NumberOfHouses > 0)
                {
                    housedProperties.Add(property);
                }
            }

            // sell houses
            while (housedProperties.Count() > 0)
            {
                var tempList = new List<Property>();
                foreach (Property property in housedProperties)
                {
                    var (buy, sell) = canChangeHouses(property);
                    while (sell)
                    {
                        player.sellHouse(property);

                        if (player.balance >= 0) goto endOfNestedLoops;

                        (buy, sell) = canChangeHouses(property);
                    }

                    if (property.NumberOfHouses > 0)
                    {
                        tempList.Add(property);
                    }
                }
                housedProperties = tempList;
            }

            // mortgage full sets
            foreach (Property property in setProperties)
            {
                if (!property.mortgaged)
                {
                    mortgageProperty(player, property);
                    if (player.balance >= 0) goto endOfNestedLoops;
                }
            }

            // sell full sets
            foreach (Property property in setProperties)
            {
                sellProperty(player, property);
                if (player.balance >= 0) goto endOfNestedLoops;
            }


        endOfNestedLoops:
            closeOptions();
            StartCoroutine(CPULogic(player));
        }

        else if (pastActions["canEndTurn"])
        {
            yield return new WaitForSeconds(1f);
            endTurn();
        }
    }

    public int checkBalance(Player_ player, int amount)
    {
        if (player.balance >= amount) // if player can pay amount, do so
        {
            Debug.Log($"Paid full amount {amount}");
            return amount;
        }
        else if (player.checkLiquidation() >= amount) // if player's liquid value can pay off amount, wait for money
        {
            manageProperty(true);
            canEndTurn(false);

            Debug.Log($"Paid full amount but requires selling {amount}");
            return amount;
        } 
        else
        {
            Debug.Log($"Bankrupt");
            return bankrupt(player);
        }
    }
}

// checkBankruptcy replace