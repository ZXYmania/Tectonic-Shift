using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Animated : MonoBehaviour
{
    public enum AnimationLayerType
    {
        sprite,
        image
    }

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
            foreach (KeyValuePair<string, AnimationLayer> item in m_animationLayer)
            {
                item.Value.Animate();
            }
        }
        m_animationTimer += Time.deltaTime;
        return true;
    }

    protected void AddAnimationLayer(string givenName ,string givenAnimation, Color givenColour, bool isPaused, bool givenVisible = true, AnimationLayerType givenType = AnimationLayerType.sprite)
    {
        if (!m_animationLayer.ContainsKey(givenName))
        {
            AnimationLayer currentAnimation = CreateAnimationLayer(givenType);
            
            GameObject currentObject = gameObject;
            if (m_animationLayer.Count > 0)
            {
                currentObject = new GameObject();
                currentObject.transform.parent = gameObject.transform;
                currentObject.transform.position = new Vector3(transform.position.x, transform.position.y, -1);
                currentObject.transform.localScale = new Vector3(1, 1, 1);
                currentObject.name = givenName;
            }
            else
            {
                currentObject.name = gameObject.name + " " + givenName;
            }           
            currentAnimation.Initialise(currentObject, givenName, givenAnimation, givenColour, isPaused, givenVisible);
            m_animationLayer.Add(givenName, currentAnimation);
        }
        else
        {
            m_animationLayer[givenName].AddSpriteMap(givenAnimation, givenColour);
        }
    }

    private AnimationLayer CreateAnimationLayer(AnimationLayerType givenType)
    {
        switch(givenType)
        {
            case AnimationLayerType.image:
                return new ImageAnimationLayer();
            case AnimationLayerType.sprite:
                return new SpriteLayer();
            default:
                throw new System.NotImplementedException();

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

