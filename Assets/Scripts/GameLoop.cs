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
public class GameLoop : MonoBehaviour
{
    // private variables that will be changed by load screen
    private int playerAmount;
    private int AIplayerAmount;
    public bool abridgedGamemode;
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

    // housing variables
    [SerializeField] private GameObject houseprefab;
    [SerializeField] private GameObject hotelprefab;

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
    public List<string> moves = new List<string>();

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
        displayName3.text = "";
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

        // If action is required to go above negative balance
        if (pastActions["canManage"] && !pastActions["canEndTurn"] && (player.balance >= 0))
        {
            canEndTurn(true);
            if (player.AI)
            {
                CPULogic(player);
            }
        }
        else if (pastActions["canManage"] && !pastActions["canEndTurn"])
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
        endTime = PlayerAmounts.UpdateGameSettingsGame() * 60;
        if (endTime > 0)
        {
            abridgedGamemode = true;
        } else
        {
            abridgedGamemode = false;
        }

        Debug.Log($"abridged: {abridgedGamemode} | endtime: {endTime}");

        // TODO get gamemode and therefore length of game

        // Checks if script is ran from Property-Tycoon scene and puts into testing mode
        if (playerAmount == 0)
        {
            playerAmount = 6;
            AIplayerAmount = 6;
            endTime = 320f;
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
        } else if (testing){
                    test();


                    testing = false;
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
            yield return new WaitForSeconds(2.5f);
        }

        // Roll the dice and wait 1.5s for it to finish
        (int roll, bool boolDouble) = DiceRoll();
        yield return new WaitForSeconds(1.5f);

        // Run roll logic
        onRoll(roll, boolDouble);

