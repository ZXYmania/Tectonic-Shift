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
        Initialise();
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
            foreach (KeyValuePair<string, AnimationLayer> item in m_animationLayer)
            {
                item.Value.Animate();
            }
        }
        m_animationTimer += Time.deltaTime;
        return true;
    }

    protected T AddAnimationLayer<T>(string givenName, string givenAnimation, Color givenColour, bool isPaused, bool givenVisible = true) where T : AnimationLayer, new()
    {

        T currentAnimation = CreateAnimationLayer<T>();

        GameObject currentObject = gameObject;
        if (m_animationLayer.Count > 0)
        {
            currentObject = new GameObject();
            currentObject.transform.parent = gameObject.transform;
            currentObject.transform.localPosition = new Vector3(0, 0, 1);
            currentObject.transform.localScale = new Vector3(1, 1, 1);
            currentObject.name = givenName;
        }
        else
        {
            currentObject.name = gameObject.name + " " + givenName;
        }
        currentAnimation.Initialise(currentObject, givenName, givenAnimation, givenColour, isPaused, givenVisible);
        m_animationLayer.Add(givenName, currentAnimation);

        return currentAnimation;
    
    }

    protected void AddAnimationLayer(string givenName, AnimationLayer givenLayer)
    {
        m_animationLayer.Add(givenName, givenLayer);
    }

    private T CreateAnimationLayer<T>() where T: AnimationLayer, new ()
    {
        return new T();
    }

    protected void AddAnimation(string givenLayer, string givenAnimation, Color givenColour)
    {
        m_animationLayer[givenLayer].AddSpriteMap(givenAnimation, givenColour);
    }
    
    protected void ChangeAnimation(string givenLayer,int givenAniamtion, int givenFrame = 0)
    {
        m_animationLayer[givenLayer].ChangeAnimation(givenAniamtion,givenFrame);
    }

    protected T GetAnimationLayer<T>(string givenLayer) where T: AnimationLayer
    {
        return (T) m_animationLayer[givenLayer];
    }
    protected void ChangeSpriteMap(string givenLayer,string givenSpriteMap, int givenAnimation = 0, int givenFrame = 0)
    {
        m_animationLayer[givenLayer].ChangeSpriteMap(givenSpriteMap, givenAnimation, givenFrame);
    }

    protected void SetVisible(bool visible)
    {
        foreach (KeyValuePair<string, AnimationLayer> item in m_animationLayer)
        {
            m_animationLayer[item.Key].SetVisible(visible);
        }
    }
}

