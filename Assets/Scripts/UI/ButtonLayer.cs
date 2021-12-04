using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonLayer : MenuLayer
{
    public Button m_button;
    public override void Initialise(GameObject given_object, string given_name, string starting_sprite_map, Color given_colour, bool given_toggle, bool given_visible)
    {
        base.Initialise(given_object, given_name, starting_sprite_map, given_colour, given_toggle, given_visible);
        GameObject button_object = given_object;
        m_button = button_object.AddComponent<Button>();
    }

}
