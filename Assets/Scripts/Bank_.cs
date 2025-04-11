using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class for all functions and variables for the bank to be functional
/// </summary>
public class Bank_ : MonoBehaviour
{
    // Game loop requirements
    public List<Property> Properties { get; private set; } = new List<Property>();
    public List<Card> PLCards { get; private set; } = new List<Card>();
    public List<Card> OKCards { get; private set; } = new List<Card>();
    public Dictionary<string, int> PropertiesPerSet = new Dictionary<string, int>();

    // Bank ownership
    public List<int> BankOwnedProperties = new List<int>();
    public int Balance;
    public int FreeParkingBalance = 0;

    // RNG import for card shuffling
    private static System.Random rng = new System.Random();

    // Info variables
    public Dictionary<string, int> setPrices = new Dictionary<string, int>();

    /// <summary>
    /// Loads csv of filepath and creates each property with it's required variables
    /// </summary>
    /// <param name="filePath">Filepath for property data csv</param>
    public void LoadProperties(string filePath)
    {
        // Reads csv into lines
        string[] lines = File.ReadAllLines(filePath);

        // For every row, create a property with it's values
        var colours = false;
        foreach (string line in lines)
        {
            if (line == ",,")
            {
                colours = true;
                continue;
            }

            // Splits each row and passes each column of the row to it's information
            string[] values = line.Split(',');

            if (colours)
            {
                setPrices[values[0]] = int.Parse(values[1]);
                continue;
            }

            int position = int.Parse(values[0]) - 1;
            string name = values[1];
            string group = values[3];
            string action = values[4];
            bool canBeBought = values[5].Trim().ToLower() == "yes";

            int cost = int.TryParse(values[7], out int noValue) ? noValue : -1;
            int rentUnimproved = int.TryParse(values[8], out int noValue1) ? noValue1 : -1;
            int rent1House = int.TryParse(values[10], out int noValue2) ? noValue2 : -1;
            int rent2Houses = int.TryParse(values[11], out int noValue3) ? noValue3 : -1;
            int rent3Houses = int.TryParse(values[12], out int noValue4) ? noValue4 : -1;
            int rent4Houses = int.TryParse(values[13], out int noValue5) ? noValue5 : -1;
            int rentHotel = int.TryParse(values[14], out int noValue6) ? noValue6 : -1;

            if (canBeBought)
            {
                BankOwnedProperties.Add(position);
            }

            if (PropertiesPerSet.ContainsKey(group))
            {
                PropertiesPerSet[group]++;
            }
            else
            {
                PropertiesPerSet[group] = 1;
            }

            // Creates new property
            Property property = new Property
            {
                Position = position,
                Name = name,
                Group = group,
                Action = action,
                CanBeBought = canBeBought,
                Cost = cost,
                RentUnimproved = rentUnimproved,
                Rent1House = rent1House,
                Rent2Houses = rent2Houses,
                Rent3Houses = rent3Houses,
                Rent4Houses = rent4Houses,
                RentHotel = rentHotel
            };

            Properties.Add(property);
        }

        Debug.Log($"Successfully loaded {Properties.Count} properties.");

        foreach (KeyValuePair<string, int> pair in setPrices)
        {
            Debug.Log("Key: " + pair.Key + ", Value: " + pair.Value);
        }
    }

    /// <summary>
    /// Loads csv of filepath and creates each card with it's required variables
    /// </summary>
    /// <param name="filePath">Filepath for card data csv</param>
    public void LoadCards(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int count = 0;
        bool oppKnocks = false;
        foreach (string line in lines)
        {
            // Start assigning pot luck cards
            // When line breaks swap to oppurtunity knocks card assignment
            if (line == ",,")
            {
                Debug.Log($"HERE: {count}");
                oppKnocks = true;
                count = 0;
                continue;
            }

            // Splits each row and passes each column of the row to it's information
            string[] values = line.Split(',');

            int id = count;
            count += 1;
            string description = values[0];
            string action = values[1];
            int integerValue = int.TryParse(values[2], out int parsedValue) ? parsedValue : -1; // passes -1 if no value

            // Creates new card
            Card card = new Card
            {
                Id = id,
                Description = description,
                Action = action,
                Integer = integerValue
            };

            if (oppKnocks) {
                OKCards.Add(card);
            } else {
                PLCards.Add(card);
            }
        }

        // Shuffles the deck (queue) of the cards
        shuffle(OKCards);
        shuffle(PLCards);

        Debug.Log($"Successfully loaded {OKCards.Count} oppurtunity knocks cards.");
        Debug.Log($"Successfully loaded {PLCards.Count} pot luck cards.");
    }

    /// <summary>
    /// On start, loads board data and card data aswell as initializes bank balance
    /// </summary>
    void Start()
    {
        Balance = 50000;
        string path = Application.dataPath + "/Resources/BoardData.csv"; // Ensure the file is inside 'Assets/Resources/'
        LoadProperties(path);
        path = Application.dataPath + "/Resources/CardData.csv";
        LoadCards(path);
        Debug.Log($"Properties Loaded: {Properties.Count}"); // Verify it loaded
    }

    /// <summary>
    /// Shuffles the list (queue) of cards
    /// </summary>
    /// <param name="cards">List of cards to be shuffled</param>
    private void shuffle(List<Card> cards)
    {
        System.Random rng = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }
}

