using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusSelectorScript : PanelScript
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void PopulatePanel()
    {
        StatusScript[] statScripts = GetComponent<PanelScript>().m_cScript.GetComponents<StatusScript>();

        for (int i = 0; i < 8; i++)
            transform.GetChild(i).GetComponent<Image>().enabled = false;

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_lifeSpan <= 0)
                continue;

            Image currImage = transform.GetChild(i).GetComponent<Image>();

            ButtonScript buttScript = currImage.GetComponentInChildren<ButtonScript>();
            buttScript.m_parent = GetComponent<SlidingPanelScript>();

            currImage.enabled = true;

            currImage.name = i.ToString();
            currImage.sprite = statScripts[i].m_sprite;
            currImage.color = statScripts[i].m_color;
        }
    }
}
