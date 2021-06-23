using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationOutofBoundsException : System.Exception
{
        public AnimationOutofBoundsException(string message) : base(message)
    {

    }
}

public class AnimationLayer 
{
	string m_name;
    public string GetName() { return m_name; }
	string currentSpriteMap;
    int currentAnimation;
	int nextAnimation;
	int currentFrame;
	int currentTime;
	bool m_pause;
    bool m_visible;
    float m_timeOffset;
    public void SetTimeOffset(float givenOffset) { m_timeOffset = givenOffset; }
    public float GetTimeOffset() { return m_timeOffset; }
    SpriteRenderer m_renderer;
    //All of the different animations
	public Dictionary<string ,SpriteMap> m_animationList;
    //Each Gameobject has it's own Layer
	public GameObject GetLayer()
    {
        if (m_animationList.Count < 0)
        {
            throw new AnimationOutofBoundsException("Animation is not set");
        }
        return m_renderer.gameObject;
    }
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
        m_visible = givenVisible;
        currentFrame = 0;
        if (m_visible)
        {
            SetTexture(m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(currentFrame));
        }
        else
        {
            SetTexture(null);
        }
    }
    public bool GetVisible() { return m_visible; }
	public int GetAnimationFrame() 	
	{
		return currentFrame;
	}

	public int GetAnimationFrame(int index) {return currentFrame;}
	public void SetTexture(Sprite givenTexture){m_renderer.sprite = givenTexture;}
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

	public void SetAnimationSize(float givenSize){m_renderer.gameObject.transform.localScale = new Vector3 (givenSize, givenSize, 1);}
	public Vector2 GetAnimationSize()		 {return m_animationList[currentSpriteMap].GetAnimation(0).GetSprite(0).rect.size;}
	public Vector2 GetDefaultAnimationSize() {return m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(0).rect.size;}
	public void SetAnimationPosition(int givenX, int givenY, int givenZ = 0) {m_renderer.gameObject.transform.localPosition = new Vector3 (givenX, givenY, givenZ);}
    public void Initialise(SpriteRenderer givenRenderer, string givenName, string startingSpriteMap, Color givenColour, int givenAnimationVariant, bool givenToggle)
    {
        m_name = givenName;
        m_renderer = givenRenderer;
        m_pause = givenToggle;
        currentFrame = 0;
        currentTime = 0;
        m_visible = true;
        m_animationList = new Dictionary<string, SpriteMap>();
        AddSpriteMap(startingSpriteMap, givenColour);
        currentSpriteMap = startingSpriteMap;
        nextAnimation = m_animationList[startingSpriteMap].GetBaseAnimationAsInt();
    }

    public void Animate()
	{
        if (m_visible)
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
                currentTime++;
            }
            if (GetAnimation() < 0 )
            {
                throw new AnimationOutofBoundsException("The animation isn't set for " + GetName());
            }
            SetTexture(m_animationList[currentSpriteMap].GetAnimation(currentAnimation).GetSprite(currentFrame));
        }
    }

}
