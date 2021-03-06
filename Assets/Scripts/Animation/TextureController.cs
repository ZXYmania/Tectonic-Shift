using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureNotfoundException : System.Exception
{
    public TextureNotfoundException(string message) : base(message)
    {

    }
}

public class MalformedTextureException : System.Exception
{
    public MalformedTextureException(string message) : base(message)
    {

    }
}

public static class TextureController 
{
    public static Material default_material;
    public static Color transparent = new Color(0, 0, 0, 0);
	private static Dictionary<string, SpriteMap> layerList;
	public static string[] folderPath;
	
	public static void Initialise()
	{
        //Fix tearing
        //Requires a default material to be built that has pixel snap turned on
        default_material = Resources.Load<Material>("default");
        if(default_material == null)
        {
            Debug.Log("To fix tearing add a material in resources called default with a shader of Sprites/Default and pixel snap turned on");
        }
        // Load Sprite
        layerList = new Dictionary<string, SpriteMap>();
		LoadSpriteMaps();
	}

    private static void LoadSpriteMaps()
	{
        //So people don't have to set a folder path if they don't care
        if (folderPath.Length == 0)
        {
            folderPath = new string[] { "" };
        }
        Color currentColour = transparent;
        for (int i = 0; i < folderPath.Length; i++)
        {
            //Get the texture folder
            string tempPath = folderPath[i];
            //Load the sprites found at the path
            Texture2D[] tempSpriteMap = Resources.LoadAll<Texture2D>(tempPath);
            //For each sprite map
            for (int j = 0; j < tempSpriteMap.Length; j++)
            {
                //Fix border affecting image
                tempSpriteMap[j].filterMode = FilterMode.Point;
                if (tempSpriteMap[j].isReadable)
                {
                    SetAnimations(tempSpriteMap[j]);
                }
                else
                {
                    Debug.Log(tempSpriteMap[j].name + " is not readable and was not loaded");
                }
            }
        }
    }

    public static string ColourToString(Color givenColour)
    {
        return "#"+givenColour.r + givenColour.b + givenColour.g;
    }

    public static void SetAnimations(Texture2D givenTexture)
    {
        Color animationColour = transparent;
        Rect currentAnimationBorder = new Rect(new Vector2(0,0), new Vector2(givenTexture.width, 0));
        SpriteMap currentSpriteMap = new SpriteMap();
        for (int i = 0; i <= givenTexture.height; i++)
        {
            Color pixelColour = givenTexture.GetPixel(0, i);
            if(pixelColour.a == 0)
            {
                if( i - currentAnimationBorder.y > 0)
                {
                    if (animationColour.a != 0)
                    {
                        if (animationColour != currentSpriteMap.GetColour())
                        {
                            currentSpriteMap = new SpriteMap();
                            currentSpriteMap.Initialise(givenTexture.name, animationColour);
                            layerList.Add(currentSpriteMap.m_index, currentSpriteMap);
                        }
                        currentAnimationBorder.size = new Vector2(currentAnimationBorder.size.x, i - currentAnimationBorder.y -1);
                        PlayAnimation currentAnimation = SetAnimation(currentAnimationBorder, animationColour, givenTexture);
                        currentSpriteMap.AddAnimation(currentAnimation);
                    }
                }
                while (givenTexture.GetPixel(0, i).a == 0)
                {
                    i++;
                    if (i == givenTexture.height)
                    {
                        break;
                    }
                }
                currentAnimationBorder.y = i;
                animationColour = pixelColour;

            }
            else if( i == givenTexture.height)
            {
                if(pixelColour.a == 0)
                {
                    throw new TextureNotfoundException(givenTexture.name +" didn't find any textures to make an animation from");
                }
                if (animationColour != currentSpriteMap.GetColour())
                {
                    currentSpriteMap = new SpriteMap();
                    currentSpriteMap.Initialise(givenTexture.name, animationColour);
                    layerList.Add(currentSpriteMap.m_index, currentSpriteMap);
                }
                currentAnimationBorder.size = new Vector2(currentAnimationBorder.size.x, i - currentAnimationBorder.y - 1);
                Rect currentFrameBorder = new Rect(currentAnimationBorder.position, new Vector2(0, currentAnimationBorder.size.y));
                PlayAnimation currentAnimation = SetAnimation(currentAnimationBorder, animationColour, givenTexture);
                currentSpriteMap.AddAnimation(currentAnimation);
            }
            else
            {
                animationColour = pixelColour;
            }
        }
    }
    public static PlayAnimation SetAnimation(Rect currentAnimationBorder, Color animationColour, Texture2D givenTexture)
    {
        PlayAnimation output = new PlayAnimation();
        output.Initialise();
        Rect currentFrameBorder = new Rect(new Vector2(0, currentAnimationBorder.y), new Vector2(0, currentAnimationBorder.size.y));
        Color pixelColour = transparent;
        for(int i = 0; i <= currentAnimationBorder.size.x; i++)
        {
            Color currentPixel = givenTexture.GetPixel(i, (int)currentFrameBorder.y);
            if (currentPixel.a == 0)
            {
                if (i - currentFrameBorder.x > 0)
                {
                    Rect imageBorder = new Rect(new Vector2(currentFrameBorder.position.x+1, currentFrameBorder.y+1),
                        new Vector2(i - currentFrameBorder.x - 2, currentFrameBorder.size.y - 1));
                    output.AddFrame(givenTexture, imageBorder);
                }
                while (givenTexture.GetPixel(i, (int)currentFrameBorder.y).a == 0)
                {
                    i++;
                    if (i >= currentAnimationBorder.size.x)
                    {
                        break;
                    }
                }
                currentFrameBorder.x = i;
            }
            else if(i == currentAnimationBorder.size.x){
                if (animationColour != pixelColour && pixelColour.a != 0)
                {
                    throw new MalformedTextureException(givenTexture.name + " doesn't terminate appropiately");
                }
                Rect imageBorder = new Rect(new Vector2(currentFrameBorder.position.x, currentFrameBorder.y),
                    new Vector2(i - currentFrameBorder.x - 2, currentFrameBorder.size.y - 1));
                output.AddFrame(givenTexture, imageBorder);
            }
        }
        return output;
    }

	public static int GetTimeOffset()
	{
		return 0;
	}

	public static SpriteMap GetSpriteMap(string layerName, Color givenColour)
	{
        string id = (layerName + givenColour);
        if (layerList.ContainsKey(id))
        {
            return layerList[id];
        }
        throw new TextureNotfoundException("Texture " + layerName + " with a border of " + givenColour + " is not found");
    }
}
