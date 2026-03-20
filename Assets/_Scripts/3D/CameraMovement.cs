using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] PointPlacement3D PointHandler;
    [SerializeField] float OrbitRadius = 20f, OrbitHeight = 10f, MoveSpeed = 5f, ZoomSpeedMouse = 5f, ZoomSpeedKeys = 5f, MouseSens = 1f;
    [SerializeField] float FreeMoveSpeed = 5f, FreeLookMult = 2f;

    float Angle = 0, RotX, RotY;

    bool FreeLook = false;
    bool LockedMouse = false;
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FreeLook = !FreeLook;
        }
        if(Input.GetMouseButtonDown(1))
        {
            LockedMouse = !LockedMouse;
        }

        Cursor.lockState = LockedMouse ? CursorLockMode.Locked : CursorLockMode.None;

        if (FreeLook)
        {
            Vector3 euler = transform.eulerAngles;
            RotX = euler.x;
            RotX = euler.x > 180f ? euler.x - 360f : euler.x;
            RotY = euler.y;

            Vector3 keyInp = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("QE"), Input.GetAxisRaw("Vertical"));
            float speed = FreeMoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? FreeLookMult : 1f);
            Vector3 moveDir = new Vector3(keyInp.x, 0f, keyInp.z).normalized * speed; //no y input for now
            //rel to world with rot in mind
            transform.position += transform.TransformDirection(moveDir) * Time.deltaTime;
            transform.position += new Vector3(0f, keyInp.y, 0f) * speed * Time.deltaTime;

            if(LockedMouse)
            {
                Vector2 mouseInp = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                RotX -= mouseInp.y * MouseSens;
                RotY += mouseInp.x * MouseSens;
                RotX = Mathf.Clamp(RotX, -89f, 89f);
            }
            transform.localRotation = Quaternion.Euler(RotX, RotY, 0f);
        }
        else
        {
            Vector2 inputXZ = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            float[] inputsZoom = { Input.GetAxisRaw("Mouse ScrollWheel") * ZoomSpeedMouse, Input.GetAxisRaw("QE") * ZoomSpeedKeys * Time.deltaTime }; //use a dict with speeds and inputs

            Angle += inputXZ.x * MoveSpeed * Time.deltaTime;
            OrbitHeight += inputXZ.y * MoveSpeed * Time.deltaTime;
            OrbitHeight = Mathf.Clamp(OrbitHeight, -2f, 2f);

            float orbCache = OrbitRadius;
            foreach (float input in inputsZoom)
            {
                if (orbCache == OrbitRadius)
                    OrbitRadius -= input;
                else
                    break;
            }

            int px = PointHandler.PX, pz = PointHandler.PZ;
            float maxRadius = (px > pz ? px : pz) * 20f;
            OrbitRadius = Mathf.Clamp(OrbitRadius, 2.5f, maxRadius);
            Vector3 orbitPos = PointHandler.CenterPos + new Vector3(Mathf.Cos(Angle), OrbitHeight, Mathf.Sin(Angle)).normalized * OrbitRadius;

            transform.position = orbitPos;
            transform.LookAt(PointHandler.CenterPos);
        }
    }
}
