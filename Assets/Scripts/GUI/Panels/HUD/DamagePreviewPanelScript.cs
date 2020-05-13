using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePreviewPanelScript : RelativeSlidePanelScript
{
    private GameManagerScript m_gamMan;

    // Start is called before the first frame update
    new void Start()
    {
        m_rectT = GetComponent<RectTransform>();

        if (GameObject.Find("Scene Manager").GetComponent<GameManagerScript>())
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    override public void PopulatePanel()
    {
        base.PopulatePanel();
         
        CharacterScript currScript = m_gamMan.m_currCharScript;
        ActionScript act = currScript.m_currAction;

        int def = m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
        int dmg = act.m_damage + currScript.m_tempStats[(int)CharacterScript.sts.DMG];

        if (act.UniqueActionProperties(ActionScript.uniAct.BYPASS) >= 0 && def > 0)
        {
            def -= act.UniqueActionProperties(ActionScript.uniAct.BYPASS) + currScript.m_tempStats[(int)CharacterScript.sts.TEC];
            if (def < 0)
                def = 0;
        }
        else if (act.UniqueActionProperties(ActionScript.uniAct.DMG_MOD) >= 0)
            dmg += currScript.m_tempStats[(int)CharacterScript.sts.TEC];

        dmg -= def;

        if (m_cScript.m_currHealth >= m_cScript.m_currHealth - dmg)
            transform.Find("HP").GetComponent<Text>().text = /*"HP: " + */m_cScript.m_currHealth.ToString() + "->" + (m_cScript.m_currHealth - dmg).ToString();
        else
            transform.Find("HP").GetComponent<Text>().text = /*"HP: " + */m_cScript.m_currHealth.ToString() + "->" + m_cScript.m_currHealth.ToString();
    }
}