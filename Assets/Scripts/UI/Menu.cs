using System;
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
        PercentageofScreen = new Vector2(m_canvas.pixelRect.width/100, m_canvas.pixelRect.height/100);
        half_screen = new Vector2(m_canvas.pixelRect.width / 2, m_canvas.pixelRect.height / 2);
    }

    public static Vector2 PercentageofScreen;
    public static Vector2 half_screen;
    protected BoxCollider2D m_collider;

    public static T CreateMenu<T>() where T: Menu
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(T).Name;
        T menu = gameObject.AddComponent<T>();
        gameObject.transform.SetParent(m_canvas.transform);
        menu.Initialise();
        return menu;
    }

    public virtual void SetScreenOffset(float x, float y)
    {
        gameObject.transform.localPosition = new Vector3(x - half_screen.x, y - half_screen.y, gameObject.transform.localPosition.z);
    }

    public virtual void SetScreenOffset(Vector2 screen_offset)
    {
        gameObject.transform.localPosition = new Vector3(screen_offset.x - half_screen.x, screen_offset.y - half_screen.y, gameObject.transform.localPosition.z);
    }

    protected override void Initialise()
    {
        this.transform.localPosition = new Vector3(0, 0, 1);
        base.Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
