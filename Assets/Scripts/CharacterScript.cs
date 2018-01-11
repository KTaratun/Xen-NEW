using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : ObjectScript {

    public enum weaps { SWORD, HGUN };
    public enum sts { HP, SPD, DMG, DEF, MOV, RNG, TEC, RAD, TOT };
    public enum trn { MOV, ACT };
    public enum uniAct { TAR_RES, IS_NOT_BLOCK, TAR_SELF, RNG_MOD, NO_RNG, // TARGETING
        SOLO_BUFF, TEAM_BUFF, BYPASS, DMG_MOD, FRIENDLY, // ATTACK
        RAD_MOD, NON_RAD, RAD_NOT_MODDABLE, // RADIUS
        MOBILITY }
    public enum bod { HIPS, LEFT_UP_LEG, RIGHT_UP_LEG, SPINE, LEFT_LEG, RIGHT_LEG,
        SPINE_1, LEFT_FOOT, RIGHT_FOOT, SPINE_2, LEFT_TOE_BASE, RIGHT_TOE_BASE,
        LEFT_SHOULDER, NECK, RIGHT_SHOULDER, LEFT_TOE_END, RIGHT_TOE_END, LEFT_ARM,
        HEAD, RIGHT_ARM, LEFT_FORE_ARM, HEAD_TOP_END, RIGHT_FORE_ARM, LEFT_HAND,
        RIGHT_HAND}
    public enum prtcles { GUN_SHOT, PROJECTILE_HIT, CHAR_MARK, GAIN_STATUS, HEALTH_EYE}

    static public Color c_green = new Color(.45f, .7f, .4f, 1);
    static public Color c_red = new Color(.8f, .1f, .15f, 1);
    static public Color c_white = new Color(.8f, .8f, .8f, 1);
    static public Color c_blue = new Color(.45f, .4f, 1, 1);

    static public int MAX_ACTIONS = 6;

    // General
    public string m_color;
    public int m_level;
    public int m_exp;
    public int m_gender;
    public string[] m_accessories;

    // Combat
    public PlayerScript m_player;
    public Color m_teamColor;
    public int[] m_stats;
    public int[] m_tempStats;
    public string[] m_actions;
    public string m_currAction;
    public List<GameObject> m_targets;

    // State Info
    public bool m_isAlive;
    public bool m_isFree;
    public int[] m_isDiabled;
    public bool m_isAI;
    public int[] m_hasActed;
    public StatusScript[] m_statuses;
    public int m_currStatus;
    public bool[] m_effects;

    // GUI
    public List<GameObject> m_turnPanels;
    public GameObject m_statusSymbol;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject[] m_popupSpheres;
    public GameObject[] m_colorDisplay;
    public GameObject[] m_RangeCheck;

    // References
    public List<GameObject> m_body;
    public GameObject[] m_weapons;
    public Animator m_anim;
    public GameObject[] m_particles;
    public AudioSource m_audio;



    // Use this for initialization
    new void Start ()
    {
        base.Start();
        // Stats
        if (m_stats.Length == 0)
            InitializeStats();

        m_tempStats = new int[(int)sts.TOT];
        for (int i = 0; i < m_stats.Length; i++)
            m_tempStats[i] = m_stats[i];

        // Status
        for (int i = 0; i < m_statuses.Length; i++)
            m_statuses[i] = gameObject.AddComponent<StatusScript>();

        // Audio
        m_audio = gameObject.AddComponent<AudioSource>();

        // Animation
        if (!m_anim)
            m_anim = GetComponentInChildren<Animator>();

        m_popupText.SetActive(false);
        m_isFree = false;

        InitBones();
        InitColors();

        // If characters have just action names, find full action data
        if (m_actions.Length > 0)
            Invoke("RetrieveActions", .1f);

        if (!m_isAlive)
            CharInit();

        //if (m_player)
        //    m_player.m_characters.Add(this);
    }

    public void CharInit()
    {
        m_isDiabled = new int[m_actions.Length];

        m_isAlive = true;
        m_effects = new bool[(int)StatusScript.effects.TOT];

        Renderer rend = transform.GetComponentInChildren<Renderer>();
        rend.materials[0].color = m_teamColor;

        if (m_hasActed.Length == 0)
            m_hasActed = new int[2];
    }

    public void InitializeStats()
    {
        m_stats = new int[(int)sts.TOT];

        m_stats[(int)sts.HP] = 12;
        m_stats[(int)sts.SPD] = 10;
        m_stats[(int)sts.MOV] = 5;


        m_accessories = new string[2];
        m_accessories[0] = "Ring of DEATH";
      
        m_level = 1;
    }

    private void InitBones()
    {
        List<GameObject> bodyParts = new List<GameObject>();
        bodyParts.Add(m_body[0]);

        while (bodyParts.Count > 0)
        {
            for (int i = 0; i < bodyParts[0].transform.childCount; i++)
            {
                if (!bodyParts[0].transform.GetChild(i).gameObject.activeSelf)
                    continue;
                m_body.Add(bodyParts[0].transform.GetChild(i).gameObject);
                bodyParts.Add(bodyParts[0].transform.GetChild(i).gameObject);
            }
            bodyParts.RemoveAt(0);
        }
    }

    private void InitColors()
    {
        SetPopupSpheres("");
        // Set up the spheres that pop up over the characters heads when they gain or lose energy
        for (int i = 0; i < m_popupSpheres.Length; i++)
        {
            MeshRenderer[] meshRends = m_popupSpheres[i].GetComponentsInChildren<MeshRenderer>();

            for (int j = 0; j < meshRends.Length; j++)
                meshRends[j].material.color = new Color(meshRends[j].material.color.r, meshRends[j].material.color.g, meshRends[j].material.color.b, 0);
        }

        //if (m_boardScript && m_boardScript.m_currCharScript != this)
        //{
        //    Renderer rend = transform.GetComponentInChildren<Renderer>();
        //    rend.materials[0].color = Color.red;
        //}
    }


    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (m_boardScript)
        {
            UpdateOverheadItems();

            if (m_particles[(int)prtcles.GUN_SHOT].activeSelf)
            {
                ParticleSystem[] pS = m_particles[(int)prtcles.GUN_SHOT].GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < pS.Length; i++)
                    pS[i].transform.SetPositionAndRotation(pS[i].transform.position, Quaternion.Euler(-90, 0, 0));
            }

            if (m_boardScript.m_currCharScript == this && Input.GetKeyDown(KeyCode.P))
                AITurn();
        }
    }

    private void UpdateOverheadItems()
    {
        // Update Health bar
        //if (m_isAlive) //m_healthBar.activeSelf
        //{
        //    Transform outline = m_healthBar.transform.parent;
        //
        //    outline.LookAt(2 * outline.position - m_boardScript.m_camera.transform.position);
        //    m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
        //    float ratio = (float)(m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];
        //
        //    Renderer hpRend = m_healthBar.GetComponent<Renderer>();
        //    hpRend.material.color = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0.2f, 1);
        //    m_healthBar.transform.localScale = new Vector3(0.95f - ratio, m_healthBar.transform.localScale.y, m_healthBar.transform.localScale.z);
        //}
        //else
        //    m_statusSymbol.transform.LookAt(2 * m_statusSymbol.transform.position - m_boardScript.m_camera.transform.position);

        if (m_isAlive) //m_healthBar.activeSelf
        {
            float ratio = (float)(m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];
        
            Renderer hpRend = m_healthBar.GetComponent<Renderer>();
            m_particles[(int)prtcles.HEALTH_EYE].GetComponent<ParticleSystem>().startColor = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0.2f, 1);
        }
        else
            m_statusSymbol.transform.LookAt(2 * m_statusSymbol.transform.position - m_boardScript.m_camera.transform.position);

        // Update character's color spheres
        for (int i = 0; i < m_colorDisplay.Length; i++)
            if (m_colorDisplay[i].activeSelf)
                m_colorDisplay[i].transform.LookAt(2 * m_colorDisplay[i].transform.position - m_boardScript.m_camera.transform.position);

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
                    meshRends[j].material.color = new Color(meshRends[j].material.color.r, meshRends[j].material.color.g,
                    meshRends[j].material.color.b, meshRends[j].material.color.a - fadeSpeed);

                    if (meshRends[j].material.color.a <= 0)
                    {
                        m_popupSpheres[i].SetActive(false);
                        if (m_boardScript.m_currCharScript)
                            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_boardScript.m_currCharScript.gameObject;
                    }
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
            }
        }

        m_RangeCheck[0].transform.parent.LookAt(2 * m_RangeCheck[0].transform.parent.position - m_boardScript.m_camera.transform.position);

    }


    // Movement
    override public void MovingStart(TileScript _newScript, bool _isForced, bool _isNetForced)
    {
        base.MovingStart(_newScript, _isForced, _isNetForced);

        if (m_isAlive)
            m_anim.Play("Running Melee", -1, 0);

        if (!_isForced)
            if (!GameObject.Find("Network") ||
                GameObject.Find("Network") && GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted ||
                _isNetForced)
            {
                m_hasActed[(int)trn.MOV] += 2;
                PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[(int)trn.MOV].interactable = false;
                if (m_boardScript.m_currButton)
                {
                    m_boardScript.m_currButton.GetComponent<Image>().color = Color.white;
                    m_boardScript.TurnMoveSelectOff();
                }
            }

        print(m_name + " has started moving.\n");
    }

    override public void MovingFinish()
    {
        if (m_tile.m_holding && m_tile.m_holding.tag == "PowerUp")
            m_tile.m_holding.GetComponent<PowerupScript>().OnPickup(this);

        base.MovingFinish();

        if (m_isAlive)
            m_anim.Play("Idle Melee", -1, 0);

        if (m_isAI && m_currAction.Length > 0 && m_targets.Count > 0 && !m_boardScript.m_isForcedMove)
        {
            m_boardScript.m_selected = m_targets[0].GetComponent<CharacterScript>().m_tile;
            transform.LookAt(m_boardScript.m_selected.transform);
            if (TileScript.CaclulateDistance(m_tile, m_boardScript.m_selected) > 1)
                m_anim.Play("Ranged", -1, 0);
            else
                m_anim.Play("Melee", -1, 0);

            print(m_name + " has started attacking.\n");
        }

        print(m_name + " has finished moving.\n");
    }


    // Action
    public void ActionTargeting(TileScript _tile)
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actRng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RNG);
        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;

        // RANGE MODS

        // If the action only targets horizontal and vertical or diagonal
        targetingRestriction = TileScript.targetRestriction.NONE;
        if (UniqueActionProperties(m_currAction, uniAct.TAR_RES) >= 0)
            targetingRestriction = (TileScript.targetRestriction)UniqueActionProperties(m_currAction, uniAct.TAR_RES);

        // If the action cannot be blocked
        bool isBlockable = true;
        if (UniqueActionProperties(m_currAction, uniAct.IS_NOT_BLOCK) >= 0 ||
            m_tempStats[(int)sts.RAD] > 0)
            isBlockable = false;

        bool targetSelf = false;

        // If the action can target the source
        if (UniqueActionProperties(m_currAction, uniAct.TAR_SELF) >= 0 || !CheckIfAttack(actName)) //|| !CheckIfAttack(actName) && actName != "Protect")
            targetSelf = true;

        int finalRNG = int.Parse(actRng) + m_tempStats[(int)sts.RNG];
        
        if (UniqueActionProperties(m_currAction, uniAct.RNG_MOD) >= 0)
            finalRNG += m_tempStats[(int)sts.TEC];

        if (finalRNG < 1)
            finalRNG = 1;

        if (UniqueActionProperties(m_currAction, uniAct.NO_RNG) >= 0)
            finalRNG = 0;

        Color color = Color.white;
        // See if it's an attack or not
        if (CheckIfAttack(actName))
            color = TileScript.c_attack;
        else
            color = TileScript.c_action;

        _tile.FetchTilesWithinRange(this, finalRNG, color, targetSelf, targetingRestriction, isBlockable);
    }

    public void ActionStart(bool _isNetForced)
    {
        transform.LookAt(m_boardScript.m_selected.transform);

        // Sometimes the character moves when we look at? We have to adjust in case this happens
        transform.SetPositionAndRotation(m_tile.transform.position, transform.rotation);

        int actRng = int.Parse(DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RNG)) + m_tempStats[(int)sts.RNG];
        int actRad = int.Parse(DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RAD)) + m_tempStats[(int)sts.RAD];
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);

        TileScript selectedTileScript = m_boardScript.m_selected;

        if (actRad == 0 && UniqueActionProperties(m_currAction, uniAct.NON_RAD) < 0) // single
        {
            if (UniqueActionProperties(m_currAction, uniAct.MOBILITY) >= 1 && actRng <= 1)
                m_anim.Play("Kick", -1, 0);
            else if (!CheckIfAttack(actName))
                m_anim.Play("Ability", -1, 0);
            else if (actRng + m_tempStats[(int)sts.RNG] > 1)
                m_anim.Play("Ranged", -1, 0);
            else
                m_anim.Play("Melee", -1, 0);

            if (m_targets.Count == 0)
                m_targets.Add(selectedTileScript.m_holding);
        }
        else // multi
        {
            if (m_targets.Count == 0)
                for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
                {
                    TileScript tarTile = selectedTileScript.m_targetRadius[i].GetComponent<TileScript>();
                    if (tarTile.m_holding)
                        m_targets.Add(tarTile.m_holding);
                }

            if (actName == "ATK(Slash)")
                m_anim.Play("Slash", -1, 0);
            else if (actName == "ATK(Stab)")//(UniqueActionProperties(m_currAction, uniAct.TAR_RES) == (int)TileScript.targetRestriction.HORVERT)
                m_anim.Play("Stab", -1, 0);
            else if (actRng <= 1)
                m_anim.Play("Sweep", -1, 0);
            else
                m_anim.Play("Throw", -1, 0);

            if (actName == "ATK(Piercing)" || actName == "ATK(Diagnal)")
            {
                for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
                {
                    if (i == 0 || TileScript.CaclulateDistance(selectedTileScript.m_targetRadius[i], m_tile) > TileScript.CaclulateDistance(m_boardScript.m_selected, m_tile))
                        m_boardScript.m_selected = selectedTileScript.m_targetRadius[i];
                }
            }
        }

        m_boardScript.m_camIsFrozen = true;
        PanelScript.GetPanel("ActionViewer Panel").m_panels[1].ClosePanel();
        PanelScript.CloseHistory();

        if (selectedTileScript.m_targetRadius.Count > 0)
            selectedTileScript.ClearRadius();

        if (!m_boardScript.m_isForcedMove)
            m_tile.GetComponent<TileScript>().ClearRadius();

        if (m_targets.Count > 1)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_boardScript.m_selected.gameObject;
        else
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_targets[0];

        if (GameObject.Find("Network"))
        {
            int actId = int.Parse(DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.ID)) - 1;
            string msg = "ACTIONSTART~" + m_id.ToString() + '|' + actId.ToString() + '|' +
                m_boardScript.m_selected.m_id + '|';

            for (int i = 0; i < m_targets.Count; i++)
                msg += m_targets[i].GetComponent<ObjectScript>().m_id.ToString() + ',';

            msg = msg.Trim(',');

            if (!GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted && !_isNetForced)
            {
                ClientScript c = GameObject.Find("Network").GetComponent<ClientScript>();
                c.Send(msg, c.m_reliableChannel);
            }
            else if (GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
            {
                ServerScript s = GameObject.Find("Network").GetComponent<ServerScript>();
                s.Send(msg, s.m_reliableChannel, s.m_clients);
            }
        }

        if (m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Idle Melee"))
            return;
    }

    public void Action()
    {
        m_boardScript.m_actionEndTimer = 0;
        m_boardScript.m_actionEndTimer += Time.deltaTime;

        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actEng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.ENERGY);

        // ATTACK MODS
        bool teamBuff = false;
        bool soloBuff = false;
        bool gainOK = false;
        if (UniqueActionProperties(m_currAction, uniAct.TEAM_BUFF) >= 0)// || actName == "Cleansing ATK")
            teamBuff = true;
        else if (UniqueActionProperties(m_currAction, uniAct.SOLO_BUFF) >= 0)
            soloBuff = true;

        if (m_targets.Count > 0)
        {
            for (int i = 0; i < m_targets.Count; i++)
            {
                if (CheckIfAttack(actName))
                    if (Attack(m_targets[i].GetComponent<ObjectScript>()))
                        gainOK = true;

                if (!soloBuff)
                    if (m_targets[i].tag == "PowerUp" || m_targets[i].tag == "Player" && !m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.REFLECT])
                        if (!teamBuff || teamBuff && m_targets[i].tag == "Player" && m_targets[i].GetComponent<CharacterScript>().m_player == m_player)
                            Ability(m_targets[i], actName);
            }
        }

        if (!m_isFree && CheckIfAttack(actName))
            m_hasActed[(int)trn.ACT] += 3;
        else if (!CheckIfAttack(actName) && actName != "SUP(Redirect)")
            m_hasActed[(int)trn.ACT] += 2;

        if (teamBuff || soloBuff && gainOK)
            Ability(gameObject, actName);

        if (!m_isFree && PlayerScript.CheckIfGains(actEng) && gainOK || !m_isFree && !PlayerScript.CheckIfGains(actEng))
            EnergyConversion(actEng);
        else
            PanelScript.GetPanel("Choose Panel").ClosePanel();

        m_exp += actEng.Length;

        m_isFree = false;
        m_targets.Clear();

        UpdateStatusImages();

        PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();
        if (PanelScript.GetPanel("HUD Panel RIGHT").m_inView)
            PanelScript.GetPanel("HUD Panel RIGHT").PopulatePanel();

        PanelScript.GetPanel("ActionViewer Panel").ClosePanel();
        if (!PanelScript.GetPanel("Choose Panel").m_inView)
            m_boardScript.m_selected = null;

        if (m_boardScript.m_currButton && m_boardScript.m_currButton.GetComponent<PanelScript>())
            m_boardScript.m_currButton.GetComponent<PanelScript>().m_inView = false;
        m_boardScript.m_currButton = null;

        print(m_name + " has finished acting.");
    }

    public bool Attack(ObjectScript _currTarget)
    {
        if (_currTarget.gameObject.tag != "Player" && _currTarget.gameObject.tag != "Environment" || 
            _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_isAlive == false)
            return false;

        string actDmg = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.DMG);

        // ATTACK MODS
        bool friendly = false;
        if (UniqueActionProperties(m_currAction, uniAct.TEAM_BUFF) >= 0 || UniqueActionProperties(m_currAction, uniAct.FRIENDLY) >= 0 ||
            m_effects[(int)StatusScript.effects.CAREFUL])// || actName == "Cleansing ATK")
            friendly = true;

        string finalDMG = (int.Parse(actDmg) + m_tempStats[(int)sts.DMG]).ToString();

        if (!friendly || friendly && _currTarget.gameObject.tag == "Environment" ||
            friendly && _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != m_player)
        {
            if (_currTarget.gameObject.tag == "Player")
            {
                CharacterScript charScript = _currTarget.GetComponent<CharacterScript>();

                // DAMAGE MODS
                if (UniqueActionProperties(m_currAction, uniAct.BYPASS) >= 0 && charScript.m_tempStats[(int)sts.DEF] > 0)
                {
                    int def = charScript.m_tempStats[(int)sts.DEF] - UniqueActionProperties(m_currAction, uniAct.BYPASS) - m_tempStats[(int)sts.TEC];
                    if (def < 0)
                        def = 0;

                    finalDMG = (int.Parse(finalDMG) - def).ToString();
                }
                else
                    finalDMG = (int.Parse(finalDMG) - charScript.m_tempStats[(int)sts.DEF]).ToString();
            }

            if (UniqueActionProperties(m_currAction, uniAct.DMG_MOD) >= 0)
                finalDMG = (int.Parse(finalDMG) + m_tempStats[(int)sts.TEC]).ToString();

            _currTarget.ReceiveDamage(finalDMG, Color.white);

            if (_currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != m_player)
                return true;
        }

        return false;
    }

    public void Ability(GameObject _currTarget, string _name)
    {
        if (_currTarget.tag == "PowerUp" && UniqueActionProperties(m_currAction, uniAct.MOBILITY) == -1)
            return;

        CharacterScript targetScript = null;
        if (_currTarget.tag == "Player")
            targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript targetTile = _currTarget.GetComponent<ObjectScript>().m_tile.GetComponent<TileScript>();
        TileScript tileScript = m_tile.GetComponent<TileScript>();

        // These attacks don't work with AI
        //case "ATK(Copy)":
        //case "ATK(Hack)":
        //case "SUP(Extension)":
        //case "ATK(Dash)":
        //case "ATK(Push)":
        // And all of the actions...

        switch (_name)
        {
            // Simple status abilities
            case "ATK(Accelerate)":
            case "ATK(Arm)":
            case "ATK(Rust)":
            case "ATK(Sight)":
            case "SUP(Boost)":
            case "SUP(Charge)":
            case "SUP(Crush)":
            case "ATK(Break)":
            case "ATK(Crash)":
            case "SUP(Explosive)":
            case "ATK(Fortify)":
            case "ATK(Hinder)":
            case "ATK(Immobilize)":
            case "ATK(Leg)":
            case "ATK(Disrupt)":
            case "SUP(Passage)":
            case "SUP(Defense)":
            //case "Protect":
            case "SUP(Secure)":
            case "ATK(Maintain)":
            case "ATK(Nullify)":
            case "ATK(Ruin)":
            case "ATK(Smoke)":
            case "SUP(Spot)":
            case "ATK(Lock)":
            case "ATK(Target)":
            case "ATK(Ward)":
            case "ATK(Weaken)":
            case "ATK(Rev)":
                StatusScript.NewStatus(_currTarget, this, m_currAction);
                break;
            case "ATK(Deplete)":
            case "ATK(Prismatic)":
            case "ATK(Syphon)":
                if (_name == "ATK(Prismatic)" && m_tempStats[(int)sts.TEC] < -2 || _name == "ATK(Deplete)" && m_tempStats[(int)sts.TEC] < -1 ||
                    _name == "ATK(Syphon)" && m_tempStats[(int)sts.TEC] < -2)
                    break;
                if (!m_isAI)
                    SelectorInit(targetScript, "Energy Selector");
                else if (_name == "ATK(Deplete)")
                    targetScript.m_player.RemoveRandomEnergy(2 + m_tempStats[(int)sts.TEC]);
                else if (_name == "ATK(Prismatic)") // REFACTOR
                    m_player.GainRandomEnergyAI(2 + m_tempStats[(int)sts.TEC]);
                else if (_name == "ATK(Syphon)") // REFACTOR
                {
                    targetScript.m_player.RemoveRandomEnergy(2 + m_tempStats[(int)sts.TEC]);
                    m_player.GainRandomEnergyAI(2 + m_tempStats[(int)sts.TEC]);
                }
                break;
            case "ATK(Copy)":
            case "ATK(Hack)":
            case "SUP(Redirect)":
                if (!m_isAI && m_boardScript.m_currCharScript.CheckIfMine())
                {
                    PanelScript.GetPanel("Choose Panel").m_inView = true;
                    PanelScript.m_history.Add(PanelScript.GetPanel("Choose Panel"));
                }
                break;
            case "SUP(Delete)":
            case "SUP(Extension)":
            //case "Modification": Permanently add a temp status effect
                if (targetScript.GetComponents<StatusScript>().Length > 0 && !m_isAI)
                    SelectorInit(targetScript, "Status Selector");
                break;
            //case "Cleansing ATK":
            case "ATK(Salvage)":
                HealHealth(2 + m_tempStats[(int)sts.TEC]);
                if (m_tempStats[(int)sts.TEC] > 0)
                    m_player.RemoveRandomEnergy(m_tempStats[(int)sts.TEC]);
                break;
            case "SUP(Restore)":
                targetScript.HealHealth(3 + m_tempStats[(int)sts.TEC]);
                if (m_tempStats[(int)sts.TEC] > 0)
                    targetScript.m_player.RemoveRandomEnergy(m_tempStats[(int)sts.TEC]);
                break;
            // Unique abilities
            case "ATK(Blast)":
                Knockback(_currTarget.GetComponent<ObjectScript>(), m_boardScript.m_selected, 2 + m_tempStats[(int)sts.TEC]);
                break;
            case "ATK(Mod)":
                targetScript.m_stats[(int)sts.DMG]++;
                StatusScript.ApplyStatus(_currTarget);
                break;
            case "SUP(Channel)":
                m_player.m_energy[(int)PlayerScript.eng.BLU] += 2;
                break;
            case "ATK(Corrupt)":
                List<StatusScript> debuffs = new List<StatusScript>();
                for (int i = 0; i < targetScript.GetComponents<StatusScript>().Length; i++)
                {
                    StatusScript currStatus = targetScript.GetComponents<StatusScript>()[i];
                    if (currStatus.m_color == StatusScript.c_buffColor)
                        debuffs.Add(currStatus);
                }
                if (debuffs.Count > 0)
                {
                    int randInd = Random.Range(0, debuffs.Count);
                    debuffs[randInd].InvertEffect();
                }
                break;
            case "ATK(Lag)":
                AlterSpeed(targetScript, -2, true);
                break;
            case "ATK(Synch)":
                AlterSpeed(targetScript, 2, true);
                break;
            case "ATK(Dash)":
                if (TileScript.CheckForEmptyNeighbor(tileScript) && !m_isAI && m_boardScript.m_currCharScript.CheckIfMine())
                {
                    m_boardScript.m_isForcedMove = gameObject;
                    tileScript.ClearRadius();
                    MovementSelection(3 + m_tempStats[(int)sts.TEC]);
                }
                break;
            case "ATK(Leak)":
                targetScript.m_player.RemoveRandomEnergy(1);
                break;
            case "ATK(Overclock)":
                AlterSpeed(this, 3, true);
                break;
            case "ATK(Lunge)":
                PullTowards(this, targetScript.m_tile, 100);
                break;
            case "ATK(Magnet)":
                PullTowards(_currTarget.GetComponent<ObjectScript>(), m_boardScript.m_selected, 100);
                break;
            case "ATK(Pull)":
                PullTowards(_currTarget.GetComponent<ObjectScript>(), tileScript, 100);
                if (targetScript)
                    targetScript.m_anim.Play("Pulled", -1, 0);
                break;
            case "ATK(Push)":
                if (TileScript.CheckForEmptyNeighbor(targetTile) && !m_isAI && m_boardScript.m_currCharScript.CheckIfMine())
                {
                    tileScript.ClearRadius();
                    m_boardScript.m_isForcedMove = _currTarget;
                    _currTarget.GetComponent<ObjectScript>().MovementSelection(3 + m_tempStats[(int)sts.TEC]);
                }
                break;
            case "SUP(Reboot)":
                targetScript.Revive(5 + m_tempStats[(int)sts.TEC]);
                break;
            case "ATK(Smash)":
                Knockback(_currTarget.GetComponent<ObjectScript>(), tileScript, 3 + m_tempStats[(int)sts.TEC]);
                break;
            //case "ATK(Purge)":
            //    StatusScript.DestroyAll(_currTarget);
            //    break;
           //case "ATK(Virus)":
           //    StatusScript.ShareStatus();
           //    break;
           //case "Rush ATK": Move towards opponent
           //    int dis = Mathf.Abs(tileScript.m_x - targetTile.m_x) + Mathf.Abs(tileScript.m_z - targetTile.m_z) - 1;
           //    targetScript.ReceiveDamage(dis.ToString(), Color.white);
           //    PullTowards(this, targetScript.m_tile.GetComponent<TileScript>());
           //    break;
            default:
                break;
        }
    }


    // Health/Damage
    override public void ReceiveDamage(string _dmg, Color _color)
    {
        TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
        m_popupText.SetActive(true);
        textMesh.color = _color;

        if (int.Parse(_dmg) <= 0)
        {
            _dmg = "0";
            m_anim.Play("Block", -1, 0);
        }
        else
        {
            m_anim.Play("Hit", -1, 0);
            m_particles[(int)prtcles.PROJECTILE_HIT].transform.LookAt(m_boardScript.m_currCharScript.transform);
            m_particles[(int)prtcles.PROJECTILE_HIT].SetActive(false);
            m_particles[(int)prtcles.PROJECTILE_HIT].SetActive(true);
        }
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
    }

    public void Dead()
    {
        m_tempStats[(int)sts.HP] = 0;
        for (int i = 0; i < m_turnPanels.Count; i++)
            m_turnPanels[i].GetComponent<Image>().color = new Color(1, .5f, .5f, 1);

        m_isAlive = false;
        m_healthBar.transform.parent.gameObject.SetActive(false);
        m_statusSymbol.SetActive(true);

        StatusScript.DestroyAll(gameObject);
        //GetComponentInChildren<Renderer>().material.color = new Color(.5f, .5f, .5f, 1);
        m_anim.Play("Death", -1, 0);
        
    }

    public void HealHealth(int _hp)
    {
        if (m_effects[(int)StatusScript.effects.SCARRING])
            return;

        if (_hp < 0)
            _hp = 0;

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

        if (this == m_boardScript.m_currCharScript)
        {
            PanelScript actPanScript = PanelScript.GetPanel("HUD Panel LEFT");
            actPanScript.PopulatePanel();
        }
    }

    public void Revive(int _hp)
    {
        if (m_isAlive)
            return;

        HealHealth(_hp);

        for (int i = 0; i < m_turnPanels.Count; i++)
            m_turnPanels[i].GetComponent<Image>().color = m_teamColor;

        m_isAlive = true;
        //m_healthBar.transform.parent.gameObject.SetActive(true);
        m_statusSymbol.SetActive(false);

        //GetComponentInChildren<Renderer>().material.color = Color.white;

    }


    // Ability functions
    static public int UniqueActionProperties(string _action, uniAct _uniAct)
    {
        string actName = DatabaseScript.GetActionData(_action, DatabaseScript.actions.NAME);

        // Only DMG MOD
        if (actName == "ATK(Burst)" || actName == "ATK(Destroy)" || actName == "ATK(Power)" || actName == "ATK(Corrupt)")
        {
            if (_uniAct == uniAct.DMG_MOD)
                return 1;
        }
        // Only SOLO BUFF
        else if (actName == "ATK(Accelerate)" || actName == "ATK(Fortify)" || actName == "ATK(Mod)" ||
            actName == "ATK(Rev)" || actName == "ATK(Salvage)" || actName == "ATK(Target)")
        {
            if (_uniAct == uniAct.SOLO_BUFF)
                return 1;
        }
        // Only RNG MOD
        else if (actName == "ATK(Leak)" || actName == "ATK(Snipe)")
        {
            if (_uniAct == uniAct.RNG_MOD)
                return 1;
        }
        // Only BYPASS
        else if (actName == "ATK(Bypass)" || actName == "ATK(Force)")
        {
            if (_uniAct == uniAct.BYPASS)
                return 1;
        }
        // ONLY TEAM BUFF
        else if (actName == "ATK(Maintain)")
        {
            if (_uniAct == uniAct.TEAM_BUFF)
                return 1;
        }

        else if (actName == "ATK(Blast)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
        }
        else if (actName == "SUP(Channel)")
        {
            if (_uniAct == uniAct.NO_RNG)
                return 1;
        }
        else if (actName == "ATK(Copy)")
        {
            if (_uniAct == uniAct.RNG_MOD)
                return 1;
            else if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (actName == "ATK(Cross)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.DIAGONAL;
            else if (_uniAct == uniAct.DMG_MOD)
                return 1;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (actName == "SUP(Delete)")
        {
            if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (actName == "ATK(Diagonal)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.DIAGONAL;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 0;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (actName == "ATK(Hack)")
        {
            if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (actName == "ATK(Lunge)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.RNG_MOD)
                return 1;
        }
        else if (actName == "ATK(Magnet)")
        {
            if (_uniAct == uniAct.RAD_MOD)
                return 1;
            else if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
        }
        else if (actName == "ATK(Piercing)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 0;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (actName == "ATK(Pull)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.RNG_MOD)
                return 1;
            else if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
        }
        else if (actName == "ATK(Push)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
            else if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (actName == "SUP(Redirect)")
        {
            if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (actName == "ATK(Slash)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 1;
            else if (_uniAct == uniAct.NON_RAD)
                return 1;
            else if (_uniAct == uniAct.TAR_SELF)
                return -1;
        }
        else if (actName == "ATK(Smash)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
        }
        else if (actName == "ATK(Synch)")
        {
            if (_uniAct == uniAct.TEAM_BUFF)
                return 1;
            else if (_uniAct == uniAct.TAR_SELF)
                return -1;
        }
        else if (actName == "ATK(Thrust)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 0;
            else if (_uniAct == uniAct.RNG_MOD)
                return 1;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }

        if (_uniAct == uniAct.TAR_SELF && int.Parse(DatabaseScript.GetActionData(_action, DatabaseScript.actions.RAD)) > 0)
            return 1;

        if (_uniAct == uniAct.IS_NOT_BLOCK && !CheckIfAttack(DatabaseScript.GetActionData(_action, DatabaseScript.actions.NAME)) ||
            _uniAct == uniAct.IS_NOT_BLOCK && int.Parse(DatabaseScript.GetActionData(_action, DatabaseScript.actions.RAD)) > 0)
            return 1;

        return -1;
    }

    private void Knockback(ObjectScript _targetScript, TileScript _away, int _force)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        TileScript nei = null;
    
        while (_force > 0)
        {
            if (targetTileScript.m_x < _away.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.left])
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.left].GetComponent<TileScript>();
            else if (targetTileScript.m_x > _away.m_x && targetTileScript.m_neighbors[(int)TileScript.nbors.right])
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.right].GetComponent<TileScript>();
            else if (targetTileScript.m_z < _away.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.bottom])
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.bottom].GetComponent<TileScript>();
            else if (targetTileScript.m_z > _away.m_z && targetTileScript.m_neighbors[(int)TileScript.nbors.top])
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.top].GetComponent<TileScript>();
            else
                nei = null;

            if (nei && !nei.m_holding || nei && nei.m_holding && nei.m_holding.tag == "PowerUp")
            {
                _targetScript.MovingStart(nei, true, false);
                targetTileScript = nei;
                _force--;
                continue;
            }

            if (_targetScript.gameObject.tag == "Player")
            {
                int extraDMG = Mathf.CeilToInt(_force / 2.0f);
                _targetScript.ReceiveDamage(((extraDMG).ToString()), Color.white);

                if (nei && nei.m_holding && nei.m_holding.tag == "Player")
                {
                    CharacterScript neiCharScript = nei.m_holding.GetComponent<CharacterScript>();
                    neiCharScript.ReceiveDamage(extraDMG.ToString(), Color.white);
                }
            }
            else if (_targetScript.gameObject.tag == "PowerUp" && nei && nei.m_holding && nei.m_holding.tag == "Player")
                _targetScript.MovingStart(nei, true, false);

            break;
        }
    }

    private void PullTowards(ObjectScript _targetScript, TileScript _towards, int _maxDis)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        TileScript nei = null;

        while (_maxDis > 0)
        {
            TileScript[] neighbors = targetTileScript.m_neighbors;
            nei = null;

            for (int i = 0; i < neighbors.Length; i++)
            {
                if (checkTargetsTiles(_targetScript, neighbors[i])) // For magnet attack.
                    {
                        if (i == (int)TileScript.nbors.bottom && targetTileScript.m_z > _towards.m_z ||
                            i == (int)TileScript.nbors.left && targetTileScript.m_x > _towards.m_x ||
                            i == (int)TileScript.nbors.top && targetTileScript.m_z < _towards.m_z ||
                            i == (int)TileScript.nbors.right && targetTileScript.m_x < _towards.m_x)
                        {
                            nei = targetTileScript.m_neighbors[i].GetComponent<TileScript>();
                            break;
                        }
                    }
            }

            if (nei)
            {
                _targetScript.MovingStart(nei, true, false);
                targetTileScript = nei;
                _maxDis--;

                continue;
            }
            break;
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

    public void AlterSpeed(CharacterScript _targetScript, int _speed, bool _isTec)
    {
        int finalVal = _speed;
        if (_isTec)
            finalVal += m_tempStats[(int)sts.TEC];

        if (finalVal == 0)
            return;

        _targetScript.m_tempStats[(int)sts.SPD] += finalVal;

        if (finalVal > 0)
            _targetScript.PlayAnimation(prtcles.GAIN_STATUS, StatusScript.c_buffColor, "Speed Symbol");
        else
            _targetScript.PlayAnimation(prtcles.GAIN_STATUS, StatusScript.c_debuffColor, "Speed Symbol");
    }


    // AI
    public void AITurn()
    {
        if (!m_anim)
            m_anim = GetComponentInChildren<Animator>();

        print(m_name + " started their turn.\n");

        m_boardScript.m_camIsFrozen = true;

        List<AIActionScript> viableActions = new List<AIActionScript>();

        // Loop through all possible targets and decide which one is the best option
        for (int i = 0; i < m_boardScript.m_characters.Count; i++)
        {
            CharacterScript target = m_boardScript.m_characters[i].GetComponent<CharacterScript>();
            if (!target.m_isAlive)
                continue;

            // Find absolute path towards target
            List<TileScript> path = null;
            if (!m_effects[(int)StatusScript.effects.IMMOBILE])
                path = m_tile.AITilePlanning(target.m_tile);
            else
                path = new List<TileScript>();

            print(m_name + " Chose a path to " + target.m_name + "\n");

            // Determine which actions are within range from which tiles along path
            AIActionScript newAct = CheckViableActions(target, path);
            print(m_name + " Chose optimal action to use on " + target.m_name + "\n");
            if (newAct)
                viableActions.Add(newAct);
        }

        AIActionScript mostViable = null;
        for (int i = 0; i < viableActions.Count; i++)
            if (i == 0 || viableActions[i].m_value > mostViable.m_value)
                mostViable = viableActions[i];

        if (mostViable && mostViable.m_value > 0)
            print(m_name + " decided that attacking " + mostViable.m_targets[0].name + " with " + 
                DatabaseScript.GetActionData(mostViable.m_action, DatabaseScript.actions.NAME) + " is most viable. \n");
        else
            print(m_name + " decided that moving is most viable. \n");

        if (mostViable && mostViable.m_value > 0)
        {
            m_targets = mostViable.m_targets;
            m_currAction = mostViable.m_action;
            if (mostViable.m_position)
                MovingStart(mostViable.m_position, false, false);
            else
            {
                m_boardScript.m_selected = m_targets[0].GetComponent<CharacterScript>().m_tile;
                transform.LookAt(m_boardScript.m_selected.transform);
                if (TileScript.CaclulateDistance(m_tile, m_boardScript.m_selected) > 1)
                    m_anim.Play("Ranged", -1, 0);
                else
                    m_anim.Play("Melee", -1, 0);

                m_hasActed[0] = 3;
                m_hasActed[1] = 3;
            }
        }
        else
        {
            m_currAction = "";
            if (mostViable)
                MovingStart(mostViable.m_position, false, false);
            else
                m_boardScript.m_camIsFrozen = false;
            m_hasActed[0] = 3;
            m_hasActed[1] = 3;
        }

        for (int i = 0; i < viableActions.Count; i++)
            Destroy(viableActions[i]);

        print(m_name + " has exited AITurn.\n");

        // check all targets
        // value will be based on distance if outside of range after moving
        // if within range of an attack before or after moving, value is based on end result of best potential action on target
    }

    public AIActionScript CheckViableActions(CharacterScript _target, List<TileScript> _path)
    {
        List<AIActionScript> viableActs = new List<AIActionScript>();
        TileScript newTile = null;

        for (int i = 0; i < m_actions.Length; i++)
        {
            string name = DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.NAME);
            string eng = DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.ENERGY);

            m_currAction = m_actions[i];
            bool found = false;

            if (m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng) && eng.Length == 2 ||
                m_effects[(int)StatusScript.effects.HINDER] && !CheckIfAttack(name) ||
                m_effects[(int)StatusScript.effects.WARD] && CheckIfAttack(name) ||
                m_isDiabled[i] > 0)
                continue;
            else if (_target != this)
            {
                for (int j = 0; j < _path.Count; j++)
                {
                    if (j > m_tempStats[(int)sts.MOV])
                        break;

                    ActionTargeting(_path[j]);

                    for (int k = 0; k < _path[j].m_radius.Count; k++)
                    {
                        if (_path[j].m_radius[k].m_holding && _path[j].m_radius[k].m_holding == _target.gameObject)
                        {
                            found = true;
                            newTile = _path[j];
                            break;
                        }
                    }
                    _path[j].ClearRadius();
                    if (found)
                        break;
                }
                if (_path.Count == 0)
                {
                    ActionTargeting(m_tile);

                    for (int k = 0; k < m_tile.m_radius.Count; k++)
                    {
                        if (m_tile.m_radius[k].m_holding && m_tile.m_radius[k].m_holding == _target.gameObject)
                        {
                            found = true;
                            break;
                        }
                    }
                    m_tile.ClearRadius();
                }
            }

            if (found && m_player.CheckEnergy(eng))
            {
                if (CheckIfAttack(name) && _target.m_player == m_player || !CheckIfAttack(name) && _target != m_player)
                    continue;
            
                AIActionScript newAct = gameObject.AddComponent<AIActionScript>();
                if (newTile)
                    newAct.m_position = newTile;
                else
                    newAct.m_position = null;
            
                newAct.m_targets = new List<GameObject>();
                newAct.CalculateValue(m_actions[i], this, _target);
                viableActs.Add(newAct);
            }
        }

        AIActionScript mostViable = null;

        if (viableActs.Count > 0) // Single out best viable action with current target in mind
        {
            for (int i = 0; i < viableActs.Count; i++)
                if (i == 0 || viableActs[i].m_value > mostViable.m_value)
                    mostViable = viableActs[i];

            // Get rid of other options
            for (int i = 0; i < viableActs.Count; i++)
                if (viableActs[i] != mostViable)
                    Destroy(viableActs[i]);
        }
        else if (_target.m_player != m_player && _path.Count > 1) // If no targets are available, move towards primary target
        {
            mostViable = gameObject.AddComponent<AIActionScript>();

            if (_path.Count > m_tempStats[(int)sts.MOV] - 1) // If target is outside of maximum movement, move as much as possible
                mostViable.m_position = _path[m_tempStats[(int)sts.MOV] - 1];
            else // If you can reach target, move to target's adjacent space
                mostViable.m_position = _path[_path.Count - 1];

            mostViable.m_value = -TileScript.CaclulateDistance(mostViable.m_position, _target.m_tile);
        }
        else
            return null;

        return mostViable;
    }


    // Network
    public bool CheckIfMine()
    {
        if (!m_boardScript || !GameObject.Find("Network") || 
            m_boardScript && GameObject.Find("Network") && GameObject.Find("Network").GetComponent<ClientScript>().m_connectionId == m_boardScript.m_currCharScript.m_player.m_id)
            return true;
        else
            return false;
    }


    // Utilities
    public void HighlightCharacter()
    {
        if (PanelScript.GetPanel("HUD Panel RIGHT").m_inView)
            return;

        if (this != m_boardScript.m_currCharScript && m_isAlive)
        {
            // Change color of turn panel to indicate where the character is in the turn order
            for (int i = 0; i < m_turnPanels.Count; i++)
            {
                Image turnPanImage = m_turnPanels[i].GetComponent<Image>();
                turnPanImage.color = Color.cyan;
            }

            // Reveal right HUD with highlighted character's data
            PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_cScript = this;
            hudPanScript.PopulatePanel();
        }
    }

    public void SelectorInit(CharacterScript _targetScript, string _pan)
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        PanelScript selector = PanelScript.GetPanel("Status Selector");
        if (_pan == "Status Selector")
        {
            selector = PanelScript.GetPanel("Status Selector");
            if (actName == "SUP(Delete)")
                selector.m_text[0].text = "Choose a status to remove";
            if (actName == "SUP(Extension)" || actName == "Modification")
                selector.m_text[0].text = "Choose a status to boost";
        }
        else if (_pan == "Energy Selector")
        {
            selector = PanelScript.GetPanel("Energy Selector");

            if (actName == "ATK(Syphon)" || actName == "ATK(Deplete)")
            {
                if (actName == "ATK(Syphon)")
                    selector.m_text[0].text = "Choose energy to steal";
                else if (actName == "ATK(Deplete)")
                    selector.m_text[0].text = "Choose energy to remove";

                for (int i = 0; i < selector.m_images.Length; i++)
                {
                    Text t = selector.m_images[i].GetComponentInChildren<Text>();
                    t.text = _targetScript.m_player.m_energy[i].ToString();
                }
            }
            else if (actName == "ATK(Prismatic)") // if channel
            {
                selector.m_text[0].text = "Choose energy to gain";
                for (int i = 0; i < selector.m_images.Length; i++)
                    selector.m_images[i].GetComponentInChildren<Text>().text = "0";
            }
        }

        selector.m_cScript = _targetScript;
        selector.PopulatePanel();

        m_tile.GetComponent<TileScript>().ClearRadius();
        m_boardScript.m_isForcedMove = gameObject;
    }

    static public bool CheckIfAttack(string _actName)
    {
        if (_actName[0] == 'A' && _actName[1] == 'T' && _actName[2] == 'K')
            return true;
        else
            return false;
    }
    
    // The only reason why this exists is because statusscript needs a way to get to HUD LEFT
    public void UpdateStatusImages()
    {
        if (this == m_boardScript.m_currCharScript)
            PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();
    }

    private void EnergyConversion(string _energy)
    {
        // Update player's energy after using an action

        // Assign _energy symbols
        for (int i = 0; i < _energy.Length; i++)
        {
            if (_energy[i] == 'g')
                m_player.m_energy[0] += 1;
            else if (_energy[i] == 'r')
                m_player.m_energy[1] += 1;
            else if (_energy[i] == 'w')
                m_player.m_energy[2] += 1;
            else if (_energy[i] == 'b')
                m_player.m_energy[3] += 1;
            else if (_energy[i] == 'G')
                m_player.m_energy[0] -= 1;
            else if (_energy[i] == 'R')
                m_player.m_energy[1] -= 1;
            else if (_energy[i] == 'W')
                m_player.m_energy[2] -= 1;
            else if (_energy[i] == 'B')
                m_player.m_energy[3] -= 1;
        }

        SetPopupSpheres(_energy);
        m_player.SetEnergyPanel(this);
    }

    static public int CheckActionLevel(string _actEng)
    {
        if (_actEng[0] == 'g' || _actEng[0] == 'r' || _actEng[0] == 'w' || _actEng[0] == 'b')
            return 0;

        return _actEng.Length;
    }
    
    public int CheckActionColor(string _act)
    {
        string _eng = DatabaseScript.GetActionData(_act, DatabaseScript.actions.ENERGY);

        if (_eng[0] == 'g' || _eng[0] == 'G')
            return 0;
        else if (_eng[0] == 'r' || _eng[0] == 'R')
            return 1;
        else if (_eng[0] == 'w' || _eng[0] == 'W')
            return 2;
        else if (_eng[0] == 'b' || _eng[0] == 'B')
            return 3;

        return -1;
    }

    public void SortActions()
    {
        for (int i = 0; i < m_actions.Length; i++)
        {
            int currEng = ConvertedCost(DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.ENERGY));

            for (int j = i + 1; j < m_actions.Length; j++)
            {
                int indexedEng = ConvertedCost(DatabaseScript.GetActionData(m_actions[j], DatabaseScript.actions.ENERGY));

                if (currEng > indexedEng ||
                    currEng == indexedEng && CheckActionColor(m_actions[i]) > CheckActionColor(m_actions[j]))
                {
                    string temp = m_actions[i];
                    m_actions[i] = m_actions[j];
                    m_actions[j] = temp;
                }
            }
        }
    }

    static public int ConvertedCost(string _eng)
    {
        if (PlayerScript.CheckIfGains(_eng))
            return -_eng.Length;
        else
            return _eng.Length;
    }

    public void SetPopupSpheres(string _energy)
    {
        GameObject[] colorSpheres = m_popupSpheres;
        if (_energy.Length == 0)
        {
            if (m_color == "")
                return;
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

        SphereCollider[] orbs = spheres.GetComponentsInChildren<SphereCollider>();
        int j = 0;

        for (int i = 0; i < orbs.Length; i++)
        {
            Renderer orbRend = orbs[i].GetComponent<Renderer>();

            
            if (orbs[i].name == "Sphere Outline")
                orbRend.material.color = new Color(0, 0, 0, 1);
            else
            {
                if (_energy[j] == 'g' || _energy[j] == 'G')
                    color = c_green;
                else if (_energy[j] == 'r' || _energy[j] == 'R')
                    color = c_red;
                else if (_energy[j] == 'w' || _energy[j] == 'W')
                    color = c_white;
                else if (_energy[j] == 'b' || _energy[j] == 'B')
                    color = c_blue;
                
                orbRend.material.color = color;
                j++;
            }

        }
    }

    public bool checkTargetsTiles(ObjectScript _target, TileScript _tile)
    {
        if (_tile == null)
            return false;

        for (int i = 0; i < m_targets.Count; i++)
        {
            TileScript tScript = m_targets[i].GetComponent<ObjectScript>().m_tile;
            if (_tile.m_holding && _tile.m_holding.tag == "Environment" ||
                _tile == tScript && m_targets[i].tag == _target.gameObject.tag ||
                _tile == m_tile && gameObject.tag == _target.gameObject.tag)
                return false;
        }

        return true;
    }

    public void RetrieveActions()
    {
        DatabaseScript db = m_boardScript.GetComponent<DatabaseScript>();
        for (int i = 0; i < m_actions.Length; i++)
        {
            if (m_actions[i].Length > 20)
                continue;
            for (int j = 0; j < db.m_actions.Length; j++)
                if (m_actions[i] == DatabaseScript.GetActionData(db.m_actions[j], DatabaseScript.actions.NAME))
                {
                    m_actions[i] = db.m_actions[j];
                    break;
                }
        }
    }

    public void PlayAnimation(prtcles _ani, Color _color, string _sprite)
    {
        if (_ani == prtcles.GAIN_STATUS)
        {
            m_particles[(int)prtcles.GAIN_STATUS].SetActive(false);
            m_particles[(int)prtcles.GAIN_STATUS].SetActive(true);
            ParticleSystem[] ps = m_particles[(int)prtcles.GAIN_STATUS].GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < ps.Length; i++)
                ps[i].startColor = _color;

            Material[] mats = Resources.LoadAll<Material>("Symbols/Materials");

            for (int i = 0; i < mats.Length; i++)
                if (mats[i].name == _sprite)
                {
                    ps[2].GetComponent<ParticleSystemRenderer>().material = mats[i];
                    break;
                }
        }
    }

    public int ActFinalDistAfterMods(string _action)
    {
        int finalRng = int.Parse(DatabaseScript.GetActionData(_action, DatabaseScript.actions.RNG)) + m_tempStats[(int)sts.RNG];

        if (UniqueActionProperties(_action, uniAct.RNG_MOD) >= 1)
            finalRng += m_tempStats[(int)sts.TEC];

        finalRng += int.Parse(DatabaseScript.GetActionData(_action, DatabaseScript.actions.RAD)) + m_tempStats[(int)sts.RAD];

        if ((TileScript.targetRestriction)UniqueActionProperties(_action, uniAct.TAR_RES) == TileScript.targetRestriction.DIAGONAL)
            finalRng *= 2;

        if (finalRng < 1)
            finalRng = 1;

        return finalRng;
    }

    static public bool IsWithinRange(CharacterScript _char, string _action, TileScript _origin, TileScript _target)
    {
        int finalRng = _char.ActFinalDistAfterMods(_action);

        if (finalRng < TileScript.CaclulateDistance(_origin, _target) || _origin.CheckIfBlocked(_target) &&
                UniqueActionProperties(_action, uniAct.IS_NOT_BLOCK) < 1 ||
                (TileScript.targetRestriction)UniqueActionProperties(_action, uniAct.TAR_RES) == TileScript.targetRestriction.HORVERT && _origin.m_x != _target.m_x && _origin.m_z != _target.m_z ||
                (TileScript.targetRestriction)UniqueActionProperties(_action, uniAct.TAR_RES) == TileScript.targetRestriction.DIAGONAL && Mathf.Abs(_origin.m_x - _target.m_x) - Mathf.Abs(_origin.m_z - _target.m_z) != 0)
            return false;

        return true;
    }

    //public void SparkRandomly()
    //{
    //    int randPart = Random.Range(0, m_body.Count);
    //    m_particle.transform.SetPositionAndRotation(m_body[randPart].transform.position, m_particle.transform.rotation);
    //}
}