        // If player is AI run cpu logic
        if (player.AI)
        {
            Debug.Log("CPU LOGIC");
            StartCoroutine(CPULogic(player));
        }
    }

    /// <summary>
    /// Function for testing code, only runs if script ran from Property-Tycoon scene
    /// </summary>
    public void test()
    {

        //Debug.Log("TEST");
        //Player_ player = playerlist[0];
        //player.balance = 100000;
        //_Teleport(player, 2);
        //player = playerlist[1];
        //_Teleport(player, 3);
        //player = playerlist[2];
        //_Teleport(player, 4);
        //player = playerlist[3];
        //_Teleport(player, 5);
        //player = playerlist[4];
        //_Teleport(player, 6);
        //player = playerlist[5];
        //_Teleport(player, 7);

        //player = playerlist[0];

        //purchaseProperty(player, bank.Properties[1]);

        //player.upgradeHouse(bank.Properties[1]);
        //SpawnHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //SpawnHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //SpawnHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //SpawnHouse(bank.Properties[1]);
        //player.upgradeHouse(bank.Properties[1]);
        //SpawnHouse(bank.Properties[1]);

        //player.upgradeHouse(bank.Properties[11]);
        //SpawnHouse(bank.Properties[11]);
        //player.upgradeHouse(bank.Properties[11]);
        //SpawnHouse(bank.Properties[11]);
        //player.upgradeHouse(bank.Properties[11]);
        //SpawnHouse(bank.Properties[11]);
        //player.upgradeHouse(bank.Properties[11]);
        //SpawnHouse(bank.Properties[11]);
        //player.upgradeHouse(bank.Properties[11]);
        //SpawnHouse(bank.Properties[11]);

        //player.upgradeHouse(bank.Properties[21]);
        //SpawnHouse(bank.Properties[21]);
        //player.upgradeHouse(bank.Properties[21]);
        //SpawnHouse(bank.Properties[21]);
        //player.upgradeHouse(bank.Properties[21]);
        //SpawnHouse(bank.Properties[21]);
        //player.upgradeHouse(bank.Properties[21]);
        //SpawnHouse(bank.Properties[21]);
        //player.upgradeHouse(bank.Properties[21]);
        //SpawnHouse(bank.Properties[21]);

        //player.upgradeHouse(bank.Properties[31]);
        //SpawnHouse(bank.Properties[31]);
        //player.upgradeHouse(bank.Properties[31]);
        //SpawnHouse(bank.Properties[31]);
        //player.upgradeHouse(bank.Properties[31]);
        //SpawnHouse(bank.Properties[31]);
        //player.upgradeHouse(bank.Properties[31]);
        //SpawnHouse(bank.Properties[31]);
        //player.upgradeHouse(bank.Properties[31]);
        //SpawnHouse(bank.Properties[31]);

        //player.sellHouse(bank.Properties[31]);
        //DeSpawnHouse(bank.Properties[31]);

    }

    /// <summary>
    /// Logic for dealing with post dice roll
    /// </summary>
    /// <param name="rollValue">Value of the double dice roll</param>
    /// <param name="boolDouble">Boolean for whether player rolled a double</param>
    public void onRoll(int rollValue, bool boolDouble)
    {
        moves.Add($"Player rolled {rollValue}");
        
        roll = rollValue;

        // If rolled a double increase rolledDouble
        if (boolDouble)
        {
            moves.Add($"It was a double!");

            rolledDouble += 1;
            Debug.Log($"You rolled a double! {rolledDouble}");
            displaydouble.text = ($"You rolled a double!");

            // If player has rolled 3 doubles go to jail and stop logic
            if (rolledDouble == 3)
            {
                moves.Add($"Player rolled a double three times in a row...");
                rolledDouble = 0;
                _GoToJail(CurrentPlayer.GetComponent<Player_>());
                return;
            }
        }
        else rolledDouble = 0;

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
            moves.Add($"Player leaves jail");
            player.inJail = -1;
        }
        // Else if player is in jail but not for 2 rounds stay in jail, can end turn and stop logic
        else if (player.inJail > -1)
        {
            moves.Add($"Player is in jail, can't do anything!");
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

            moves.Add($"Player passed go!");
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
            moves.Add($"Player received {amount} from bank");
            player.ReceiveMoneyFromBank(amount);
        }
        // Else paybank amount
        else
        {
            moves.Add($"Player paid {-amount} to bank");
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
        moves.Add($"{sender.playerName} sent {amount} to {receiver.playerName}");
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
        canEndTurn(false);

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
                }
                else canRoll(true);

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
            if (player.properties.Contains(position))
            {
                Debug.Log("Property owned by the same player");
                displayName3.text = ("Property owned by the same player");
            }
            // Else payrent
            else
            {
                moves.Add($"Property owned by another player");
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
            moves.Add($"Landed on oppurtunity knocks");
            Debug.Log("Landed on oppurtunity knocks");
            displayName3.text = ("Landed on oppurtunity knocks");
            oppKnock(player);
        }

        // Landed on pot luck 2, 17, 33
        else if (position == 2 | position == 17 | position == 33)
        {
            moves.Add($"Landed on pot luck");
            Debug.Log("Landed on pot luck");
            displayName3.text = ("Landed on pot luck");
            potLuck(player);
        }

        // Landed on income tax 4
        else if (position == 4)
        {
            moves.Add($"Landed on income tax and charged $200");

           Debug.Log("Landed on Income Tax and charged $200");
            displayName3.text = ("Landed on Income Tax and charged $200");
            _DepositToFreeParking(player, 200);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on free parking 20
        else if (position == 20)
        {
            moves.Add($"Landed on free parking you have gained {bank.FreeParkingBalance}");

            Debug.Log($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            displayName3.text = ($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            player.balance += bank.FreeParkingBalance;
            bank.FreeParkingBalance = 0;
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go to jail 30
        else if (position == 30)
        {
            moves.Add($"Landed on go to jail!");
            Debug.Log("Go to jail");
            displayName3.text = ("Go to jail");
            _GoToJail(player);
        }

        // Landed on super tax 38
        else if (position == 38)
        {
            moves.Add($"Landed on Super Tax and charged $100");
            Debug.Log("Landed on Super Tax and charged $100");
            Debug.Log(player.balance);
            _DepositToFreeParking(player, 100);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go 0
        else if (position == 0)
        {
            // Doesn't need any logic because pass go logic implemented in moveForward()
            moves.Add($"Landed on GO");
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

        moves.Add($"{player.playerName} bought {location.Name} for {amount}");

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
        moves.Add($"{player.playerName} mortgaged {location.Name} for {location.Cost / 2}");
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

        moves.Add($"{player.playerName} unmortgaged {location.Name} for {location.Cost / 2}");

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
            moves.Add($"{player.playerName} sold {location.Name} for {location.Cost / 2}");
            BankTrans(location.Cost / 2);
        }
        else {

            moves.Add($"{player.playerName} sold {location.Name} for {location.Cost}");
            BankTrans(location.Cost); 
        
        }

        player.removeProperty(location);
        location.Owner = null;
        UpdateBalanceUI();
    }

    /// <summary>
    /// Pay rent logic, calculates correct amount owed for the property
    /// </summary>
    /// <param name="player"></param>
    /// <param name="location"></param>
    void payRent(Player_ player, Property location)
    {
        Debug.Log($"{location.Name} {location.NumberOfHouses} {location.RentUnimproved} {location.Rent1House} {location.Rent2Houses} {location.Rent3Houses} {location.Rent4Houses} {location.RentHotel}");

        moves.Add($"{player.playerName} has to pay rent to {location.Owner.playerName}");

        // If location is a station pay station rent
        if (location.Group == "Station")
        {
            int amount = 25 * System.Convert.ToInt32(System.Math.Pow(2, (location.Owner.Sets["Station"] - 1)));
            PlayerTrans(player, location.Owner, amount);
        }

        // Else if location is a utility pay utility rent
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

        // Else If checks number of houses and if in a owned set and pays correct amount
        else if (location.NumberOfHouses == 0)
        {
            if (location.Owner.OwnedSets.Contains(location.Group))
            {
                PlayerTrans(player, location.Owner, (location.RentUnimproved * 2));
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

    /// <summary>
    /// Performs first pot luck card in the queue on the player
    /// </summary>
    /// <param name="player">Player action will be performed on</param>
    public void potLuck(Player_ player)
    {
        // Takes first card in the queue
        Card card = bank.PLCards[0];
        bank.PLCards.RemoveAt(0);

        moves.Add($"{card.Description}");
        Debug.Log($"{card.Description}");

        var action = card.Action;
        var amount = card.Integer;

        // Runs it's equivalent function
        runMethod(player, action, amount);

        // Adds card back to the end of the queue
        bank.PLCards.Add(card);
    }

    /// <summary>
    /// Performs first oppurtunity knocks card in the queue on the player
    /// </summary>
    /// <param name="player">Player action will be performed on</param>
    public void oppKnock(Player_ player)
    {
        // Takes first card in the queue
        Card card = bank.OKCards[0];
        bank.OKCards.RemoveAt(0);

        moves.Add($"{card.Description}");
        Debug.Log($"{card.Description}");
        displayName3.text = ($"{card.Description}");

        var action = card.Action;
        var amount = card.Integer;

        // Runs it's equivalent function
        runMethod(player, action, amount);

        // Adds card back to the end of the queue
        bank.OKCards.Add(card);
    }

    /// <summary>
    /// Run method "action"
    /// </summary>
    /// <param name="player">Player being affected</param>
    /// <param name="action">Action occuring</param>
    /// <param name="Integer">Indicator of by how much or of what kind</param>
    public void runMethod(Player_ player, string action, int Integer)
    {
        var method = this.GetType().GetMethod(action, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (method != null)
        {
            method.Invoke(this, new object[] { player, Integer });
        }
    }


    /// <summary>
    /// Teleport player to new position without passing go
    /// </summary>
    /// <param name="player"></param>
    /// <param name="newPosition"></param>
    public void _Teleport(Player_ player, int newPosition)
    {
        player.pos = newPosition;
        player.gameObject.transform.position = boardPosition[newPosition];
        positionHandling(player);

        if (newPosition == 0)
        {
            BankTrans(200);
        }

    }

    /// <summary>
    /// Makes property UI visible or invisible determined by boolean
    /// </summary>
    /// <param name="boolean">Whether it should be visible</param>
    public void canBuyProperty(bool boolean)
    {
        // CPU Logic
        pastActions["canBuyProperty"] = boolean;

        auctionButton.SetActive(boolean);
        propertyButton.SetActive(boolean);
        canEndTurn(false);
    }

    /// <summary>
    /// Makes end turn UI visible or invisible determined by boolean
    /// </summary>
    /// <param name="boolean">Whether it should be visible</param>
    public void canEndTurn(bool boolean)
    {
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        // If player balance is negative, do not let player end turn
        if ((player.balance < 0) && boolean)
        {
            canEndTurn(false);
            return;
        }

        // CPU Logic
        pastActions["canEndTurn"] = boolean;
        endButton.SetActive(boolean);

        // If false and balance is positive disables manage properties button
        if (!boolean)
        {
            if (player.balance < 0)
            {
                canManage(true);
            }
            else canManage(false);
            return;
        }

        canRoll(false);

        // If player can theoretically manage properties allow them
        if (player.properties.Count() > 0)
        {
            canManage(boolean);
        }

        if (player.inJail > -1)
        {
            canManage(false);
        }
    }

    /// <summary>
    /// Makes start auction button visible or invisible determined by boolean
    /// And game check logic
    /// </summary>
    /// <param name="boolean">Whether it should be visible</param>
    public void canStartAuction(bool boolean)
    {
        // If boolean is false disable auction button
        if (!boolean)
        {
            pastActions["canStartAuction"] = false;
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

        // Create a list of permitted bidders
        bidders.Clear();
        foreach (var item in playerlist)
        {
            if ((item != player) && item.passedGo && (item.inJail == -1)) //checks to make sure players that have passed go added
            {
                bidders.Add(item);
            }
        }

        // If less than 2 permitted bidders, disable auction
        if (bidders.Count() < 2)
        {
            canStartAuction(false);
            return;
        }

        // CPU Logic
        pastActions["canStartAuction"] = true;
        auctionButton.SetActive(boolean);
    }

    /// <summary>
    /// If button is clicked it runs this function to determine which button
    /// and therefore which action is performed
    /// </summary>
    /// <param name="button">Button gameobject that was clicked</param>
    public void DecidedToClick(GameObject button)
    {
        // If dice is rolling no action can occur
        if (diceRoller.isRolling)
        {
            return;
        }

        // Determine which button was clicked
        string buttonName = button.name;
        Debug.Log("button name " + buttonName);

        // Self explanatory, will comment when required
        if (buttonName == "buyPropertyButton")
        {
            buyProperty();
        }

        else if (buttonName == "endTurnButton")
        {
            endTurn();
        }

        else if (buttonName == "startAuctionButton")
        {
            startAuction();
        }

        else if (buttonName == "bidButton")
        {
            string input = bidInputField.text;

            // If highest bidder is undefined, define it as current player
            if (highestBidder == null)
            {
                highestBidder = CurrentPlayer.GetComponent<Player_>();
            }

            // If bid is a valid bid parse it to nextbid()
            if (int.TryParse(input, out int bid) && (bid <= highestBidder.balance) && (bid > highestBid))
            {
                Debug.Log("Bid entered: " + bid);
                displayName3.text = ("latest bid is " + bid);
                nextBid(bid);
            }
            // Else retry
            else
            {
                Debug.LogWarning("Invalid bid value.");
                displayName3.text = "invalid bid value please try again";
            }
        }

        else if (buttonName == "skipButton")
        {
            nextBid(-1);
        }

        else if (buttonName == "manageButton")
        {
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();
            canEndTurn(false);

            // If player owns no sets, do not give them the option of managing their sets
            if (player.OwnedSets.Count() > 0)
            {
                managePanel.SetActive(true);
                endButton.SetActive(true);
            }
            else manageProperty(true);
        }

        else if (buttonName == "setsButton")
        {
            // This button is actually the change of a dropdown and is handled accordingly
            managePanel.SetActive(false);
            manageSets(true);
            setDropdownChange();
        }

        else if (buttonName == "propertiesButton")
        {
            managePanel.SetActive(false);
            manageProperty(true);
        }

        else if (buttonName == "closeButton")
        {
            closeOptions();
        }

        else if (buttonName == "propertiesDropdown")
        {
            manageProperty(true);
        }

        else if (buttonName == "sellPropertyButton")
        {
            var (loc, player) = returnPropertyOnShow("Properties");

            sellProperty(player, loc);

            if (player.properties.Count() == 0)
            {
                closeOptions();
            }
            else manageProperty(true);
        }

        else if (buttonName == "mortgageButton")
        {
            var (loc, player) = returnPropertyOnShow("Properties");

            if (loc.mortgaged)
            {
                unMortgageProperty(player, loc);
            }
            else mortgageProperty(player, loc);

            manageProperty(true);
        }

        else if (buttonName == "sellHouseButton")
        {
            var (loc, player) = returnPropertyOnShow("Sets");
            player.sellHouse(loc);
            DeSpawnHouse(loc);

            setDropdownChange();
        }

        else if (buttonName == "upgradeHouseButton")
        {
            var (loc, player) = returnPropertyOnShow("Sets");
            player.upgradeHouse(loc);
            SpawnHouse(loc);

            setDropdownChange();
        }

        else if (buttonName == "pay50")
        {
            pay50();
        }

        else if (buttonName == "jailFree")
        {
            usedJailFree();
        }

        else if (buttonName == "stayInJail")
        {
            jailOption.SetActive(false);
            canEndTurn(true);
            canManage(false);
        }

        else if (buttonName == "pay10")
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
        else if (buttonName == "oppKnock")
        {
            oppKnocksOption.SetActive(false);
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            oppKnock(player);
        }
    }

    /// <summary>
    /// If used jail free, get out of jail and decrease jail free card count
    /// </summary>
    public void usedJailFree()
    {
        moves.Add($"Player used get out of jail free");
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        player.JailFreeCards--;
        player.inJail = -1;
        jailOption.SetActive(false);
        canEndTurn(true);
        canManage(false);
    }

    /// <summary>
    /// If property is purchased from UI perform neccessary actions
    /// </summary>
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

    /// <summary>
    /// When turn is ended it checks if anyone has one the game if in abridged gamemode
    /// And makes the neccessary changes to the visible UI
    /// </summary>
    public void endTurn()
    {
        moves.Add($"Player ended turn");

        Debug.Log("vvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
        string items = "";
        foreach (string item in moves)
        {
            items = $"{items}\n{item}";
            Debug.Log(item); ;
        }
        displayName3.text = items;
        Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
        moves.Clear();

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

    /// <summary>
    /// Pays 50 to free parking to leave jail
    /// </summary>
    public void pay50()
    {
        moves.Add($"Player paid 50 to leave jail");
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        _DepositToFreeParking(player, -50);
        player.inJail = -1;
        jailOption.SetActive(false);
        canEndTurn(true);
        canManage(false);
    }

    /// <summary>
    /// When property dropdown is changed, edit what the mortgage button says
    /// </summary>
    public void propertyDropdownChange()
    {
        editMortgageButton();
    }

    /// <summary>
    /// If the sets dropdown is changed, edit whether u can see the upgrade or sell house options
    /// </summary>
    public void setDropdownChange()
    {
        Property loc;
        Player_ player;

        (loc, player) = returnPropertyOnShow("Sets");
        Debug.Log($"Name: {loc.Name}");

        // Runs the check on number of houses compared to it's fellow set members and returns whether player can buy/sell a house
        housesInGroup(player, loc.Group);
        var (buy, sell) = canChangeHouses(loc);

        upgradeHouseButton.SetActive(buy);
        sellHouseButton.SetActive(sell);
    }


    /// <summary>
    /// Check whether a house can be bought or sold on the particular property
    /// </summary>
    /// <param name="loc">Location being checked</param>
    /// <returns>(bool buy, bool sell) whewre they both represent whether you can buy or sell a house</returns>
    public (bool, bool) canChangeHouses(Property loc)
    {
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

    /// <summary>
    /// Sets the lowestHouses and highestHouses in the set to their correct values
    /// </summary>
    /// <param name="player">Player who owns the set</param>
    /// <param name="group">The set in question</param>
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

    /// <summary>
    /// Is an oppurtunity knocks action and calls bank to player payment of amount
    /// </summary>
    /// <param name="player">Player receiving money</param>
    /// <param name="amount">Amount of money</param>
    public void _ReceiveMoneyFromBank(Player_ player, int amount)
    {
        BankTrans(amount);
    }

    /// <summary>
    /// Is an oppurtunity knocks action and calls player to bank payment of amount
    /// </summary>
    /// <param name="player">Player receiving money</param>
    /// <param name="amount">Amount of money</param>
    public void _PayBank(Player_ player, int amount)
    {
        BankTrans(-amount);
    }

    /// <summary>
    /// Is an oppurtunity knocks action and gives the option between paying $10 or taking another oppurtunity knocks card
    /// </summary>
    /// <param name="player">Player given the option</param>
    /// <param name="amount">Amount of money</param>
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
            }
            else
            {
                oppKnock(player);
            }

            return;
        }

        oppKnocksOption.SetActive(true);
    }

    /// <summary>
    /// Adds amount from player to the free parking balance
    /// </summary>
    /// <param name="player">Player sending the money</param>
    /// <param name="amount">Amount of money being sent</param>
    public void _DepositToFreeParking(Player_ player, int amount)
    {
        moves.Add($"Player deposited: ${amount} to Free Parking");
        Debug.Log("_DepositToFreeParking");
        displayName3.text = ($"Deposited: ${amount} to Free Parking");
        player.DepositToFreeParking(checkBalance(player, amount));

        canEndTurn(true);
    }

    /// <summary>
    /// Sends player to jail and then gives them the option to get out
    /// </summary>
    /// <param name="player">Player to be sent to jail</param>
    /// <param name="amount">Redundant</param>
    public void _GoToJail(Player_ player, int amount = 0)
    {
        moves.Add($"Player is in jail!");
        Debug.Log("_GoToJail");
        _Teleport(player, 10);
        player.inJail = 0;
        canManage(false);
        canEndTurn(false);
        jailOptions(player);
    }

    /// <summary>
    /// Player receives money from every other player
    /// </summary>
    /// <param name="player">Player receiving money</param>
    /// <param name="amount">Amount of money the player is receiving from every other player</param>
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

    /// <summary>
    /// Append a jail free card to player
    /// </summary>
    /// <param name="player">Player receiving the jail free card</param>
    /// <param name="amount">Redundant</param>
    public void _JailFreeCard(Player_ player, int amount=1)
    {
        Debug.Log("_JailFreeCard");
        displayName3.text = "You got a Get out of Jail free card!";
        player.JailFreeCards += 1;
    }

    /// <summary>
    /// Is an oppurtunity knocks action and will calculate how many positions to move to get to the desired square
    /// </summary>
    /// <param name="player">Player to move</param>
    /// <param name="amount">Which location index to move to</param>
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

    /// <summary>
    /// Is an oppurtunity knocks action that calculates how much repairs would cost player and then charges them said amount
    /// </summary>
    /// <param name="player">Player who needs repairs</param>
    /// <param name="version">0 | else : If 0 $115 per hotel and 40 per house Else $100 per hotel and $25 per house</param>
    public void _Repairs(Player_ player, int version)
    {
        int noHouses = 0;
        int noHotels = 0;

        int amount = 0;

        // Calculates number of hotels and houses player owns
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

        /// Charges depending on version of the card
        if (version == 0)
        {
            amount += 115 * noHotels;
            amount += 40 * noHouses;
        }
        else
        {
            amount += 100 * noHotels;
            amount += 25 * noHouses;
        }
        BankTrans(-amount);

    }

    /// <summary>
    /// Shows the dice depending on boolean
    /// </summary>
    /// <param name="canRoll">Boolean whether the player can roll</param>
    public void canRoll(bool canRoll)
    {
        // CPU LOGIC
        pastActions["canRoll"] = canRoll;

        next = canRoll;
        showing = !canRoll;
    }

    /// <summary>
    /// Shows the management UI depending on boolean
    /// </summary>
    /// <param name="boolean">Boolean whether the player can manage properties or not</param>
    public void canManage(bool boolean)
    {
        pastActions["canManage"] = boolean;
        manageButton.SetActive(boolean);
    }

    /// <summary>
    /// Starts the auction and shows the auction UI
    /// </summary>
    public void startAuction()
    {
        moves.Add($"Auction started!");
        // Disable previous UI
        canBuyProperty(false);
        canStartAuction(false);

        // Sets default values
        highestBid = 0;
        highestBidder = null;

        playerBidPanel.SetActive(true);

        // If bidder 1 is AI start the bid
        if (bidders[0].AI)
        {
            CPUAuction();
        }


        playerNameText.text = $"{bidders[0].playerName}, please enter your bid or skip";

        // CPU Logic
        pastActions["canStartAuction"] = false;
    }

    /// <summary>
    /// Performs the logic after a bid is entered
    /// </summary>
    /// <param name="bid">Amount player has bid</param>
    public void nextBid(int bid)
    {
        Player_ bidder = bidders[nextBidder];
        Debug.Log($"{bidder.playerName} bid {bid}");

        // If bid is higher than highest bid replace it
        if (bid > highestBid)
        {
            highestBid = bid;
            highestBidder = bidder;
            Debug.Log("Highest bidder");
            moves.Add($"New highest bidder {bidder.playerName} with a bid of {highestBid}");
        }
        // Else remove the bidder from the bidding rotation
        else
        {
            bidders.Remove(bidder);
            nextBidder--;
            Debug.Log($"Bidder removed, now only {bidders.Count()} people bidding");
            moves.Add($"Bidder removed, now only {bidders.Count()} people bidding");
        }

        // If bidder is the final bidder, they won and purchase the property for the highest bid var
        if (bidders.Count() == 1)
        {
            if (highestBid != 0)
            {
                Player_ player = playerlist[playerTurn];
                moves.Add($"{bidder.playerName} won with a bid of {highestBid}!");
                purchaseProperty(highestBidder, bank.Properties[player.pos], highestBid);
            }

            // End the auction and break
            endAuction();
            return;
        }

        // If nextbidder is higher than their are bidders, loop back
        if (nextBidder == bidders.Count - 1)
        {
            nextBidder = -1;
        }

        nextBidder++;

        Debug.Log(bidders.Count());
        Debug.Log(nextBidder);

        // CPU Logic
        if (bidders[nextBidder].AI)
        {
            CPUAuction();
        }

        playerNameText.text = $"{bidders[nextBidder].playerName}, please enter your bid or skip";
    }

    /// <summary>
    /// Ran when it is an AI player's turn to bid in an auction
    /// </summary>
    public void CPUAuction()
    {
        int position = playerlist[playerTurn].pos;

        Property property = bank.Properties[position];
        Player_ player = bidders[nextBidder];

        string set = property.Group;
        float randomValue;
        
        // If player has atleast one of the set
        if (player.Sets.ContainsKey(set))
        {
            // If player is one off full set
            if (bank.PropertiesPerSet[set] == (player.Sets[set] + 1))
            {
                randomValue = Random.Range(1.5f, 2f);
                if ((property.Cost * randomValue) <= player.balance)
                {
                    nextBid(Mathf.CeilToInt(property.Cost * randomValue));
                    return;
                }
            }

            // Else
            randomValue = Random.Range(1f, 1.5f);
            if ((property.Cost * randomValue) <= player.balance)
            {
                nextBid(Mathf.CeilToInt(property.Cost * randomValue));
                return;
            }
        }

        // Else
        randomValue = Random.Range(0.6f, 1f);
        if ((property.Cost * randomValue) <= player.balance)
        {
            nextBid(Mathf.CeilToInt(property.Cost * randomValue));
            return;
        }

        // Else
        nextBid(-1);
    }

    /// <summary>
    /// End auction by disabling all the relative UI and letting the player who's go it is continue
    /// </summary>
    public void endAuction()
    {
        canStartAuction(false);

        // If player did not roll a double let them end turn
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

        // CPU Logic
        Player_ player = playerlist[playerTurn];
        if (player.AI)
        {
            StartCoroutine(CPULogic(player));
        }
    }

    /// <summary>
    /// UI for managing property when button is clicked. Visibility depends on boolean
    /// </summary>
    /// <param name="boolean">Whether to make management visible or not</param>
    public void manageProperty(bool boolean)
    {
        // If visibility to be set to true, show all properties owned by player in the dropdown
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

    /// <summary>
    /// UI for managing sets when button is clicked. Visibility depends on boolean
    /// </summary>
    /// <param name="boolean">Whether to make management of sets visible or not</param>
    public void manageSets(bool boolean)
    {
        // If visibility to be set true, show all properties in owned sets by player in the dropdown
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

    /// <summary>
    /// On close button press make every option deactivated
    /// </summary>
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

    /// <summary>
    /// Returns the property on show on either the "Sets" or "Properties" dropdown
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns location and location owner selected in dropdown</returns>
    public (Property, Player_) returnPropertyOnShow(string type)
    {
        // Set variables
        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();
        string propertyName = "";

        // If sets on show then property = property on show in set dropdown
        if (type == "Sets")
        {
            propertyName = setsDropdown.options[setsDropdown.value].text;
        }
        // Else if properties on show then property = property on show in properties dropdown
        else if (type == "Properties")
        {
            propertyName = propertiesDropdown.options[propertiesDropdown.value].text; ;
        }

        // Searches owned properties and returns it's location and the player
        foreach (var locIdx in player.properties)
        {
            var loc = bank.Properties[locIdx];
            if (loc.Name == propertyName)
            {
                return (loc, player);
            }
        }

        // If nothing return Null
        Debug.Log("SENDING NULL");
        return (null, null);
    }

    /// <summary>
    /// When called it checks whether the location on show is mortgaged and changes the button text
    /// </summary>
    public void editMortgageButton()
    {
        var (loc, player) = returnPropertyOnShow("Properties");

        if (loc.mortgaged)
        {
            mortgageText.text = "Unmortgage";
        }
        else mortgageText.text = "Mortgage";
    }

    /// <summary>
    /// Ran when a player cannot afford to pay the full amount owed even when liquid
    /// Returns the max amount they can pay and returns all property to the bank
    /// </summary>
    /// <param name="player">Player who is bankrupt</param>
    /// <returns>Amount the can afford to pay</returns>
    public int bankrupt(Player_ player)
    {
        moves.Add($"{player.playerName} went bankrupt!");
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

    /// <summary>
    /// When the time limit is reached, calculate every player's worth and crown the winner
    /// End the game
    /// </summary>
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

    /// <summary>
    /// If long game ends, crown winner as the only remaining player
    /// </summary>
    public void endLongGame()
    {
        Debug.Log($"Winner: {playerlist[0]}");
        endGame(playerlist[0]);
    }

    /// <summary>
    /// Logic to end the game and show the winner and their networth
    /// </summary>
    /// <param name="winner">Player that won the game</param>
    public void endGame(Player_ winner)
    {
        // TODO new scene with player wins and there value
        winnerPanel.SetActive(true);
        winningText.text = $"You won {winner.playerName}!";
        winningBalance.text = $"End NetWorth: {winner.checkLiquidation()}";
    }

    /// <summary>
    /// Acivates the UI for which buttons appear when a player first enters jail
    /// </summary>
    /// <param name="player">Player being given the options</param>
    public void jailOptions(Player_ player)
    {
        // CPU LOGIC
        if (player.AI)
        {
            if (player.JailFreeCards > 0)
            {

                Debug.Log("Used Jail Free Card");
                usedJailFree();
            }
            else if (player.balance > 50)
            {
                Debug.Log("Paid 50");
                pay50();
            }
            else
            {
                Debug.Log("Stayed in jail");
                jailOption.SetActive(false);
                canEndTurn(true);
                canManage(false);
            }
            return;
        }

        // Human player UI show
        jailOption.SetActive(true);
        if (player.JailFreeCards == 0)
        {
            jailButton.SetActive(false);
        }
        else jailButton.SetActive(true);
    }

    /// <summary>
    /// Coroutine for the cpu logic to run, looks at every possible thing a human player could do in it's position and makes decisions accordingly
    /// </summary>
    /// <param name="player">AI player needing decision making</param>
    /// <returns>Yield returns timings to make the bot more lifelike</returns>
    IEnumerator CPULogic(Player_ player)
    {
        UpdateBalanceUI();
        yield return new WaitForSeconds(1f);
        displayName3.text = "";
        Property location = bank.Properties[player.pos];

        // Outputs all possible UI options to console
        Debug.Log("--------------------");
        foreach (KeyValuePair<string, bool> pair in pastActions)
        {
            Debug.Log($"Key: {pair.Key}, Value: {pair.Value}");
        }
        Debug.Log("--------------------");

        // If a possible action is buy property consider it's purchase
        if (pastActions["canBuyProperty"])
        {
            if (location.Cost <= player.balance)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log($"COST: {location.Cost}");
                buyProperty();
                Debug.Log("BOT PURCHASED");
            }
            else if (pastActions["canStartAuction"]) // If bot does not purchase and can start auction, do accordingly
            {
                yield return new WaitForSeconds(1f);
                startAuction();
                Debug.Log("STARTED AUCTION");
            }

            StartCoroutine(CPULogic(player)); // After purchase, auction or skip re-equate decisions
        }
        else if (pastActions["canStartAuction"]) // If no option of purchasing but option of auctioning, start auction
        {
            yield return new WaitForSeconds(1f);
            startAuction();
            Debug.Log("STARTED AUCTION");
        }

        else if (pastActions["canRoll"]) // if can roll, roll
        {
            running = false;
        }

        else if (pastActions["canManage"] && pastActions["canEndTurn"]) // if can manage or end turn decide whether to purchase houses or not
        {
            // if player owns sets check if they are upgradeable
            if (player.OwnedSets.Count() > 0)
            {
                List<Property> upgradeableProperties = new List<Property>();

                foreach (string set in player.OwnedSets)
                {
                    // check whether upgrade is possible and decide depending on overflow money
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

                // sort upgradeablePositions by property.position so most desirable are first choice
                upgradeableProperties = upgradeableProperties
                    .OrderByDescending(p => p.Position)
                    .ToList();

                // For every upgradeable position, make decision on upgrade
                foreach (Property loc in upgradeableProperties)
                {
                    if (player.balance > (player.checkColourPrice(loc.Group) + 400))
                    {
                        player.upgradeHouse(loc);
                        SpawnHouse(loc);
                    }
                }

                // If player has a large overflow of money, run again
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

        else if (pastActions["canManage"] && !pastActions["canEndTurn"]) // if can manage but not end turn therefore requiring selling to the bank
        {
            List<int> sortedProperties = player.properties.OrderBy(p => p).ToList();

            List<Property> noSetProperties = new List<Property>();
            List<Property> setProperties = new List<Property>();

            // sort properties into sets and no sets
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
                        DeSpawnHouse(property);

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

    /// <summary>
    /// Logic for checking if a player can afford a required transaction
    /// If player can afford, return normal amount
    /// If player can't afford but can sell stuff to afford, make that a requirement, return normal amount
    /// If player can't afford even if liquidified, return liquid value and bankrupt player
    /// </summary>
    /// <param name="player">Player who is being checked</param>
    /// <param name="amount">Amount they are required to send</param>
    /// <returns></returns>
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
        else // if liquid amount is not enough, bankrupt player
        {
            Debug.Log($"Bankrupt");
            return bankrupt(player);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="prop"></param>
    public void SpawnHouse(Property prop, bool hotel = false, int house = -1)
    {
        var houses = prop.NumberOfHouses;
        var position = prop.Position;

        float constant = .23333f;
        var obj = houseprefab;

        if (house != -1)
        {
            houses = house;
        }

        if (hotel)
        {
            houses = 1;
            obj = hotelprefab;
        }
        else if (houses > 4)
        {
            Debug.Log($"Number of houses {prop.NumberOfHouses}");
            Destroy(GameObject.Find($"House {prop.Position}: 4"));
            Destroy(GameObject.Find($"House {prop.Position}: 3"));
            Destroy(GameObject.Find($"House {prop.Position}: 2"));
            Destroy(GameObject.Find($"House {prop.Position}: 1"));
            SpawnHouse(prop, true);
            return;
        }

        

        Vector3 targetpos = boardPosition[position];
        float multiplehouses = constant * (houses - 1);
        var trans = false;

        if (!hotel) {
            if (position > 0 && position < 10)
            {
                targetpos = new Vector3(targetpos.x - 0.4f + multiplehouses, targetpos.y, targetpos.z + 0.55f);
            }
            else if (position > 10 && position < 20)
            {
                targetpos = new Vector3(targetpos.x + 0.5f, targetpos.y, targetpos.z + 0.4f + multiplehouses);
                trans = true;
            }
            else if (position > 20 && position < 30)
            {
                targetpos = new Vector3(targetpos.x + 0.4f - multiplehouses, targetpos.y, targetpos.z - 0.35f);

            }
            else if (position > 30 && position < 40)
            {
                targetpos = new Vector3(targetpos.x - 0.5f, targetpos.y, targetpos.z + 0.4f - multiplehouses);
                trans = true;
            } 
        }
        else
        {
      
            if (position > 0 && position < 10)
            {
                targetpos = new Vector3(targetpos.x, targetpos.y, targetpos.z + 0.5f);
                
            }
            else if (position > 10 && position < 20)
            {
                targetpos = new Vector3(targetpos.x + 0.4f, targetpos.y, targetpos.z);
                trans = true;
            }
            else if (position > 20 && position < 30)
            {
                targetpos = new Vector3(targetpos.x, targetpos.y, targetpos.z - 0.35f);
            }
            else if (position > 30 && position < 40)
            {
                targetpos = new Vector3(targetpos.x - 0.5f, targetpos.y, targetpos.z);
                trans = true;
            }
        }
        
        obj = Instantiate(obj, targetpos, Quaternion.identity);
        if (trans)
        {
            obj.transform.Rotate(0f, 90f, 0f);
        }
        
        obj.name = $"House {prop.Position}: {prop.NumberOfHouses}";
    }

    public void DeSpawnHouse(Property prop)
    {
        Debug.Log("DESPAWN");
        Debug.Log($"House {prop.Position}: 5");
        if (prop.NumberOfHouses == 4)
        {
            Destroy(GameObject.Find($"House {prop.Position}: 5"));
            Debug.Log("Destroyed");
            SpawnHouse(prop, house: 1);
            SpawnHouse(prop, house: 2);
            SpawnHouse(prop, house: 3);
            SpawnHouse(prop, house: 4);
        } else
        {
            Destroy(GameObject.Find($"House {prop.Position}: {prop.NumberOfHouses+1}"));
        }

        
    }
}
