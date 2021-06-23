using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Animated : MonoBehaviour
{

    protected string animationName;
    protected float m_animationTimer;
    protected Dictionary<string,AnimationLayer> m_animationLayer;
    protected int GetAmountofAnimationLayers() { return m_animationLayer.Count; }
    protected bool HasTicked() { float tempTimer = m_animationTimer + Time.deltaTime; return (Mathf.Floor(m_animationTimer / 0.04f) < Mathf.Floor(tempTimer / 0.04f)); }
    protected virtual void Initialise(Vector2 givenPosition)
    {
        SetTransform(givenPosition);
        m_animationLayer = new Dictionary<string, AnimationLayer>();
    }
    protected virtual void Initialise()
    {
        m_animationLayer = new Dictionary<string, AnimationLayer>();
    }

    protected virtual void SetTransform(Vector2 givenVector)
    {
        transform.position = new Vector3(givenVector.x, givenVector.y, transform.position.z);
    }

    protected virtual void SetTransform(int x, int y)
    {
        transform.position = new Vector3(x, y, transform.position.z);
    }

    protected virtual bool Animate()
    {
        if (HasTicked())
        {
            foreach (KeyValuePair<string,AnimationLayer> item in m_animationLayer)
            {
                    if(item.Value.GetVisible())
                    {
                        item.Value.Animate();
                    }
            }
        }
        m_animationTimer += Time.deltaTime;
        return true;
    }

    protected void AddAnimationLayer(string givenName ,string givenAnimation, Color givenColour, bool isPaused, int givenAnimationVariant = 0)
    {
        if (!m_animationLayer.ContainsKey(givenName))
        {
            AnimationLayer currentAnimation = new AnimationLayer();
            GameObject currentObject = gameObject;
            SpriteRenderer currentRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (m_animationLayer.Count > 0)
            {
                currentObject = new GameObject();
                currentObject.transform.parent = gameObject.transform;
                currentObject.transform.position = new Vector3(transform.position.x, transform.position.y, -1);
                currentObject.transform.localScale = new Vector3(1, 1, 1);
                currentObject.name = givenName;
                currentRenderer = null;
            }
            else
            {
                currentObject.name = gameObject.name + " " + givenName;
            }
            if(currentRenderer == null)
            {
                currentRenderer = currentObject.AddComponent<SpriteRenderer>();
            }             
            currentAnimation.Initialise(currentRenderer, givenName, givenAnimation, givenColour, givenAnimationVariant, isPaused);
            m_animationLayer.Add(givenName, currentAnimation);
        }
        else
        {
            m_animationLayer[givenName].AddSpriteMap(givenAnimation, givenColour);
        }
    }

    protected void AddAnimation(string givenLayer, string givenAnimation, Color givenColour)
    {
        m_animationLayer[givenLayer].AddSpriteMap(givenAnimation, givenColour);
    }
    
    protected void ChangeAnimation(string givenLayer,int givenAniamtion, int givenFrame = 0)
    {
        m_animationLayer[givenLayer].ChangeAnimation(givenAniamtion,givenFrame);
    }
    protected void ChangeSpriteMap(string givenLayer,string givenSpriteMap, int givenAnimation = 0, int givenFrame = 0)
    {
        m_animationLayer[givenLayer].ChangeSpriteMap(givenSpriteMap, givenAnimation, givenFrame);
    }

}

