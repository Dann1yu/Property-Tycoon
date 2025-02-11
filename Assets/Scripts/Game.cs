using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;

// Base Player class
public abstract class Player
{
    public string Name;
    public int Money;
    public int Position;
    public List<Property> OwnedProperties;
    public int numberOfGetOutOfJailFreeCards;
    public bool InJail;
    public int GoesInJail;

    public Player(string name, int startingMoney)
    {
        Name = name;
        Money = startingMoney;
        Position = 0;
        InJail = false;
        GoesInJail = 0;
    }

    public abstract void TakeTurn(); // travels down to the individual derived classes
    public void Move(int spaces)
    {
        // implement move by amount 'spaces'
    }

    public void BuyProperty(Property property)
    {
        // if money > price of property
        // give bank price of property
        // add property to OwnedProperties
        // remove property from bank
    }

    public void MortgageProperty(Property property)
    {
        // set property mortgaged state to true
        // recieve mortgage amount from bank
    }

    public void UpgradeProperty(Property property)
    {
        // if property can be upgraded
        // if money > propert upgrade price
        // upgrade property
        // transfer property upgrade price to bank
    }

    public void DowngradeProperty(Property property)
    {
        // if property can be downgraded
        // downgrade property
        // transfer property downgrade amount to player from bank
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
    public override void TakeTurn()
    {
        Console.WriteLine($"{Name} (Bot) is taking a turn...");
        // Implement AI logic here
    }
}

// HumanPlayer class, controlled by a user)
public class HumanPlayer : Player
{
    public HumanPlayer(string name, int startingMoney) : base(name, startingMoney) { }
    public override void TakeTurn()
    {
        Console.WriteLine($"{Name} (Human) is taking a turn...");
        // Implement UI and player choices here
    }
}

// Banker class, TBD HOW THIS CLASS IS RUN
public class Banker : Player
{
    public Banker(string name, int startingMoney) : base(name, startingMoney) { }
    public override void TakeTurn()
    {
        Console.WriteLine($"{Name} (Banker) is managing the finances...");
        // Implement banker responsibilities here
    }
}

// Bank class
public class Bank
{
    public int Money;
    public List<Property> AvailableProperties;
    public List<Card> PotLuckCards;
    public List<Card> OppurtunityKnocksCards;
    public int FreeParkingMoney = 0;

    public Bank(int startingMoney)
    {
        Money = startingMoney;
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
    public Dictionary<int, Property> Properties = new Dictionary<int, Property>(); // Key: Propery ID, Value: Property name
    public Dictionary<int, int> Houses = new Dictionary<int, int>(); // Key: Property ID, Value: Number of houses

    

   
        

    public void MovePlayer(Player player, int spaces)
    {
        // move player by 'spaces'
        // if space is a 'special property' act accordingly
        // if player owns space pay them and move on
        // if no one owns space offer the space to the player that landed
    }
}

// Property class
public class Property
{
    public string Name;
    public int Price;
    public Player Owner;

    public Property(string name, int price)
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




public class Program
{

    void Start()
    {
        //CreateBoardCoords();
    }
    public static void Main()
    {
        Debug.Log("Game started");

        // Sample game setup
        Bank bank = new Bank(10000);
        Board board = new Board();
        Player player1 = new HumanPlayer("Alice", 1500);
        Player player2 = new Bot("Bot1", 1500);

        Debug.Log("Game has started!");
        player1.TakeTurn();
        player2.TakeTurn();
    }
}
