using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.UI;

public class ActionScript : MonoBehaviour {

    public enum uniAct
    {
        // TARGETING
        TAR_RES, // Used to lock range either horizontally, vertically or diagnally
        IS_NOT_BLOCK, // Targeting will not be affected by obstacles
        TAR_SELF, // Specifies whether or not you can target your own space
        RNG_MOD, // Tech is added to range
        NO_RNG, // Range is not added
        SOLO_BUFF, // The action's ability targets the user
        TEAM_BUFF, // Affects user and ally characters

        // ATTACK
        BYPASS,
        DMG_MOD,
        NO_DMG,
        FRIENDLY,

        // RADIUS
        RAD_MOD,
        NON_RAD, // RNG is added to RAD
        RAD_NOT_MODDABLE,
        MOBILITY,

        FREE
    }

    [XmlAttribute("Name")]
    public string m_name;
    [XmlAttribute("Energy")]
    public string m_energy;
    [XmlAttribute("DMG")]
    public int m_damage;
    [XmlAttribute("RNG")]
    public int m_range;
    [XmlAttribute("RAD")]
    public int m_radius;
    [XmlAttribute("Description")]
    public string m_effect; // Description

    public int m_id;
    public bool m_isRevealed = false;
    public int m_isDisabled = 0;

    // References
    CharacterScript m_charScript;
    private SlidingPanelManagerScript m_panMan;

    private void Start()
    {
        m_charScript = GetComponent<CharacterScript>();

        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
    }


    // Action
    public void ActionTargeting(TileScript _tile)
    {
        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;

        // RANGE MODS

        // If the action only targets horizontal and vertical or diagonal
        targetingRestriction = TileScript.targetRestriction.NONE;
        if (UniqueActionProperties(uniAct.TAR_RES) >= 0)
            targetingRestriction = (TileScript.targetRestriction)UniqueActionProperties(uniAct.TAR_RES);

        // If the action cannot be blocked
        bool isBlockable = true;
        if (UniqueActionProperties(uniAct.IS_NOT_BLOCK) >= 0 ||
            m_charScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0)
            isBlockable = false;

        bool targetSelf = false;

        // If the action can target the source
        if (UniqueActionProperties(uniAct.TAR_SELF) >= 0 || !CheckIfAttack()) //|| !CheckIfAttack(m_name) && m_name != "Protect")
            targetSelf = true;

        int finalRNG = m_range + m_charScript.m_tempStats[(int)CharacterScript.sts.RNG];

        if (UniqueActionProperties(uniAct.RNG_MOD) >= 0)
            finalRNG += m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];

        if (finalRNG < 1)
            finalRNG = 1;

        if (UniqueActionProperties(uniAct.NO_RNG) >= 0)
            finalRNG = m_range;

        Color color = Color.white;
        // See if it's an attack or not
        if (CheckIfAttack())
            color = TileScript.c_attack;
        else
            color = TileScript.c_action;

