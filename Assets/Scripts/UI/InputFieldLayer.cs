using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextField : InputField
{
    public static event Action<TextField> Selected = delegate { };
    public static event Action<TextField> UnSelected = delegate { };
    public bool selected { protected set; get; }
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        Selected(this);
        selected = true;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        UnSelected(this);
        selected = false;
    }
}

public class InputFieldLayer : MenuLayer
{

    public Text m_text;
    public TextField m_input;
    public override void Initialise(GameObject given_object, string given_name, string starting_sprite_map, Color given_colour, bool given_toggle, bool given_visible)
    {
        base.Initialise(given_object, given_name, starting_sprite_map, given_colour, given_toggle, given_visible);
        GameObject input_object = given_object;
        m_input = input_object.AddComponent<TextField>();
        input_object.name = given_name + " Text Box";
        GameObject text_object = new GameObject();
        m_text = text_object.AddComponent<Text>();
        text_object.name = given_name + " Text";
        m_text.transform.SetParent(input_object.transform);
        m_input.textComponent = m_text;
        m_input.targetGraphic = m_image;
        m_input.image.rectTransform.localPosition = new Vector3(0, 0, 1);
        Rect input_bounds = m_image.rectTransform.rect;
        m_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        m_text.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, input_bounds.width*2);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, input_bounds.height*2);
        m_text.rectTransform.localPosition = new Vector3(0, 0, 1);
        m_text.supportRichText = false;
        m_text.resizeTextForBestFit = true;
        m_text.resizeTextMaxSize = 300;
        m_input.caretBlinkRate = 1.25f;
    }
   
    public bool Selected()
    {
        return m_input.selected;
    }
    public override void DisplayImage()
    {
        m_image.enabled = m_visible;
        m_text.enabled = m_visible;
    }

    public override void SetAnimationSize(float givenSize)
    {
        base.SetAnimationSize(givenSize);
        Rect input_bounds = m_image.rectTransform.rect;
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, input_bounds.width * 2);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, input_bounds.height * 2);
    }

    public override void SetScreenSize(float givenSize, RectTransform.Axis screenScale = RectTransform.Axis.Horizontal)
    {
        base.SetScreenSize(givenSize, screenScale);
        Rect input_bounds = m_image.rectTransform.rect;
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (input_bounds.width-givenSize) * 2);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (input_bounds.height-givenSize) * 2);
    }

    public void SetText(string text)
    {
        m_input.text = text;
    }

    public void Unselect()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }


}
