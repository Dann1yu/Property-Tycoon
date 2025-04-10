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
    [SerializeField] private int playerAmount;
    [SerializeField] private GameObject PlayerObject;
    public int AIplayerAmount;
    public bool abridgedGamemode = true;

    public List<GameObject> characterPrefabs = new List<GameObject>(); 

    public int lowestHouses = 5;
    public int highestHouses = 0;

    public TextMeshProUGUI displayName1;
    public TextMeshProUGUI displayName2;
    public TextMeshProUGUI displayName3;
    public TextMeshProUGUI displaydouble;

    public GameObject ButtonObject;
    public TextMeshProUGUI txt;
    public Transform WhereYouWantButtonsParented;

    public GameObject auctionButton;
    public GameObject endButton;
    public GameObject propertyButton;
    public GameObject manageButton;
    public GameObject closeButton;
    public GameObject jailButton;

    public GameObject sellHouseButton;
    public GameObject upgradeHouseButton;

    public GameObject playerBidPanel;
    public TextMeshProUGUI playerNameText;
    public TMP_InputField bidInputField;

    public GameObject managePanel;
    public GameObject setsPanel;
    public GameObject propertiesPanel;
    public GameObject oppKnocksOption;
    public GameObject jailOption;
    public TMP_Dropdown propertiesDropdown;
    public TMP_Dropdown setsDropdown;
    public TextMeshProUGUI mortgageText;

    private bool admin;
    public float startTime;
    public float endTime;


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

    public bool next = true;
    public bool showing = true;

    public List<Player_> bidders = new List<Player_>();
    public int nextBidder = 0;
    public int highestBid = 0;
    public Player_ highestBidder;

    public List<string> cpuStack = new List<string>();

    private bool end = true;
    private bool buy = true;
    private bool auc = true;
    private bool rol = false;

    private bool running = false;

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
        canRoll(false);
        //showing = false;
        //diceRoller.RollDice();
        //return (3, false);
        return diceRoller.RollDice();
    }

    // Ends current term and starts next player's go

    public void PlayerStartedTheGame(int AmountOfPlayers=6, int AmountOfAI=0, int gamemode=0)
    {
        playerAmount = AmountOfPlayers;
        //add the other stuff here
        Debug.Log("tests: " + AmountOfPlayers +" "+ AmountOfAI +" "+ gamemode);
    }
    public void NextTurn()
    {
        cpuStack.Clear();
        playerTurn++;

        if (playerTurn >= playerlist.Count) // Loops back to first player
        {
            playerTurn = 0;
        }

        UpdateBalanceUI();
    }

    public void UpdateBalanceUI()
    {
        string current = playerlist[playerTurn].ToString();
        current = current.Remove(current.Length - 9);
        displayName1.text = current;
        displayName2.text = "Balance: $" + playerlist[playerTurn].balance.ToString();
        displaydouble.text = "";
        displayName3.text = "";
    }

    // Spawns x players with attached scripts "Player_" to hold required variables
    public void spawnPlayers(int Human, int AI)
    {
        var amount = Human + AI;

        for (int i = 0; i < amount; i++)
        {
            PlayerObject = characterPrefabs[i];
            var spawnedPlayer = Instantiate(PlayerObject, new Vector3(10, 0.5f, 0), Quaternion.identity);
            spawnedPlayer.name = $"Player {i}";

            // Attach and initialize the Player_ script
            Player_ playerComponent = spawnedPlayer.AddComponent<Player_>();
            playerComponent.Initialize($"Player {i}", 1500); 

            // CPU LOGIC
            if (i >= Human)
            {
                playerComponent.AI = true;
            }

            playerlist.Add(playerComponent);
            Debug.Log("player count" + playerlist.Count);
        }
    }

    // Creates the board, spawns the player and sets current player to player 0
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
        closeButton.SetActive(false);
        oppKnocksOption.SetActive(false);
        jailOption.SetActive(false);

        // Calling the info from the loading scene
        var PlayerAmounts = GameObject.Find("GameController").GetComponent<LoadScene>();

        playerAmount = PlayerAmounts.UpdateGameSettingsPlayers();

        // CPU LOGIC
        AIplayerAmount = PlayerAmounts.UpdateGameSettingsAI();

        admin = false;
        if (playerAmount == 0)
        {
            playerAmount = 1;
            AIplayerAmount = 2;
            admin = true;
            Debug.Log("ADMIN MODE");

        }

        canBuyProperty(false);
        canEndTurn(false);
        canStartAuction(false);

        bank = FindFirstObjectByType<Bank_>(); // Finds the Bank_ instance in the scene

        spawnPlayers(playerAmount, AIplayerAmount);
        CurrentPlayer = GameObject.Find("Player 0");

        NextTurn();
    }

    // Checks every few frames
    public void Update()
    {
        // Checks if player can roll the dice and if it isn't showing
        if (!diceRoller.isRolling && next && !showing && !propertyButton.activeSelf)
        {
            diceRoller.ShowDice(true);
            canRoll(true);
        } 

        if (Input.GetKeyDown(KeyCode.UpArrow) | (playerlist[playerTurn].AI && !running))
        {
            if (next && rolledDouble == 0)
            {
                canRoll(false);

                if (admin)
                {
                    test();
                    admin = false;
                }
            }
            else if (!next)
            {
                return;
            }

            // if the dice is rolling do nothing
            if (diceRoller.isRolling)
            {
                return;
            }

            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();
            
            running = true;
            StartCoroutine(diceRollRoutine(player));
        }
    }

    IEnumerator diceRollRoutine(Player_ player)
    {
        if (player.AI)
        {
            yield return new WaitForSeconds(1f);
        }

        (int roll, bool boolDouble) = DiceRoll();
        yield return new WaitForSeconds(1.5f);

        onRoll(roll, boolDouble);
        
        if (player.AI) {
            Debug.Log("CPU LOGIC");
            CPULogic(player); 
        }

        running = false;
    }

    public void test()
    {
        endTime = 1200f;

        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        player.balance = 100000;

        purchaseProperty(player, bank.Properties[1]);
        purchaseProperty(player, bank.Properties[3]);

        purchaseProperty(player, bank.Properties[5]);
        purchaseProperty(player, bank.Properties[15]);

        purchaseProperty(player, bank.Properties[12]);
        purchaseProperty(player, bank.Properties[28]);

        player.upgradeHouse(bank.Properties[1]);
        player.upgradeHouse(bank.Properties[1]);
        player.upgradeHouse(bank.Properties[1]);
        player.upgradeHouse(bank.Properties[1]);
        player.upgradeHouse(bank.Properties[1]);
        player.upgradeHouse(bank.Properties[3]);
        player.upgradeHouse(bank.Properties[3]);
        player.upgradeHouse(bank.Properties[3]);
        player.upgradeHouse(bank.Properties[3]);

        mortgageProperty(player, bank.Properties[15]);

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

        Debug.Log(player.checkLiquidation());

        //_Repairs(player, 0);
        //_Repairs(player, 1);
    }

    // Logic for just after dice has been rolled
    public void onRoll(int nextRoll, bool boolDouble)
    {
        roll = nextRoll;
        if (boolDouble)
        {
            //showing = true;
            rolledDouble += 1;
            Debug.Log($"You rolled a double! {rolledDouble}");
            displaydouble.text = ($"You rolled a double!");

            if (rolledDouble == 3)
            {
                rolledDouble = 0;
                _GoToJail(CurrentPlayer.GetComponent<Player_>());
                return;
            }
        }        else rolledDouble = 0;

        moveForward(roll, CurrentPlayer.transform.position);
    }

    // move x spaces forward (if x is -ve then |x| backwards)
    void moveForward(int distance, Vector3 currentpos)
    {
        Debug.Log($"DiceRoll: {distance}");
        displayName3.text = ($"DiceRoll: {distance}");
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        // if player has been in jail for 2 rounds then let free
        if (player.inJail == 2)
        {
            player.inJail = -1;
        }
        // else if player is in jail but not for 2 rounds stay in jail
        else if (player.inJail > -1)
        {
            player.inJail += 1;
            Debug.Log($"YOU ARE IN JAIL LOSER NO GO FOR YOU {player.inJail}");
            displayName3.text = ($"YOU ARE IN JAIL NO GO FOR YOU! {player.inJail}");
            canEndTurn(true);
            return;
        }

        int oldposition = player.pos;
        int new_position =  distance + oldposition;

        if (new_position >= 40)
        {
            new_position = new_position - 40;
        }

        // if player is on or has passed go reward with $200
        if (new_position < oldposition && distance > 0)
        {
            Debug.Log("You passed go!");
            displayName3.text = ("You passed go!");
            BankTrans(200);

            if (!player.passedGo)
            {
                player.passedGo = true;
            }
        }

        Debug.Log("newpos: " + new_position);
        CurrentPlayer.transform.position = boardPosition[new_position];
        player.pos = new_position;
        //potential to add animation here

        positionHandling(player);
    }

    // Transaction to or from bank 
    // +ve amount is from the bank, -ve amount is to the bank
    public void BankTrans(int amount, Player_ player = null)
    {
        if (player == null)
        {
            player = CurrentPlayer.GetComponent<Player_>();
        }

        if (amount > 0)
        {
            player.ReceiveMoneyFromBank(amount);
        }
        else
        {
            if (!checkBankruptcy(player, amount))
            {
                player.PayBank(-amount);
            }
            else if (!checkTotalBankruptcy(player,amount))
            {
                managePanel.SetActive(true);
                displayName3.text = "please sell assets so that you may pay the charge";
                positionHandling(player);
                //returns them back to try again
            }
            else
            {
                player.PayBank(payMax(player));
            }
        }
        UpdateBalanceUI();
    }
    
    // Transaction from player to player
    public void PlayerTrans(Player_ sender, Player_ receiver, int amount)
    {
        if (!checkBankruptcy(sender, amount))
        {
            sender.PayPlayer(receiver, amount);
        }
        else if (!checkTotalBankruptcy(sender, amount))
        {
            managePanel.SetActive(true);
            displayName3.text = "please sell assets so that you may pay the charge";
            positionHandling(sender);
            //returns them back to try again
        }
        else
        {
            sender.PayBank(payMax(sender));
        }
        UpdateBalanceUI();
    }

    // Checks where the player landed and performs the correct action
    void positionHandling(Player_ player)
    {
        // handling
        var position = player.pos;
        var location = bank.Properties[position];
        canEndTurn(false);

        // Landed on property that can be purchased
        if (location.CanBeBought && bank.BankOwnedProperties.Contains(position))
        {
            canEndTurn(false);
            Debug.Log("Property for sale");
            displayName3.text = ("The Property is for sale!");
            if (!player.passedGo)
            {
                if (rolledDouble == 0)
                {
                    canEndTurn(true);
                } else canRoll(true);

                    Debug.Log("not passed go yet!");
                return;
            }
            if (location.Cost < player.balance)
            {
                canBuyProperty(true);
            }
            int totalpassed = 0;
            for (int i= 0;i<playerlist.Count; i++)
            {
                if (playerlist[i].passedGo)
                {
                    totalpassed++;
                }
   
            }
            if (totalpassed > 1)
            {
                canStartAuction(true); //checks to make sure at least 2 can auction before auctioning
            }
            totalpassed = 0;
            return;
        }
        // Landed on property that is owned by a player
        else if (location.CanBeBought && !bank.BankOwnedProperties.Contains(position))
        {
            if (player.properties.Contains(position)) {
                Debug.Log("Property owned by the same player");
                displayName3.text = ("Property owned by the same player");
            }
            else {
                Debug.Log("Property owned by another player");
                displayName3.text = ("Property owned by another player");
                if (!location.mortgaged && (location.Owner.inJail == -1))
                {
                    payRent(player, location);
                }

                if (rolledDouble > 0)
                {
                    Debug.Log("CAN ROLL TRUE AND NEXT TRUE");
                    canRoll(true);
                    //next = true;
                }
            }
        } 

        // Landed on oppurtunity knocks 8, 37
        else if (position == 7 | position == 22 | position == 36)
        {
            Debug.Log("Landed on oppurtunity knocks");
            displayName3.text = ("Landed on oppurtunity knocks");
            oppKnock(player);
        }

        // Landed on pot luck
        else if (position == 2 | position == 17 | position == 33)
        {
            Debug.Log("Landed on pot luck");
            displayName3.text = ("Landed on pot luck");
            potLuck(player);
        }

        // Landed on income tax
        else if (position == 4)
        {
            Debug.Log("Landed on Income Tax and charged $200");
            displayName3.text = ("Landed on Income Tax and charged $200");
            _DepositToFreeParking(player, 200);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on free parking
        else if (position == 20)
        {
            Debug.Log($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            displayName3.text = ($"Landed on free parking you have gained {bank.FreeParkingBalance}");
            player.balance += bank.FreeParkingBalance;
            bank.FreeParkingBalance = 0;
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go to jail
        else if (position == 30)
        {
            Debug.Log("Go to jail");
            displayName3.text = ("Go to jail");
            _GoToJail(player);
        }

        // Landed on super tax
        else if (position == 38)
        {
            Debug.Log("Landed on Super Tax and charged $100");
            Debug.Log(player.balance);
            _DepositToFreeParking(player, 100);
            Debug.Log($"New balance: {player.balance}");
        }

        // Landed on go
        else if (position == 0)
        {
            // should not need any logic if there is pass go logic implemented in move forward
            Debug.Log("Landed on GO");
            displayName3.text = ("Landed on GO");
        }

        // if no actions are available, next turn
        Debug.Log($"{location.CanBeBought}, {!bank.BankOwnedProperties.Contains(position)}, {rolledDouble}");
        if ((location.Group == "" && rolledDouble > 0) | ((location.CanBeBought && !bank.BankOwnedProperties.Contains(position) && (rolledDouble > 0))))
        {
            Debug.Log("SPECIALX");
            displaydouble.text = "you rolled a double!";
            canRoll(true);
        }
        else canRoll(false);//next = false;
        if ((rolledDouble == 0) && (!jailOption.activeSelf) && (!propertyButton.activeSelf))
        {
            canEndTurn(true);
            Debug.Log($"544");
        }
    }


    void purchaseProperty(Player_ player, Property location, int amount = -1)
    {
        if (amount == -1)
        {
            amount = location.Cost;
        }
        player.addProperty(location);
        BankTrans(-amount, player);
        location.Owner = player;
        //Debug.Log($"{player.properties}");
        //Debug.Log($"{bank.BankOwnedProperties}");
        UpdateBalanceUI();
    }

    void mortgageProperty(Player_ player, Property location)
    {
        BankTrans(location.Cost / 2);
        location.mortgaged = true;
        UpdateBalanceUI();

    }

    void unMortgageProperty(Player_ player, Property location)
    {
        if (player.balance < (location.Cost / 2))
        {
            return;
        }
        BankTrans(-(location.Cost / 2));
        location.mortgaged = false;
        UpdateBalanceUI();

    }

    void sellProperty(Player_ player, Property location)
    {
        player.removeProperty(location);
        if (location.mortgaged)
        {
            BankTrans(location.Cost / 2);
        } else BankTrans(location.Cost);

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
        if (buy == boolean)
        {
            return;
        }
        else buy = boolean;

        // CPU LOGIC
        CPUPush($"canBuyProperty {boolean}");

        auctionButton.SetActive(boolean);
        propertyButton.SetActive(boolean);
        canEndTurn(false);
    }
    public void canEndTurn(bool boolean)
    {
        Debug.Log($"Now {boolean}");
        // CPU LOGIC
        if (end == boolean)
        {
            return;
        }
        else end = boolean;

        CPUPush($"canEndTurn {boolean}");
        endButton.SetActive(boolean);

        if (!boolean) // IDK what this does
        {
            manageButton.SetActive(boolean);
            return;
        }

        canRoll(false);
        //next = false;

        CurrentPlayer = playerlist[playerTurn].gameObject;
        Player_ player = CurrentPlayer.GetComponent<Player_>();

        if (player.properties.Count() > 0)
        {
            manageButton.SetActive(boolean);
        }

        if (player.inJail > -1)
        {
            manageButton.SetActive(false);
        }

        
    }
    public void canStartAuction(bool boolean)
    {
        if (auc == boolean)
        {
            return;
        }
        else auc = boolean;
            // CPU LOGIC
            CPUPush($"canStartAuction {boolean}");

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
            Debug.Log($"{playerTurn}");
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
        if (buttonName == "endTurnButton")
        {
            Debug.Log("HERE 2");
            canEndTurn(false);
            if (abridgedGamemode && (playerTurn == (playerlist.Count()-1)))
            {
                if ((Time.time - startTime) > endTime)
                {
                    endAbridgedGame();
                }
            }
 
            Debug.Log("EndTurn");
            NextTurn();
            UpdateBalanceUI();
            canRoll(true);  
        }
        if (buttonName == "startAuctionButton")
        {
            canBuyProperty(false);
            canStartAuction(false);
            Debug.Log("Start auction");
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();
            startAuction(player);
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
                displayName3.text = ("latest bid is "+ bid);
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
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            BankTrans(-50, player);
            player.inJail = -1;
            jailOption.SetActive(false);
            canEndTurn(true);
            manageButton.SetActive(false);

        }

        if (buttonName == "jailFree")
        {
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            player.JailFreeCards--;
            player.inJail = -1;
            jailOption.SetActive(false);
            canEndTurn(true);
            manageButton.SetActive(false);
        }

        if (buttonName == "stayInJail")
        {
            jailOption.SetActive(false);
            canEndTurn(true);
            manageButton.SetActive(false);
        }

        if (buttonName == "pay10")
        {
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
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();

            oppKnock(player);
        }
    }

    public void propertyDropdownChange()
    {
        editMortgageButton();
    }

    public void setDropdownChange()
    {
        var (loc, player) = returnPropertyOnShow("Sets");
        Debug.Log($"Name: {loc.Name}");

        lowestHouses = 5;
        highestHouses = 0;
        foreach (var index in player.properties)
        {
            Property location = bank.Properties[index];
            if (location.Group == loc.Group)
            {
                Debug.Log(location.Name);
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
        
        if (loc.NumberOfHouses > 0 && loc.NumberOfHouses == highestHouses)
        {
            sellHouseButton.SetActive(true);
        }
        else sellHouseButton.SetActive(false);

        if (loc.NumberOfHouses == 5 | ((loc.NumberOfHouses != lowestHouses) && (lowestHouses != highestHouses)))
        {
            upgradeHouseButton.SetActive(false);
        }
        else upgradeHouseButton.SetActive(true);


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
        // CPU LOGIC
        CPUPush("_OppKnocksOption");

        Debug.Log("Pay a $10 fine or take opportunity knocks");
        displayName3.text = ("Pay a $10 fine or take opportunity knocks");

        oppKnocksOption.SetActive(true);
    }
    public void _DepositToFreeParking(Player_ player, int amount)
    {
        Debug.Log("_DepositToFreeParking");
        displayName3.text = ($"Deposited: ${amount} to Free Parking");
        player.DepositToFreeParking(amount);
    }
    public void _GoToJail(Player_ player, int amount = 0)
    {
        // CPU LOGIC
        CPUPush("_GoToJail");

        Debug.Log("_GoToJail");
        _Teleport(player, 10);
        player.inJail = 0;
        manageButton.SetActive(false);
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
        if (rol == canRoll)
        {
            return;
        }
        else rol = canRoll;
            // CPU LOGIC
            CPUPush($"canRoll {canRoll}");

        next = canRoll;
        showing = !canRoll;
    }
    public void startAuction(Player_ player)
    {
        bidders.Clear();
        nextBidder = 0;
        foreach (var item in playerlist)
        {
            if (item != player && item.passedGo && (item.inJail == -1)) //checks to make sure players that have passed go added
            {
                bidders.Add(item);
            }
        }
        playerBidPanel.SetActive(true);

        playerNameText.text = $"{bidders[0].playerName}, please enter your bid or skip";
    }

    public void nextBid(int bid)
    {
        Player_ bidder = bidders[nextBidder];
        if (bid > highestBid)
        {
            highestBid = bid;
            highestBidder = bidder;
        }

        if (bid == -1)
        {
            //remove this bidder from the bidding
            bidders.Remove(bidder);
            nextBidder--;
        }

        if (bidders.Count == 1)
        {
            //if only one left to bid
            endAuction();
            CurrentPlayer = playerlist[playerTurn].gameObject;
            Player_ player = CurrentPlayer.GetComponent<Player_>();
            if (highestBid != 0) purchaseProperty(highestBidder, bank.Properties[player.pos], highestBid);
        }


        if (nextBidder == bidders.Count-1)
        {
            nextBidder = 0;
        } else
        {
            // CPU LOGIC NEEDED UNIQUE

            nextBidder++;
            playerNameText.text = $"{bidders[nextBidder].playerName}, please enter your bid or skip";
        }   
    }
    public void endAuction()
    {
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
                Debug.Log($"{loc}");
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
        manageButton.SetActive(true);
        canEndTurn(true);
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
            Debug.Log(propertyName);
            Debug.Log(loc.Name);
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
    public bool checkBankruptcy(Player_ player, int amount)
    {
        if (player.balance < amount)
        {

            playerlist[playerTurn].gameObject.SetActive(false);
            
            playerlist.Remove(playerlist[playerTurn]);
            playerTurn--;
            playerTurn--;

            if (playerlist.Count == 1)
            {
                endLongGame();
                return true;
            }

            NextTurn();
            return true;
        } return false;
    }

    public bool checkTotalBankruptcy(Player_ player, int amount)
    {
        if (player.balance < amount && player.checkLiquidation() < amount)
        {
            displayName3.text = "bankrupt!";
            return true;
        }
        return true;
    }
    public int payMax(Player_ player)
    {
        return player.LiquidatePlayer();
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
    }

    // Code for which buttons appear when a player first enters jail
    public void jailOptions(Player_ player)
    {
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

    public void CPULogic(Player_ player)
    {
        // CPU LOGIC
        foreach (string str in cpuStack)
        {
            Debug.Log(str);
        }


    }
}
