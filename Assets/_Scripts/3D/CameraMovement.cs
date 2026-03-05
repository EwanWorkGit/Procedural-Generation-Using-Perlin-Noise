using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] PointPlacement3D PointHandler;
    [SerializeField] float OrbitRadius = 20f, OrbitHeight = 10f, MoveSpeed = 5f, ScrollSpeed = 5f;

    float Angle = 0;

    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Mouse ScrollWheel"));

        Angle += input.x * MoveSpeed * Time.deltaTime;
        OrbitHeight += input.y * MoveSpeed * Time.deltaTime;
        OrbitHeight = Mathf.Clamp(OrbitHeight, -2f, 2f);
        OrbitRadius -= input.z * ScrollSpeed;
        int px = PointHandler.PX, pz = PointHandler.PZ;
        float maxRadius = (px > pz ? px : pz) * 20f;
        OrbitRadius = Mathf.Clamp(OrbitRadius, 2.5f, maxRadius);
        transform.position = PointHandler.CenterPos + new Vector3(Mathf.Cos(Angle), OrbitHeight, Mathf.Sin(Angle)).normalized * OrbitRadius;
        transform.LookAt(PointHandler.CenterPos);
    }
}
