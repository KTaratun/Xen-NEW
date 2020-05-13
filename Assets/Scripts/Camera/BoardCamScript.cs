using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCamScript : MonoBehaviour {

    // General
    private GameObject m_freeCam;
    public GameObject m_target;
    public bool m_zoomIn;
    public bool m_rotate;

    // References
    private BoardScript m_boardScript;
    public FieldScript m_fieldScript;
    private SlidingPanelManagerScript m_panMan;
    private GameManagerScript m_gamMan;

    // Use this for initialization
    void Start ()
    {
        if (m_rotate != true)
            m_rotate = false;

        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
        }
        if (GameObject.Find("Board"))
            m_boardScript = GameObject.Find("Board").GetComponent<BoardScript>();

        m_freeCam = transform.parent.gameObject;
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    private void FixedUpdate()
    {
        // REFACTOR: Make all of this into FUNCTIONS

        if (m_rotate)
        {
            float rotSpeed = 0.4f;

            transform.RotateAround(m_freeCam.transform.position, Vector3.up, rotSpeed);
        }

        if (m_target)
            ForcedCamMovement();

        if (m_rotate || m_target)
            return;

        //if (m_boardScript && m_boardScript.m_battle)
            BattleCamControls();
        //else
        //    FieldCamControls();
    }

    private void BattleCamControls()
    {
        float wheelSpeed = 100.0f;
        float maxDis = 200;
        float minDis = 20;
        float w = Input.GetAxis("Mouse ScrollWheel");

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(1)) //Input.GetMouseButton(1)
        {
            float rotSpeed = 4.5f;

            // If right click is held down, moving the mouse side-to-side rotates the screen around the world's y-axis
            transform.RotateAround(m_freeCam.transform.position, Vector3.up, x * rotSpeed);
        }

        float wheelMovSpeed = 4.0f;
        // Holding down the mouse wheel will allow you to rotate the camera up and down
        if (Input.GetMouseButton(2))
        {
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.forward.x * -y * wheelMovSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.forward.z * -y * wheelMovSpeed), Quaternion.identity);
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + transform.right.x * -x * wheelMovSpeed, m_freeCam.transform.position.y, m_freeCam.transform.position.z + transform.right.z * -x * wheelMovSpeed), Quaternion.identity);
        }

        // if right click is held down, moiving the mouse wheel rotates the camera around the world's x-axis
        if (!Input.GetKey(KeyCode.E))
        {
            if (Input.GetMouseButton(1) && w < 0 && transform.position.y > m_freeCam.transform.position.y + 10.0f || Input.GetMouseButton(1) && w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + transform.up.y * w * wheelSpeed, transform.position.z), transform.rotation);
            else if (w > 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) > minDis || w < 0 && Vector3.Distance(transform.position, m_freeCam.transform.position) < maxDis)
                transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * w * wheelSpeed, transform.position.y + transform.forward.y * w * wheelSpeed, transform.position.z + transform.forward.z * w * wheelSpeed), transform.rotation);
        }
        transform.LookAt(m_freeCam.transform);


        if (m_boardScript.m_selected && m_boardScript.m_selected.m_holding && m_boardScript.m_selected.m_holding.tag == "Player")
        {
            // If character is not being viewed
            CharacterScript charScript = m_boardScript.m_selected.m_holding.GetComponent<CharacterScript>();
            if (charScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor == Color.magenta)
                return;
        }


        float forwardMoveSpeed = 1.7f;
        float sideMoveSpeed = 1.7f;
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
            m_target = m_gamMan.m_currCharScript.gameObject;

        transform.LookAt(m_freeCam.transform);
    }

    private void FieldCamControls()
    {
        float forwardMoveSpeed = 0.5f;
        float sideMoveSpeed = 0.5f;
        float x = Input.GetAxis("Mouse X");
        float rotSpeed = 4.5f;

        GameObject mainChar = m_fieldScript.m_mainChar.gameObject;

        // If right click is held down, moving the mouse side-to-side rotates the screen around the world's y-axis
        mainChar.transform.RotateAround(mainChar.transform.position, Vector3.up, x * rotSpeed);

        // WASD will move the camera
        if (Input.GetKey(KeyCode.W))
            mainChar.transform.SetPositionAndRotation(new Vector3(mainChar.transform.position.x + mainChar.transform.forward.x * forwardMoveSpeed, mainChar.transform.position.y, mainChar.transform.position.z + mainChar.transform.forward.z * forwardMoveSpeed), mainChar.transform.rotation);
        if (Input.GetKey(KeyCode.S))
            mainChar.transform.SetPositionAndRotation(new Vector3(mainChar.transform.position.x + mainChar.transform.forward.x * -forwardMoveSpeed, mainChar.transform.position.y, mainChar.transform.position.z + mainChar.transform.forward.z * -forwardMoveSpeed), mainChar.transform.rotation);
        if (Input.GetKey(KeyCode.A))
            mainChar.transform.SetPositionAndRotation(new Vector3(mainChar.transform.position.x + mainChar.transform.right.x * -sideMoveSpeed, mainChar.transform.position.y, mainChar.transform.position.z + mainChar.transform.right.z * -sideMoveSpeed), mainChar.transform.rotation);
        if (Input.GetKey(KeyCode.D))
            mainChar.transform.SetPositionAndRotation(new Vector3(mainChar.transform.position.x + mainChar.transform.right.x * sideMoveSpeed, mainChar.transform.position.y, mainChar.transform.position.z + mainChar.transform.right.z * sideMoveSpeed), mainChar.transform.rotation);

        float zoom = 15f;
        Vector3 camPos = new Vector3(mainChar.transform.position.x - mainChar.transform.forward.x * zoom, mainChar.transform.position.y + 12f, mainChar.transform.position.z - mainChar.transform.forward.z * zoom);
        transform.SetPositionAndRotation(camPos, transform.rotation);
        transform.LookAt(mainChar.transform);
        transform.Rotate(Vector3.right, -30.0f);
    }

    private void ForcedCamMovement()
    {
        // Determine how much camera will be moving this update
        float camAcceleration = 0.04f;
        float camSpeed = 0.03f;
        float camMovement = Vector3.Distance(m_freeCam.transform.position, m_target.transform.position) * camAcceleration + camSpeed;

        Transform looker = m_freeCam.transform;
        looker.LookAt(m_target.transform);
        bool camZoomComplete = false;

        if (m_boardScript.m_camIsFrozen)
            camZoomComplete = AdjustZoom();
        else
            camZoomComplete = true;

        if (transform.position != m_target.transform.position)
        {

            // Move camera along it's forward vector towards target
            m_freeCam.transform.SetPositionAndRotation(new Vector3(m_freeCam.transform.position.x + looker.forward.x * camMovement, m_freeCam.transform.position.y, m_freeCam.transform.position.z + looker.forward.z * camMovement), Quaternion.identity);

            // If the distance is small enough, snap the camera in position to avoid overshooting
            if (Vector3.Distance(m_freeCam.transform.position, m_target.transform.position) < 3.0f && camZoomComplete)
            {
                if (m_panMan.GetPanel("Round End Panel").m_inView && m_gamMan.m_currCharScript && m_target != m_gamMan.m_currCharScript.gameObject)
                    m_gamMan.m_actionEndTimer += Time.deltaTime;
                else
                {
                    if (m_panMan.GetPanel("Round End Panel").m_inView && m_gamMan.m_currCharScript)
                        m_panMan.GetPanel("Round End Panel").ClosePanel();

                    if (m_gamMan.m_actionEndTimer == 0)
                        m_boardScript.m_camIsFrozen = false;
                    m_target = null;
                    m_zoomIn = true;
                }
            }
        }

        // Set camera back to looking at it's empty object
        transform.LookAt(m_freeCam.transform);
    }

    private bool AdjustZoom()
    {
        float zoomSpeed = 2.0f;
        float dis = Vector3.Distance(transform.position, m_freeCam.transform.position);
        if (m_zoomIn)
        {
            if (dis > 50.0f)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * zoomSpeed, transform.position.y + transform.forward.y * zoomSpeed, transform.position.z + transform.forward.z * zoomSpeed), transform.rotation);
                return false;
            }
        }
        else
        {
            if (dis < 80.0f)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x - transform.forward.x * zoomSpeed, transform.position.y - transform.forward.y * zoomSpeed, transform.position.z - transform.forward.z * zoomSpeed), transform.rotation);
                return false;
            }
        }

        return true;
    }
}
