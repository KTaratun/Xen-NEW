using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : ObjectScript {

    public enum gren { BLAST, EXPLOSION, MAGNET }

    private Vector3 m_origin;
    public GameObject[] m_effects;

    // Use this for initialization
    new void Start () {
		
	}

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        //if (base)
    }

    override public void MovingStart(TileScript newScript, bool _isForced, bool _isNetForced)
    {
        if (tag == "Grenade")
            for (int i = 0; i < m_effects.Length; i++)
                m_effects[i].SetActive(false);
        
            // Set active to play the animation again
            gameObject.SetActive(true);
            m_origin = m_boardScript.m_currCharScript.m_body[(int)CharacterScript.bod.RIGHT_HAND].transform.position;
            transform.SetPositionAndRotation(m_boardScript.m_currCharScript.m_body[(int)CharacterScript.bod.RIGHT_HAND].transform.position, Quaternion.identity);
            m_tile = newScript;
            m_boardScript.m_currCharScript.m_particles[(int)CharacterScript.prtcles.GUN_SHOT].SetActive(true);
    }

    override public void MovementUpdate()
    {
        // Determine how much the character will be moving this update
        float charSpeed = 2.0f;
        if (tag == "Grenade")
            charSpeed = 1.2f;
        float charMovement = charSpeed;

        transform.LookAt(m_tile.transform);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
        float dis = Vector3.Distance(myPos, newPos);
        float newY = transform.position.y;

        if (tag == "Grenade")
        {
            Vector3 charPos = new Vector3(m_origin.x, 0, m_origin.z);
            float originDis = Vector3.Distance(charPos, newPos);

            newY = m_origin.y;
            float yDis = m_origin.y - m_tile.transform.position.y;
            float num = dis * (dis / originDis);
            float final = 1 - (num / originDis);
            float ratio = 1 - dis / originDis;

            newY -= yDis * (final * ratio);
        }

        transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement,newY, transform.position.z + transform.forward.z * charMovement), transform.rotation);
        if (!PanelScript.GetPanel("Round End Panel").m_inView)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = gameObject;

        // Check to see if character is close enough to the point
        float snapDistance = 1.0f;
        myPos = new Vector3(transform.position.x, 0, transform.position.z);
        newPos = new Vector3(m_tile.transform.position.x, 0, m_tile.transform.position.z);
        dis = Vector3.Distance(myPos, newPos);

        if (dis < snapDistance)
            transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation); // new Vector3(m_tile.transform.position.x, transform.position.y, m_tile.transform.position.z)
    }

    public override void MovingFinish()
    {
        if (tag == "Grenade")
        {
            if (DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME) == "ATK(Magnet)")
                m_effects[(int)gren.MAGNET].SetActive(true);
            else if (DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME) == "ATK(Blast)")
                m_effects[(int)gren.BLAST].SetActive(true);
            else
                m_effects[(int)gren.EXPLOSION].SetActive(true);

            m_boardScript.m_currCharScript.m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Explosion Sound 1"));
        }
        else
        {
            gameObject.SetActive(false);
            //m_boardScript.m_currCharScript.m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Explosion Sound 2"));
        }

        m_boardScript.m_currCharScript.Action();
    }
}
