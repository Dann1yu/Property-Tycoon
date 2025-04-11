using System.Collections.Generic;
using UnityEngine;
using static Bank_;

// Class that handles all unique player information 
public class Player_ : MonoBehaviour
{
    // Pointers
    public int pos = 0;
    public int inJail = -1; // -1 for not in jail and +1 each turn starting from 0 in jail
    public int JailFreeCards = 0;
    public bool passedGo = false;

    // Important player variables
    private Bank_ bank;
    public bool AI;
    public string playerName;
    public int balance;

    // Properties owned variables
    public List<int> properties = new List<int>();
    public Dictionary<string, int> Sets = new Dictionary<string, int>();
    public List<string> OwnedSets = new List<string>();

    /// <summary>
    /// Sets the basic variables for the player when initialized
    /// </summary>
    /// <param name="name">Players name</param>
    /// <param name="startBalance">Players initial balance</param>
    public void Initialize(string name, int startBalance)
    {
        playerName = name;
        balance = startBalance;
        bank = FindFirstObjectByType<Bank_>();
    }

    /// <summary>
    /// Adds money to player from bank
    /// </summary>
    /// <param name="amount">Amount of money to be received</param>
    public void ReceiveMoneyFromBank(int amount)
    {
        balance += amount;
        bank.Balance -= amount;
        Debug.Log($"{playerName} received ${amount}. New balance: ${balance}");
        Debug.Log($"{bank.Balance}");
    }

    /// <summary>
    /// Adds money to player, theoretically from another player but that player's balance is handled in a seperate call (sender.PayPlayer())
    /// </summary>
    /// <param name="amount">Amount of money to be received</param>
    public void ReceiveMoney(int amount)
    {
        balance += amount;
        Debug.Log($"{playerName} received ${amount}. New balance: ${balance}");
    }

    /// <summary>
    /// Pay money to the bank
    /// </summary>
    /// <param name="amount">Amount of money to be sent </param>
    public void PayBank(int amount)
    {
        balance -= amount;
        bank.Balance += amount;
        Debug.Log($"{playerName} paid ${amount} to the bank. New balance: ${balance}");
    }

    /// <summary>
    /// Pay money to another player
    /// </summary>
    /// <param name="receiver">Player being sent the money</param>
    /// <param name="amount">Amount of money</param>
    public void PayPlayer(Player_ receiver, int amount)
    {
        balance -= amount;
        receiver.ReceiveMoney(amount);
        Debug.Log($"{playerName} paid ${amount} to {receiver.playerName}. New balance: ${balance}");
    }

    /// <summary>
    /// Adds property to properties aswell as removing from bank and can append to Sets / OwnedSets if required
    /// </summary>
    /// <param name="property">Property to be added</param>
    public void addProperty(Property property)
    {
        // Sets basic variables
        var idx = property.Position;
        var group = property.Group;
        // Adds to properties and removes from bankownedproperties
        properties.Add(idx);
        bank.BankOwnedProperties.Remove(idx);

        // If the properties group is in the sets dictionary and either add one to the value or set it to 1
        if (Sets.ContainsKey(group))
        {
            Sets[group]++;
        }
        else
        {
            Sets[group] = 1;
        }

        // If this completes a set add it to the OwnedSets list
        if ((Sets[group] == bank.PropertiesPerSet[group]) && (group != "Station") && (group != "Utilities"))
        {
            OwnedSets.Add(group);
        }
    }

    /// <summary>
    /// Removes property from player aswell as adding to the bank and ammending sets / ownedsets
    /// </summary>
    /// <param name="property">Property to be removed</param>
    public void removeProperty(Property property)
    {
        // Sets basic variables
        var idx = property.Position;
        var group = property.Group;

        // Removes from properties and adds to bankownedproperties
        properties.Remove(idx);
        bank.BankOwnedProperties.Add(idx);

        // If set in ownedsets, remove it
        if (OwnedSets.Contains(group)) OwnedSets.Remove(group);
        Sets[group]--;
    }

    /// <summary>
    /// Deposits money to free parking balance
    /// </summary>
    /// <param name="amount">Amount of money to be deposited</param>
    public void DepositToFreeParking(int amount)
    {
        balance -= amount;
        bank.FreeParkingBalance += amount;
    }

    /// <summary>
    /// Increases the number of houses of the property for the groups price
    /// </summary>
    /// <param name="loc">Property to upgrade</param>
    public void upgradeHouse(Property loc)
    {
        int amount = checkHousePrice(loc);
        PayBank(amount);
        loc.NumberOfHouses++;
    }

    /// <summary>
    /// Decrease the number of houses of the property for the groups price
    /// </summary>
    /// <param name="loc">Property to downgrade</param>
    public void sellHouse(Property loc)
    {
        int amount = checkHousePrice(loc);
        ReceiveMoneyFromBank(amount);
        loc.NumberOfHouses--;
    }

    /// <summary>
    /// Returns the price to upgrade the property
    /// </summary>
    /// <param name="loc">Property</param>
    /// <returns>Cost to upgrade/downgrade location</returns>
    public int checkHousePrice(Property loc)
    {
        return checkColourPrice(loc.Group);
    }

    /// <summary>
    /// Returns the price to upgrade the set
    /// </summary>
    /// <param name="colour">String of set colour</param>
    /// <returns>Cost to upgrade/downgrade set</returns>
    public int checkColourPrice(string colour)
    {
        return bank.setPrices[colour];
    }

    /// <summary>
    ///  Returns the networth of the player (Houses/Hotels, Properties and Balance)
    /// </summary>
    /// <returns>int: Networth</returns>
    public int checkLiquidation()
    {
        int total = 0;

        // For every property add value to the total of the nunber of houses and the sell price
        foreach (var locIdx in properties)
        {
            var loc = bank.Properties[locIdx];

            if (loc.NumberOfHouses > 0)
            {
                total += checkHousePrice(loc) * loc.NumberOfHouses;
            }
            
            if (loc.mortgaged)
            {
                total += loc.Cost / 2;
            }
            else total += loc.Cost;
        }

        total += balance;
        Debug.Log($"Liquidation value {total}");

        return total;
    }
}
