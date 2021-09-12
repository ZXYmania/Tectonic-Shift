using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public abstract class AnimationLayer 
{
    public class AnimationOutofBoundsException : System.Exception
    {
        public AnimationOutofBoundsException(string message) : base(message)
        {

        }
    }
    string m_name;
    public string GetName() { return m_name; }
	protected string currentSpriteMap;
    protected int currentAnimation;
    protected int nextAnimation;
    protected int currentFrame;
    protected int currentTime;
    protected int setAnimation;
    protected bool m_pause;
    protected bool m_visible;
    protected bool nextVisible;
    protected float m_timeOffset;
    public void SetTimeOffset(float givenOffset) { m_timeOffset = givenOffset; }
    public float GetTimeOffset() { return m_timeOffset; }
    //All of the different animations
	public Dictionary<string ,SpriteMap> m_animationList;
    //The amount of animations
	public int GetAnimationListSize(){return m_animationList.Count;}
	public int GetAnimation() {return currentAnimation;}
    public string GetAnimationName() { return m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(0).name; }
    //Toggle the pause the animation
	public void TogglePause() {m_pause = !m_pause;}
    //Set the pause
	public void SetPause(bool givenToggle) {m_pause = givenToggle;}
	public void SetVisible(bool givenVisible)
    {
        nextVisible = givenVisible;
        currentFrame = 0;
    }
    public bool GetVisible() { return m_visible; }
	public int GetAnimationFrame() 	
	{
		return currentFrame;
	}

	public int GetAnimationFrame(int index) {return currentFrame;}
	public void SetNextAnimation(int givenAnimation){nextAnimation = givenAnimation;}
    public void AddSpriteMap(string givenAnimation, Color givenColor)
    {
        SpriteMap tempSpriteMap = TextureController.GetSpriteMap(givenAnimation, givenColor);
        if (m_animationList.ContainsKey(tempSpriteMap.GetName()))
        {
            m_animationList[tempSpriteMap.GetName()].AddAnimation(tempSpriteMap.GetAnimations());
        }
        else
        {
            m_animationList.Add(tempSpriteMap.GetName(), tempSpriteMap);
        }
    }

    public void ChangeSpriteMap(string givenSpriteMap, int givenAnimation=0, int givenFrame= 0)
    {
        if (currentSpriteMap != givenSpriteMap || currentAnimation != givenAnimation)
        {
            currentFrame = 0;
            currentTime = 0;
            currentSpriteMap = givenSpriteMap;
            currentAnimation = givenAnimation;
        }
        nextAnimation = m_animationList[currentSpriteMap].GetBaseAnimationAsInt();
        currentFrame = givenFrame;
    }

    public void ChangeAnimation(int givenAnimation, int givenFrame = 0) 
	{
		if(currentAnimation != givenAnimation)
		{
			currentTime = 0;
		}
		currentAnimation = givenAnimation; 
		nextAnimation = m_animationList[currentSpriteMap].GetBaseAnimationAsInt();
		currentFrame = givenFrame;
    }
	public void ChangeAnimation()
	{
		currentAnimation = nextAnimation;
		nextAnimation = m_animationList[currentSpriteMap].GetBaseAnimationAsInt();
		currentFrame = 0;
		currentTime = 0;
	}


	public Vector2 GetAnimationSize()		 {return m_animationList[currentSpriteMap].GetAnimation(0).GetSprite(0).rect.size;}
	public Vector2 GetDefaultAnimationSize() {return m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(0).rect.size;}
    public virtual void Initialise(GameObject givenObject, string givenName, string startingSpriteMap, Color givenColour, bool givenToggle, bool givenVisible)
    {
        m_name = givenName;
        m_pause = givenToggle;
        currentFrame = 0;
        currentTime = 0;
        nextVisible = givenVisible;
        m_animationList = new Dictionary<string, SpriteMap>();
        AddSpriteMap(startingSpriteMap, givenColour);
        currentSpriteMap = startingSpriteMap;
        nextAnimation = m_animationList[startingSpriteMap].GetBaseAnimationAsInt();
        SetTexture(m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(currentFrame));
        setAnimation = -1;
    }

    public virtual void Animate()
    {
        if (m_visible || nextVisible)
        {
            if (!m_pause)
            {
                //If the time is on speed with the animation
                if (currentTime >= GetTimeOffset())
                {
                    currentFrame++;
                    if (m_animationList[currentSpriteMap].GetAnimation(currentAnimation).UpdateFrame(currentFrame))
                    {
                        ChangeAnimation();
                    }
                    currentTime = 0;
                }
                if (GetAnimation() < 0)
                {
                    throw new AnimationOutofBoundsException("The animation isn't set for " + GetName());
                }
                SetTexture(m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(currentFrame));
                setAnimation = currentAnimation;
                currentTime++;
            }
            else if (setAnimation != currentAnimation)
            {
                SetTexture(m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(currentFrame));
                setAnimation = currentAnimation;
            }
            m_visible = nextVisible;
            ChangeVisible();
        }
    }
    public abstract void SetTexture(Sprite givenTexture);
    public abstract void SetAnimationSize(float givenSize);
    public abstract void SetAnimationPosition(int givenX, int givenY, int givenZ = 0);

    public abstract void ChangeVisible();
}
