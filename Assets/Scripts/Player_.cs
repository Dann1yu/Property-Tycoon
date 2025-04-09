using System.Collections.Generic;
using UnityEngine;
using static Bank_;

// Class that handles all unique player information 
public class Player_ : MonoBehaviour
{
    public string playerName;
    public int balance;
    public List<int> properties = new List<int>();
    public int pos = 0;
    public int inJail = -1; // -1 for not in jail and +1 each turn starting from 0 in jail
    private Bank_ bank;
    public int JailFreeCards = 0;
    public Dictionary<string, int> Sets = new Dictionary<string, int>();
    public List<string> OwnedSets = new List<string>();
    public bool passedGo = false;
    public bool AI = false;

    // Initializes the players variables
    public void Initialize(string name, int startBalance)
    {
        playerName = name;
        balance = startBalance;
        bank = FindFirstObjectByType<Bank_>();
    }

    // returns whether player has enough money for the payment
    public bool BalanceCheck(int amount)
    {   
        if (balance < amount) {
            Debug.Log($"{playerName} Does not have enough money! Implement bankrupcy.");
            return false;
        }
        return true;
    }

    // Receive money from the bank
    public void ReceiveMoneyFromBank(int amount)
    {
        balance += amount;
        bank.Balance -= amount;
        Debug.Log($"{playerName} received ${amount}. New balance: ${balance}");
        Debug.Log($"{bank.Balance}");
    }

    // Receive money from the bank
    public void ReceiveMoney(int amount)
    {
        balance += amount;
        Debug.Log($"{playerName} received ${amount}. New balance: ${balance}");
    }

    // Pay money to the bank
    public bool PayBank(int amount)
    {
        if (BalanceCheck(amount))
        {
            balance -= amount;
            bank.Balance += amount;
            Debug.Log($"{playerName} paid ${amount} to the bank. New balance: ${balance}");
            return true;
        }
        return false;
    }

    // Send money to another player
    public bool PayPlayer(Player_ receiver, int amount)
    {
        if (BalanceCheck(amount))
        {
            balance -= amount;
            receiver.ReceiveMoney(amount);
            Debug.Log($"{playerName} paid ${amount} to {receiver.playerName}. New balance: ${balance}");
            return true;
        }
        return false;
    }

    // Adds property to player aswell as removing from bank and appending to sets / ownedsets
    public void addProperty(Property property)
    {
        var idx = property.Position;
        var group = property.Group;

        properties.Add(idx);
        bank.BankOwnedProperties.Remove(idx);

        // check if all properties of that group are owned by player
        if (Sets.ContainsKey(group))
        {
            Sets[group]++;
        }
        else
        {
            Sets[group] = 1;
        }

        if ((Sets[group] == bank.PropertiesPerSet[group]) && (group != "Station") && (group != "Utilities"))
        {
            OwnedSets.Add(group);
        }

    }

    // Removes property from player aswell as adding to the bank and ammending sets / ownedsets
    public void removeProperty(Property property)
    {
        var idx = property.Position;
        var group = property.Group;

        properties.Remove(idx);
        bank.BankOwnedProperties.Add(idx);

        if (OwnedSets.Contains(group)) OwnedSets.Remove(group);
        
        Sets[group]--;
    }

    // Deposits amount to free parking
    public void DepositToFreeParking(int amount)
    {
        balance -= amount;
        bank.FreeParkingBalance += amount;
    }
     
    // Increases the number of houses of the property for the given price
    public void upgradeHouse(Property loc)
    {
        int amount = checkHousePrice(loc);
        PayBank(amount);
        loc.NumberOfHouses++;
    }

    // Decreases the number of houses of the property and gain the given price
    public void sellHouse(Property loc)
    {
        int amount = checkHousePrice(loc);
        ReceiveMoneyFromBank(amount);
        loc.NumberOfHouses--;
    }

    // Returns the price to upgrade the property
    public int checkHousePrice(Property loc)
    {
        int amount;
        if (loc.Group == "Brown" | loc.Group == "Blue")
        {
            amount = 50;
        }
        else if (loc.Group == "Purple" | loc.Group == "Orange")
        {
            amount = 100;
        }
        else if (loc.Group == "Red" | loc.Group == "Yellow")
        {
            amount = 150;
        }
        else
        {
            amount = 200;
        }

        return amount;
    }

}
