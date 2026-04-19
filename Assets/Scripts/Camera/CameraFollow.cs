/*******************************************************************************
File:      CameraFollow.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/11/2022
Course:    DES 214

Description:
    This component is added to a camera to have it follow a specified target.
    It follows the target using an adjusted 2D linear interpolation on FixedUpdate.

*******************************************************************************/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////
    // DESIGNER ADJUSTABLE VALUES
    //////////////////////////////////////////////////////////////////////////

    //Maximum acceleration rate of the camera
    private float MaxAccel = 1.0f;
    //Percentage of distance to interpolate over one second
    private float Interpolant = 1.0f;
    //Percentage to interpolate zooming out over one second
    private float ZoomOutInterpolant = 1.0f;
    //Percentage to interpolate zooming in over one second
    private float ZoomInInterpolant = 0.6f;
    //Map mode zoom size
    private float MapModeZoom = 26.0f; //This might need to be bigger if you have a large level
    //Speed for when manually moving the camera
    private float CameraMoveSpeed = 5.0f;
    //////////////////////////////////////////////////////////////////////////

    //The camera target being followed
    [HideInInspector]
    public Transform ObjectToFollow;

    //Calculated speed limit used to enforce maximum acceleration
    private float SpeedLimit = 0.0f;

    private bool SpectatePlayer = false;

    // for debug draw
    private static List<(Vector3 position, Vector3 size, Color color)> _cubes = new();
    private Material _mat;

    private void Awake()
    {
        _mat = new Material(Shader.Find("Sprites/Default"));
        _mat.renderQueue = 9990;
    }

    //Fixed update should always be used for smoother camera movement
    void FixedUpdate()
    {
        //Nothing to follow...
        if (ObjectToFollow == null)
            return;

        if (SpectatePlayer == false)
        {
            transform.position = new Vector3(0, 0, transform.position.z);

            GetComponent<Camera>().orthographicSize = MapModeZoom;
        }

        // if not pressing C and we are spectating the player
        if (Input.GetKey(KeyCode.C) == false && SpectatePlayer == true)
        {
            //Follow the camera target
            FollowTarget();

            //Adjust the zoom level
            AdjustZoom();
        }
        // if we are pressing C and are spectating the player
        else if(Input.GetKey(KeyCode.C) == true && SpectatePlayer == true)
        {
            if (Input.GetKey(KeyCode.UpArrow)) // move camera up
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + (CameraMoveSpeed * Time.deltaTime), transform.position.z);
            }
            if (Input.GetKey(KeyCode.DownArrow)) // move camera down
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - (CameraMoveSpeed * Time.deltaTime), transform.position.z);
            }
            if (Input.GetKey(KeyCode.RightArrow)) // move camera right
            {
                transform.position = new Vector3(transform.position.x + (CameraMoveSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) // move camera left
            {
                transform.position = new Vector3(transform.position.x - (CameraMoveSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            }
        }

        //M to see the whole map
        if (Input.GetKey(KeyCode.M))
            GetComponent<Camera>().orthographicSize = MapModeZoom;
    }

    //Follow the camera target
    void FollowTarget()
    {
        //Find the offset to the target
        Vector3 targetPos = ObjectToFollow.position;
        Vector2 adjust = targetPos - transform.position;
        float distance2D = adjust.magnitude; //Use later to detect overshooting

        //Determine amount to interpolate
        adjust *= Interpolant * Time.deltaTime;

        //Adjust if it is going too fast
        if (adjust.magnitude > SpeedLimit)
            adjust = adjust.normalized * SpeedLimit;

        //Adjust if it is going too slow so it doesn't take forever at the end
        if (adjust.magnitude < 0.5f * Time.deltaTime)
            adjust = adjust.normalized * 0.5f * Time.deltaTime;

        //Save old position for speed limit calculation below
        var oldPosition = transform.position;

        //Move towards the target, but not along the Z axis
        if (adjust.magnitude < distance2D)
            transform.Translate(adjust.x, adjust.y, 0.0f);
        else //Don't overshoot the target
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);

        //Limit how fast the camera can accelerate so it doesn't feel too jumpy
        SpeedLimit = (transform.position - oldPosition).magnitude / Time.deltaTime + MaxAccel * Time.deltaTime;
    }

    //Adjust the zoom level over time
    void AdjustZoom()
    {
        //Find the target zoom level
        float targetZoom = ObjectToFollow.GetComponent<CameraTarget>().Zoom;
        float zoomAdjust = targetZoom - GetComponent<Camera>().orthographicSize;

        //Use later to detect overshooting
        float zoomDistance = Mathf.Abs(zoomAdjust);

        //Determine amount to interpolate
        if (zoomAdjust > 0.0f)
            zoomAdjust *= ZoomOutInterpolant * Time.deltaTime;
        else
            zoomAdjust *= ZoomInInterpolant * Time.deltaTime;

        //Adjust if it is going too slow
        if (zoomAdjust < 0.5f * Time.deltaTime && zoomAdjust > 0)
            zoomAdjust = 0.5f * Time.deltaTime;
        else if (zoomAdjust > -0.01f && zoomAdjust < 0)
            zoomAdjust = -0.5f * Time.deltaTime;

        //Move towards the target zoom level
        if (Mathf.Abs(zoomAdjust) < zoomDistance)
            GetComponent<Camera>().orthographicSize += zoomAdjust;
        else //Don't overshoot the target zoom level
            GetComponent<Camera>().orthographicSize = targetZoom;
    }

    public void SetCameraSpectatePlayer(bool doSpectatePlayer)
    {
        SpectatePlayer = doSpectatePlayer;
    }
    public bool GetCameraSpectatePlayer()
    {
        return SpectatePlayer;
    }



    public void DrawCube(Vector3 position, Vector3 size, Color color)
    {
        _cubes.Add((position, size, color));
    }

    void OnPostRender()
    {
        if (_cubes.Count == 0) return;

        _mat.SetPass(0);
        GL.Begin(GL.QUADS);

        foreach (var (position, size, color) in _cubes)
        {
            float halfX = size.x / 2f;
            float halfY = size.y / 2f;

            GL.Color(color);
            GL.Vertex(position + new Vector3(-halfX, -halfY, 0)); // bottom left
            GL.Vertex(position + new Vector3(halfX, -halfY, 0)); // bottom right
            GL.Vertex(position + new Vector3(halfX, halfY, 0)); // top right
            GL.Vertex(position + new Vector3(-halfX, halfY, 0)); // top left
        }

        GL.End();
        _cubes.Clear();
    }
}
