﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectUnits : MonoBehaviour {
    // GUI Rect Source code found at:
    // http://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/

    public GameObject DestinationPrefab;

    public GameObject destination;

    // Currently selected objects
    private List<GameObject> selectedUnits;
    // Friendly (selectable) vehicles
    private GameObject[] selectableUnits;


    #region Selection Utility Rectangles
    static Texture2D _whiteTexture;
    private static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }


    private static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
        var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
        // Create Rect
        return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
    }

    private static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
        // Left
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
        // Right
        DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
        // Bottom
        DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
    }

    private static Bounds GetViewportBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        var v1 = Camera.main.ScreenToViewportPoint( screenPosition1 );
        var v2 = Camera.main.ScreenToViewportPoint( screenPosition2 );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;
 
        var bounds = new Bounds();
        bounds.SetMinMax( min, max );
        return bounds;
    }

    bool isSelecting = false;
    Vector3 mousePosition1;

    private bool IsWithinSelectionBounds(GameObject gameObject)
    {
        var camera = Camera.main;
        var viewportBounds =
            GetViewportBounds(camera, mousePosition1, Input.mousePosition);

        return viewportBounds.Contains(
            camera.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(mousePosition1, Input.mousePosition);
            // Draw transparent rectangle
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            // Draw rectangle border
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
    #endregion Selection Utility Rectangles

    void Start()
    {
        selectedUnits = new List<GameObject>();
    }

	void Update () { 
        RaycastHit hit;

        // If we press the left mouse button, save mouse location and begin selection
        if (Input.GetMouseButtonDown(0))
        {
            selectableUnits = GameObject.FindGameObjectsWithTag("Unit");

            // If player clicked on a selectable unit
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.transform.gameObject.tag == "Unit")
                {
                    ClearSelectedUnits();
                    //hit.transform.gameObject.GetComponent<WheeledVehicle>().SetSelected(true);
                    selectedUnits.Add(hit.transform.gameObject);
                    isSelecting = false;

                    return;
                }
            }
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }
        // If we let go of the left mouse button, end selection
        if (Input.GetMouseButtonUp(0))
        {
            if (!isSelecting)
                return;
            isSelecting = false;
            // Find selectable gameObjects in rectangle
            foreach(GameObject go in selectableUnits)
                if (IsWithinSelectionBounds(go))
                {
                    
                    selectedUnits.Add(go);
                }
                else
                {                  
                    
                    selectedUnits.Remove(go);
                }
        }

        // Right mouse button : Check for new destination
        if (Input.GetMouseButtonDown(1))
        { 
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && selectedUnits.Count > 0)
            {
                if(!destination)
                {
                    destination = GameObject.Instantiate(DestinationPrefab, hit.point, Quaternion.identity);
                }else
                {
                    destination.transform.position = hit.point;
                }
                
                BroadcastNewTarget(hit.point);
            }
        }
	}

    /// <summary>
    /// Mark all selected units as unselected
    /// </summary>
    void ClearSelectedUnits()
    {
        foreach (GameObject go in selectedUnits)
        {
            //go.GetComponent<WheeledVehicle>().SetSelected(false);
        }
        selectedUnits = new List<GameObject>();
    }

    /// <summary>
    /// Broadcast to all selected units that a new destination has been given.
    /// </summary>
    public void BroadcastNewTarget(Vector3 position)
    {
        foreach (GameObject go in selectedUnits)
        {
            go.GetComponent<VehicleBase>().SetDestination(position);
        }
    }    
}
