using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamScript : MonoBehaviour {

    private LineRenderer m_lineRenderer;
    private float m_counter;
    private float m_dist;

    public Transform m_origin;
    public Transform m_destination;

    public float m_drawSpeed = .002f;
    public bool m_backwards;

    public BoardScript m_boardScript;
    public ParticleSystem m_particle;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (gameObject.activeSelf)
        {
            Vector3 pointA = m_origin.position;
            Vector3 pointB = new Vector3(m_destination.position.x, m_origin.position.y, m_destination.position.z);
            Vector3 pointAlongLine = pointB;

            if (m_counter < 1 && gameObject.activeSelf && !m_backwards)
            {
                m_counter += m_drawSpeed;

                float x = Mathf.Lerp(0, m_dist, m_counter);


                // Get unit vector in the desired direction, multiply by the desired length and add the starting point
                pointAlongLine = x * Vector3.Normalize(pointB - pointA) + pointA;

                if (m_counter >= 1)
                {
                    Transform[] t = gameObject.GetComponentsInChildren<Transform>(true);
                    //t[1].gameObject.SetActive(true);
                    //m_boardScript.m_currCharScript.m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Explosion Sound 1"));
                    m_boardScript.m_currCharScript.Action();
                }
            }

            m_lineRenderer.SetPosition(0, m_origin.position);
            m_lineRenderer.SetPosition(1, pointAlongLine);
            m_particle.transform.position = m_lineRenderer.GetPosition(1);
        }
	}

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
    }

    public void OnEnable()
    {
        m_lineRenderer.SetPosition(0, m_origin.position);
        m_lineRenderer.SetPosition(1, m_origin.position);
        m_lineRenderer.SetWidth(1f, 1f);

        m_dist = Vector3.Distance(m_origin.position, m_destination.position);
        m_counter = 0;
        m_backwards = false;
    }
}
