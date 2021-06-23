using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WrongFrameSizeException : System.Exception
{
    public WrongFrameSizeException(string message) : base(message)
    {

    }
}
public class FrameDoesnotExistException : System.Exception
{
    public FrameDoesnotExistException(string message)
    {

    }
}

public class PlayAnimation
{

	bool endAnimation;
	protected List<Sprite> m_frames;

	public Sprite GetSprite(int currentFrame)
    {
        if(m_frames.Count < 0)
        {
            throw new FrameDoesnotExistException("This animation has no frames");
        }
        if(currentFrame < 0)
        {
            throw new FrameDoesnotExistException("The frame has not been set for "+m_frames[0].name);
        }
        if(currentFrame > m_frames.Count)
        {
            throw new FrameDoesnotExistException("The " + currentFrame + "th doesn't exist for "+m_frames[0].name);
        }
        return m_frames[currentFrame];}

	public void Initialise()
	{
        m_frames = new List<Sprite>();
	}
	public bool UpdateFrame(int currentFrame)
	{
		return (currentFrame >= m_frames.Count);
	}	
	
	public void AddFrame(Sprite givenSprite)
	{
		
	}
    public void AddFrame(Texture2D givenTexture, Rect givenRect)
    {
        Sprite givenSprite = Sprite.Create(givenTexture, givenRect, new Vector2(0, 0));
        if (m_frames.Count > 0)
        {
            if(m_frames[0].rect.size != givenRect.size)
            { 
                throw new WrongFrameSizeException("Frame sizes do not match on animation for "+GetSprite(0).name+", "+"OriginalSize is "+m_frames[0].rect.size.x+", "+m_frames[0].rect.size+
                                                "New Size is "+givenRect.size.x + ", " + givenRect.size);
            }
        }
        m_frames.Add(givenSprite);
    }
    //Removes the coloured edge from the sprites
    public Rect Resize(Sprite givenSprite)
	{
		Rect tempRect = new Rect(new Vector2(givenSprite.rect.position.x+2,givenSprite.rect.position.y+2),
		                         new Vector2(givenSprite.rect.size.x-4,givenSprite.rect.size.y-4));
		return tempRect;
		
	}
    //Find the centre of the rectangle
	public Vector2 CenterRect(Rect givenRect)
	{
		return new Vector2((givenRect.width/2)/givenRect.width,(givenRect.height/2)/givenRect.height);
	}

}
