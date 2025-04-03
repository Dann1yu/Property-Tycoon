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
    private List<Card> JailFreeCards;

    // Initializes the players variables
    public void Initialize(string name, int startBalance)
    {
        playerName = name;
        balance = startBalance;
        pos = 1;
        properties = new List<int>();
        inJail = -1;
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
        Debug.Log($"!!{bank.Balance}");
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

    public void addProperty(int idx)
    {
        properties.Add(idx);
        bank.BankOwnedProperties.Remove(idx);
    }

    public void DepositToFreeParking(int amount)
    {
        balance -= amount;
        bank.FreeParkingBalance += amount;
    }

}
