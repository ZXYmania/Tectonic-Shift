using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface Clickable
{
    public void OnSelect();
    public void UnSelect();

    public void OnHover();

    public void UnHover();
}

public interface DrawContoller
{
    public int GetDistance(Position from, Position to);
    public bool QuickStop();
}

[Serializable]
public class SaveData
{
	public Position size;
	public SaveData(Position size)
	{
		this.size = size;
	}
}
public abstract class Mode : MonoBehaviour
{
    protected static List<Clickable> selected;
    private static Camera m_camera;
    private static float camera_speed = 45;
    public static Mode current_menu { get; protected set; }

    public void SetUp()
    {
		TextureController.folderPath = new string[] { "Texture" };
        TextureController.Initialise();
        m_camera = gameObject.GetComponent<Camera>();
        Map.Initialise();
        m_camera.orthographicSize = Map.size.y / 2;
        m_camera.transform.position = new Vector3(Map.size.x / 2, Map.size.y / 2, -10);
		Menu.Initialise(m_camera);
	}

	public void SaveFile<T>(List<T> data)
	{
		string destination = Application.persistentDataPath + "/map.dat";
		FileStream file;

		if (File.Exists(destination)) file = File.OpenWrite(destination);
		else file = File.Create(destination);

		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, new SaveData(Map.size));
		bf.Serialize(file, data);
		file.Close();
	}

	public List<T> LoadFile<T>()
	{
		string destination = Application.persistentDataPath + "/map.dat";
		FileStream file;

		if (File.Exists(destination)) file = File.OpenRead(destination);
		else
		{
			Debug.LogError("File not found");
			throw new FileNotFoundException("map.dat does not exist");
		}

		BinaryFormatter bf = new BinaryFormatter();
		object test =  bf.Deserialize(file);
		List<T> data = (List<T>) bf.Deserialize(file);
		file.Close();
		return data;
	}

	public void MoveCamera()
	{
		Vector3 movevector = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			movevector.y += 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			movevector.y -= 1;
		}
		if (Input.GetKey(KeyCode.A))
		{
			movevector.x -= 1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			movevector.x += 1;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			m_camera.orthographicSize -= camera_speed / 5 * Time.deltaTime;
			if (m_camera.orthographicSize < camera_speed/5)
			{
				m_camera.orthographicSize = camera_speed / 5;
			}

		}
		if (Input.GetKey(KeyCode.E))
		{
			m_camera.orthographicSize += camera_speed / 5 * Time.deltaTime;
		}
		transform.position += Vector3.Normalize(movevector) * camera_speed * Time.deltaTime;

	}

    public virtual void Clear()
    {
        for(int i = 0; i < selected.Count; i++)
        {
            selected[i].UnSelect();
        }
        selected.Clear();
    }

    public abstract void OnModeEnter();
    public abstract void OnModeExit();
}
