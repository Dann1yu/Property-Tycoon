using System.Collections.Generic;
using UnityEngine;
using static Bank_;

// Class that handles all unique player information 
public class Player_ : MonoBehaviour
{
    public string playerName;
    public int balance;
    public List<int> properties;
    public int pos;
    public int inJail; // -1 for not in jail and +1 each turn starting from 1 in jail
    private Bank_ bank;
    public int JailFreeCards;
    public Dictionary<string, int> Sets = new Dictionary<string, int>();
    public List<string> OwnedSets = new List<string>();

    // Initializes the players variables
    public void Initialize(string name, int startBalance)
    {
        playerName = name;
        balance = startBalance;
        pos = 0;
        properties = new List<int>();
        inJail = -1;
        bank = FindFirstObjectByType<Bank_>();
        JailFreeCards = 0;
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

    public void removeProperty(Property property)
    {
        var idx = property.Position;
        var group = property.Group;

        properties.Remove(idx);
        bank.BankOwnedProperties.Add(idx);

        if (OwnedSets.Contains(group)) OwnedSets.Remove(group);
        
        Sets[group]--;
    }

    public void DepositToFreeParking(int amount)
    {
        balance -= amount;
        bank.FreeParkingBalance += amount;
    }

    public void upgradeHouse(Player_ player, Property loc)
    {
        int amount = checkHousePrice(loc);
        PayBank(amount);
        loc.NumberOfHouses++;
    }

    public void sellHouse(Player_ player, Property loc)
    {
        int amount = checkHousePrice(loc);
        ReceiveMoneyFromBank(amount);
        loc.NumberOfHouses--;
    }

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
