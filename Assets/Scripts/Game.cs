using UnityEngine;
using UnityEngine.XR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UIElements;
//using static PlayerMovement;

// Base Player class
public class Player : MonoBehaviour
{
    public string Name;
    public int Balance;
    public int Position;
    public List<PropertyTEMP> OwnedProperties;
    public int numberOfGetOutOfJailFreeCards;
    public bool InJail;
    public int GoesInJail;
    
    Vector3[] boardPosition = new Vector3[40];

    public GameObject CurrentPlayer;
    public int playerTurn = -1;

    public Player(string name, int startingMoney)
    {
        Name = name;
        Balance = startingMoney;
        Position = 0;
        InJail = false;
        GoesInJail = 0;
    }

    public void BuyPropertyTEMP(PropertyTEMP PropertyTEMP)
    {
        // if money > price of PropertyTEMP
        // give bank price of PropertyTEMP
        // add PropertyTEMP to OwnedProperties
        // remove PropertyTEMP from bank
    }

    public void MortgagePropertyTEMP(PropertyTEMP PropertyTEMP)
    {
        // set PropertyTEMP mortgaged state to true
        // recieve mortgage amount from bank
    }

    public void UpgradePropertyTEMP(PropertyTEMP PropertyTEMP)
    {
        // if PropertyTEMP can be upgraded
        // if money > propert upgrade price
        // upgrade PropertyTEMP
        // transfer PropertyTEMP upgrade price to bank
    }

    public void DowngradePropertyTEMP(PropertyTEMP PropertyTEMP)
    {
        // if PropertyTEMP can be downgraded
        // downgrade PropertyTEMP
        // transfer PropertyTEMP downgrade amount to player from bank
    }

    public void PayMoney(int amount)
    {
        // money - amount
    }

    public void ReceiveMoney(int amount)
    {
        // money + amount
    }
}

// Bot class, controlled by AI
public class Bot : Player
{
    public Bot(string name, int startingMoney) : base(name, startingMoney) { }
    //public override void TakeTurn()
    //{
    //    Console.WriteLine($"{Name} (Bot) is taking a turn...");
    //    // Implement AI logic here
    //}
}

// HumanPlayer class, controlled by a user)
public class HumanPlayer : Player
{
    public HumanPlayer(string name, int startingMoney) : base(name, startingMoney) { }
    //public override void TakeTurn()
    //{
    //    Console.WriteLine($"{Name} (Human) is taking a turn...");
    //    // Implement UI and player choices here
    //}
}

// Banker class, TBD HOW THIS CLASS IS RUN
public class Banker : Player
{
    public Banker(string name, int startingMoney) : base(name, startingMoney) { }
    
}

// Bank class
public class Bank : Player
{
    public int BankBalance = 300000;
    public List<PropertyTEMP> AvailableProperties;
    public List<Card> PotLuckCards;
    public List<Card> OppurtunityKnocksCards;
    
    

    public Bank(string name, int startingMoney) : base(name, startingMoney)
    {
        //Balance = startingMoney;
    }

    public void Withdraw(int amount, Player player)
    {
        player.Balance  += amount;
        BankBalance -= amount;
    }

    public void Deposit(int amount, Player player)
    {
        player.Balance -= amount;
        BankBalance += amount;
    }

    public bool canAfford(int amount, Player player) // check if player can play for something
    {
       
        return !(amount > player.Balance);
    }

    public void DepositToFreeParking(int amount)
    {
        // if player pays fine, pay to free parking bank

    }
    public int WithdrawFromFreeParking()
    {
        // pay player all free parking amount
        // set free parking to 0
        return 0;
    }
}

public class Dice
{
    public int RollDice()
    {
        // random roll 2 dice 1 -> 6
        // return summated value
        return 0;
    }
}

// Board class
public class Board
{
    public static int TotalSpaces = 40;
    public Dictionary<int, PropertyTEMP> Properties = new Dictionary<int, PropertyTEMP>(); // Key: Propery ID, Value: PropertyTEMP name
    public Dictionary<int, int> Houses = new Dictionary<int, int>(); // Key: PropertyTEMP ID, Value: Number of houses

    

    


    public void MovePlayer(Player player, int spaces)
    {
        // move player by 'spaces'
        // if space is a 'special PropertyTEMP' act accordingly
        // if player owns space pay them and move on
        // if no one owns space offer the space to the player that landed
    }

   
}

// PropertyTEMP class
public class PropertyTEMP
{
    public string Name;
    public int Price;
    public Player Owner;

    public PropertyTEMP(string name, int price)
    {
        Name = name;
        Price = price;
        Owner = null;
    }
}

// Placeholder Card class
public class Card
{
    public string Description;
    public Action<Player> Effect; // this stores an action on the player for when the card is 'ran' 

    public Card(string description, Action<Player> effect)
    {
        Description = description;
        Effect = effect;
    }
}




public class Program : MonoBehaviour
{


    
    public static void Main()
    {
        Debug.Log("Game started");

        // Sample game setup
        //Bank bank = new Bank(10000);
        Board board = new Board();
        Player player1 = new HumanPlayer("Alice", 1500);
        Player player2 = new Bot("Bot1", 1500);

        Debug.Log("Game has started!");
        //player1.TakeTurn();
        //player2.TakeTurn();

    }
}
