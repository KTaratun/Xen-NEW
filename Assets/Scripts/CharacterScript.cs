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
    public bool m_isForcedMove;
    public int m_currRadius;

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
        m_isForcedMove = false;
        m_currRadius = 0;

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
                && mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable == false
                && !m_isForcedMove)
            {
                m_boardScript.NewTurn();
                mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = true;
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = true;
            }

            if (m_healthBar.activeSelf)
            {
                Transform outline = m_healthBar.transform.parent;

                outline.LookAt(2 * outline.position - m_boardScript.m_camera.transform.position);
                m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
                float ratio = ((float)m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];

                Renderer hpRend = m_healthBar.GetComponent<Renderer>();
                hpRend.material.color = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0.2f, 1);
                m_healthBar.transform.localScale = new Vector3(0.95f - ratio, m_healthBar.transform.localScale.y, m_healthBar.transform.localScale.z);
            }

            for (int i = 0; i < m_colorDisplay.Length; i++)
            {
                if (m_colorDisplay[i].activeSelf)
                    m_colorDisplay[i].transform.LookAt(2 * m_colorDisplay[i].transform.position - m_boardScript.m_camera.transform.position);
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
    }

    private void OnMouseDown()
    {
        TileScript tileScript = m_tile.GetComponent<TileScript>();
        tileScript.OnMouseDown();
    }

    public void MovementSelection(int _forceMove)
    {
        int move = 0;

        if (_forceMove > 0)
        {
            move = _forceMove;
            m_isForcedMove = true;
        }
        else
            move = m_tempStats[(int)sts.MOV];

        TileScript tileScript = m_tile.GetComponent<TileScript>();
        tileScript.FetchTilesWithinRange(move, new Color (0, 0, 1, 0.5f), false, false);
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

        if (!m_isForcedMove)
        {
            PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
            mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        }
        m_isForcedMove = false;
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
    {
        string[] actsSeparated = m_currAction.Split('|');
        string[] id = actsSeparated[0].Split(':');
        string[] rng = actsSeparated[5].Split(':');
        string[] rad = actsSeparated[6].Split(':');


        TileScript tileScript = m_tile.GetComponent<TileScript>();

        bool isOnlyHorVert = false;
        if (int.Parse(id[1]) == 4)
            isOnlyHorVert = true;

        m_currRadius = int.Parse(rad[1]);
        bool targetSelf = false;
        if (m_currRadius > 0)
            targetSelf = true;

        tileScript.FetchTilesWithinRange(int.Parse(rng[1]), new Color(1, 0, 0, 0.5f), targetSelf, isOnlyHorVert);

        PanelScript actionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.m_inView = false;
    }

    public void Action(List<GameObject> _targets)
    {
        transform.LookAt(m_boardScript.m_selected.transform);

        string[] actsSeparated = m_currAction.Split('|');
        string[] id = actsSeparated[0].Split(':');
        string[] engPreSplit = actsSeparated[2].Split(':');
        string eng = engPreSplit[1];
        string[] hit = actsSeparated[3].Split(':');
        string[] dmg = actsSeparated[4].Split(':');
        string[] crt = actsSeparated[7].Split(':');
        bool miss = false;

        for (int i = 0; i < _targets.Count; i++)
        {
            CharacterScript targetScript = _targets[i].GetComponent<CharacterScript>();
            targetScript.m_popupText.SetActive(true);
            TextMesh textMesh = targetScript.m_popupText.GetComponent<TextMesh>();

            int roll = Random.Range(0, 20);

            textMesh.color = Color.white;
            if (roll >= int.Parse(crt[1]) + m_tempStats[(int)sts.CRT])
            {
                targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]) * 2;
                textMesh.text = (int.Parse(dmg[1]) * 2).ToString();
                textMesh.color = Color.red;
                EnergyConversion(eng);
            }
            else if (roll < int.Parse(hit[1]) - m_tempStats[(int)sts.HIT] + targetScript.m_tempStats[(int)sts.EVA])
            {
                textMesh.text = "MISS";
                miss = true;
            }
            else
            {
                targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]);
                textMesh.text = dmg[1];
                EnergyConversion(eng);
            }

            if (!miss)
                Ability(_targets[i], id[1]);
        }

        m_currRadius = 0;
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
    }

    public void Ability(GameObject _currTarget, string _id)
    {
        CharacterScript targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript tileScript = m_tile.GetComponent<TileScript>();

        switch (int.Parse(_id))
        {
            case 1: // Dash ATK
                tileScript.ClearRadius(tileScript);
                MovementSelection(3);
                break;
            case 4: // Pull ATK
                PullTowards(targetScript, tileScript);
                break;
            case 5: // Magnet ATK
                PullTowards(targetScript, m_boardScript.m_selected.GetComponent<TileScript>());
                break;
            case 6:
                tileScript.ClearRadius(tileScript);
                targetScript.MovementSelection(4);
                break;
            default:
                break;
        }
    }

    private void PullTowards(CharacterScript _targetScript, TileScript _towards)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        GameObject adjacentTile = _towards.gameObject;

        //if (!_towards.m_holding)

        if (targetTileScript.m_x < _towards.m_x)
            adjacentTile = _towards.m_neighbors[(int)TileScript.nbors.left];
        if (targetTileScript.m_x > _towards.m_x)
            adjacentTile = _towards.m_neighbors[(int)TileScript.nbors.right];
        if (targetTileScript.m_z < _towards.m_z)
            adjacentTile = _towards.m_neighbors[(int)TileScript.nbors.bottom];
        if (targetTileScript.m_z > _towards.m_z)
            adjacentTile = _towards.m_neighbors[(int)TileScript.nbors.top];

        TileScript adjTileScript = adjacentTile.GetComponent<TileScript>();
        _targetScript.Movement(targetTileScript, adjTileScript);
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

    public void SetCharColor()
    {
        GameObject colors = m_colorDisplay[0];

        if (m_color.Length == 1)
        {
            colors = m_colorDisplay[0];
            colors.SetActive(true);
        }
        else if (m_color.Length == 2)
        {
            colors = m_colorDisplay[1];
            colors.SetActive(true);
        }
        else if (m_color.Length == 3)
        {
            colors = m_colorDisplay[2];
            colors.SetActive(true);
        }
        else if (m_color.Length == 4)
        {
            colors = m_colorDisplay[3];
            colors.SetActive(true);
        }
    
        SphereCollider[] orbs = colors.GetComponentsInChildren<SphereCollider>();
        int j = 0;

        for (int i = 0; i < orbs.Length; i++)
        {
            if (orbs[i].name == "Sphere Outline")
                continue;

            Renderer orbRend = orbs[i].GetComponent<Renderer>();

            if (m_color[j] == 'G')
                orbRend.material.color = new Color(.45f, .7f, .4f, 1);
            else if (m_color[j] == 'R')
                orbRend.material.color = new Color(.8f, .1f, .15f, 1);
            else if (m_color[j] == 'W')
                orbRend.material.color = new Color(.8f, .8f, .8f, 1);
            else if (m_color[j] == 'B')
                orbRend.material.color = new Color(.45f, .4f, 1, 1);

            j++;
        }
    }
}
