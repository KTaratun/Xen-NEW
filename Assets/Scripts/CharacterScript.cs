using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : ObjectScript {

    public enum weaps { SWORD, HGUN };
    public enum sts { HP, SPD, DMG, DEF, MOV, RNG, TEC, RAD, TOT };
    public enum trn { MOV, ACT };
    public enum HUDPan { CHAR_PORT, ACT_PAN, MOV_PASS, ENG_PAN };
    public enum uniAct { TAR_RES, IS_NOT_BLOCK, TAR_SELF, RNG_MOD, NO_RNG, // TARGETING
        SOLO_BUFF, TEAM_BUFF, BYPASS, DMG_MOD, // ATTACK
        RAD_MOD, NON_RAD} // RADIUS

    static public Color c_green = new Color(.45f, .7f, .4f, 1);
    static public Color c_red = new Color(.8f, .1f, .15f, 1);
    static public Color c_white = new Color(.8f, .8f, .8f, 1);
    static public Color c_blue = new Color(.45f, .4f, 1, 1);

    public List<GameObject> m_turnPanels;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject m_statusSymbol;
    public GameObject[] m_popupSpheres;
    public GameObject[] m_colorDisplay;
    public PlayerScript m_player;
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
    public int m_gender;
    public GameObject[] m_weapons;
    public List<GameObject> m_targets;
    public AudioSource m_audio;
    public int[] m_hasActed;

    // Use this for initialization
    void Start ()
    {
        if (m_hasActed.Length == 0)
            m_hasActed = new int[2];

        m_audio = gameObject.AddComponent<AudioSource>();

        if (!m_anim)
            m_anim = GetComponentInChildren<Animator>();
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

        if (m_boardScript && m_boardScript.m_currCharScript != this)
        {
            Renderer rend = transform.GetComponentInChildren<Renderer>();
            rend.materials[1].shader = rend.materials[0].shader;
            rend.materials[2].shader = rend.materials[0].shader;
        }
    }

    public void InitializeStats()
    {
        if (m_stats.Length == 0)
        {
            m_stats = new int[(int)sts.TOT];
            m_tempStats = new int[(int)sts.TOT];
        }

        m_accessories = new string[2];
        m_accessories[0] = "Ring of DEATH";

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
    new void Update()
    {
        base.Update();
        if (m_boardScript)
        {
            //MovementUpdate();
            UpdateOverheadItems();

            if (m_boardScript.m_currCharScript == this && Input.GetKeyDown(KeyCode.P))
                AITurn();
        }
    }

    private void UpdateOverheadItems()
    {
        // Update Health bar
        if (m_isAlive) //m_healthBar.activeSelf
        {
            Transform outline = m_healthBar.transform.parent;

            outline.LookAt(2 * outline.position - m_boardScript.m_camera.transform.position);
            m_healthBar.transform.LookAt(2 * m_healthBar.transform.position - m_boardScript.m_camera.transform.position);
            float ratio = (float)(m_stats[(int)sts.HP] - (float)m_tempStats[(int)sts.HP]) / (float)m_stats[(int)sts.HP];

            Renderer hpRend = m_healthBar.GetComponent<Renderer>();
            hpRend.material.color = new Color(ratio + 0.2f, 1 - ratio + 0.2f, 0.2f, 1);
            m_healthBar.transform.localScale = new Vector3(0.95f - ratio, m_healthBar.transform.localScale.y, m_healthBar.transform.localScale.z);
        }
        else
            //m_healthBar.GetComponent<Renderer>().material.color = Color.black;
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
    }


    // Movement
    public void MovementSelection(int _forceMove)
    {
        int move = 0;

        if (_forceMove > 0)
            move = _forceMove;
        else
            move = m_tempStats[(int)sts.MOV];

        m_tile.FetchTilesWithinRange(move, new Color (0, 0, 1, 0.5f), false, TileScript.targetRestriction.NONE, false);
    }

    public void CharStartMoving(TileScript newScript, bool _isForced)
    {
        if (m_isAlive)
            m_anim.Play("Running Melee", -1, 0);

        if (!_isForced)
        {
            m_hasActed[(int)trn.MOV] += 2;
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[(int)trn.MOV].interactable = false;
            if (m_boardScript.m_currButton)
                m_boardScript.m_currButton.GetComponent<Image>().color = Color.white;
        }

        PanelScript.CloseHistory();
        print(m_name + " has started moving.\n");
    }

    public void CharFinishMoving()
    {
        if (m_tile.m_holding && m_tile.m_holding.tag == "PowerUp")
            m_tile.m_holding.GetComponent<PowerupScript>().OnPickup(this);

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
        else if (m_isAI && m_currAction.Length > 0 || m_isAI && m_targets.Count > 0)
            return;

        print(m_name + " has finished moving.\n");
    }


    // Action
    public void ActionTargeting(TileScript _tile)
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actRng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RNG);
        string actRad = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RAD);
        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;

        // RANGE MODS

        // If the action only targets horizontal and vertical or diagonal
        targetingRestriction = TileScript.targetRestriction.NONE;
        if (UniqueActionProperties(m_currAction, uniAct.TAR_RES) >= 0)
            targetingRestriction = (TileScript.targetRestriction)UniqueActionProperties(m_currAction, uniAct.TAR_RES);

        // If the action cannot be blocked
        bool isBlockable = true;
        if (UniqueActionProperties(m_currAction, uniAct.IS_NOT_BLOCK) >= 0)
            isBlockable = false;

        m_currRadius = int.Parse(actRad);
        bool targetSelf = false;

        // If the action can target the source
        if (m_currRadius > 0 && UniqueActionProperties(m_currAction, uniAct.TAR_SELF) == -1 || !CheckIfAttack(actName)) //|| !CheckIfAttack(actName) && actName != "Protect")
            targetSelf = true;

        int finalRNG = int.Parse(actRng) + m_tempStats[(int)sts.RNG];
        
        if (UniqueActionProperties(m_currAction, uniAct.RNG_MOD) >= 0)
            finalRNG += m_tempStats[(int)sts.TEC];

        if (finalRNG < 1)
            finalRNG = 1;

        if (UniqueActionProperties(m_currAction, uniAct.NO_RNG) >= 0)
            finalRNG = 0;

        // See if it's an attack or not
        if (CheckIfAttack(actName))
            _tile.FetchTilesWithinRange(finalRNG, TileScript.c_attack, targetSelf, targetingRestriction, isBlockable);
        else
            _tile.FetchTilesWithinRange(finalRNG, TileScript.c_action, targetSelf, targetingRestriction, isBlockable);
    }

    public void ActionAnimation()
    {
        transform.LookAt(m_boardScript.m_selected.transform);

        string actRng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.RNG);
        Renderer r = m_boardScript.m_selected.GetComponent<Renderer>();
        TileScript selectedTileScript = m_boardScript.m_selected;

        if (r.material.color == Color.red
            || r.material.color == Color.green) // single
        {
            if (r.material.color == Color.green)
                m_anim.Play("Ability", -1, 0);
            else if (int.Parse(actRng) + m_tempStats[(int)sts.RNG] > 1)
                m_anim.Play("Ranged", -1, 0);
            else
                m_anim.Play("Melee", -1, 0);

            m_targets.Add(selectedTileScript.m_holding);
        }
        else if (r.material.color == Color.yellow) // multi
        {
            for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
            {
                TileScript tarTile = selectedTileScript.m_targetRadius[i].GetComponent<TileScript>();
                if (tarTile.m_holding && tarTile.m_holding.tag == "Player")
                    m_targets.Add(tarTile.m_holding);
            }
            m_anim.Play("Throw", -1, 0);
        }

        m_boardScript.m_camIsFrozen = true;
        PanelScript.GetPanel("ActionViewer Panel").m_panels[1].m_inView = false;
        PanelScript.CloseHistory();
        m_currRadius = 0;

        if (m_targets.Count > 0)
        {
            if (selectedTileScript.m_targetRadius.Count > 0)
                selectedTileScript.ClearRadius();

            if (!m_boardScript.m_isForcedMove)
                m_tile.GetComponent<TileScript>().ClearRadius();
        }
    }

    public void Action()
    {
        m_boardScript.m_actionEndTimer += Time.deltaTime;

        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actEng = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.ENERGY);

        if (CheckIfAttack(actName) && m_targets.Count > 0) // See if it's an attack
            Attack();
        else // If it's not an attack
        {
            if (m_targets.Count > 0)
            {
                for (int i = 0; i < m_targets.Count; i++)
                    Ability(m_targets[i], actName);
            }
            else    // && !m_targets[0].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.REFLECT])
                Ability(gameObject, actName);

            if (actName != "SUP(Redirect)")
                m_hasActed[(int)trn.ACT] += 2;
        }

        if (!m_isFree)
            EnergyConversion(actEng);
        else
            PanelScript.GetPanel("Choose Panel").m_inView = false;

        m_exp += actEng.Length;

        m_isFree = false;
        m_targets.Clear();

        UpdateStatusImages();

        PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();
        PanelScript.GetPanel("HUD Panel RIGHT").PopulatePanel();

        PanelScript.GetPanel("ActionViewer Panel").m_inView = false;
        if (!PanelScript.GetPanel("Choose Panel").m_inView)
            m_boardScript.m_selected = null;
        m_boardScript.m_currButton = null;

        print(m_name + " has finished acting.");
    }

    public void Attack()
    {
        string actName = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.NAME);
        string actDmg = DatabaseScript.GetActionData(m_currAction, DatabaseScript.actions.DMG);

        // ATTACK MODS
        bool teamBuff = false;
        bool soloBuff = false;
        if (UniqueActionProperties(m_currAction, uniAct.TEAM_BUFF) >= 0)// || actName == "Cleansing ATK")
            teamBuff = true;
        else if (UniqueActionProperties(m_currAction, uniAct.SOLO_BUFF) >= 0)
            soloBuff = true;

        for (int i = 0; i < m_targets.Count; i++)
        {
            CharacterScript targetScript = m_targets[i].GetComponent<CharacterScript>();

            if (targetScript.m_isAlive == false)
                continue;

            string finalDMG = "0";

            if (!teamBuff || teamBuff && targetScript.m_player != m_player)
            {
                // DAMAGE MODS
                if (UniqueActionProperties(m_currAction, uniAct.BYPASS) >= 0)
                {
                    int def = targetScript.m_tempStats[(int)sts.DEF] - UniqueActionProperties(m_currAction, uniAct.BYPASS) + m_tempStats[(int)sts.TEC];
                    if (def < 0)
                        def = 0;
                    finalDMG = (int.Parse(actDmg) + m_tempStats[(int)sts.DMG] - def).ToString();
                }
                else if (int.Parse(actDmg) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF] >= 0)
                    finalDMG = (int.Parse(actDmg) + m_tempStats[(int)sts.DMG] - targetScript.m_tempStats[(int)sts.DEF]).ToString();

                if (UniqueActionProperties(m_currAction, uniAct.DMG_MOD) >= 0)
                    finalDMG = (int.Parse(finalDMG) + m_tempStats[(int)sts.TEC]).ToString();

                targetScript.ReceiveDamage(finalDMG, Color.white);
            }

            if (!targetScript.m_effects[(int)StatusScript.effects.REFLECT])
                if (!teamBuff  && !soloBuff || teamBuff && targetScript.m_player == m_player && !soloBuff)
                    Ability(m_targets[i], actName);

        }
        if (teamBuff || soloBuff)
            Ability(gameObject, actName);

        // REFACTOR
        if (!m_isFree)
            m_hasActed[(int)trn.ACT] += 3;

        if (m_targets.Count > 1)
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_boardScript.m_selected.gameObject;
        else
            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_targets[0];
    }

    public void Ability(GameObject _currTarget, string _name)
    {
        CharacterScript targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript targetTile = targetScript.m_tile.GetComponent<TileScript>();
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
                if (!m_isAI)
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
                if (m_tempStats[(int)sts.TEC] > 0)
                {
                    if (m_player.TotalEnergy() >= m_tempStats[(int)sts.TEC])
                    {
                        HealHealth(2 + m_tempStats[(int)sts.TEC]);
                        m_player.RemoveRandomEnergy(m_tempStats[(int)sts.TEC]);
                    }
                    else
                    {
                        HealHealth(2 + m_tempStats[targetScript.m_player.TotalEnergy()]);
                        m_player.RemoveRandomEnergy(targetScript.m_player.TotalEnergy());
                    }
                }
                else
                    targetScript.HealHealth(2);
                break;
            case "SUP(Restore)":
                if (m_tempStats[(int)sts.TEC] > 0)
                {
                    if (targetScript.m_player.TotalEnergy() >= m_tempStats[(int)sts.TEC])
                    {
                        targetScript.HealHealth(3 + m_tempStats[(int)sts.TEC]);
                        targetScript.m_player.RemoveRandomEnergy(m_tempStats[(int)sts.TEC]);
                    }
                    else
                    {
                        targetScript.HealHealth(3 + m_tempStats[targetScript.m_player.TotalEnergy()]);
                        targetScript.m_player.RemoveRandomEnergy(targetScript.m_player.TotalEnergy());
                    }
                }
                else
                    targetScript.HealHealth(3);
                break;
            // Unique abilities
            case "ATK(Blast)":
                Knockback(targetScript, m_boardScript.m_selected, 2 + m_tempStats[(int)sts.TEC]);
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
                targetScript.m_tempStats[(int)sts.SPD] -= 2 + m_tempStats[(int)sts.TEC];
                break;
            case "ATK(Synch)":
                targetScript.m_tempStats[(int)sts.SPD] += 2 + m_tempStats[(int)sts.TEC];
                break;
            case "ATK(Dash)":
                if (TileScript.CheckForEmptyNeighbor(tileScript) && !m_isAI)
                {
                    tileScript.ClearRadius();
                    m_boardScript.m_isForcedMove = gameObject;
                    MovementSelection(3 + m_tempStats[(int)sts.TEC]);
                }
                break;
            case "ATK(Leak)":
                targetScript.m_player.RemoveRandomEnergy(1);
                break;
            case "ATK(Overclock)":
                m_tempStats[(int)sts.SPD] += 3 + m_tempStats[(int)sts.TEC];
                break;
            case "ATK(Lunge)":
                PullTowards(this, targetScript.m_tile, 100);
                break;
            case "ATK(Magnet)":
                PullTowards(targetScript, m_boardScript.m_selected, 100);
                break;
            case "ATK(Pull)":
                PullTowards(targetScript, tileScript, 100);
                break;
            case "ATK(Push)":
                if (TileScript.CheckForEmptyNeighbor(targetTile) && !m_isAI)
                {
                    tileScript.ClearRadius();
                    m_boardScript.m_isForcedMove = _currTarget;
                    targetScript.MovementSelection(3 + m_tempStats[(int)sts.TEC]);
                }
                break;
            case "SUP(Reboot)":
                targetScript.Revive(5 + m_tempStats[(int)sts.TEC]);
                break;
            case "ATK(Smash)":
                Knockback(targetScript, tileScript, 3 + m_tempStats[(int)sts.TEC]);
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
        if (int.TryParse(_dmg, out res) && int.Parse(_dmg) > -1)
            textMesh.text = (int.Parse(textMesh.text) + int.Parse(_dmg)).ToString();
        else
            textMesh.text = _dmg;
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
        m_healthBar.transform.parent.gameObject.SetActive(true);
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
        else if (actName == "ATK(Accelerate)" || actName == "ATK(Fortify)" || actName == "ATK(Rev)" || 
            actName == "ATK(Salvage)" || actName == "ATK(Target)")
        {
            if (_uniAct == uniAct.SOLO_BUFF)
                return 1;
        }
        // Only RNG MOD
        else if (actName == "ATK(Copy)" || actName == "ATK(Leak)" || actName == "ATK(Snipe)")
        {
            if (_uniAct == uniAct.RNG_MOD)
                return 1;
        }
        // Only HOR RNG MOD
        else if (actName == "ATK(Lunge)" || actName == "ATK(Pull)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.RNG_MOD)
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

        else if (actName == "SUP(Channel)")
        {
            if (_uniAct == uniAct.NO_RNG)
                return 1;
        }
        else if (actName == "ATK(Synch)")
        {
            if (_uniAct == uniAct.TEAM_BUFF)
                return 1;
            else if (_uniAct == uniAct.TAR_SELF)
                return 0;
        }
        else if (actName == "ATK(Diagonal)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.DIAGONAL;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 0;
            if (_uniAct == uniAct.TAR_SELF)
                return 0;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (actName == "ATK(Cross)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.DIAGONAL;
            else if (_uniAct == uniAct.DMG_MOD)
                return 1;
            if (_uniAct == uniAct.TAR_SELF)
                return 0;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (actName == "ATK(Magnet)")
        {
            if (_uniAct == uniAct.RAD_MOD)
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
        else if (actName == "ATK(Slash)")
        {
            if (_uniAct == uniAct.TAR_SELF)
                return 0;
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 1;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
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

        return -1;
    }

    private void Knockback(CharacterScript _targetScript, TileScript _away, int _force)
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
                _targetScript.StartMoving(nei, true);
                targetTileScript = nei;
                _force--;
                continue;
            }

            int extraDMG = Mathf.CeilToInt(_force / 2.0f);
            _targetScript.ReceiveDamage(((extraDMG).ToString()), Color.white);

            if (nei && nei.m_holding && nei.m_holding.tag == "Player")
            {
                CharacterScript neiCharScript = nei.m_holding.GetComponent<CharacterScript>();
                neiCharScript.ReceiveDamage(extraDMG.ToString(), Color.white);
            }

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
            if (targetTileScript.m_x < _towards.m_x && neighbors[(int)TileScript.nbors.right] && !neighbors[(int)TileScript.nbors.right].m_holding && !neighbors[(int)TileScript.nbors.right].m_traversed ||
                targetTileScript.m_x < _towards.m_x && neighbors[(int)TileScript.nbors.right] && neighbors[(int)TileScript.nbors.right].m_holding && neighbors[(int)TileScript.nbors.right].m_holding.tag == "PowerUp" && !neighbors[(int)TileScript.nbors.right].m_traversed)
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.right].GetComponent<TileScript>();
            else if (targetTileScript.m_x > _towards.m_x && neighbors[(int)TileScript.nbors.left] && !neighbors[(int)TileScript.nbors.left].m_holding && !neighbors[(int)TileScript.nbors.left].m_traversed ||
                targetTileScript.m_x > _towards.m_x && neighbors[(int)TileScript.nbors.left] && neighbors[(int)TileScript.nbors.left].m_holding && neighbors[(int)TileScript.nbors.left].m_holding.tag == "PowerUp" && !neighbors[(int)TileScript.nbors.left].m_traversed)
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.left].GetComponent<TileScript>();
            else if (targetTileScript.m_z < _towards.m_z && neighbors[(int)TileScript.nbors.top] && !neighbors[(int)TileScript.nbors.top].m_holding && !neighbors[(int)TileScript.nbors.top].m_traversed ||
                targetTileScript.m_z < _towards.m_z && neighbors[(int)TileScript.nbors.top] && neighbors[(int)TileScript.nbors.top].m_holding && neighbors[(int)TileScript.nbors.top].m_holding.tag == "PowerUp" && !neighbors[(int)TileScript.nbors.top].m_traversed)
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.top].GetComponent<TileScript>();
            else if (targetTileScript.m_z > _towards.m_z && neighbors[(int)TileScript.nbors.bottom] && !neighbors[(int)TileScript.nbors.bottom].m_holding && !neighbors[(int)TileScript.nbors.bottom].m_traversed ||
                targetTileScript.m_z > _towards.m_z && neighbors[(int)TileScript.nbors.bottom] && neighbors[(int)TileScript.nbors.bottom].m_holding && neighbors[(int)TileScript.nbors.bottom].m_holding.tag == "PowerUp" && !neighbors[(int)TileScript.nbors.bottom].m_traversed)
                nei = targetTileScript.m_neighbors[(int)TileScript.nbors.bottom].GetComponent<TileScript>();
            else
                nei = null;

            if (nei)
            {
                _targetScript.StartMoving(nei, true);
                targetTileScript = nei;
                _maxDis--;
                if (targetTileScript == _towards)
                    _towards.m_traversed = true;

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


    // AI
    public void AITurn()
    {
        if (!m_anim)
            m_anim = GetComponentInChildren<Animator>();

        print(m_name + " started their turn.\n");

        m_boardScript.m_camIsFrozen = true;

        List<AIActionScript> viableActions = new List<AIActionScript>();
        for (int i = 0; i < m_boardScript.m_characters.Count; i++)
        {
            CharacterScript target = m_boardScript.m_characters[i].GetComponent<CharacterScript>();
            if (!target.m_isAlive)
                continue;

            List<TileScript> path = null;
            if (!m_effects[(int)StatusScript.effects.IMMOBILE])
                path = m_tile.AITilePlanning(target.m_tile);
            else
                path = new List<TileScript>();

            print(m_name + " Chose a path to " + target.m_name + "\n");

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
                StartMoving(mostViable.m_position, false);
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
                StartMoving(mostViable.m_position, false);
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

        if (viableActs.Count > 0)
        {
            for (int i = 0; i < viableActs.Count; i++)
                if (i == 0 || viableActs[i].m_value > mostViable.m_value)
                    mostViable = viableActs[i];

            for (int i = 0; i < viableActs.Count; i++)
            {
                if (viableActs[i] != mostViable)
                    Destroy(viableActs[i]);
            }
        }
        else if (_target.m_player != m_player && _path.Count > 1)
        {
            mostViable = gameObject.AddComponent<AIActionScript>();
            if (_path.Count > m_tempStats[(int)sts.MOV] - 1)
                mostViable.m_position = _path[m_tempStats[(int)sts.MOV] - 1];
            else
                mostViable.m_position = _path[_path.Count - 1];

            mostViable.m_value = -TileScript.CaclulateDistance(mostViable.m_position, _target.m_tile);
        }
        else
            return null;

        return mostViable;
    }


    // Utilities
    private void OnMouseDown()
    {
        m_tile.OnMouseDown();
    }

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

    static public bool CheckIfAttack(string _action)
    {
        if (_action[0] == 'A' && _action[1] == 'T' && _action[2] == 'K')
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
        // Gain or use energy

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

    static public int CheckActionLevel(string _action)
    {
        if (_action[0] == 'g' || _action[0] == 'r' || _action[0] == 'w' || _action[0] == 'b')
            return 0;

        return _action.Length;
    }

    public void SortActions()
    {
        for (int i = 0; i < m_actions.Length; i++)
        {
            int currEng = ConvertedCost(DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.ENERGY));

            for (int j = i + 1; j < m_actions.Length; j++)
            {
                int indexedEng = ConvertedCost(DatabaseScript.GetActionData(m_actions[j], DatabaseScript.actions.ENERGY));

                if (currEng > indexedEng)
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
}
