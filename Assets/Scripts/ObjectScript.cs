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
    public TileScript.nbors m_facing;

    // Use this for initialization
    protected void Start ()
    {
        m_totalHealth = 5 * m_width;
        m_currHealth = m_totalHealth;
        m_facing = TileScript.nbors.bottom;

        if (GameObject.Find("Board"))
            GameObject.Find("Board").GetComponent<BoardScript>().AddToOBJList(this);
	}
	
	// Update is called once per frame
	protected void Update ()
    {
        if (m_boardScript && m_tile)
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

        m_tile.FetchTilesWithinRange(GetComponent<CharacterScript>(), move, new Color(0, 0, 1, 0.5f), false, TileScript.targetRestriction.NONE, false);
    }

    virtual public void MovingStart(TileScript newScript, bool _isForced, bool _isNetForced)
    {
        if (m_tile == newScript || newScript == null)
            return;

        if (GameObject.Find("Network") && !GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted &&
            !_isNetForced)
        {
            ClientScript c = GameObject.Find("Network").GetComponent<ClientScript>();
            string msg = "MOVESTART~" + m_id.ToString() + '|' + newScript.m_id + '|' + _isForced.ToString();
            c.Send(msg, c.m_reliableChannel);
            return;
        }
        else if (GameObject.Find("Network") && GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
        {
            ServerScript s = GameObject.Find("Network").GetComponent<ServerScript>();
            string msg = "MOVESTART~" + m_id.ToString() + '|' + newScript.m_id + '|' + _isForced.ToString();
            s.Send(msg, s.m_reliableChannel, s.m_clients);
        }

        if (m_tile)
        {
            m_tile.m_holding = null;
            m_tile.ClearRadius();
        }
        m_tile = newScript;
        m_boardScript.m_camIsFrozen = true;

        //m_tile.m_holding = gameObject;

        if (!_isForced)
            m_boardScript.m_selected = null;

        PanelScript.CloseHistory();
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
        if (!PanelScript.GetPanel("Round End Panel").m_inView)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = gameObject;

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
        if (!PanelScript.GetPanel("Round End Panel").m_inView)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = null;

        if (tag == "PowerUp" && m_tile.m_holding && m_tile.m_holding.tag == "Player")
            GetComponent<PowerupScript>().OnPickup(m_tile.m_holding.GetComponent<CharacterScript>());
        else
            m_tile.m_holding = gameObject;

        if (m_boardScript.m_projectiles[(int)BoardScript.prjcts.BEAM].activeSelf)
            m_boardScript.m_projectiles[(int)BoardScript.prjcts.BEAM].SetActive(false);

        m_boardScript.m_currButton = null;

        if (!PanelScript.GetPanel("Round End Panel").m_inView)
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
            m_facing = TileScript.nbors.bottom;
        else if (randomRot == 1)
            m_facing = TileScript.nbors.left;
        else if (randomRot == 2)
            m_facing = TileScript.nbors.top;
        else if (randomRot == 3)
            m_facing = TileScript.nbors.right;
    }

    public void SetRotation(TileScript.nbors _facing)
    {
        transform.Rotate(0, (int)_facing * 90, 0);
        m_facing = _facing;
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
                else if (m_width == 2)
                    if (script.m_neighbors[(int)m_facing] && !script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding)
                    {
                        isPlacable = true;
                        TileScript nei = script.m_neighbors[(int)m_facing].GetComponent<TileScript>();
                        nei.m_holding = gameObject;
                    }

            }
        } while (!isPlacable);

        script.m_holding = gameObject;
        transform.SetPositionAndRotation(m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].transform.position, transform.rotation);
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
