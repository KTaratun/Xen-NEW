using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterScript : MonoBehaviour {

    public enum sts { HP, SPD, HIT, EVA, CRT, DMG, DEF, MOV, RNG, TOT };
    enum trn { MOV, ACT };

    public GameObject m_tile;
    public BoardScript m_boardScript;
    public GameObject m_turnPanel;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject[] m_colorDisplay;
    public GameObject m_player;
    public string[] m_actions;
    public string m_currAction;
    public int[] m_stats;
    public int[] m_tempStats;
    public string[] m_accessories;
    public string m_color;

	// Use this for initialization
	void Start ()
    {
        m_popupText.SetActive(false);

        m_accessories = new string[2];
        m_accessories[0] = "Ring of DEATH";
        m_stats = new int[(int)sts.TOT];
        m_stats[(int)sts.HP] = 12;
        m_stats[(int)sts.SPD] = 10;
        m_stats[(int)sts.MOV] = 5;
        m_tempStats = new int[(int)sts.TOT];

        for (int i = 0; i < m_tempStats.Length; i++)
            m_tempStats[i] = m_stats[i];
    }

    // Update is called once per frame
    void Update()
    {
        // Move towards new tile
        if (m_tile && transform.position != m_tile.transform.position)
        {
            // Determine how much the character will be moving this update
            float charAcceleration = 0.04f;
            float charSpeed = 0.03f;
            float charMovement = Vector3.Distance(transform.position, m_tile.transform.position) * charAcceleration + charSpeed;
            transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement, transform.position.y, transform.position.z + transform.forward.z * charMovement), transform.rotation);

            float snapDistance = 0.02f;
            if (Vector3.Distance(transform.position, m_tile.transform.position) < snapDistance)
                transform.SetPositionAndRotation(m_tile.transform.position, new Quaternion());
        }

        // If it's my turn and I did all my actions, reset everything and end my turn
        if (m_boardScript)
        {
            PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
            if (m_boardScript.m_currPlayer == gameObject && mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable == false
                && mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable == false)
            {
                m_boardScript.NewTurn();
                mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = true;
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = true;
            }
        }

        if (m_healthBar.activeSelf)
        {
            m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
            float ratio = ((float)m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];

            Renderer hpRend = m_healthBar.GetComponent<Renderer>();
            hpRend.material.color = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0, 1);
            m_healthBar.transform.localScale = new Vector3(1 - ratio, 0.2f, 1);
        }

        if (m_popupText.activeSelf)
        {
            float fadeSpeed = .01f;
            m_popupText.transform.LookAt(2 * m_popupText.transform.position - m_boardScript.m_camera.transform.position);
            TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, textMesh.color.a - fadeSpeed);
            
            if (textMesh.color.a <= 0)
            {
                textMesh.color = Color.white;
                m_popupText.SetActive(false);
            }
        }
    }

    private void OnMouseDown()
    {
        TileScript tileScript = m_tile.GetComponent<TileScript>();
        tileScript.OnMouseDown();
    }

    public void MovementSelection()
    {
        FetchTilesWithinRange(m_stats[(int)sts.MOV], new Color (0, 0, 1, 0.5f), true);
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanScript.m_inView = false;
    }

    public void Movement(TileScript selectedScript, TileScript newScript)
    {
        newScript.m_holding = selectedScript.m_holding;
        selectedScript.m_holding = null;
        m_tile = newScript.gameObject;
        m_boardScript.m_currTile = m_tile;
        transform.LookAt(m_tile.transform);
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
    }

    public void ActionSelection()
    {
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript aPScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        mainPanScript.m_inView = false;
        aPScript.m_inView = true;

        aPScript.m_character = gameObject;
        aPScript.m_cScript = this;
        aPScript.PopulateActionButtons(m_actions);
    }

    public void ActionTargeting()
    {;
        string[] actsSeparated = m_currAction.Split('|');
        string[] rng = actsSeparated[5].Split(':');

        FetchTilesWithinRange(int.Parse(rng[1]), new Color(1, 0, 0, 0.5f), false);

        PanelScript actionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.m_inView = false;
    }

    public void Action(GameObject[] targets)
    {
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;

        string[] actsSeparated = m_currAction.Split('|');
        string[] engPreSplit = actsSeparated[2].Split(':');
        string eng = engPreSplit[1];
        string[] hit = actsSeparated[3].Split(':');
        string[] dmg = actsSeparated[4].Split(':');
        string[] crt = actsSeparated[6].Split(':');

        for (int i = 0; i < targets.Length; i++)
        {
            CharacterScript targetScript = targets[i].GetComponent<CharacterScript>();
            targetScript.m_popupText.SetActive(true);
            TextMesh textMesh = targetScript.m_popupText.GetComponent<TextMesh>();

            int roll = Random.Range(0, 20);

            if (roll >= int.Parse(crt[1]) + m_tempStats[(int)sts.CRT])
            {
                targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]) * 2;
                textMesh.text = (int.Parse(dmg[1]) * 2).ToString();
                textMesh.color = Color.red;
                EnergyConversion(eng);

                return;
            }

            textMesh.color = Color.white;

            if (roll < int.Parse(hit[1]) - m_tempStats[(int)sts.HIT] + targetScript.m_tempStats[(int)sts.EVA])
                textMesh.text = "MISS";
            else
            {
                targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]);
                textMesh.text = dmg[1];
                EnergyConversion(eng);
            }
        }
    }

    private void EnergyConversion(string energy)
    {
        PlayerScript playScript = m_player.GetComponent<PlayerScript>();
        // Assign energy symbols
        for (int i = 0; i < energy.Length; i++)
        {
            if (energy[i] == 'g')
                playScript.m_energy[0] += 1;
            else if (energy[i] == 'r')
                playScript.m_energy[1] += 1;
            else if (energy[i] == 'w')
                playScript.m_energy[2] += 1;
            else if (energy[i] == 'b')
                playScript.m_energy[3] += 1;
            else if (energy[i] == 'G')
                playScript.m_energy[0] -= 1;
            else if (energy[i] == 'R')
                playScript.m_energy[1] -= 1;
            else if (energy[i] == 'W')
                playScript.m_energy[2] -= 1;
            else if (energy[i] == 'B')
                playScript.m_energy[3] -= 1;
        }

        playScript.SetEnergyPanel();
    }

    private void FetchTilesWithinRange(int range, Color color, bool isMove)
    {
        TileScript parentScript = m_tile.GetComponent<TileScript>();

        // REFACTOR: Maybe less lists?
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        // Start with current tile in oddGen
        oddGen.Add(m_tile.GetComponent<TileScript>());

        for (int i = 0; i < range; i++)
        {
            // Alternate between gens. Unload current gen and load up the gen and then swap next iteration
            if (oddGen.Count > 0)
            {
                workingList = oddGen;
                storingList = evenGen;
            }
            else if (evenGen.Count > 0)
            {
                workingList = evenGen;
                storingList = oddGen;
            }

            while (workingList.Count > 0)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (!workingList[0].m_neighbors[k])
                        continue;

                    TileScript tScript = workingList[0].m_neighbors[k].GetComponent<TileScript>();

                    if (isMove && tScript.m_holding || !isMove && workingList[0].m_neighbors[k] == m_boardScript.m_selected)
                        continue;

                    if (workingList[0].m_neighbors[k])
                    {
                        Renderer tR = workingList[0].m_neighbors[k].GetComponent<Renderer>();
                        if (tR.material.color != color)
                        {
                            tR.material.color = color;
                            storingList.Add(workingList[0].m_neighbors[k].GetComponent<TileScript>());
                            parentScript.m_radius.Add(workingList[0].m_neighbors[k]);
                        }
                    }
                }
                workingList.RemoveAt(0);
            }
        }
    }

    public void Pass()
    {
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
        mainPanelScript.m_inView = false;
    }

    public void Status()
    {
        PanelScript sPScript = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript auxPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();

        if (mainPanScript.m_inView)
        {
            mainPanScript.m_inView = false;
            sPScript.m_parent = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL];
        }
        else if (auxPanScript.m_inView)
        {
            auxPanScript.m_inView = false;
            sPScript.m_parent = m_boardScript.m_panels[(int)BoardScript.pnls.AUXILIARY_PANEL];
        }

        sPScript.m_inView = true;
        sPScript.m_character = gameObject;
        sPScript.m_cScript = this;
        sPScript.PopulateText();
    }

    //public void SetCharColor()
    //{
    //    GameObject colors = colorDisplay[0];
    //    if ()
    //
    //    int numEle = 0;
    //    List<char> used = new List<char>();
    //
    //    Image[] orbs = panel.GetComponentsInChildren<Game>();
    //
    //    for (int i = 0; i < num; i++)
    //    {
    //        if (energy[i] == 'g')
    //            orbs[i + 1].color = new Color(.5f, 1, .5f, 1);
    //        else if (energy[i] == 'r')
    //            orbs[i + 1].color = new Color(1, .5f, .5f, 1);
    //        else if (energy[i] == 'w')
    //            orbs[i + 1].color = new Color(1, 1, 1, 1);
    //        else if (energy[i] == 'b')
    //            orbs[i + 1].color = new Color(.5f, .5f, 1, 1);
    //        else if (energy[i] == 'G')
    //            orbs[i + 1].color = new Color(.1f, .9f, .1f, 1);
    //        else if (energy[i] == 'R')
    //            orbs[i + 1].color = new Color(.9f, .1f, .1f, 1);
    //        else if (energy[i] == 'W')
    //            orbs[i + 1].color = new Color(.9f, .9f, .9f, 1);
    //        else if (energy[i] == 'B')
    //            orbs[i + 1].color = new Color(.1f, .1f, .9f, 1);
    //    }
    //}
}
