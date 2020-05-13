using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDStatusScript : SlidingPanelScript
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public void HoverTrue(BaseEventData eventData)
    {
        if (m_inView)
        {
            m_cScript = transform.parent.GetComponent<PanelScript>().m_cScript;
            m_cScript.m_currStatus = int.Parse(name);
            m_panMan.GetPanel("StatusViewer Panel").m_cScript = m_cScript;
            m_panMan.GetPanel("StatusViewer Panel").PopulatePanel();
        }
    }

    public void HoverFalse()
    {
        if (!m_inView)
            return;

        m_panMan.GetPanel("StatusViewer Panel").GetComponent<SlidingPanelScript>().ClosePanel();
    }

    static public void StatusSymbolSetup(Transform _panel)
    {
        StatusScript[] statScripts = _panel.GetComponent<PanelScript>().m_cScript.GetComponents<StatusScript>();
        SlidingPanelScript[] StatusButts = _panel.GetComponentsInChildren<SlidingPanelScript>();

        for (int i = 0; i < StatusButts.Length; i++)
            StatusButts[i].m_inView = false;

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_lifeSpan <= 0)
                continue;

            StatusButts[i].m_inView = true;
            Image currImage = StatusButts[i].GetComponent<Image>();

            currImage.name = i.ToString();
            currImage.sprite = statScripts[i].m_sprite;
            currImage.color = statScripts[i].m_color;
        }
    }
}
