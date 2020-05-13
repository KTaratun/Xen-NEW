using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionViewerPanel : SlidingPanelScript
{
    private ActionLoaderScript m_actLoad;
    public RelativeSlidePanelScript m_actSlide;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        m_actSlide = transform.Find("ActionView Slide").GetComponent<RelativeSlidePanelScript>();

        if (GameObject.Find("Scene Manager"))
            m_actLoad = GameObject.Find("Scene Manager").GetComponent<ActionLoaderScript>();
    }

    private void Update()
    {
        if (!m_hovered && !m_actSlide.m_inView && m_actSlide.m_state == state.CLOSED)
            m_inView = false;
        if (m_hovered && m_inView && m_state == state.OPEN)
            m_actSlide.PopulatePanel();
    }

    // Update is called once per frame
    new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    override public void PopulatePanel()
    {
        base.PopulatePanel();
        m_hovered = true;

        //string actName = DatabaseScript.GetActionData(m_cScript.m_currAction, DatabaseScript.actions.NAME);
        //string[] actStats = m_cScript.m_currAction.Split('|');
        //string[] currStat = actStats[1].Split(':');
        ActionScript act = m_cScript.m_currAction;

        PanelScript actView = transform.Find("ActionView Slide/ActionView").GetComponent<PanelScript>();
        PanelScript main = transform.Find("Main").GetComponent<PanelScript>();
        actView.transform.Find("Name").GetComponent<Text>().text = act.m_name;

        Button energy = GetComponentInChildren<Button>();
        EnergyButtonScript engScript = energy.GetComponent<EnergyButtonScript>();
        engScript.SetTotalEnergy(act.m_energy);

        Text DMGText = main.transform.Find("DMG").GetComponent<Text>();
        Text RNGText = main.transform.Find("RNG").GetComponent<Text>();
        Text RADText = main.transform.Find("RAD").GetComponent<Text>();
        Text EffectText = main.transform.Find("Text").GetComponent<Text>();

        if (act.CheckIfAttack())
        {
            int finalDmg = act.m_damage + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG];

            if (act.UniqueActionProperties(ActionScript.uniAct.NO_DMG) == 1)
                finalDmg = act.m_damage;

            if (act.UniqueActionProperties(ActionScript.uniAct.DMG_MOD) >= 0)
                finalDmg += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalDmg > 0)
                DMGText.text = "DMG: " + finalDmg;
            else
                DMGText.text = "DMG: 0";
        }
        else
            DMGText.text = "DMG: 0";

        int finalRng = (act.m_range + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG]);

        if (act.UniqueActionProperties(ActionScript.uniAct.RNG_MOD) >= 0)
            finalRng += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

        if (finalRng > 0)
            RNGText.text = "RNG: " + finalRng;
        else
            RNGText.text = "RNG: 1";

        int finalRad = (act.m_radius + m_cScript.m_tempStats[(int)CharacterScript.sts.RAD]);

        if (act.UniqueActionProperties(ActionScript.uniAct.RAD_MOD) >= 0)
            finalRad += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

        if (finalRad > 0)
            RADText.text = "RAD: " + finalRad;
        else
            RADText.text = "RAD: 0";

        EffectText.text = m_actLoad.ModifyActions(m_cScript.m_tempStats[(int)CharacterScript.sts.TEC], act.m_effect);
    }

    override public void ClosePanel()
    {
        m_hovered = false;
        m_actSlide.ClosePanel();
    }
}
