using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour {

    public TileScript m_tile;
    public BoardScript m_boardScript;
    public int m_width;
    public TileScript.nbors m_facing;

    // Use this for initialization
    void Start ()
    {
        m_width = 1;
        m_facing = TileScript.nbors.bottom;
	}
	
	// Update is called once per frame
	protected void Update ()
    {
        if (m_boardScript)
        {
            if (m_tile && transform.position != m_tile.transform.position)
            {
                MovementUpdate();
                if (transform.position == m_tile.transform.position)
                    FinishMoving();
            }
        }
	}

    public void StartMoving(TileScript newScript, bool _isForced)
    {
        if (m_tile == newScript || newScript == null)
            return;

        m_tile.m_holding = null;
        if (!_isForced)
            m_boardScript.m_selected = null;

        m_tile.ClearRadius();
        m_tile = newScript;
        m_boardScript.m_camIsFrozen = true;

        if (this == m_boardScript.m_currCharScript)
            m_boardScript.m_currTile = m_tile;

        if (gameObject.tag == "Player")
            GetComponent<CharacterScript>().CharStartMoving(newScript, _isForced);
    }

    public void MovementUpdate()
    {
        // Determine how much the character will be moving this update
        float charAcceleration = 0.02f;
        float charSpeed = 0.015f;
        float charMovement = Vector3.Distance(transform.position, m_tile.transform.position) * charAcceleration + charSpeed;
        transform.LookAt(m_tile.transform);
        transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement, transform.position.y, transform.position.z + transform.forward.z * charMovement), transform.rotation);
        m_boardScript.m_camera.GetComponent<CameraScript>().m_target = gameObject;

        // Check to see if character is close enough to the point
        float snapDistance = 0.007f;
        float dis = Vector3.Distance(transform.position, m_tile.transform.position);
        if (dis < snapDistance)
            transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation);
    }

    public void FinishMoving()
    {
        CameraScript cam = m_boardScript.m_camera.GetComponent<CameraScript>();
        cam.m_target = null;

        // Reset tile
        m_tile.m_holding = gameObject;
        m_boardScript.m_currButton = null;
        m_tile.m_traversed = false;

        m_boardScript.m_camIsFrozen = false;
        m_boardScript.m_isForcedMove = null;

        if (gameObject.tag == "Player")
            GetComponent<CharacterScript>().CharFinishMoving();
    }

    public void RandomRotation()
    {
        int randomRot = Random.Range(0, 5);
        transform.Rotate(0, randomRot * 90, 0);

        if (randomRot == 0)
            m_facing = TileScript.nbors.bottom;
        else if (randomRot == 1)
            m_facing = TileScript.nbors.left;
        else if (randomRot == 2)
            m_facing = TileScript.nbors.top;
        else if (randomRot == 3)
            m_facing = TileScript.nbors.right;
    }

    public void PlaceRandomly(BoardScript bScript)
    {
        m_boardScript = bScript;

        // Set up position
        TileScript script;
        bool isPlacable = false;
        int randX;
        int randZ;

        do
        {
            randX = Random.Range(0, m_boardScript.m_width - 1);
            randZ = Random.Range(0, m_boardScript.m_height - 1);

            script = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].GetComponent<TileScript>();

            if (!script.m_holding)
            {
                if (m_width <= 1)
                    isPlacable = true;
                else if (m_width == 2 && script.m_neighbors[(int)m_facing] && !script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding)
                {
                    isPlacable = true;
                    script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding = gameObject;
                }

            }
        } while (!isPlacable);

        script.m_holding = gameObject;
        transform.SetPositionAndRotation(m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].transform.position, transform.rotation);
        m_tile = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width];
    }

}
