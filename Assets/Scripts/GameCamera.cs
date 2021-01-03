using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private void Awake()
    {

    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f) {
            AdjustZoom(zoomDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f) {
            AdjustPosition(xDelta, zDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f) {
            AdjustRotation(rotationDelta);
        }
    }

    void AdjustZoom(float delta)
    {
        Vector3 position = transform.localPosition;
        position.y -= delta * 50f;
        transform.localPosition = position;
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 position = transform.localPosition;
        position += new Vector3(xDelta, 0f, zDelta) * 5f;
        transform.localPosition = position;
    }

    void AdjustRotation(float delta)
    {
        Quaternion rotation = transform.localRotation;
        Vector3 eulerAngles = rotation.eulerAngles;
        eulerAngles.y += delta * 5f;
        rotation.eulerAngles = eulerAngles;
        transform.localRotation = rotation;
    }
}
