using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour {

    public enum sts { HP, SPD, HIT, EVA, CRT, DMG, DEF, MOV, RNG, RAD, TOT };
    enum trn { MOV, ACT };
    
    public GameObject m_tile;
    public BoardScript m_boardScript;
    public GameObject m_turnPanel;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject[] m_colorDisplay;
    public GameObject m_player;
    public string m_currAction;
    public int m_currStatus;
    public string[] m_actions;
    public int[] m_stats;
    public int[] m_tempStats;
    public string[] m_accessories;
    public string m_color;
    public int m_currRadius;
    public bool[] m_effects;

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
        m_effects = new bool[(int)StatusScript.effects.TOT];

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
                StatusScript.UpdateStatus(gameObject, StatusScript.mode.TURN_END);
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
                    textMesh.text = "0";
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
        tileScript.FetchTilesWithinRange(move, new Color (0, 0, 1, 0.5f), false, TileScript.targetRestriction.NONE, false);
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

        aPScript.m_cScript = this;
        aPScript.PopulateActionButtons(m_actions);
    }

    public void ActionTargeting()
    {
        string[] actsSeparated = m_currAction.Split('|');
        string[] name = actsSeparated[(int)DatabaseScript.actions.NAME].Split(':');
        string[] rng = actsSeparated[(int)DatabaseScript.actions.RNG].Split(':');
        string[] rad = actsSeparated[(int)DatabaseScript.actions.RAD].Split(':');


        TileScript tileScript = m_tile.GetComponent<TileScript>();

        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;
        if (name[1] == "Pull ATK" || name[1] == "Piercing ATK")
            targetingRestriction = TileScript.targetRestriction.HORVERT;
        else if (name[1] == "Cross ATK")
            targetingRestriction = TileScript.targetRestriction.DIAGONAL;

        bool isBlockable = true;
        if (name[1] == "Piercing ATK" || name[1] == "Cross ATK")
            isBlockable = false;

        m_currRadius = int.Parse(rad[1]);
        bool targetSelf = false;
        if (m_currRadius > 0)
            targetSelf = true;

        if (CheckIfAttack(name[1])) // See if it's an attack
            tileScript.FetchTilesWithinRange(int.Parse(rng[1]) + m_tempStats[(int)sts.RNG], new Color(1, 0, 0, 0.5f), targetSelf, targetingRestriction, isBlockable);
        else
            tileScript.FetchTilesWithinRange(int.Parse(rng[1]) + m_tempStats[(int)sts.RNG], new Color(0, 1, 0, 0.5f), true, targetingRestriction, isBlockable);

        PanelScript actionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.m_inView = false;
    }

    public void Action(List<GameObject> _targets)
    {
        transform.LookAt(m_boardScript.m_selected.transform);

        string[] actsSeparated = m_currAction.Split('|');
        string[] name = actsSeparated[(int)DatabaseScript.actions.NAME].Split(':');
        string[] eng = actsSeparated[(int)DatabaseScript.actions.ENERGY].Split(':');
        string[] hit = actsSeparated[(int)DatabaseScript.actions.HIT].Split(':');
        string[] dmg = actsSeparated[(int)DatabaseScript.actions.DMG].Split(':');
        string[] crt = actsSeparated[(int)DatabaseScript.actions.CRT].Split(':');

        if (CheckIfAttack(name[1])) // See if it's an attack
        {
            int roll = Random.Range(1, 21); // Because Random.Range doesn't include the max number


            for (int i = 0; i < _targets.Count; i++)
            {
                bool miss = false;
                CharacterScript targetScript = _targets[i].GetComponent<CharacterScript>();
                int DC = int.Parse(hit[1]) - m_tempStats[(int)sts.HIT] + targetScript.m_tempStats[(int)sts.EVA];
                int totalCrit = int.Parse(crt[1]) + m_tempStats[(int)sts.CRT];
                string finalDMG = "0";

                print("Rolled " + roll + " against " + hit[1] + " - my HIT mod of " + m_tempStats[(int)sts.HIT] + " + my opponents EVA mod of " + targetScript.m_tempStats[(int)sts.EVA] +
                    " for a total DC of " + DC + ". Crit: " + crt[1] + " + my CRT mod of " + m_tempStats[(int)sts.CRT] + " for a total of Crit: " + totalCrit + ".\n");

                if (name[1] == "Bypass ATK")
                {
                    DC -= targetScript.m_tempStats[(int)sts.EVA];
                    finalDMG = (0 + targetScript.m_tempStats[(int)sts.DEF]).ToString();
                }

                if (roll >= int.Parse(crt[1]) + m_tempStats[(int)sts.CRT]) // ADD EQUIPMENT
                {
                    if ((int.Parse(dmg[1]) * 2) + m_tempStats[(int)sts.DMG] + targetScript.m_tempStats[(int)sts.DEF] >= 0)
                        finalDMG = ((int.Parse(dmg[1]) * 2) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF]).ToString();

                    print("Hit with a crit dealing " + dmg[1] + "x2 + my DMG mod of " + m_tempStats[(int)sts.DMG] + " - my opponent's DEF mod of " + targetScript.m_tempStats[(int)sts.DEF] + " for a total of " + finalDMG + ".\n");
                    targetScript.ReceiveDamage(finalDMG, Color.red);
                    EnergyConversion(eng[1]);
                }
                else if (roll < DC) // ADD EQUIPMENT
                {
                    targetScript.ReceiveDamage("MISS", Color.white);
                    miss = true;

                    print("Missed.\n");
                    if (eng[1][0] == 'G' || eng[1][0] == 'R' || eng[1][0] == 'W' || eng[1][0] == 'B')
                        EnergyConversion(eng[1]);
                }
                else
                {
                    if (int.Parse(dmg[1]) + m_tempStats[(int)sts.DMG] + targetScript.m_tempStats[(int)sts.DEF] >= 0)
                        finalDMG = (int.Parse(dmg[1]) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF]).ToString();

                    print("Hit dealing " + dmg[1] + " + my DMG mod of " + m_tempStats[(int)sts.DMG] + " - my opponent's DEF mod of " + targetScript.m_tempStats[(int)sts.DEF] + " for a total of " + finalDMG + ".\n");
                    targetScript.ReceiveDamage(finalDMG, Color.white);
                    EnergyConversion(eng[1]);
                }

                if (!miss && !m_effects[(int)StatusScript.effects.HINDER])
                    Ability(_targets[i], name[1]);
            }
        }
        else // If it's not an attack
            for (int i = 0; i < _targets.Count; i++)
                Ability(_targets[i], name[1]);

        m_currRadius = 0;
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
    }

    static public bool CheckIfAttack(string _action)
    {
        if (_action[_action.Length - 3] == 'A' && _action[_action.Length - 2] == 'T' && _action[_action.Length - 1] == 'K')
            return true;
        else
            return false;
    }

    public void ReceiveDamage(string _dmg, Color _color)
    {
        TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
        m_popupText.SetActive(true);
        textMesh.color = _color;

        int parsedDMG;
        if (int.TryParse(_dmg, out parsedDMG))
        {
            m_tempStats[(int)sts.HP] -= parsedDMG;
            textMesh.text = _dmg;
        }
        else
            textMesh.text = _dmg;

        if (gameObject == m_boardScript.m_currPlayer)
        {
            PanelScript actPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
            actPanScript.PopulatePanel();
        }
    }

    public void HealHealth(int _hp)
    {
        if (m_effects[(int)StatusScript.effects.SCARRING])
            return;

        TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
        m_popupText.SetActive(true);
        textMesh.color = Color.green;

        if (m_tempStats[(int)sts.HP] + _hp > m_stats[(int)sts.HP])
        {
            _hp = m_stats[(int)sts.HP] - m_tempStats[(int)sts.HP];
            m_tempStats[(int)sts.HP] = m_stats[(int)sts.HP];
        }
        else
            m_tempStats[(int)sts.HP] += _hp;

        textMesh.text = _hp.ToString();

        if (gameObject == m_boardScript.m_currPlayer)
        {
            PanelScript actPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
            actPanScript.PopulatePanel();
        }
    }

    public void Ability(GameObject _currTarget, string _name)
    {
        CharacterScript targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript targetTile = targetScript.m_tile.GetComponent<TileScript>();
        TileScript tileScript = m_tile.GetComponent<TileScript>();

        switch (_name)
        {
            case "Dash ATK":
                if (TileScript.CheckForEmptyNeighbor(tileScript))
                {
                    tileScript.ClearRadius(tileScript);
                    m_boardScript.m_isForcedMove = gameObject;
                    MovementSelection(3);
                }
                break;
            case "Pull ATK":
                PullTowards(targetScript, tileScript);
                break;
            case "Magnet ATK":
                PullTowards(targetScript, m_boardScript.m_selected.GetComponent<TileScript>());
                break;
            case "Push ATK":
                if (TileScript.CheckForEmptyNeighbor(targetTile))
                {
                    tileScript.ClearRadius(tileScript);
                    m_boardScript.m_isForcedMove = _currTarget;
                    targetScript.MovementSelection(4);
                }
                break;
            case "Smash ATK":
                Knockback(targetScript, tileScript, 3);
                break;
            case "Blast ATK":
                Knockback(targetScript, m_boardScript.m_selected.GetComponent<TileScript>(), 1);
                break;
            case "Spot":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Passage":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Rush ATK":
                int dis = Mathf.Abs(tileScript.m_x - targetTile.m_x) + Mathf.Abs(tileScript.m_z - targetTile.m_z) - 1;
                targetScript.ReceiveDamage(dis.ToString(), Color.white);
                PullTowards(this, targetScript.m_tile.GetComponent<TileScript>());
                break;
            case "Explosive":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Winding ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Weakening ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Scarring ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Boost":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "AIM":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Bleed ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Critical":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Break ATK":
                targetScript.m_stats[(int)sts.DEF]--;
                StatusScript.ApplyStatus(_currTarget);
                break;
            case "Hindering ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Unnerving ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Defensive ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Healing ATK":
                HealHealth(2);
                break;
            case "Disrupting ATK":
                if (targetScript.GetComponents<StatusScript>().Length > 0)
                {
                    PanelScript statSel = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_SELECTOR].GetComponent<PanelScript>();
                    statSel.m_inView = true;
                    statSel.m_cScript = targetScript;
                    statSel.PopulatePanel();
                    tileScript.ClearRadius(tileScript);
                    m_boardScript.m_isForcedMove = gameObject;
                }
                break;
            case "Blinding ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Fortifying ATK":
                StatusScript.NewStatus(gameObject, m_currAction);
                break;
            case "Immobilizing ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Heal":
                targetScript.HealHealth(3);
                break;
            case "Purification":
                StatusScript.DestroyAll(_currTarget);
                break;
            case "Ward ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Bolstering ATK":
                StatusScript.NewStatus(gameObject, m_currAction);
                break;
            case "Prepare":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Protect":
                StatusScript.NewStatus(_currTarget, m_currAction);
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
        TileScript nei = _targetScript.m_tile.GetComponent<TileScript>();
    
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

            int extraDMG = Mathf.CeilToInt(_force / 2.0f);
            TextMesh textMesh = _targetScript.m_popupText.GetComponent<TextMesh>();
            _targetScript.ReceiveDamage(((int.Parse(textMesh.text) + Mathf.CeilToInt(_force / 2.0f)).ToString()), Color.white);

            if (nei.m_holding && nei.m_holding.tag == "Player")
            {
                CharacterScript neiCharScript = nei.m_holding.GetComponent<CharacterScript>();
                neiCharScript.ReceiveDamage(Mathf.CeilToInt(_force / 2.0f).ToString(), Color.white);
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
        sPScript.m_cScript = this;
        sPScript.PopulatePanel();
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
