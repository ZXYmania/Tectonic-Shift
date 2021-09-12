using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Menu : Animated
{
    public static Canvas m_canvas;
    public static EventSystem m_event;

    public static void Initialise(Camera given_camera)
    {
        GameObject canvas_object = new GameObject();
        canvas_object.name = "ui_canvas";
        m_canvas = canvas_object.gameObject.AddComponent<Canvas>();
        canvas_object.AddComponent<GraphicRaycaster>();
        canvas_object.AddComponent<CanvasScaler>();
        canvas_object.transform.SetParent(given_camera.transform);
        m_canvas.worldCamera = given_camera;
        m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_canvas.planeDistance = 1;
        GameObject event_object = new GameObject();
        m_event = event_object.AddComponent<EventSystem>();
        m_event.name = "Main Event Handler";
        event_object.AddComponent<StandaloneInputModule>();
        event_object.AddComponent<BaseInput>();
        PercentageofScreen = m_canvas.pixelRect.width;
        half_screen = new Vector2(m_canvas.pixelRect.width / 2, m_canvas.pixelRect.height / 2);
    }

    public static float PercentageofScreen;
    public static Vector2 half_screen;
    protected BoxCollider2D m_collider;
    public static Vector2 ConvertImagetoWorldSize(Vector2 image_size)
    {
        return new Vector2(100 / image_size.x, 100 / image_size.y);
    }
    public static T CreateMenu<T>() where T: Menu
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(T).Name;
        T menu = gameObject.AddComponent<T>();
        gameObject.transform.SetParent(m_canvas.transform);
        return menu;
    }

    public virtual void SetSize(float size)
    {
        gameObject.transform.localScale = new Vector3(PercentageofScreen * size, PercentageofScreen * size, 0);
    }

    public virtual void SetOffset(float x, float y)
    {
        gameObject.transform.localPosition = new Vector3(x - half_screen.x, y - half_screen.y, 0);
    } 

    public void SetCollider(Vector2 image_size)
    {
        m_collider.offset = new Vector2(1 / image_size.x / 2, 1 / image_size.y / 2);
        m_collider.size = new Vector2(1 / image_size.x, 1 / image_size.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
