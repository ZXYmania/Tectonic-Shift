using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tectonic_shift : MonoBehaviour
{
	private static Camera m_camera;
	private static float camera_speed = 45;
	private static int rowCount;

    void Start()
    {
		rowCount = 0;
		TextureController.Initialise();
		m_camera =  gameObject.GetComponent<Camera>();
		Map.Initialise();
		m_camera.orthographicSize = Map.size.y / 2;
		m_camera.transform.position = new Vector3(Map.size.x / 2, Map.size.y / 2, -10);
		PlateMode menu = new PlateMode();
	}


    // Update is called once per frame
    void Update()
    {
		if (rowCount < Map.size.x)
		{
			Map.AddColumn(rowCount);
			rowCount++;
		}
		MoveCamera();
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
			if ((camera_speed/5 - 1 * Time.deltaTime) > m_camera.orthographicSize)
			{
				m_camera.orthographicSize -= camera_speed/5 * Time.deltaTime;

			}
			m_camera.orthographicSize = 5;

		}
		if (Input.GetKey(KeyCode.E))
		{
			m_camera.orthographicSize += camera_speed/5 * Time.deltaTime;
		}
		transform.position += Vector3.Normalize(movevector) * camera_speed * Time.deltaTime;

	}
}
