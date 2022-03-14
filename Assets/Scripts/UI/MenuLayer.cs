using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VAnchor
{
    Centre,
    Top,
    Bottom,
}
public enum HAnchor
{
    Centre,
    Left,
    Right,
}

public struct Anchor
{
    public VAnchor vertical;
    public HAnchor horizontal;
    public Anchor(VAnchor given_v)
    {
        vertical = given_v;
        horizontal = HAnchor.Centre;
    }
    public Anchor(HAnchor given_h)
    {
        vertical = VAnchor.Centre;
        horizontal = given_h;
    }
    public Anchor(VAnchor given_v, HAnchor given_h)
    {
        vertical = given_v;
        horizontal = given_h;
    }
}

public class MenuLayer : ImageAnimationLayer
{
    BoxCollider2D m_collider;
    public void Initialise(GameObject givenObject, string givenName, string startingSpriteMap, Color givenColour, bool givenToggle, bool givenVisible, bool boxCollider)
    {
        base.Initialise(givenObject, givenName, startingSpriteMap, givenColour, givenToggle, givenVisible);
        if(boxCollider)
        {
            AddBoxCollider();
        }
    }
    public override void Initialise(GameObject givenObject, string givenName, string startingSpriteMap, Color givenColour, bool givenToggle, bool givenVisible)
    {
        base.Initialise(givenObject, givenName, startingSpriteMap, givenColour, givenToggle, givenVisible);
    }

    public void OnMouseDown()
    {
        Debug.Log("Menu Clicked event sent");
    }

    public virtual void SetScreenSize(float givenSize, RectTransform.Axis screenScale = RectTransform.Axis.Horizontal)
    {
        if (screenScale == RectTransform.Axis.Horizontal)
        {
            SetAnimationSize(givenSize * Menu.PercentageofScreen.x);
        }
        else
        {
            SetAnimationSize(givenSize * Menu.PercentageofScreen.y);
        }
        if(m_collider != null)
        {
            m_collider.size = m_image.rectTransform.rect.size;
        }
    }

    public virtual Vector2 GetOffset(Anchor given_anchor)
    {
        Vector2 offset = m_image.rectTransform.rect.size;
        if (given_anchor.vertical == VAnchor.Top)
        {
            offset.y *= -1;
        }
        else if (given_anchor.vertical == VAnchor.Centre)
        {
            offset.y = 0;
        }
        if (given_anchor.horizontal == HAnchor.Right)
        {
            offset.x *= -1;
        }
        else if (given_anchor.horizontal == HAnchor.Centre)
        {
            offset.x *= 0;
        }
        offset *= 0.5f;
        return offset;
    }

    public void AddBoxCollider()
    {
        if (m_collider == null)
        {
            m_collider = m_image.gameObject.AddComponent<BoxCollider2D>();
            m_collider.size = m_image.rectTransform.rect.size;
        }
    }

}
