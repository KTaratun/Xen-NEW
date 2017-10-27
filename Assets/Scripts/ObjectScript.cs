using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour {

    public int m_currHealth;
    public int m_totalHealth;
    public MeshRenderer[] m_meshRend;
    public TileScript m_tile;
    public BoardScript m_boardScript;
    public int m_width;
    public TileScript.nbors m_facing;

    // Use this for initialization
    void Start ()
    {
        m_totalHealth = 5;
        m_currHealth = m_totalHealth;
        m_width = 1;
        m_facing = TileScript.nbors.bottom;
	}
	
	// Update is called once per frame
	protected void Update ()
    {
        if (m_boardScript)
        {
            Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
            if (m_tile && myPos != newPos)
            {
                MovementUpdate();
                myPos = new Vector3(transform.position.x, 0, transform.position.z);
                if (myPos == newPos)
                    MovingFinish();
            }
        }
	}


    // Movement
    public void MovementSelection(int _forceMove)
    {
        int move = 0;

        if (_forceMove > 0)
            move = _forceMove;
        else if (gameObject.tag == "Player")
            move = GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.MOV];

        m_tile.FetchTilesWithinRange(move, new Color(0, 0, 1, 0.5f), false, TileScript.targetRestriction.NONE, false);
    }

    virtual public void MovingStart(TileScript newScript, bool _isForced)
    {
        if (m_tile == newScript || newScript == null)
            return;

        m_tile.m_holding = null;
        m_tile.ClearRadius();
        m_tile = newScript;
        m_boardScript.m_camIsFrozen = true;

        //m_tile.m_holding = gameObject;

        if (!_isForced)
            m_boardScript.m_selected = null;

        PanelScript.CloseHistory();
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
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
        float dis = Vector3.Distance(myPos, newPos);

        if (dis < snapDistance)
            transform.SetPositionAndRotation(new Vector3(m_tile.transform.position.x, transform.position.y, m_tile.transform.position.z), transform.rotation);
    }

    virtual public void MovingFinish()
    {
        m_boardScript.m_camera.GetComponent<CameraScript>().m_target = null;

        if (gameObject.tag == "PowerUp" && m_tile.m_holding && m_tile.m_holding.tag == "Player")
            GetComponent<PowerupScript>().OnPickup(m_tile.m_holding.GetComponent<CharacterScript>());
        else
            m_tile.m_holding = gameObject;

        m_boardScript.m_currButton = null;
        m_boardScript.m_camIsFrozen = false;
        m_boardScript.m_isForcedMove = null;
    }


    // Damage
    public void ReceiveDamage(string _dmg, Color _color)
    {
        if (gameObject.tag == "Environment")
        {
            int parsedDMG;
            if (int.TryParse(_dmg, out parsedDMG))
            {
                if (m_currHealth - parsedDMG <= 0)
                {
                    m_tile.m_holding = null;
                    gameObject.SetActive(false);
                }
                else
                {
                    m_currHealth -= parsedDMG;
                    float ratio = m_currHealth / m_totalHealth;
                    for (int i = 0; i < m_meshRend.Length; i++)
                        m_meshRend[i].material.color = new Color(1, ratio, ratio, 1);
                }
            }
        }
    }


    // Placement
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
