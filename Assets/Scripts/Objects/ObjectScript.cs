using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour {

    public string m_name;
    public int m_id;
    public int m_currHealth;
    public int m_totalHealth;
    public MeshRenderer[] m_meshRend;
    public TileScript m_tile;
    public BoardScript m_boardScript;
    public int m_width;
    public TileLinkScript.nbors m_facing = TileLinkScript.nbors.BOTTOM;
    
    protected SlidingPanelManagerScript m_panMan;
    protected Camera m_camera;
    protected GameManagerScript m_gamMan;

    // Use this for initialization
    protected void Start ()
    {
        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            if (GameObject.Find("Scene Manager").GetComponent<NetworkedGameScript>())
                GameObject.Find("Scene Manager").GetComponent<NetworkedGameScript>().AddToOBJList(this);
            if (GameObject.Find("Scene Manager").GetComponent<GameManagerScript>())
                m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
        }

        if (m_totalHealth == 0)
            m_totalHealth = 5 * m_width;
        m_currHealth = m_totalHealth;

        if (GameObject.Find("Board"))
            m_boardScript = GameObject.Find("Board").GetComponent<BoardScript>();

        if (GameObject.Find("BoardCam/Main Camera"))
            m_camera = GameObject.Find("BoardCam/Main Camera").GetComponent<Camera>();
    }
	
	// Update is called once per frame
	protected void Update ()
    {
	}

    protected void FixedUpdate()
    {
        if (m_boardScript && m_tile)
        {
            Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);

            if (m_tile && Vector3.Distance(myPos, newPos) > 0.1f)
            {
                MovementUpdate();
                myPos = new Vector3(transform.position.x, 0, transform.position.z);
                if (myPos == newPos)
                    MovingFinish();
            }
            else if (m_tile && myPos == newPos)
                if (myPos.y != newPos.y)
                    myPos.x = 4;
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

        TileLinkScript.FetchTilesWithinRange(m_tile, GetComponent<CharacterScript>(), move, new Color(0, 0, 1, 0.5f), false, TileLinkScript.targetRestriction.NONE, false);
    }

    // The entry point for all movement
    virtual public void MovingStart(TileScript newScript, bool _isForced, bool _isNetForced)
    {
        if (m_tile == newScript || newScript == null)
            return;

        if (GameObject.Find("Network").GetComponent<CustomDirect>().m_isStarted && !_isNetForced)
        {
            CustomDirect s = GameObject.Find("Network").GetComponent<CustomDirect>();
            string msg = "MOVESTART~" + m_id.ToString() + '|' + newScript.m_id + '|' + _isForced.ToString();
            s.SendMessageCUSTOM(msg);
        }

        if (m_tile)
        {
            m_tile.m_holding = null;
            TileLinkScript.ClearRadius(m_tile);
        }
        m_tile = newScript;
        m_boardScript.m_camIsFrozen = true;

        //m_tile.m_holding = gameObject;

        if (!_isForced)
            m_boardScript.m_selected = null;

        m_panMan.CloseHistory();
    }

    virtual public void MovementUpdate()
    {
        // Determine how much the character will be moving this update
        float charAcceleration = 0.02f;
        float charSpeed = 0.3f;
        float charMovement = Vector3.Distance(transform.position, m_tile.transform.position) * charAcceleration + charSpeed;

        transform.LookAt(m_tile.transform);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
        float dis = Vector3.Distance(myPos, newPos);

        transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement, transform.position.y, transform.position.z + transform.forward.z * charMovement), transform.rotation);
        if (!m_panMan.GetPanel("Round End Panel").m_inView)
            m_camera.GetComponent<BoardCamScript>().m_target = gameObject;

        // Check to see if character is close enough to the point
        float snapDistance = 0.2f;
        myPos = new Vector3(transform.position.x, 0, transform.position.z);
        newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
        dis = Vector3.Distance(myPos, newPos);

        if (dis < snapDistance)
            transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation); // new Vector3(m_tile.transform.position.x, transform.position.y, m_tile.transform.position.z)
    }

    virtual public void MovingFinish()
    {
        if (!m_panMan.GetPanel("Round End Panel").m_inView)
            m_camera.GetComponent<BoardCamScript>().m_target = null;

        if (tag == "PowerUp" && m_tile.m_holding && m_tile.m_holding.tag == "Player")
            GetComponent<PowerupScript>().OnPickup(m_tile.m_holding.GetComponent<CharacterScript>());
        else
            m_tile.m_holding = gameObject;

        m_boardScript.m_currButton = null;

        if (!m_panMan.GetPanel("Round End Panel").m_inView)
        {
            m_boardScript.m_camIsFrozen = false;
            m_boardScript.m_isForcedMove = null;
        }
    }


    // Damage
    virtual public void ReceiveDamage(string _dmg, Color _color)
    {
        if (gameObject.tag == "Environment")
        {
            int parsedDMG;
            if (int.TryParse(_dmg, out parsedDMG))
            {
                if (m_currHealth - parsedDMG <= 0)
                {
                    m_tile.m_holding = null;
                    if (m_width == 2)
                        m_tile.m_neighbors[(int)m_facing].m_holding = null;
                    gameObject.SetActive(false);
                }
                else
                {
                    m_currHealth -= parsedDMG;
                    float ratio = (float)m_currHealth / (float)m_totalHealth;
                    for (int i = 0; i < m_meshRend.Length; i++)
                        m_meshRend[i].material.color = new Color(1, ratio, ratio, 1);
                }
            }
        }
    }


    // Placement
    public void RandomRotation()
    {
        int randomRot = Random.Range(0, 4);
        transform.Rotate(0, randomRot * 90, 0);

        if (randomRot == 0)
            m_facing = TileLinkScript.nbors.BOTTOM;
        else if (randomRot == 1)
            m_facing = TileLinkScript.nbors.LEFT;
        else if (randomRot == 2)
            m_facing = TileLinkScript.nbors.TOP;
        else if (randomRot == 3)
            m_facing = TileLinkScript.nbors.RIGHT;
    }

    public void SetRotation(TileLinkScript.nbors _facing)
    {
        transform.Rotate(0, (int)_facing * 90, 0);
        m_facing = _facing;
    }

    virtual public void PlaceRandomly(BoardScript bScript)
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
        transform.position = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].transform.position;
        m_tile = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width];
    }

    virtual public void PlaceWithinRange(BoardScript bScript, int _xMin, int _xMax, int _zMin, int __zMax)
    {
        m_boardScript = bScript;

        // Set up position
        TileScript script;
        bool isPlacable = false;
        int randX;
        int randZ;

        do
        {
            randX = Random.Range(_xMin, _xMax - 1);
            randZ = Random.Range(_zMin, __zMax - 1);

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
        transform.position = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].transform.position;
        m_tile = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width];
    }

    public void PlaceOBJ(BoardScript bScript, int _x, int _z)
    {
        // This func trust you know where you are placing your OBJ...
        m_boardScript = bScript;

        // Set up position
        TileScript script;

        script = m_boardScript.m_tiles[_x + _z * m_boardScript.m_width].GetComponent<TileScript>();

        if (m_width == 2 && script.m_neighbors[(int)m_facing] && !script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding)
            script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding = gameObject;

        script.m_holding = gameObject;
        transform.SetPositionAndRotation(m_boardScript.m_tiles[_x + _z * m_boardScript.m_width].transform.position, transform.rotation);
        m_tile = m_boardScript.m_tiles[_x + _z * m_boardScript.m_width];
    }


    // Utilities
    private void OnMouseDown()
    {
        m_tile.OnMouseDown();
    }
}