        _tile.FetchTilesWithinRange(m_charScript, finalRNG, color, targetSelf, targetingRestriction, isBlockable);
    }

    public void ActionStart()
    {
        transform.LookAt(m_charScript.m_boardScript.m_selected.transform);

        Renderer r = m_charScript.m_boardScript.m_selected.GetComponent<Renderer>();
        TileScript selectedTileScript = m_charScript.m_boardScript.m_selected;

        if (r.material.color == Color.red
            || r.material.color == Color.green) // single
        {
            if (UniqueActionProperties(uniAct.MOBILITY) >= 1 && m_range <= 1)
                m_charScript.m_anim.Play("Kick", -1, 0);
            else if (r.material.color == Color.green)
                m_charScript.m_anim.Play("Ability", -1, 0);
            else if (m_range + m_charScript.m_tempStats[(int)CharacterScript.sts.RNG] > 1)
                m_charScript.m_anim.Play("Ranged", -1, 0);
            else
                m_charScript.m_anim.Play("Melee", -1, 0);

            m_charScript.m_targets.Add(selectedTileScript.m_holding);
        }
        else if (r.material.color == TileScript.c_radius) // multi
        {
            for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
            {
                TileScript tarTile = selectedTileScript.m_targetRadius[i].GetComponent<TileScript>();
                if (tarTile.m_holding)
                    m_charScript.m_targets.Add(tarTile.m_holding);
            }

            if (m_name == "ATK(Slash)")
                m_charScript.m_anim.Play("Slash", -1, 0);
            else if (UniqueActionProperties(uniAct.TAR_RES) == (int)TileScript.targetRestriction.HORVERT)
                m_charScript.m_anim.Play("Stab", -1, 0);
            else if (m_range <= 1)
                m_charScript.m_anim.Play("Sweep", -1, 0);
            else
                m_charScript.m_anim.Play("Throw", -1, 0);
        }

        m_charScript.m_boardScript.m_camIsFrozen = true;
        m_panMan.CloseHistory();

        if (selectedTileScript.m_targetRadius.Count > 0)
            selectedTileScript.ClearRadius();

        if (!m_charScript.m_boardScript.m_isForcedMove)
            m_charScript.m_tile.GetComponent<TileScript>().ClearRadius();
    }

    public void Action()
    {
        m_charScript.m_boardScript.m_actionEndTimer = Time.deltaTime;

        m_isRevealed = true;

        // ATTACK MODS
        bool teamBuff = false;
        bool soloBuff = false;
        bool gainOK = false;
        if (UniqueActionProperties(uniAct.TEAM_BUFF) >= 0)// || m_name == "Cleansing ATK")
            teamBuff = true;
        else if (UniqueActionProperties(uniAct.SOLO_BUFF) >= 0)
            soloBuff = true;

        if (m_charScript.m_targets.Count > 0)
        {
            for (int i = 0; i < m_charScript.m_targets.Count; i++)
            {
                if (CheckIfAttack())
                    if (Attack(m_charScript.m_targets[i].GetComponent<ObjectScript>()))
                        gainOK = true;

                if (!soloBuff)
                    if (m_charScript.m_targets[i].tag == "PowerUp" ||
                        m_charScript.m_targets[i].tag == "Player" && !m_charScript.m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.REFLECT] && !m_charScript.m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.NULLIFY] ||
                        m_charScript.m_targets[i].tag == "Player" && m_charScript.m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.REFLECT] && m_charScript.m_targets[i].GetComponent<CharacterScript>().m_player == m_charScript.m_player ||
                        m_charScript.m_targets[i].tag == "Player" && m_charScript.m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.NULLIFY] && m_charScript.m_targets[i].GetComponent<CharacterScript>().m_player != m_charScript.m_player)
                        if (!teamBuff ||
                            teamBuff && m_charScript.m_targets[i].tag == "Player" && m_charScript.m_targets[i].GetComponent<CharacterScript>().m_player == m_charScript.m_player)
                            Ability(m_charScript.m_targets[i], m_name);
            }


            if (m_charScript.m_targets.Count > 1)
                m_charScript.m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_charScript.m_boardScript.m_selected.gameObject;
            else
                m_charScript.m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_charScript.m_targets[0];
        }

        if (UniqueActionProperties(uniAct.FREE) == -1)
            m_charScript.m_hasActed[(int)CharacterScript.trn.ACT] = true;

        if (teamBuff || soloBuff && gainOK)
            Ability(gameObject, m_name);

        EnergyConversion();

        m_charScript.m_exp += m_energy.Length;

        m_charScript.m_targets.Clear();

        m_charScript.UpdateStatusImages();

        m_panMan.GetPanel("HUD Panel LEFT").PopulatePanel();
        if (m_panMan.GetPanel("HUD Panel RIGHT").m_inView)
            m_panMan.GetPanel("HUD Panel RIGHT").PopulatePanel();

        m_panMan.GetPanel("ActionViewer Panel").ClosePanel();
        if (!m_panMan.GetPanel("Choose Panel").m_inView)
            m_charScript.m_boardScript.m_selected = null;

        if (m_charScript.m_boardScript.m_currButton.GetComponent<SlidingPanelScript>())
            m_charScript.m_boardScript.m_currButton.GetComponent<SlidingPanelScript>().m_inView = false;
        m_charScript.m_boardScript.m_currButton = null;

        print(m_charScript.m_name + " has finished acting.");
    }

    private bool Attack(ObjectScript _currTarget)
    {
        if (_currTarget.gameObject.tag != "Player" && _currTarget.gameObject.tag != "Environment" ||
            _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_isAlive == false)
            return false;

        // ATTACK MODS
        bool friendly = false;
        if (UniqueActionProperties(uniAct.TEAM_BUFF) >= 0 || UniqueActionProperties(uniAct.FRIENDLY) >= 0 ||
            m_charScript.m_effects[(int)StatusScript.effects.CAREFUL])// || m_name == "Cleansing ATK")
            friendly = true;


        string finalDMG = finalDMG = (m_damage + m_charScript.m_tempStats[(int)CharacterScript.sts.DMG]).ToString();

        if (UniqueActionProperties(uniAct.NO_DMG) == 1)
            finalDMG = m_damage.ToString();

        if (!friendly || friendly && _currTarget.gameObject.tag == "Environment" ||
            friendly && _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != m_charScript.m_player)
        {
            if (_currTarget.gameObject.tag == "Player")
            {
                CharacterScript charScript = _currTarget.GetComponent<CharacterScript>();

                // DAMAGE MODS
                if (UniqueActionProperties(uniAct.BYPASS) >= 0 && charScript.m_tempStats[(int)CharacterScript.sts.DEF] > 0)
                {
                    int def = charScript.m_tempStats[(int)CharacterScript.sts.DEF] - UniqueActionProperties(uniAct.BYPASS) - m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                    if (def < 0)
                        def = 0;

                    finalDMG = (int.Parse(finalDMG) - def).ToString();
                }
                else
                    finalDMG = (int.Parse(finalDMG) - charScript.m_tempStats[(int)CharacterScript.sts.DEF]).ToString();
            }

            if (UniqueActionProperties(uniAct.DMG_MOD) >= 0)
                finalDMG = (int.Parse(finalDMG) + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]).ToString();

            _currTarget.ReceiveDamage(finalDMG, Color.white);

            if (_currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != m_charScript.m_player)
                return true;
        }

        return false;
    }

    private void Ability(GameObject _currTarget, string _name)
    {
        if (_currTarget.tag == "PowerUp" && UniqueActionProperties(uniAct.MOBILITY) == -1)
            return;

        GameObject gO = m_charScript.gameObject;

        CharacterScript targetScript = null;
        if (_currTarget.tag == "Player")
            targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript targetTile = _currTarget.GetComponent<ObjectScript>().m_tile.GetComponent<TileScript>();
        TileScript tileScript = m_charScript.m_tile.GetComponent<TileScript>();

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
            case "SUP(Accelerate)":
            case "ATK(Arm)":
            case "ATK(Body)":
            case "SUP(Boost)":
            case "SUP(Charge)":
            case "ATK(Break)":
            case "ATK(Caution)":
            case "ATK(Crash)":
            case "ATK(Disrupt)":
            case "SUP(Explosive)":
            case "SUP(Fortify)":
            case "ATK(Hinder)":
            case "ATK(Immobilize)":
            case "ATK(Leg)":
            case "ATK(Lock)":
            //case "Protect":
            //case "SUP(Secure)":
            case "ATK(Maintain)":
            case "ATK(Mind)":
            case "ATK(Nullify)":
            case "ATK(Rev)":
            case "ATK(Ruin)":
            case "ATK(Rust)":
            case "ATK(Sight)":
            case "ATK(Smoke)":
            case "SUP(Spot)":
            //case "ATK(Target)":
            case "ATK(Ward)":
            case "ATK(Weaken)":
            case "ATK(Whole)":
                StatusScript.NewStatus(_currTarget, m_charScript, this);
                break;
            case "ATK(Deplete)":
                if (_name == "ATK(Prismatic)" && m_charScript.m_tempStats[(int)CharacterScript.sts.TEC] < -2 || _name == "ATK(Deplete)" && m_charScript.m_tempStats[(int)CharacterScript.sts.TEC] < -1 ||
                    _name == "ATK(Syphon)" && m_charScript.m_tempStats[(int)CharacterScript.sts.TEC] < -2)
                    break;
                if (!m_charScript.m_isAI)
                    SelectorInit(targetScript, "Energy Selector");
                else if (_name == "ATK(Deplete)")
                    targetScript.m_player.RemoveRandomEnergy(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                else if (_name == "ATK(Prismatic)") // REFACTOR
                    m_charScript.m_player.GainRandomEnergyAI(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                else if (_name == "ATK(Syphon)") // REFACTOR
                {
                    targetScript.m_player.RemoveRandomEnergy(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                    m_charScript.m_player.GainRandomEnergyAI(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "ATK(Copy)":
            case "ATK(Hack)":
            case "SUP(Redirect)":
                if (!m_charScript.m_isAI)
                {
                    m_panMan.GetPanel("Choose Panel").m_inView = true;
                    m_panMan.m_history.Add(m_panMan.GetPanel("Choose Panel"));
                }
                break;
            //case "SUP(Delete)":
            case "SUP(Extension)":
                //case "Modification": Permanently add a temp status effect
                if (targetScript.GetComponents<StatusScript>().Length > 0 && !m_charScript.m_isAI)
                    SelectorInit(targetScript, "Status Selector");
                break;
            case "ATK(Prism)":
                m_charScript.m_player.AddRandomEnergy(1);
                break;
            case "ATK(Combo)":
                m_charScript.m_player.AddRandomEnergy(2);
                break;
            case "ATK(Syphon)":
                targetScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                StatusScript.ApplyStatus(targetScript.gameObject);
                m_charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                StatusScript.ApplyStatus(gameObject);
                break;
            case "SUP(Defrag)":
                StatusScript.DestroyAllDebuffs(_currTarget);
                StatusScript.NewStatus(_currTarget, m_charScript, this);
                break;
            //case "Cleansing ATK":
            case "ATK(Salvage)":
                m_charScript.HealHealth(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                if (m_charScript.m_tempStats[(int)CharacterScript.sts.TEC] > 0)
                    m_charScript.m_player.RemoveRandomEnergy(m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "SUP(Restore)":
                targetScript.HealHealth(2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                if (m_charScript.m_tempStats[(int)CharacterScript.sts.TEC] > 0)
                    targetScript.m_player.RemoveRandomEnergy(m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            // Unique abilities
            case "ATK(Blast)":
                Knockback(_currTarget.GetComponent<ObjectScript>(), m_charScript.m_boardScript.m_selected, 2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "ATK(Focus)":
                m_charScript.m_stats[(int)CharacterScript.sts.DMG]++;
                m_charScript.m_stats[(int)CharacterScript.sts.TEC]++;
                StatusScript.ApplyStatus(gameObject);
                break;
            case "SUP(Channel)":
                m_charScript.m_player.m_energy[(int)PlayerScript.eng.BLU] += 1;
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
                targetScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                StatusScript.ApplyStatus(targetScript.gameObject);
                break;
            case "SUP(Synch)":
                targetScript.m_tempStats[(int)CharacterScript.sts.SPD] += 2 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                StatusScript.ApplyStatus(targetScript.gameObject);
                break;
            case "ATK(Burst)":
                m_charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 1 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                StatusScript.ApplyStatus(gO);
                break;
            case "ATK(Intel)":
                targetScript.RevealRandomAction();
                break;
            case "ATK(Dark":
                m_charScript.HideAllActions();
                break;
            case "ATK(Dash)":
                if (TileScript.CheckForEmptyNeighbor(tileScript) && !m_charScript.m_isAI)
                {
                    tileScript.ClearRadius();
                    m_charScript.m_boardScript.m_isForcedMove = m_charScript.gameObject;
                    m_charScript.MovementSelection(3 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "ATK(Leak)":
                targetScript.m_player.RemoveRandomEnergy(1);
                break;
            case "ATK(Overclock)":
                m_charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 3 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC];
                break;
            case "ATK(Lunge)":
                PullTowards(m_charScript, targetScript.m_tile, 100);
                break;
            case "ATK(Magnet)":
                PullTowards(_currTarget.GetComponent<ObjectScript>(), m_charScript.m_boardScript.m_selected, 100);
                break;
            case "ATK(Pull)":
                PullTowards(_currTarget.GetComponent<ObjectScript>(), tileScript, 100);
                break;
            case "ATK(Push)":
                if (TileScript.CheckForEmptyNeighbor(targetTile) && !m_charScript.m_isAI)
                {
                    tileScript.ClearRadius();
                    m_charScript.m_boardScript.m_isForcedMove = _currTarget;
                    _currTarget.GetComponent<ObjectScript>().MovementSelection(3 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "SUP(Passage)":
                tileScript.ClearRadius();
                m_charScript.m_boardScript.m_isForcedMove = _currTarget;
                TileScript.FetchAllEmptyTiles();
                break;
            case "SUP(Reboot)":
                targetScript.Revive((m_charScript.m_totalHealth / 2) + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                targetScript.m_anim.Play("Getting Up", -1, 0);
                break;
            case "ATK(Smash)":
                Knockback(_currTarget.GetComponent<ObjectScript>(), tileScript, 3 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "SUP(Engage)":
                m_name = "ATK(Destroy)";
                m_damage = 16;
                m_range = 1;
                m_radius = 0;
                m_effect = "When used, turns into SUP(Engage) (ENG: RR, RNG: 0, DMG: 0).";
                break;
            case "ATK(Destroy)":
                Knockback(_currTarget.GetComponent<ObjectScript>(), tileScript, 3 + m_charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
                m_name = "SUP(Engage)";
                m_damage = 0;
                m_range = 0;
                m_radius = 0;
                m_effect = "When used, turns into ATK(Destroy) (ENG: RR, RNG: 1, DMG: 16).";
                break;
            case "SUP(Queue)":
                GameObject.Find("Board").GetComponent<BoardScript>().m_priorityQueue.Add(targetScript);
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

    // Ability functions
    public int UniqueActionProperties(uniAct _uniAct)
    {
        // Only DMG MOD
        if (m_name == "ATK(Power)" || m_name == "ATK(Corrupt)")
        {
            if (_uniAct == uniAct.DMG_MOD)
                return 1;
        }
        // NO DMG applied
        if (m_name == "ATK(AUX)")
        {
            if (_uniAct == uniAct.NO_DMG)
                return 1;
        }
        // Only SOLO BUFF
        else if (m_name == "ATK(Mod)" || m_name == "ATK(Salvage)" || m_name == "ATK(Caution)")
        {
            if (_uniAct == uniAct.SOLO_BUFF)
                return 1;
        }
        // Only RNG MOD
        else if (m_name == "ATK(Leak)" || m_name == "ATK(Snipe)" || m_name == "SUP(Upgrade)")
        {
            if (_uniAct == uniAct.RNG_MOD)
                return 1;
        }
        // Only BYPASS
        else if (m_name == "ATK(Bypass)")
        {
            if (_uniAct == uniAct.BYPASS)
                return 1;
        }
        // ONLY TEAM BUFF
        else if (m_name == "ATK(Maintain)")
        {
            if (_uniAct == uniAct.TEAM_BUFF)
                return 1;
        }
        else if (m_name == "ATK(Blast)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
        }
        else if (m_name == "SUP(Channel)" || m_name == "SUP(Engage)")
        {
            if (_uniAct == uniAct.NO_RNG)
                return 1;
        }
        else if (m_name == "ATK(Copy)")
        {
            if (_uniAct == uniAct.RNG_MOD)
                return 1;
            else if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (m_name == "ATK(Cross)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        //else if (m_name == "SUP(Delete)")
        //{
        //    if (_uniAct == uniAct.RAD_NOT_MODDABLE)
        //        return 1;
        //}
        else if (m_name == "ATK(Diagonal)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.DIAGONAL;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 0;
            if (_uniAct == uniAct.NON_RAD)
                return 1;
        }
        else if (m_name == "ATK(Hack)")
        {
            if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (m_name == "ATK(Lunge)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.RNG_MOD)
                return 1;
        }
        else if (m_name == "ATK(Magnet)")
        {
            if (_uniAct == uniAct.RAD_MOD)
                return 1;
            else if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
        }
        else if (m_name == "ATK(Pull)")
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
        else if (m_name == "ATK(Push)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
            else if (_uniAct == uniAct.FRIENDLY)
                return 1;
            else if (_uniAct == uniAct.RAD_NOT_MODDABLE)
                return 1;
        }
        else if (m_name == "ATK(Slash)")
        {
            if (_uniAct == uniAct.TAR_RES)
                return (int)TileScript.targetRestriction.HORVERT;
            else if (_uniAct == uniAct.IS_NOT_BLOCK)
                return 1;
            else if (_uniAct == uniAct.NO_RNG)
                return 1;
            else if (_uniAct == uniAct.TAR_SELF)
                return -1;
        }
        else if (m_name == "ATK(Smash)")
        {
            if (_uniAct == uniAct.MOBILITY)
                return 1;
        }
        else if (m_name == "ATK(Synch)")
        {
            if (_uniAct == uniAct.TEAM_BUFF)
                return 1;
            else if (_uniAct == uniAct.TAR_SELF)
                return -1;
        }
        else if (m_name == "ATK(Thrust)")
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

        if (_uniAct == uniAct.TAR_SELF && m_radius > 0)
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
                    if (i == 0 && targetTileScript.m_x > _towards.m_x ||
                        i == 1 && targetTileScript.m_x < _towards.m_x ||
                        i == 2 && targetTileScript.m_z < _towards.m_z ||
                        i == 3 && targetTileScript.m_z > _towards.m_z)
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

    public void SelectorInit(CharacterScript _targetScript, string _pan)
    {
        PanelScript selector = m_panMan.GetPanel("Status Selector");
        if (_pan == "Status Selector")
        {
            if (m_name == "SUP(Delete)")
                selector.transform.Find("Message").GetComponent<Text>().text = "Choose a status to remove";
            if (m_name == "SUP(Extension)" || m_name == "Modification")
                selector.transform.Find("Message").GetComponent<Text>().text = "Choose a status to boost";
        }
        else if (_pan == "Energy Selector")
        {
            selector = m_panMan.GetPanel("Energy Selector");

            if (m_name == "ATK(Syphon)" || m_name == "ATK(Deplete)")
            {
                if (m_name == "ATK(Syphon)")
                    selector.transform.Find("Message").GetComponent<Text>().text = "Choose energy to steal";
                else if (m_name == "ATK(Deplete)")
                    selector.transform.Find("Message").GetComponent<Text>().text = "Choose energy to remove";

                Transform energy = selector.transform.Find("Energy");

                for (int i = 0; i < energy.childCount; i++)
                {
                    Text t = energy.GetChild(i).GetComponentInChildren<Text>();
                    t.text = _targetScript.m_player.m_energy[i].ToString();
                }
            }
            else if (m_name == "ATK(Prismatic)") // if channel
            {
                selector.transform.Find("Message").GetComponent<Text>().text = "Choose energy to gain";
                for (int i = 0; i < selector.transform.Find("Status").childCount; i++)
                    selector.transform.Find("Status").GetChild(i).GetComponentInChildren<Text>().text = "0";
            }
        }

        selector.m_cScript = _targetScript;
        selector.PopulatePanel();

        m_charScript.m_tile.GetComponent<TileScript>().ClearRadius();
        m_charScript.m_boardScript.m_isForcedMove = m_charScript.gameObject;
    }

    public bool CheckIfAttack()
    {
        if (m_name[0] == 'A' && m_name[1] == 'T' && m_name[2] == 'K')
            return true;
        else
            return false;
    }

    private void EnergyConversion()
    {
        // Gain or use energy

        // Assign mm_energy symbols
        for (int i = 0; i < m_energy.Length; i++)
        {
            if (m_energy[i] == 'g')
                m_charScript.m_player.m_energy[0] += 1;
            else if (m_energy[i] == 'r')
                m_charScript.m_player.m_energy[1] += 1;
            else if (m_energy[i] == 'w')
                m_charScript.m_player.m_energy[2] += 1;
            else if (m_energy[i] == 'b')
                m_charScript.m_player.m_energy[3] += 1;
            else if (m_energy[i] == 'G')
                m_charScript.m_player.m_energy[0] -= 1;
            else if (m_energy[i] == 'R')
                m_charScript.m_player.m_energy[1] -= 1;
            else if (m_energy[i] == 'W')
                m_charScript.m_player.m_energy[2] -= 1;
            else if (m_energy[i] == 'B')
                m_charScript.m_player.m_energy[3] -= 1;
        }

        m_charScript.SetPopupSpheres(m_energy);
        m_charScript.m_player.SetEnergyPanel(m_charScript);
    }

    public int CheckActionLevel()
    {
        if (m_energy[0] == 'g' || m_energy[0] == 'r' || m_energy[0] == 'w' || m_energy[0] == 'b')
            return 0;

        return m_energy.Length;
    }

    public int CheckActionColor()
    {
        if (m_energy[0] == 'g' || m_energy[0] == 'G')
            return 0;
        else if (m_energy[0] == 'r' || m_energy[0] == 'R')
            return 1;
        else if (m_energy[0] == 'w' || m_energy[0] == 'W')
            return 2;
        else if (m_energy[0] == 'b' || m_energy[0] == 'B')
            return 3;

        return -1;
    }

    public int ConvertedCost()
    {
        if (PlayerScript.CheckIfGains(m_energy))
            return -m_energy.Length;
        else
            return m_energy.Length;
    }

    public bool checkTargetsTiles(ObjectScript _target, TileScript _tile)
    {
        if (_tile == null)
            return false;

        for (int i = 0; i < m_charScript.m_targets.Count; i++)
        {
            TileScript tScript = m_charScript.m_targets[i].GetComponent<ObjectScript>().m_tile;
            if (_tile.m_holding && _tile.m_holding.tag == "Environment" ||
                _tile == tScript && m_charScript.m_targets[i].tag == _target.gameObject.tag ||
                _tile == m_charScript.m_tile && m_charScript.gameObject.tag == _target.gameObject.tag)
                return false;
        }

        return true;
    }

    //public void RetrieveActions(CharacterScript _char)
    //{
    //    DatabaseScript db = _char.m_boardScript.GetComponent<DatabaseScript>();
    //    for (int i = 0; i < _char.m_actions.Length; i++)
    //    {
    //        if (_char.m_actions[i].Length > 20)
    //            continue;
    //        for (int j = 0; j < db.m_actions.Length; j++)
    //            if (_char.m_actions[i] == DatabaseScript.GetActionData(db.m_actions[j], DatabaseScript.actions.NAME))
    //            {
    //                _char.m_actions[i] = db.m_actions[j];
    //                break;
    //            }
    //    }
    //}
}
