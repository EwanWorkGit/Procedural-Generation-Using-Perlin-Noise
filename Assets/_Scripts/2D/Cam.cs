using UnityEngine;

public class Cam : MonoBehaviour
{
    //zoom, movement (wont lag like GOL)
    [SerializeField] float Speed = 20f, Sensitivity = 5f;

    Camera PlayerCam;
    float CamSize;

    private void Start()
    {
        PlayerCam = FindObjectOfType<Camera>();
        CamSize = PlayerCam.orthographicSize;
    }

    private void Update()
    {
        Vector2 inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 movement = inputs * Time.deltaTime * Speed;
        transform.position += movement;

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        CamSize -= scroll * Sensitivity;
        Debug.Log(CamSize);
        if(CamSize <= 0)
        {
            CamSize = 0.01f;
        }
        PlayerCam.orthographicSize = CamSize;
    }
}
