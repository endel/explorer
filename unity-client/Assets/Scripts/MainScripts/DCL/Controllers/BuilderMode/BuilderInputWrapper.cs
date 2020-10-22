using Builder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInputWrapper : MonoBehaviour
{
    public LayerMask layerToStopClick;
    public float msClickThreshold = 200;
    public float movementClickThreshold = 50;
    public Action<int,Vector3> OnMouseClick;
    public Action<int,Vector3,float,float> OnMouseDrag;
    public Action<int,Vector3,float,float> OnMouseDragRaw;
    public Action<float> OnMouseWheel;

    float lastTimeMouseDown = 0;
    Vector3 lastMousePosition;
    bool canInputBeMade = true;
    private void Awake()
    {
        DCLBuilderInput.OnMouseDrag += MouseDrag;
        DCLBuilderInput.OnMouseRawDrag += MouseRawDrag;
        DCLBuilderInput.OnMouseWheel += MouseWheel;
        DCLBuilderInput.OnMouseDown += MouseDown;
        DCLBuilderInput.OnMouseUp += MouseUp;
    }

    public void StopInput()
    {
        canInputBeMade = false;
    }
    public void ResumeInput()
    {
        canInputBeMade = true;
    }



    private void MouseUp(int buttonId, Vector3 mousePosition)
    {
        if (!canInputBeMade) return;
        if (Vector3.Distance(mousePosition, lastMousePosition) >= movementClickThreshold) return;
        if (Time.unscaledTime >= lastTimeMouseDown + msClickThreshold / 1000) return;

        if(!BuildModeUtils.IsPointerOverUIElement() && !BuildModeUtils.IsPointerOverMaskElement(layerToStopClick))OnMouseClick?.Invoke(buttonId, mousePosition);     
    }

    private void MouseDown(int buttonId, Vector3 mousePosition)
    {
        lastTimeMouseDown = Time.unscaledTime;
        lastMousePosition = mousePosition;
    }

    private void MouseWheel(float axisValue)
    {
        if (!canInputBeMade) return;
        if (!BuildModeUtils.IsPointerOverUIElement()) OnMouseWheel?.Invoke(axisValue);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!canInputBeMade) return;
        if (!BuildModeUtils.IsPointerOverUIElement()) OnMouseDrag?.Invoke(buttonId, mousePosition, axisX, axisY);
    }
    private void MouseRawDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!canInputBeMade) return;
        if (!BuildModeUtils.IsPointerOverUIElement()) OnMouseDragRaw?.Invoke(buttonId, mousePosition, axisX, axisY);
    }
}