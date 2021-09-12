using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLayer : AnimationLayer
{
    public override  void Initialise(GameObject givenObject, string givenName, string startingSpriteMap, Color givenColour, bool givenToggle, bool givenVisible)
    {
        m_renderer = givenObject.GetComponent<SpriteRenderer>();
        if (m_renderer == null)
        {
            m_renderer = givenObject.AddComponent<SpriteRenderer>();
        }
        m_renderer.enabled = givenVisible;
        if (TextureController.default_material != null)
        {
            m_renderer.material = TextureController.default_material;
        }
        base.Initialise(givenObject, givenName, startingSpriteMap, givenColour, givenToggle, givenVisible);
    }
    SpriteRenderer m_renderer;
    //Each Gameobject has it's own Layer
    public GameObject GetLayer()
    {
        if (m_animationList.Count < 0)
        {
            throw new AnimationOutofBoundsException("Animation is not set");
        }
        return m_renderer.gameObject;
    }

    public override void SetTexture(Sprite givenTexture) { m_renderer.sprite = givenTexture; }
    public override void SetAnimationPosition(int givenX, int givenY, int givenZ = 0) { m_renderer.gameObject.transform.localPosition = new Vector3(givenX, givenY, givenZ); }
    public override void SetAnimationSize(float givenSize) { m_renderer.gameObject.transform.localScale = new Vector3(givenSize, givenSize, 1); }

    public override void ChangeVisible()
    {
        m_renderer.enabled = m_visible;
    }

}
