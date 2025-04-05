using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
public class UI : PlayerMovement
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button PropertyBtn;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void propertyBtnOff()
    {
        
        DecidedToClick(this.name);
        Debug.Log("button off");
    }
}
