using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface Clickable
{
    public void OnSelect();
    public void UnSelect();

    public void OnHover();

    public void UnHover();
}

public interface UserAction
{
	public int GetPriority();
	public bool IsPermanent();
	public void Execute();
	public void Undo();

}

public interface DrawController
{
	public void Initialise();
	public int GetDistance(Position from, Position to);
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
    public static Mode current_mode { get; protected set; }
	protected static List<JobHandle> finished_jobs = new List<JobHandle>();
	protected static bool focused;
	public List<UserAction> current_actions;
	public List<UserAction> history;
	public List<UserAction> reverted_history;

	public void SetUp()
    {
		TextureController.folderPath = new string[] { "Texture" };
        TextureController.Initialise();
        m_camera = gameObject.GetComponent<Camera>();
        Map.Initialise();
        m_camera.orthographicSize = Map.size.y / 2;
        m_camera.transform.position = new Vector3(Map.size.x / 2, Map.size.y / 2, -10);
		Menu.Initialise(m_camera);
		/*TextField.Selected += Focus;
		TextField.UnSelected += Unfocus;*/
		history = new List<UserAction>();
		reverted_history = new List<UserAction>();
		current_actions = new List<UserAction>();
	}

	public static float GetCameraSize()
    {
		return m_camera.orthographicSize;
    }

	public static void ChangeFocus(InputFieldLayer given_field)
	{
		focused = given_field.Selected();
	}

	public static bool IsFocused()
    {
		return focused;
    }
	public static void AddToQueue(UserAction givenAction)
	{
		current_mode.current_actions.Add(givenAction);
	}

	protected static void FlushHistory()
    {
		current_mode.history.RemoveAll((item) => !item.IsPermanent());
		current_mode.reverted_history.RemoveAll((item) => !item.IsPermanent());
    }

	public class PathScheduler<T> where T : struct, DrawController
	{
		public JobHandle current_job;
		public Map.FindPath<T> path;
		public Guid controller_id;
		public PathScheduler()
		{
			Map.FindPath<T>.Finish += Clean;
		}

		~PathScheduler()
		{
			Map.FindPath<T>.Finish -= Clean;
		}

		public void ScheduleFindPath(Tile start, Tile end)
		{
			path = new Map.FindPath<T>(start, end);
			controller_id = path.m_id;
			current_job = path.Schedule();
			Map.FindPath<T>.current.Add( controller_id);
		}

		public void ScheduleFindPath(Tile start, Tile end, T given_controller)
		{
			path = new Map.FindPath<T>(start, end, given_controller);
			controller_id = path.m_id;
			current_job = path.Schedule();
			Map.FindPath<T>.current.Add(controller_id);
		}
		public void EndJob()
        {
			Map.FindPath<T>.current.Remove(controller_id);
        }

		public void Clean()
		{
			finished_jobs.Add(current_job);
		}
	}
	public virtual void CleanJobs()
	{
		for (int i = 0; i < finished_jobs.Count; i++)
		{
			finished_jobs[i].Complete();
		}
		finished_jobs.Clear();
	}

	public static void SaveFile<T>(List<T> data, string name = "map.dat")
	{
		string destination = Application.persistentDataPath + "/" + name;
		FileStream file;

		if (File.Exists(destination)) file = File.OpenWrite(destination);
		else file = File.Create(destination);

		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, new SaveData(Map.size));
		bf.Serialize(file, data);
		file.Close();
	}

	public static List<T> LoadFile<T>(string name = "map.dat")
	{
		string destination = Application.persistentDataPath + "/" + name;
		FileStream file;

		if (File.Exists(destination)) file = File.OpenRead(destination);
		else
		{
			Debug.LogError("File not found");
			return new List<T>();
		}

		BinaryFormatter bf = new BinaryFormatter();
		SaveData test =  (SaveData) bf.Deserialize(file);
		try
		{
			List<T> data = (List<T>)bf.Deserialize(file);
			file.Close();
			return data;
		}
		catch (Exception e)
        {
			file.Close();
			string type_name = typeof(T).Name;
			throw new Exception(name + " has been changed or " + file + " is not of type " + name + " | " + e);
        }

	}

	public void MoveCamera()
	{
		camera_speed = m_camera.orthographicSize;
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
			m_camera.orthographicSize -= camera_speed * Time.deltaTime;
			if (m_camera.orthographicSize < 20)
			{
				m_camera.orthographicSize = 20;
			}

		}
		if (Input.GetKey(KeyCode.E))
		{
			m_camera.orthographicSize += camera_speed * Time.deltaTime;
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
