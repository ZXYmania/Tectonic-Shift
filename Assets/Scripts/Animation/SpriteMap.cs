using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteMap 
{
	List<PlayAnimation> m_animationList;
    private string m_base;
    public string GetBase()
    {
        if(m_base == null)
        {
            return m_name;
        }
        return m_base;
    }
	public string m_name { get; protected set; }
	public Color m_borderColour { get; protected set; }
    public string m_index { get; protected set; }
    int m_baseAnimation;

    public override int GetHashCode()
    {
        return m_index.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as SpriteMap);
    }

    public bool Equals(SpriteMap obj)
    {
        return obj != null && obj.m_index == this.m_index;
    }

    public int GetBaseAnimationAsInt() { return m_baseAnimation; }
    public void SetBaseAnimation(int givenAnimation) { m_baseAnimation = givenAnimation; }
	public void SetName(string givenName){m_name = givenName;}
	public string GetName(){return m_name;}
	public Color GetColour(){return m_borderColour;}

	public List<PlayAnimation> GetAnimations()
	{
		return m_animationList;
	}
	public PlayAnimation GetAnimation(int givenAnimation=0)
    {
        return m_animationList[givenAnimation];
    }
	public void AddAnimation(PlayAnimation givenAnimation)
	{
            m_animationList.Add(givenAnimation);
	}

    public void AddAnimation(List<PlayAnimation> givenAnimations)
    {
        m_animationList.AddRange(givenAnimations);
    }
	
	public void Initialise(string givenName, Color givenColour)
	{
		m_name = givenName;
		m_borderColour = givenColour;
        m_animationList = new List<PlayAnimation>();
        m_baseAnimation = 0;
        m_index = (m_name + m_borderColour);
	}
}
