using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject m_freeCam;
    public GameObject m_target;
    public BoardScript m_boardScript;
    public bool m_rotate;
    //public float m_oldZ;

	// Use this for initialization
	void Start ()
    {
        m_rotate = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // REFACTOR: Make all of this into FUNCTIONS

        if (m_rotate)
        {
            float rotSpeed = 0.4f;

            transform.RotateAround(m_freeCam.transform.position, Vector3.up, rotSpeed);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + rotSpeed, transform.position.z), transform.rotation);

            return;
        }

        if (m_target) // If the camera is told to focus on something
        {
            // Determine how much camera will be moving this update
            float camAcceleration = 0.04f;
            float camSpeed = 0.03f;
            float camMovement = Vector3.Distance(m_freeCam.transform.position, m_target.transform.position) * camAcceleration + camSpeed;

            Transform looker = m_freeCam.transform;
            looker.LookAt(m_target.transform);

            if (m_boardScript.m_camIsFrozen)
            {
                float zoomSpeed = 0.1f;
                if (Vector3.Distance(transform.position, m_freeCam.transform.position) > 2)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * zoomSpeed, transform.position.y + transform.forward.y * zoomSpeed, transform.position.z + transform.forward.z * zoomSpeed), transform.rotation);
            }

            if (transform.position != m_target.transform.position)
            {

                // Move camera along it's forward vector towards target
                m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + looker.forward.x * camMovement, m_freeCam.transform.position.y, m_freeCam.transform.position.z + looker.forward.z * camMovement), Quaternion.identity);

                // If the distance is small enough, snap the camera in position to avoid overshooting
                if (Vector3.Distance(m_freeCam.transform.position, m_target.transform.position) < 0.3f)
                    m_target = null;
            }

            // Set camera back to looking at it's empty object
            transform.LookAt(m_freeCam.transform);
            return;
        }

        float wheelSpeed = 4.0f;
        float maxDis = 12;
        float w = Input.GetAxis("Mouse ScrollWheel");

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (Input.GetMouseButton(1)) //Input.GetMouseButton(1)
        {
            float rotSpeed = 4.5f;

            // If right click is held down, moving the mouse side-to-side rotates the screen around the world's y-axis
            transform.RotateAround(m_freeCam.transform.position, Vector3.up, x * rotSpeed);
        }

        // if right click is held down, moiving the mouse wheel rotates the camera around the world's x-axis
        if (!Input.GetKey(KeyCode.E))
        {
            if (Input.GetMouseButton(1) && w < 0 && transform.position.y > m_freeCam.transform.position.y + .5f || Input.GetMouseButton(1) && w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + transform.up.y * w * wheelSpeed, transform.position.z), transform.rotation);
            else if (w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) > 1 || w < 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * w * wheelSpeed, transform.position.y + transform.forward.y * w * wheelSpeed, transform.position.z + transform.forward.z * w * wheelSpeed), transform.rotation);
        }
        transform.LookAt(m_freeCam.transform);

        float forwardMoveSpeed = 0.1f;
        float sideMoveSpeed = 0.1f;
        float wheelMovSpeed = 0.2f;

        if (m_boardScript.m_selected && m_boardScript.m_selected.m_holding && m_boardScript.m_selected.m_holding.tag == "Player")
        {
            Renderer oldRend = m_boardScript.m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
            if (oldRend.materials[2].shader != oldRend.materials[0].shader)
                return;
        }

        if (Input.GetMouseButton(2))
        {
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.forward.x * -y * wheelMovSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.forward.z * -y * wheelMovSpeed), Quaternion.identity);
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.right.x * -x * wheelMovSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.right.z * -x * wheelMovSpeed), Quaternion.identity);
        }

        // WASD will move the camera
        if (Input.GetKey(KeyCode.W))
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.forward.x * forwardMoveSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.forward.z * forwardMoveSpeed), Quaternion.identity);
        if (Input.GetKey(KeyCode.S))
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.forward.x * -forwardMoveSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.forward.z * -forwardMoveSpeed), Quaternion.identity);
        if (Input.GetKey(KeyCode.A))
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.right.x * -sideMoveSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.right.z * -sideMoveSpeed), Quaternion.identity);
        if (Input.GetKey(KeyCode.D))
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.right.x * sideMoveSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.right.z * sideMoveSpeed), Quaternion.identity);
        if (Input.GetKey(KeyCode.F))
            m_target = m_boardScript.m_currCharScript.gameObject;

        transform.LookAt(m_freeCam.transform);
    }
}
