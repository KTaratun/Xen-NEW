using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : ObjectScript {

    public enum sts { HP, SPD, HIT, EVA, CRT, DMG, DEF, MOV, RNG, RAD, TOT };
    enum trn { MOV, ACT };

    static public Color c_green = new Color(.45f, .7f, .4f, 1);
    static public Color c_red = new Color(.8f, .1f, .15f, 1);
    static public Color c_white = new Color(.8f, .8f, .8f, 1);
    static public Color c_blue = new Color(.45f, .4f, 1, 1);

    static public Color c_attack = new Color(1, 0, 0, 0.5f);
    static public Color c_action = new Color(0, 1, 0, 0.5f);
    static public Color c_move = new Color(0, 0, 1, 0.5f);

    public GameObject m_turnPanel;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject[] m_popupSpheres;
    public GameObject[] m_colorDisplay;
    public GameObject m_player;
    public Animator m_anim;
    public Color m_teamColor;
    public string m_name;
    public string m_currAction;
    public int m_currStatus;
    public string[] m_actions;
    public int[] m_stats;
    public int[] m_tempStats;
    public string[] m_accessories;
    public string m_color;
    public bool[] m_effects;
    public int m_level;
    public int m_exp;
    public int m_currRadius;
    public bool m_isAlive;
    public bool m_isAI;
    public bool m_isFree;
    public int[] m_isDiabled;

	// Use this for initialization
	void Start ()
    {
        m_anim = GetComponent<Animator>();
        m_popupText.SetActive(false);
        m_isFree = false;

        m_currRadius = 0;
        m_effects = new bool[(int)StatusScript.effects.TOT];
        m_isAlive = true;

        if (m_stats.Length == 0)
            InitializeStats();

        // Set up the spheres that pop up over the characters heads when they gain or lose energy
        for (int i = 0; i < m_popupSpheres.Length; i++)
        {
            MeshRenderer[] meshRends = m_popupSpheres[i].GetComponentsInChildren<MeshRenderer>();

            for (int j = 0; j < meshRends.Length; j++)
                meshRends[j].material.color = new Color(meshRends[j].material.color.r, meshRends[j].material.color.g, meshRends[j].material.color.b, 0);
        }

    }

    public void InitializeStats()
    {
        if (m_stats.Length == 0)
        {
            m_stats = new int[(int)sts.TOT];
            m_tempStats = new int[(int)sts.TOT];
            m_accessories = new string[2];
            m_accessories[0] = "Ring of DEATH";
        }

        for (int i = 0; i < m_stats.Length; i++)
            m_stats[i] = 0;
      
        m_stats[(int)sts.HP] = 12;
        m_stats[(int)sts.SPD] = 10;
        m_stats[(int)sts.MOV] = 5;

        m_level = 1;

        for (int i = 0; i < m_stats.Length; i++)
            m_tempStats[i] = m_stats[i];
    }

    // Update is called once per frame
    void Update()
    {
        // Move towards new tile
        MovementUpdate();

        if (m_boardScript)
        {
            EndTurn(false);
            UpdateOverheadItems();
        }
    }

    private void MovementUpdate()
    {
        if (m_tile && transform.position != m_tile.transform.position && m_isAlive)
        {
            // Determine how much the character will be moving this update
            float charAcceleration = 0.02f;
            float charSpeed = 0.015f;
            float charMovement = Vector3.Distance(transform.position, m_tile.transform.position) * charAcceleration + charSpeed;
            transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement, transform.position.y, transform.position.z + transform.forward.z * charMovement), transform.rotation);
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = gameObject;

            float snapDistance = 0.007f;
            if (Vector3.Distance(transform.position, m_tile.transform.position) < snapDistance)
            {
                transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation);
                m_boardScript.m_camIsFrozen = false;
                CameraScript cam = m_boardScript.m_camera.GetComponent<CameraScript>();
                cam.m_target = null;
                m_anim.Play("Idle", -1, 0);
            }
        }
    }

    public void EndTurn(bool _isForced)
    {
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();

        if (_isForced && !m_boardScript.m_camIsFrozen || m_boardScript.m_currPlayer == gameObject && mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable == false
                && mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable == false && !m_boardScript.m_isForcedMove && !m_boardScript.m_camIsFrozen)
        {
            mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = true;
            mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = true;
            mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].GetComponent<Image>().color = Color.white;
            mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].GetComponent<Image>().color = Color.white;
            StatusScript.UpdateStatus(gameObject, StatusScript.mode.TURN_END);
            m_boardScript.NewTurn();
        }
    }

    private void UpdateOverheadItems()
    {
        // Update Health bar
        if (m_healthBar.activeSelf && m_isAlive)
        {
            Transform outline = m_healthBar.transform.parent;

            outline.LookAt(2 * outline.position - m_boardScript.m_camera.transform.position);
            m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
            float ratio = ((float)m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];

            Renderer hpRend = m_healthBar.GetComponent<Renderer>();
            hpRend.material.color = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0.2f, 1);
            m_healthBar.transform.localScale = new Vector3(0.95f - ratio, m_healthBar.transform.localScale.y, m_healthBar.transform.localScale.z);
        }
        else
            m_healthBar.GetComponent<Renderer>().material.color = Color.black;

        // Update character's color spheres
        for (int i = 0; i < m_colorDisplay.Length; i++)
        {
            if (m_colorDisplay[i].activeSelf)
                m_colorDisplay[i].transform.LookAt(2 * m_colorDisplay[i].transform.position - m_boardScript.m_camera.transform.position);
        }

        float fadeSpeed = .01f;

        // Update energy gained/lost
        for (int i = 0; i < m_popupSpheres.Length; i++)
        {
            if (m_popupSpheres[i].activeSelf)
            {
                m_popupSpheres[i].transform.LookAt(2 * m_popupSpheres[i].transform.position - m_boardScript.m_camera.transform.position);

                MeshRenderer[] meshRends = m_popupSpheres[i].GetComponentsInChildren<MeshRenderer>();
                for (int j = 0; j < meshRends.Length; j++)
                {
                    meshRends[j].material.color =
                    new Color(meshRends[j].material.color.r, meshRends[j].material.color.g,
                    meshRends[j].material.color.b, meshRends[j].material.color.a - fadeSpeed);

                    if (meshRends[j].material.color.a <= 0)
                        m_popupSpheres[i].SetActive(false);
                }
            }
        }

        // Update character's popup text
        if (m_popupText.activeSelf)
        {
            m_popupText.transform.LookAt(2 * m_popupText.transform.position - m_boardScript.m_camera.transform.position);
            TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, textMesh.color.a - fadeSpeed);

            if (textMesh.color.a <= 0)
            {
                textMesh.color = Color.white;
                textMesh.text = "0";
                m_popupText.SetActive(false);
                m_boardScript.m_camIsFrozen = false;
                m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_boardScript.m_currPlayer;
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
            move = m_tempStats[(int)sts.MOV];

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
        m_boardScript.m_camIsFrozen = true;
        m_anim.Play("Running", -1, 0);

        if (gameObject == m_boardScript.m_currPlayer)
            m_boardScript.m_currTile = m_tile;
        if (m_boardScript.m_isForcedMove == null && !_isForced)
        {
            PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
            mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;

            if (mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].GetComponent<Image>().color == Color.yellow)
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
        }

        PanelScript.CloseHistory();
        transform.LookAt(m_tile.transform);
        m_boardScript.m_isForcedMove = null;

        selectedScript.ClearRadius(selectedScript);
    }

    public void ActionTargeting()
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actRng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RNG);
        string actRad = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RAD);

        // If the action doesn't target
        if (actName == "Channel")
        {
            m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>().m_inView = false;
            List<GameObject> myself = new List<GameObject>();
            myself.Add(gameObject);
            Action();
            return;
        }

        TileScript tileScript = m_tile.GetComponent<TileScript>();
        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;

        // If the action only targets horizontal and vertical or diagonal
        if (actName == "Pull ATK" || actName == "Piercing ATK" || actName == "Lunge ATK" || actName == "Thrust ATK")
            targetingRestriction = TileScript.targetRestriction.HORVERT;
        else if (actName == "Cross ATK")
            targetingRestriction = TileScript.targetRestriction.DIAGONAL;

        // If the action cannot be blocked
        bool isBlockable = true;
        if (actName == "Piercing ATK" || actName == "Cross ATK" || actName == "Thrust ATK")
            isBlockable = false;

        m_currRadius = int.Parse(actRad);
        bool targetSelf = false;

        // If the action can target the source
        if (m_currRadius > 0 && actName != "Slash ATK")
            targetSelf = true;

        int finalRNG = int.Parse(actRng);
        if (finalRNG > 1 && finalRNG + m_tempStats[(int)sts.RNG] > 1)
            finalRNG += m_tempStats[(int)sts.RNG];

        // See if it's an attack or not
        if (CheckIfAttack(actName))
            tileScript.FetchTilesWithinRange(finalRNG, c_attack, targetSelf, targetingRestriction, isBlockable);
        else
            tileScript.FetchTilesWithinRange(finalRNG, c_action, true, targetingRestriction, isBlockable);

        PanelScript actionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.m_inView = false;
    }

    public void Action()
    {
        TileScript selectedTileScript = m_boardScript.m_selected.GetComponent<TileScript>();
        List<GameObject> targets = new List<GameObject>();
        Renderer r = m_boardScript.m_selected.GetComponent<Renderer>();
        TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
        textMesh.text = "0";

        if (r.material.color == Color.red
            || r.material.color == Color.green) // single
        {
            targets.Add(selectedTileScript.m_holding);
        }
        else if (r.material.color == Color.yellow) // multi
        {
            for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
            {
                TileScript tarTile = selectedTileScript.m_targetRadius[i].GetComponent<TileScript>();
                if (tarTile.m_holding && tarTile.m_holding.tag == "Player")
                    targets.Add(tarTile.m_holding);
            }
        }

        PanelScript.CloseHistory();
        transform.LookAt(m_boardScript.m_selected.transform);

        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actEng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.ENERGY);
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        bool miss = true;

        if (CheckIfAttack(actName)) // See if it's an attack
            miss = Attack(targets);
        else // If it's not an attack
        {
            miss = false;
            for (int i = 0; i < targets.Count; i++)
                if (!m_effects[(int)StatusScript.effects.HINDER])
                    Ability(targets[i], actName);

            if (mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable == false ||
                mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].GetComponent<Image>().color == Color.yellow
                || m_isFree)
            {
                mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
            }
            else if (!m_isFree)
                mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].GetComponent<Image>().color = Color.yellow;
        }

        if (PlayerScript.CheckIfGains(actEng) && !miss || !PlayerScript.CheckIfGains(actEng) && !m_isFree)
            EnergyConversion(actEng);

        if (!miss)
            m_exp += actEng.Length;

        m_isFree = false;
        m_currRadius = 0;
        m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PREVIEW].GetComponent<PanelScript>().m_inView = false;

        if (targets.Count > 0)
        {
            if (selectedTileScript.m_targetRadius.Count > 0)
                selectedTileScript.ClearRadius(selectedTileScript);

            if (!m_boardScript.m_isForcedMove)
                m_tile.GetComponent<TileScript>().ClearRadius(m_tile.GetComponent<TileScript>());
        }
        }

    public bool Attack(List<GameObject> _targets)
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actHit = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.HIT);
        string actDmg = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.DMG);
        string actCrt = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.CRT);
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();

        bool miss = true;
        int roll = Random.Range(0, 100); // Because Random.Range doesn't include the max number

        for (int i = 0; i < _targets.Count; i++)
        {
            CharacterScript targetScript = _targets[i].GetComponent<CharacterScript>();

            if (targetScript.m_isAlive == false)
                continue;

            int DC = 100 - int.Parse(actHit) - m_tempStats[(int)sts.HIT] + targetScript.m_tempStats[(int)sts.EVA];
            int CRT = 100 - int.Parse(actCrt) - m_tempStats[(int)sts.CRT];
            string finalDMG = "0";

            print("Rolled " + roll + " against " + actHit + " - my HIT mod of " + m_tempStats[(int)sts.HIT] + " + my opponents EVA mod of " + targetScript.m_tempStats[(int)sts.EVA] +
                " for a total DC of " + DC + ". Crit: " + actCrt + " - my CRT mod of " + m_tempStats[(int)sts.CRT] + " for a total of Crit: " + (int.Parse(actCrt) - m_tempStats[(int)sts.CRT]) + ".\n");

            if (actName == "Bypass ATK")
            {
                DC -= targetScript.m_tempStats[(int)sts.EVA];
                finalDMG = (0 + targetScript.m_tempStats[(int)sts.DEF]).ToString();
            }

            // If Critical
            if (roll >= CRT)
            {
                if ((int.Parse(actDmg) * 2) + m_tempStats[(int)sts.DMG] + targetScript.m_tempStats[(int)sts.DEF] >= 0)
                    finalDMG = ((int.Parse(actDmg) * 2) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF]).ToString();

                print("Hit with a crit dealing " + actDmg + "x2 + my DMG mod of " + m_tempStats[(int)sts.DMG] + " - my opponent's DEF mod of " + targetScript.m_tempStats[(int)sts.DEF] + " for a total of " + finalDMG + ".\n");
                targetScript.ReceiveDamage(finalDMG, Color.red);
                miss = false;
            }
            else if (roll < DC) // If Miss
            {
                targetScript.ReceiveDamage("MISS", Color.white);
                print("Missed.\n");
            }
            else // If Hit
            {
                if (int.Parse(actDmg) + m_tempStats[(int)sts.DMG] + targetScript.m_tempStats[(int)sts.DEF] >= 0)
                    finalDMG = (int.Parse(actDmg) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF]).ToString();

                print("Hit dealing " + actDmg + " + my DMG mod of " + m_tempStats[(int)sts.DMG] + " - my opponent's DEF mod of " + targetScript.m_tempStats[(int)sts.DEF] + " for a total of " + finalDMG + ".\n");
                targetScript.ReceiveDamage(finalDMG, Color.white);
                miss = false;
            }

            // Use ability
            if (!miss && !m_effects[(int)StatusScript.effects.HINDER])
                Ability(_targets[i], actName);
        }

        // REFACTOR
        if (actName == "Redirect ATK" && m_boardScript.m_isForcedMove ||
            actName == "Copy ATK" && m_boardScript.m_isForcedMove)
            m_boardScript.m_isForcedMove = null;
        else
        {
            mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
            if (mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].GetComponent<Image>().color == Color.yellow)
                EndTurn(true);
        }

        if (_targets.Count > 1)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_boardScript.m_selected;
        else
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = _targets[0];

        m_boardScript.m_camIsFrozen = true;
        return miss;
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
            if (m_tempStats[(int)sts.HP] - parsedDMG <= 0)
                Dead();
            else
                m_tempStats[(int)sts.HP] -= parsedDMG;
        }

        int res = 0;
        if (int.TryParse(_dmg, out res))
            textMesh.text = (int.Parse(textMesh.text) + int.Parse(_dmg)).ToString();
        else
            textMesh.text = _dmg;

        if (gameObject == m_boardScript.m_currPlayer)
        {
            PanelScript actPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
            actPanScript.PopulatePanel();
        }
    }

    public void Dead()
    {
        m_tempStats[(int)sts.HP] = 0;
        if (m_turnPanel)
            m_turnPanel.GetComponent<Image>().color = new Color(1, .5f, .5f, 1);
        m_isAlive = false;

        StatusScript.DestroyAll(gameObject);

        transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.rotation);
        GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f, 1);
    }

    public void Revive(int _hp)
    {
        if (m_isAlive)
            return;

        HealHealth(_hp);

        if (m_turnPanel)
            m_turnPanel.GetComponent<Image>().color = m_teamColor;
        m_isAlive = true;

        transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation);
        GetComponent<Renderer>().material.color = Color.white;

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
            // Simple status abilities
            case "Shot ATK":
            case "Spot":
            case "Passage":
            case "Explosive":
            case "Smoke ATK":
            case "Winding ATK":
            case "Weakening ATK":
            case "Scarring ATK":
            case "Boost":
            case "Aim":
            case "Bleed ATK":
            case "Critical":
            case "Hindering ATK":
            case "Unnerving ATK":
            case "Defensive ATK":
            case "Blinding ATK":
            case "Immobilizing ATK":
            case "Ward ATK":
            case "Protect":
            case "Prepare":
            case "Stun ATK":
            case "Delay ATK":
            case "Disorienting ATK":
                StatusScript.NewStatus(_currTarget, m_currAction);
                break;
            case "Fortifying ATK":
            case "Bolstering ATK":
                StatusScript.NewStatus(gameObject, m_currAction);
                break;

            // Unique abilities
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
           //case "Rush ATK":
           //    int dis = Mathf.Abs(tileScript.m_x - targetTile.m_x) + Mathf.Abs(tileScript.m_z - targetTile.m_z) - 1;
           //    targetScript.ReceiveDamage(dis.ToString(), Color.white);
           //    PullTowards(this, targetScript.m_tile.GetComponent<TileScript>());
           //    break;
            case "Lunge ATK":
                PullTowards(this, targetScript.m_tile.GetComponent<TileScript>());
                break;
            case "Break ATK":
                targetScript.m_stats[(int)sts.DEF]--;
                StatusScript.ApplyStatus(_currTarget);
                break;
            case "Healing ATK":
                HealHealth(2);
                break;
            case "Disrupting ATK":
            case "Extension":
            //case "Modification":
                if (targetScript.GetComponents<StatusScript>().Length > 0)
                    SelectorInit(targetScript, BoardScript.pnls.STATUS_SELECTOR);
                break;
            case "Heal":
                targetScript.HealHealth(4);
                break;
            case "Purification":
                StatusScript.DestroyAll(_currTarget);
                break;
            case "Revive":
                targetScript.Revive(10);
                break;
            case "Feint ATK":
                targetScript.m_tempStats[(int)sts.SPD] -= 2;
                break;
            case "Diminish ATK":
                targetScript.m_player.GetComponent<PlayerScript>().RemoveRandomEnergy();
                break;
            case "Coordinated ATK":
                m_tempStats[(int)sts.SPD] += 2;
                break;
            case "Prismatic ATK":
            case "Channel":
            case "Syphon ATK":
            case "Deplete ATK":
                SelectorInit(targetScript, BoardScript.pnls.ENERGY_SELECTOR);
                break;
            case "Redirect ATK":
            case "Copy ATK":
            case "Hack ATK":
                SelectorInit(targetScript, BoardScript.pnls.ACTION_PANEL);
                break;
            case "Focus":
                targetScript.m_tempStats[(int)sts.SPD] += 2;
                break;
            case "Concussive ATK":
                targetScript.m_tempStats[(int)sts.SPD] -= 3;
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
            _targetScript.ReceiveDamage(((extraDMG).ToString()), Color.white);

            if (nei.m_holding && nei.m_holding.tag == "Player")
            {
                CharacterScript neiCharScript = nei.m_holding.GetComponent<CharacterScript>();
                neiCharScript.ReceiveDamage(extraDMG.ToString(), Color.white);
            }
            break;
        }
    }

    private void EnergyConversion(string _energy)
    {
        PlayerScript playScript = m_player.GetComponent<PlayerScript>();
        // Assign _energy symbols
        for (int i = 0; i < _energy.Length; i++)
        {
            if (_energy[i] == 'g')
                playScript.m_energy[0] += 1;
            else if (_energy[i] == 'r')
                playScript.m_energy[1] += 1;
            else if (_energy[i] == 'w')
                playScript.m_energy[2] += 1;
            else if (_energy[i] == 'b')
                playScript.m_energy[3] += 1;
            else if (_energy[i] == 'G')
                playScript.m_energy[0] -= 1;
            else if (_energy[i] == 'R')
                playScript.m_energy[1] -= 1;
            else if (_energy[i] == 'W')
                playScript.m_energy[2] -= 1;
            else if (_energy[i] == 'B')
                playScript.m_energy[3] -= 1;
        }

        SetPopupSpheres(_energy);
        playScript.SetEnergyPanel();
    }

    public void SetPopupSpheres(string _energy)
    {
        GameObject[] colorSpheres = m_popupSpheres;
        if (_energy.Length == 0)
        {
            _energy = m_color;
            colorSpheres = m_colorDisplay;
        }

        GameObject spheres = null;
        if (_energy.Length == 1)
        {
            colorSpheres[0].SetActive(true);
            spheres = colorSpheres[0];
        }
        else if (_energy.Length == 2)
        {
            colorSpheres[1].SetActive(true);
            spheres = colorSpheres[1];
        }
        else if (_energy.Length == 3)
        {
            colorSpheres[2].SetActive(true);
            spheres = colorSpheres[2];
        }
        else if (_energy.Length == 4)
        {
            colorSpheres[3].SetActive(true);
            spheres = colorSpheres[3];
        }

        Color color = Color.white;
        if (_energy[0] == 'g' || _energy[0] == 'G')
            color = c_green;
        else if (_energy[0] == 'r' || _energy[0] == 'R')
            color = c_red;
        else if (_energy[0] == 'w' || _energy[0] == 'W')
            color = c_white;
        else if (_energy[0] == 'b' || _energy[0] == 'B')
            color = c_blue;

        SphereCollider[] orbs = spheres.GetComponentsInChildren<SphereCollider>();
        int j = 0;

        for (int i = 0; i < orbs.Length; i++)
        {
            Renderer orbRend = orbs[i].GetComponent<Renderer>();
            
            if (orbs[i].name == "Sphere Outline")
                orbRend.material.color = new Color(0, 0, 0, 1);
            else
                orbRend.material.color = color;

            j++;
        }
    }

    public void Pass()
    {
        PanelScript mainPanelScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.m_buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        mainPanelScript.m_buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
        PanelScript.CloseHistory();
    }

    // Maybe move  REFACTOR: This looks odd
    public void SelectorInit(CharacterScript _targetScript, BoardScript.pnls _selector)
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        PanelScript selector = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_SELECTOR].GetComponent<PanelScript>();
        if (_selector == BoardScript.pnls.STATUS_SELECTOR)
        {
            selector = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_SELECTOR].GetComponent<PanelScript>();
            if (actName == "Disrupting ATK")
                selector.m_text[0].text = "Choose a status to remove";
            //if (actName == "Hack ATK")
            //    selector.m_text[0].text = "Choose a status to steal";
            if (actName == "Extension" || actName == "Modification")
                selector.m_text[0].text = "Choose a status to boost";
        }
        else if (_selector == BoardScript.pnls.ENERGY_SELECTOR)
        {
            selector = m_boardScript.m_panels[(int)BoardScript.pnls.ENERGY_SELECTOR].GetComponent<PanelScript>();

            if (actName == "Syphon ATK" || actName == "Deplete ATK")
            {
                if (actName == "Syphon ATK")
                    selector.m_text[0].text = "Choose energy to steal";
                else if (actName == "Deplete ATK")
                    selector.m_text[0].text = "Choose energy to remove";

                for (int i = 0; i < selector.m_images.Length; i++)
                {
                    Text t = selector.m_images[i].GetComponentInChildren<Text>();
                    PlayerScript pScript = _targetScript.m_player.GetComponent<PlayerScript>();
                    t.text = pScript.m_energy[i].ToString();
                }
            }
            else if (actName == "Prismatic ATK" || actName == "Channel")
            {
                selector.m_text[0].text = "Choose energy to gain";
                for (int i = 0; i < selector.m_images.Length; i++)
                    selector.m_images[i].GetComponentInChildren<Text>().text = "0";
            }
        }
        else if (_selector == BoardScript.pnls.ACTION_PANEL)
            selector = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();

        selector.m_cScript = _targetScript;
        selector.PopulatePanel();

        m_tile.GetComponent<TileScript>().ClearRadius(m_tile.GetComponent<TileScript>());
        m_boardScript.m_isForcedMove = gameObject;
    }

    // The only reason why this exists is because statusscript needs a way to get to HUD LEFT
    public void UpdateStatusImages()
    {
        if (gameObject == m_boardScript.m_currPlayer)
        {
            PanelScript panScript = m_boardScript.m_panels[(int)BoardScript.pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
            panScript.PopulatePanel();
        }
    }

    public void DisableRandomAction()
    {
        List<int> viableActs = new List<int>();
        for (int i = 0; i < m_actions.Length; i++)
        {
            string currAct = m_actions[i];
            if (!PlayerScript.CheckIfGains(DatabaseScript.GetActionData(currAct, DatabaseScript.actions.ENERGY)) &&
                m_isDiabled[i] == 0)
                viableActs.Add(i);
        }
        if (viableActs.Count < 1)
            return;

        m_isDiabled[viableActs[Random.Range(0, viableActs.Count)]] = 1; // 1 == temp, 2 == perm
    }

    public void DisableSelectedAction(int _ind)
    {
        m_isDiabled[_ind] = 2; // 1 == temp, 2 == perm
        m_boardScript.m_isForcedMove = null;
        PanelScript.CloseHistory();
    }
}
