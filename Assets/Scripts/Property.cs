/// <summary>
/// Class for all property values
/// </summary>
public class Property
{
    public int Position { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public string Action { get; set; }
    public bool CanBeBought { get; set; }
    public int Cost { get; set; }
    public int RentUnimproved { get; set; }
    public int Rent1House { get; set; }
    public int Rent2Houses { get; set; }
    public int Rent3Houses { get; set; }
    public int Rent4Houses { get; set; }
    public int RentHotel { get; set; }
    public Player_ Owner { get; set; }
    public int NumberOfHouses { get; set; }
    public bool mortgaged { get; set; }
}
