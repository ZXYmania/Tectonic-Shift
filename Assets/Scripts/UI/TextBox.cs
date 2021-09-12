using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class TextBox : Menu
{
    public Text m_text;

    protected void Initialise(string layer_name, string animation, Color border)
    {
        base.Initialise();
        AddAnimationLayer(layer_name, animation, border, true, true, AnimationLayerType.image);
        float scale = PercentageofScreen / 10;
        float image_scale = scale / m_animationLayer[layer_name].GetAnimationSize().x;
        transform.localScale = new Vector3(image_scale, image_scale, 1);
        Image image = ((ImageAnimationLayer)m_animationLayer["delete"]).m_image;
        image.rectTransform.pivot = new Vector2(0, 0);
        GameObject input_object = gameObject;
        InputField input = input_object.AddComponent<InputField>();
        input_object.name = layer_name + " Text Box";
        GameObject text_object = new GameObject();
        m_text = text_object.AddComponent<Text>();
        text_object.name = layer_name + " Text";
        m_text.transform.SetParent(input_object.transform);
        m_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        m_text.text = "I am here";
        m_text.fontSize = 75;
        m_text.transform.localScale = new Vector3(0.1f, 0.1f, 1);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
        m_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1000);
        m_text.rectTransform.localPosition = new Vector3(0, 0, 1);
        m_text.rectTransform.pivot = image.rectTransform.pivot;
        m_text.supportRichText = false;
        input.textComponent = m_text;
        input.targetGraphic = image;
    }

    // Update is called once per frame
    void Update()
    {
        Animate();
    }
}
