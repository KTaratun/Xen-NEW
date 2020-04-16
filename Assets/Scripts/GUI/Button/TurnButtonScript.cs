using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnButtonScript : EnergyButtonScript
{
    public CharacterScript m_cScript;

    // Start is called before the first frame update
    void Start()
    {
        EnergyInit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void HoverTrue(BaseEventData eventData)
    {
        m_cScript.HighlightCharacter();

        Renderer rend = m_cScript.transform.GetComponentInChildren<Renderer>();
        rend.materials[0].color = Color.cyan;
    }

    override public void HoverFalse()
    {
        m_cScript.DeselectCharacter();

        Renderer rend = m_cScript.transform.GetComponentInChildren<Renderer>();
        rend.materials[0].color = m_cScript.m_teamColor;
    }

    override public void Select()
    {
        CameraScript cam = GameObject.Find("FreeCam").transform.Find("Main Camera").GetComponent<CameraScript>();

        cam.m_target = m_cScript.gameObject;
    }
}
