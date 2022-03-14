using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveMenu : Menu
{
    public static event Action<bool> Save = delegate {};

    protected override void Initialise()
    {
        base.Initialise();
        MenuLayer background = AddAnimationLayer<MenuLayer>("background", "save_menu", Color.green, true);
        background.SetScreenSize(8f); 
        background.m_image.transform.rotation = new Quaternion(0, 0, 90f, 0);
        ButtonLayer save_button = AddAnimationLayer<ButtonLayer>("save", "menu_option", Color.black, true);
        ButtonLayer load_button = AddAnimationLayer<ButtonLayer>("load", "menu_option", Color.black, true);
        save_button.ChangeAnimation(1);
        save_button.SetScreenSize(6f);
        load_button.SetScreenSize(6f);
        Vector2 image_offset = background.GetOffset(new Anchor(VAnchor.Top, HAnchor.Left));
        SetScreenOffset(image_offset.x, image_offset.y + (Menu.PercentageofScreen.y * 100));
        Vector2 save_offset = save_button.GetOffset(new Anchor(VAnchor.Bottom)) + image_offset;
        save_button.m_object.transform.localPosition= new Vector3(1, save_offset.y + (Menu.PercentageofScreen.y * 0.5f), 1);
        Vector2 load_offset = load_button.GetOffset(new Anchor(VAnchor.Top)) + background.GetOffset(new Anchor(VAnchor.Bottom));
        load_button.m_object.transform.localPosition = new Vector3(1, load_offset.y - (Menu.PercentageofScreen.y *0.5f), 1);
        save_button.m_button.onClick.AddListener(()=> Save(true));
        load_button.m_button.onClick.AddListener(() => Save(false));

    }

    // Update is called once per frame
    void Update()
    {
        base.Animate();
    }
}
