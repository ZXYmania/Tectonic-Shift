using UnityEngine;
using System.Collections;
using System.Collections.Generic;

	public class AudioController 
{
	List<AudioClip> m_sound;
	// Use this for initialization
	void Start () 
	{
		Object[] sounds = Resources.LoadAll("Sounds");
		m_sound = new List<AudioClip>();
		for(int i = 0; i < sounds.Length; i++)
		{
			m_sound.Add(sounds[i] as AudioClip);
		}
	}
	
	// Update is called once per frame
	public AudioClip GetAudio(string givenString)
	{
		return m_sound.Find(item => item.name == givenString);
	}
}
