using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int m_currRadius;

	// Use this for initialization
	void Start ()
    {
        m_popupText.SetActive(false);

        m_accessories = new string[2];
        m_accessories[0] = "Ring of DEATH";
        m_stats = new int[(int)sts.TOT];
        m_tempStats = new int[(int)sts.TOT];
        m_stats[(int)sts.HP] = 12;
        m_stats[(int)sts.SPD] = 10;
        m_stats[(int)sts.MOV] = 5;
        m_currRadius = 0;

        for (int i = 0; i < m_stats.Length; i++)
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

        if (m_boardScript)
        {
            PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();

            // END OF TURN
            if (m_boardScript.m_currPlayer == gameObject && mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable == false
                && mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable == false
                && !m_boardScript.m_isForcedMove)
            {
                mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = true;
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = true;
                StatusScript.UpdateStatus(gameObject, 0);
                m_boardScript.NewTurn();
            }

            if (m_healthBar.activeSelf)
            {
                Transform outline = m_healthBar.transform.parent;

                outline.LookAt(2 * outline.position - m_boardScript.m_camera.transform.position);
                m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
                float ratio = ((float)m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP]; // ADD EQUIPMENT

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
            move = _forceMove;
        else
            move = m_tempStats[(int)sts.MOV]; // ADD EQUIPMENT

        TileScript tileScript = m_tile.GetComponent<TileScript>();
        tileScript.FetchTilesWithinRange(move, new Color (0, 0, 1, 0.5f), false, false);
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanScript.m_inView = false;
    }

    public void Movement(TileScript selectedScript, TileScript newScript, bool _isForced)
    {
        newScript.m_holding = selectedScript.m_holding;
        selectedScript.m_holding = null;
        m_tile = newScript.gameObject;

        if (gameObject == m_boardScript.m_currPlayer)
            m_boardScript.m_currTile = m_tile;
        if (m_boardScript.m_isForcedMove == null && !_isForced)
        {
            PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
            mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        }

        transform.LookAt(m_tile.transform);
        m_boardScript.m_isForcedMove = null;

        selectedScript.ClearRadius(selectedScript);
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
        string[] id = actsSeparated[(int)DatabaseScript.actions.ID].Split(':');
        string[] name = actsSeparated[(int)DatabaseScript.actions.NAME].Split(':');
        string[] rng = actsSeparated[(int)DatabaseScript.actions.RNG].Split(':');
        string[] rad = actsSeparated[(int)DatabaseScript.actions.RAD].Split(':');


        TileScript tileScript = m_tile.GetComponent<TileScript>();

        bool isOnlyHorVert = false;
        if (int.Parse(id[1]) == 4 || int.Parse(id[1]) == 11)
            isOnlyHorVert = true;

        m_currRadius = int.Parse(rad[1]);
        bool targetSelf = false;
        if (m_currRadius > 0)
            targetSelf = true;


        if (name[1][name[1].Length - 3] == 'A' && name[1][name[1].Length - 2] == 'T' && name[1][name[1].Length - 1] == 'K') // See if it's an attack
            tileScript.FetchTilesWithinRange(int.Parse(rng[1]) + m_tempStats[(int)sts.RNG], new Color(1, 0, 0, 0.5f), targetSelf, isOnlyHorVert);
        else
            tileScript.FetchTilesWithinRange(int.Parse(rng[1]) + m_tempStats[(int)sts.RNG], new Color(0, 1, 0, 0.5f), true, isOnlyHorVert);

        PanelScript actionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.m_inView = false;
    }

    public void Action(List<GameObject> _targets)
    {
        transform.LookAt(m_boardScript.m_selected.transform);

        string[] actsSeparated = m_currAction.Split('|');
        string[] id = actsSeparated[(int)DatabaseScript.actions.ID].Split(':');
        string[] name = actsSeparated[(int)DatabaseScript.actions.NAME].Split(':');
        string[] eng = actsSeparated[(int)DatabaseScript.actions.ENERGY].Split(':');
        string[] hit = actsSeparated[(int)DatabaseScript.actions.HIT].Split(':');
        string[] dmg = actsSeparated[(int)DatabaseScript.actions.DMG].Split(':');
        string[] crt = actsSeparated[(int)DatabaseScript.actions.CRT].Split(':');

        if (name[1][name[1].Length - 3] == 'A' && name[1][name[1].Length - 2] == 'T' && name[1][name[1].Length - 1] == 'K') // See if it's an attack
        {
            int roll = Random.Range(0, 21); // Because Random.Range doesn't include the max number

            for (int i = 0; i < _targets.Count; i++)
            {
                bool miss = false;
                CharacterScript targetScript = _targets[i].GetComponent<CharacterScript>();
                targetScript.m_popupText.SetActive(true);
                TextMesh textMesh = targetScript.m_popupText.GetComponent<TextMesh>();


                textMesh.color = Color.white;
                if (roll >= int.Parse(crt[1]) + m_tempStats[(int)sts.CRT]) // ADD EQUIPMENT
                {
                    targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]) * 2;
                    textMesh.text = (int.Parse(dmg[1]) * 2).ToString();
                    textMesh.color = Color.red;
                    EnergyConversion(eng[1]);
                }
                else if (roll < int.Parse(hit[1]) - m_tempStats[(int)sts.HIT] + targetScript.m_tempStats[(int)sts.EVA]) // ADD EQUIPMENT
                {
                    textMesh.text = "MISS";
                    miss = true;
                }
                else
                {
                    targetScript.m_tempStats[(int)sts.HP] -= int.Parse(dmg[1]);
                    textMesh.text = dmg[1];
                    EnergyConversion(eng[1]);
                }

                if (!miss)
                    Ability(_targets[i], id[1]);
            }
        }
        else // If it's not an attack
            for (int i = 0; i < _targets.Count; i++)
                Ability(_targets[i], id[1]);

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
                m_boardScript.m_isForcedMove = gameObject;
                MovementSelection(3);
                break;
            case 4: // Pull ATK
                PullTowards(targetScript, tileScript);
                break;
            case 5: // Magnet ATK
                PullTowards(targetScript, m_boardScript.m_selected.GetComponent<TileScript>());
                break;
            case 6: // Push ATK
                tileScript.ClearRadius(tileScript);
                m_boardScript.m_isForcedMove = _currTarget;
                targetScript.MovementSelection(4);
                break;
            case 7: // Smash ATK
                Knockback(targetScript, tileScript, 3);
                break;
            case 8: // Blast ATK
                Knockback(targetScript, m_boardScript.m_selected.GetComponent<TileScript>(), 1);
                break;
            case 9: // Spot
                StatusScript.NewStatus(_currTarget, int.Parse(_id));
                break;
            case 10: // Passage
                StatusScript.NewStatus(_currTarget, int.Parse(_id));
                break;
            case 11:
                break;
            default:
                break;
        }
    }

    private void PullTowards(CharacterScript _targetScript, TileScript _towards)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        TileScript nei;

        while (true)
        {
            if (targetTileScript.m_x < _towards.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.right])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.right].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    continue;
                }
            }
            else if (targetTileScript.m_x > _towards.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.left])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.left].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    continue;
                }
            }
            else if (targetTileScript.m_z < _towards.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.top])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.top].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    continue;
                }
            }
            else if (targetTileScript.m_z > _towards.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.bottom])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.bottom].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    continue;
                }
            }
            break;
        }
    }

    private void Knockback(CharacterScript _targetScript, TileScript _away, int _force)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        TileScript nei;
    
        while (_force > 0)
        {
            if (targetTileScript.m_x < _away.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.left])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.left].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    _force--;
                    continue;
                }
            }
            else if (targetTileScript.m_x > _away.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.right])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.right].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    _force--;
                    continue;
                }
            }
            else if (targetTileScript.m_z < _away.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.bottom])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.bottom].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    _force--;
                    continue;
                }
            }
            else if (targetTileScript.m_z > _away.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.top])
            {
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.top].GetComponent<TileScript>();
                if (!nei.m_holding)
                {
                    _targetScript.Movement(targetTileScript, nei, true);
                    targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
                    _force--;
                    continue;
                }
            }
            break;
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

    public void Pass()
    {
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
        mainPanelScript.m_inView = false;
    }

    public void ViewStatus()
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

    public void UpdateStatusImages()
    {
        if (gameObject == m_boardScript.m_currPlayer)
        {
            PanelScript panScript = m_boardScript.m_panels[(int)BoardScript.pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
            panScript.PopulateHUD();
        }
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
