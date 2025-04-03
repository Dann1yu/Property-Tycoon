using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Bank_ : MonoBehaviour
{
    public int Balance;
    public List<Property> Properties { get; private set; } = new List<Property>();
    public List<int> BankOwnedProperties = new List<int>();
    public int FreeParkingBalance = 0;

    public void LoadProperties(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                Debug.Log(line);

                // Parsing values
                Debug.Log(values[0]);
                //int position = int.TryParse(values[0]) - 1;
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
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading properties: {ex.Message}");
        }
    }

    void Start()
    {
        Balance = 15000;
        string path = Application.dataPath + "/Resources/BoardData.csv"; // Ensure the file is inside 'Assets/Resources/'
        LoadProperties(path);
        Debug.Log($"Properties Loaded: {Properties.Count}"); // Verify it loaded
    }


    public void info(int idx)
    {
        Debug.Log(idx);
        Debug.Log(Properties.Count);
        var property = Properties[idx];
        Debug.Log($"Position: {property.Position} \n" +
            $"Name: {property.Name} \n" +
            $"Group: {property.Group} \n" +
            $"Action: {property.Action} \n" +
            $"Purchasable?: {property.CanBeBought} \n" +
            $"Cost: {property.Cost} \n" +
            $"Base Rent: {property.RentUnimproved} \n" +
            $"One house rent: {property.Rent1House} \n" +
            $"Two house rent: {property.Rent2Houses} \n" +
            $"Three house rent: {property.Rent3Houses} \n" +
            $"Four house rent: {property.Rent4Houses} \n" +
            $"One hotel rent: {property.RentHotel}");
    }
}

