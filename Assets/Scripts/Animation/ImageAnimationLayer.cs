using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimationLayer : AnimationLayer
{
    public Image m_image {protected set; get; }
    public override void Initialise(GameObject givenObject, string givenName, string startingSpriteMap, Color givenColour, bool givenToggle, bool givenVisible)
    {
        m_image = givenObject.AddComponent<Image>();
        base.Initialise(givenObject, givenName, startingSpriteMap, givenColour, givenToggle, givenVisible);
        Vector2 size = GetDefaultAnimationSize();
        m_image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        m_image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        m_image.transform.localScale = new Vector3(1, 1, 1);
        
    }
    //Each Gameobject has it's own Layer
    public override GameObject GetLayer()
    {
        if (m_animationList.Count < 0)
        {
            throw new AnimationOutofBoundsException("Animation is not set");
        }
        return m_image.gameObject;
    }

    public override void SetTexture(Sprite givenTexture) { m_image.sprite = givenTexture; }
    public override void SetAnimationPosition(int givenX, int givenY, int givenZ = 0) { m_image.gameObject.transform.localPosition = new Vector3(givenX, givenY, givenZ); }
    public override void SetAnimationSize(float givenSize) {
        Vector2 size = GetDefaultAnimationSize();
        float ratio = size.y/size.x;
        m_image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, givenSize);
        m_image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, givenSize *  ratio);
    }

    public override void DisplayImage()
    {
        m_image.enabled = m_visible;
    }
}
