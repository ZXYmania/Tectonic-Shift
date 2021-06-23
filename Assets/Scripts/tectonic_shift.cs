using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tectonic_shift : MonoBehaviour
{
    private static int size = 100;
    private static Dictionary<Position, Tile> map;
	private static float camera_speed = 1;

    void Start()
    {
		map = new Dictionary<Position, Tile>();
		for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Tile curr_tile = Tile.CreateTile(i, j);
                map.Add(curr_tile.position, curr_tile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
		MoveCamera();
    }

	public void MoveCamera()
	{
		Vector3 movevector = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			movevector.z += 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			movevector.z -= 1;
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

			if ((camera_speed - 1 * Time.deltaTime) > 5)
			{
				gameObject.GetComponent<Camera>().orthographicSize -= 1 * Time.deltaTime;
				camera_speed -= 1 * Time.deltaTime;
			}
		}
		if (Input.GetKey(KeyCode.E))
		{
			gameObject.GetComponent<Camera>().orthographicSize += 1 * Time.deltaTime;
			camera_speed += 1 * Time.deltaTime; ;
		}
		transform.position += Vector3.Normalize(movevector) * camera_speed * Time.deltaTime;
	}
}
