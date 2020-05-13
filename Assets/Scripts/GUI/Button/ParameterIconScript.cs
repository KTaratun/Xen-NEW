using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParameterIconScript : ButtonScript
{
    static public ParameterIconScript m_currParameter;
    public Image m_image;
    public string m_title;
    public string m_desc;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        m_image = transform.parent.parent.Find("Images/" + name).GetComponent<Image>();

        if (name == "SPD")
        {
            m_title = "SPD: ";
            m_desc = "Speed determines turn order for each round.";
        }
        else if (name == "DMG")
        {
            m_title = "DMG: ";
            m_desc = "Damage is added to each attack you make.";
        }
        else if (name == "DEF")
        {
            m_title = "DEF: ";
            m_desc = "Defence reduces incoming damage.";
        }
        else if (name == "MOV")
        {
            m_title = "MOV: ";
            m_desc = "Movement affects how many spaces you can move.";
        }
        else if (name == "RNG")
        {
            m_title = "RNG: ";
            m_desc = "Range increases the distance of most actions.";
        }
        else if (name == "TEC")
        {
            m_title = "TEC: ";
            m_desc = "Tech increases the effectiveness of abilities.";
        }
    }

    // Update is called once per frame
    new void Update()
    {

    }

    override public void HoverTrue(BaseEventData eventData)
    {
        base.HoverTrue(eventData);


        m_currParameter = this;

        m_panMan.GetPanel("StatusViewer Panel").m_cScript = transform.parent.parent.parent.GetComponent<PanelScript>().m_cScript;
        m_panMan.GetPanel("StatusViewer Panel").PopulatePanel();
    }

    override public void HoverFalse()
    {
        base.HoverFalse();

        m_currParameter = null;

        m_panMan.GetPanel("StatusViewer Panel").ClosePanel();
    }
}
