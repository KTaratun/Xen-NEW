using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject m_freeCam;
    public GameObject m_target;
    public GameObject m_board;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_target) // If the camera is told to focus on something
        {
            if (transform.position != m_target.transform.position)
            {
                // Determine how much camera will be moving this update
                float camAcceleration = 0.04f;
                float camSpeed = 0.03f;
                float camMovement = Vector3.Distance(m_freeCam.transform.position, m_target.transform.position) * camAcceleration + camSpeed;

                Transform looker = m_freeCam.transform;
                looker.LookAt(m_target.transform);

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

        if (Input.GetMouseButton(1))
        {
            float rotSpeed = 4.5f;
            float x = Input.GetAxis("Mouse X");

            // If right click is held down, moving the mouse side-to-side rotates the screen around the world's y-axis
            transform.RotateAround(m_freeCam.transform.position, Vector3.up, x * rotSpeed);

            // if right click is held down, moiving the mouse wheel rotates the camera around the world's x-axis
            if (w < 0 && transform.position.y > m_freeCam.transform.position.y + .5f || w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + transform.up.y * w * wheelSpeed, transform.position.z), transform.rotation);
        }
        else
            // if you are not holding down the right click, moving the mouse wheel will zoom the camera in and out
            if (w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) > 1 || w < 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * w * wheelSpeed, transform.position.y + transform.forward.y * w * wheelSpeed, transform.position.z + transform.forward.z * w * wheelSpeed), transform.rotation);

        float forwardMoveSpeed = 0.1f;
        float sideMoveSpeed = 0.1f;

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
        {
            BoardScript boardScript = m_board.GetComponent<BoardScript>();
            m_target = boardScript.m_currPlayer;
        }

        transform.LookAt(m_freeCam.transform);
    }
}
