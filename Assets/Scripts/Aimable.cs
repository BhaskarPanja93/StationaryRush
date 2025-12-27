using System;
using UnityEngine;

public class Aimable : MonoBehaviour
{
    [HideInInspector] public string uniqueName;
    [HideInInspector] public bool restockable;
    
    public Action OnClickAction, OnHoverAction, OnUnHoverAction;

    public void Awake()
    {
        if (gameObject.GetComponent<Outline>() == null)
            gameObject.AddComponent<Outline>();
        gameObject.GetComponent<Outline>().OutlineColor = Color.magenta;
        gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
        gameObject.GetComponent<Outline>().enabled = false;
    }

    public void Hovered()
    {
        gameObject.GetComponent<Outline>().enabled = true;
        if (OnHoverAction != null)
            OnHoverAction();
    }

    public void UnHovered()
    {
        gameObject.GetComponent<Outline>().enabled = false;
        if (OnUnHoverAction != null)
            OnUnHoverAction();
    }

    public void Clicked()
    {
        if (OnClickAction != null)
            OnClickAction();
    }
}
