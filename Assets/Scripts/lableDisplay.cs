using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class labelDisplay: PlayerMovement
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI displayName;
    void Start()
    {
        //displayName.text = "test";
    }


    // Update is called once per frame
    void Update()
    {
        //displayName.text = findPlayerName();
    }

    public void testlable(Player_ current)
    {
        Debug.Log("testable run");
        //displayName.text = current.ToString();
    }
}
