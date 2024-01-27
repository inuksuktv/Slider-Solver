using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    private Transform _background;

    void Start()
    {
        // Assign a reference to save letters.
        _background = GameObject.Find("Background").transform;

        var position = OrientCameraAndBackground();
        SetBackground(position);
    }
    private Vector3 OrientCameraAndBackground()
    {
        var width = GridManager.Instance.BoardWidth;
        var height = GridManager.Instance.BoardHeight;

        // The bigger the gameboard, the higher the camera is set.
        var largerDimension = Mathf.Max(width, height);
        Vector3 position = new(width * 0.5f - 0.5f, largerDimension + 6, height * 0.5f - 0.5f - (largerDimension * 0.125f));
        transform.SetPositionAndRotation(position, Quaternion.Euler(90, 0, 0));

        return position;
    }

    private void SetBackground(Vector3 position)
    {
        // Also set the background underneath the camera.
        Vector3 backgroundPosition = position;
        backgroundPosition.y = GridManager.Instance.TileOffset - 1;
        _background.position = backgroundPosition;
    }
}
