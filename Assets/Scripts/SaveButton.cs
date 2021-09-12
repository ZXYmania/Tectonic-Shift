using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveButton : Menu, Clickable
{
    public SaveButton CreateSaveButton(bool save)
    {
        SaveButton output = CreateMenu<SaveButton>();
        output.save = save;
        return output;

    }
    public bool save { get; protected set; }

    void OnMouseDown()
    {
        Mode.Select(this);
    }

    public void OnHover()
    {
    }

    public void OnSelect()
    {
    }

    public void UnHover()
    {
    }

    public void UnSelect()
    {
    }

    protected override void Initialise()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
