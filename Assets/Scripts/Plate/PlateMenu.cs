using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditPlateMenu : TextBox, Clickable
{
    void Clickable.OnHover()
    {
        
    }

    void Clickable.OnSelect()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialise("delete", "plate_menu", Color.black);
    }
    void Clickable.UnHover()
    {
        
    }

    void Clickable.UnSelect()
    {

    }
}
