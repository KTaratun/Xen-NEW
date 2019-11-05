using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionScript : MonoBehaviour {

    public enum uniAct
    {
        TAR_RES, IS_NOT_BLOCK, TAR_SELF, RNG_MOD, NO_RNG, // TARGETING
        SOLO_BUFF, TEAM_BUFF, BYPASS, DMG_MOD, FRIENDLY, // ATTACK
        RAD_MOD, NON_RAD, RAD_NOT_MODDABLE, // RADIUS
        MOBILITY
    }

    CharacterScript m_charScript;

    // Use this for initialization
    void Start ()
    {
        m_charScript = GetComponent<CharacterScript>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Action
    static public void ActionTargeting(CharacterScript _char, TileScript _tile)
    {
        GameObject gO = _char.gameObject;

        string actName = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.NAME);
        string actRng = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.RNG);
        string actRad = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.RAD);
        TileScript.targetRestriction targetingRestriction = TileScript.targetRestriction.NONE;

        // RANGE MODS

        // If the action only targets horizontal and vertical or diagonal
        targetingRestriction = TileScript.targetRestriction.NONE;
        if (UniqueActionProperties(_char.m_currAction, uniAct.TAR_RES) >= 0)
            targetingRestriction = (TileScript.targetRestriction)UniqueActionProperties(_char.m_currAction, uniAct.TAR_RES);

        // If the action cannot be blocked
        bool isBlockable = true;
        if (UniqueActionProperties(_char.m_currAction, uniAct.IS_NOT_BLOCK) >= 0 ||
            _char.m_tempStats[(int)CharacterScript.sts.RAD] > 0)
            isBlockable = false;

        bool targetSelf = false;

        // If the action can target the source
        if (UniqueActionProperties(_char.m_currAction, uniAct.TAR_SELF) >= 0 || !CheckIfAttack(actName)) //|| !CheckIfAttack(actName) && actName != "Protect")
            targetSelf = true;

        int finalRNG = int.Parse(actRng) + _char.m_tempStats[(int)CharacterScript.sts.RNG];

        if (UniqueActionProperties(_char.m_currAction, uniAct.RNG_MOD) >= 0)
            finalRNG += _char.m_tempStats[(int)CharacterScript.sts.TEC];

        if (finalRNG < 1)
            finalRNG = 1;

        if (UniqueActionProperties(_char.m_currAction, uniAct.NO_RNG) >= 0)
            finalRNG = 0;

        Color color = Color.white;
        // See if it's an attack or not
        if (CheckIfAttack(actName))
            color = TileScript.c_attack;
        else
            color = TileScript.c_action;

        _tile.FetchTilesWithinRange(_char, finalRNG, color, targetSelf, targetingRestriction, isBlockable);
    }

    static public void ActionStart(CharacterScript _char)
    {
        GameObject gO = _char.gameObject;

        gO.transform.LookAt(_char.m_boardScript.m_selected.transform);

        int rng = int.Parse(DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.RNG)) + _char.m_tempStats[(int)CharacterScript.sts.RNG];
        string actName = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.NAME);
        Renderer r = _char.m_boardScript.m_selected.GetComponent<Renderer>();
        TileScript selectedTileScript = _char.m_boardScript.m_selected;

        if (r.material.color == Color.red
            || r.material.color == Color.green) // single
        {
            if (UniqueActionProperties(_char.m_currAction, uniAct.MOBILITY) >= 1 && rng <= 1)
                _char.m_anim.Play("Kick", -1, 0);
            else if (r.material.color == Color.green)
                _char.m_anim.Play("Ability", -1, 0);
            else if (rng + _char.m_tempStats[(int)CharacterScript.sts.RNG] > 1)
                _char.m_anim.Play("Ranged", -1, 0);
            else
                _char.m_anim.Play("Melee", -1, 0);

            _char.m_targets.Add(selectedTileScript.m_holding);
        }
        else if (r.material.color == Color.yellow) // multi
        {
            for (int i = 0; i < selectedTileScript.m_targetRadius.Count; i++)
            {
                TileScript tarTile = selectedTileScript.m_targetRadius[i].GetComponent<TileScript>();
                if (tarTile.m_holding)
                    _char.m_targets.Add(tarTile.m_holding);
            }

            if (actName == "ATK(Slash)")
                _char.m_anim.Play("Slash", -1, 0);
            else if (UniqueActionProperties(_char.m_currAction, uniAct.TAR_RES) == (int)TileScript.targetRestriction.HORVERT)
                _char.m_anim.Play("Stab", -1, 0);
            else if (rng <= 1)
                _char.m_anim.Play("Sweep", -1, 0);
            else
                _char.m_anim.Play("Throw", -1, 0);
        }

        _char.m_boardScript.m_camIsFrozen = true;
        PanelManagerScript.GetPanel("ActionViewer Panel").m_panels[1].m_slideScript.ClosePanel();
        PanelManagerScript.CloseHistory();

        if (selectedTileScript.m_targetRadius.Count > 0)
            selectedTileScript.ClearRadius();

        if (!_char.m_boardScript.m_isForcedMove)
            _char.m_tile.GetComponent<TileScript>().ClearRadius();
    }

    static public void Action(CharacterScript _char)
    {
        GameObject gO = _char.gameObject;

        _char.m_boardScript.m_actionEndTimer = 0;
        _char.m_boardScript.m_actionEndTimer += Time.deltaTime;

        string actName = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.NAME);
        string actEng = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.ENERGY);
        int actRng = int.Parse(DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.RNG));
        int actRad = int.Parse(DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.RAD));

        // ATTACK MODS
        bool teamBuff = false;
        bool soloBuff = false;
        bool gainOK = false;
        if (UniqueActionProperties(_char.m_currAction, uniAct.TEAM_BUFF) >= 0)// || actName == "Cleansing ATK")
            teamBuff = true;
        else if (UniqueActionProperties(_char.m_currAction, uniAct.SOLO_BUFF) >= 0)
            soloBuff = true;

        if (_char.m_targets.Count > 0)
        {
            for (int i = 0; i < _char.m_targets.Count; i++)
            {
                if (CheckIfAttack(actName))
                    if (Attack(_char, _char.m_targets[i].GetComponent<ObjectScript>()))
                        gainOK = true;

                if (!soloBuff)
                    if (_char.m_targets[i].tag == "PowerUp" || _char.m_targets[i].tag == "Player" && !_char.m_targets[i].GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.REFLECT])
                        if (!teamBuff || teamBuff && _char.m_targets[i].tag == "Player" && _char.m_targets[i].GetComponent<CharacterScript>().m_player == _char.m_player)
                            Ability(_char, _char.m_targets[i], actName);
            }


            if (_char.m_targets.Count > 1)
                _char.m_boardScript.m_camera.GetComponent<CameraScript>().m_target = _char.m_boardScript.m_selected.gameObject;
            else
                _char.m_boardScript.m_camera.GetComponent<CameraScript>().m_target = _char.m_targets[0];
        }

        if (!_char.m_isFree && CheckIfAttack(actName))
            _char.m_hasActed[(int)CharacterScript.trn.ACT] += 3;
        else if (!CheckIfAttack(actName) && actName != "SUP(Redirect)")
            _char.m_hasActed[(int)CharacterScript.trn.ACT] += 2;

        if (teamBuff || soloBuff && gainOK)
            Ability(_char, gO, actName);

        if (!_char.m_isFree && PlayerScript.CheckIfGains(actEng) && gainOK || !_char.m_isFree && !PlayerScript.CheckIfGains(actEng))
            EnergyConversion(_char, actEng);
        else
            PanelManagerScript.GetPanel("Choose Panel").m_slideScript.ClosePanel();

        _char.m_exp += actEng.Length;

        _char.m_isFree = false;
        _char.m_targets.Clear();

        _char.UpdateStatusImages();

        PanelManagerScript.GetPanel("HUD Panel LEFT").PopulatePanel();
        if (PanelManagerScript.GetPanel("HUD Panel RIGHT").m_slideScript.m_inView)
            PanelManagerScript.GetPanel("HUD Panel RIGHT").PopulatePanel();

        PanelManagerScript.GetPanel("ActionViewer Panel").m_slideScript.ClosePanel();
        if (!PanelManagerScript.GetPanel("Choose Panel").m_slideScript.m_inView)
            _char.m_boardScript.m_selected = null;

        if (_char.m_boardScript.m_currButton.GetComponent<PanelScript>())
            _char.m_boardScript.m_currButton.GetComponent<PanelScript>().m_slideScript.m_inView = false;
        _char.m_boardScript.m_currButton = null;

        print(_char.m_name + " has finished acting.");
    }

    static private bool Attack(CharacterScript _char, ObjectScript _currTarget)
    {
        if (_currTarget.gameObject.tag != "Player" && _currTarget.gameObject.tag != "Environment" ||
            _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_isAlive == false)
            return false;

        string actDmg = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.DMG);

        // ATTACK MODS
        bool friendly = false;
        if (UniqueActionProperties(_char.m_currAction, uniAct.TEAM_BUFF) >= 0 || UniqueActionProperties(_char.m_currAction, uniAct.FRIENDLY) >= 0 ||
            _char.m_effects[(int)StatusScript.effects.CAREFUL])// || actName == "Cleansing ATK")
            friendly = true;

        string finalDMG = (int.Parse(actDmg) + _char.m_tempStats[(int)CharacterScript.sts.DMG]).ToString();

        if (!friendly || friendly && _currTarget.gameObject.tag == "Environment" ||
            friendly && _currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != _char.m_player)
        {
            if (_currTarget.gameObject.tag == "Player")
            {
                CharacterScript charScript = _currTarget.GetComponent<CharacterScript>();

                // DAMAGE MODS
                if (UniqueActionProperties(_char.m_currAction, uniAct.BYPASS) >= 0 && charScript.m_tempStats[(int)CharacterScript.sts.DEF] > 0)
                {
                    int def = charScript.m_tempStats[(int)CharacterScript.sts.DEF] - UniqueActionProperties(_char.m_currAction, uniAct.BYPASS) - _char.m_tempStats[(int)CharacterScript.sts.TEC];
                    if (def < 0)
                        def = 0;

                    finalDMG = (int.Parse(finalDMG) - def).ToString();
                }
                else
                    finalDMG = (int.Parse(finalDMG) - charScript.m_tempStats[(int)CharacterScript.sts.DEF]).ToString();
            }

            if (UniqueActionProperties(_char.m_currAction, uniAct.DMG_MOD) >= 0)
                finalDMG = (int.Parse(finalDMG) + _char.m_tempStats[(int)CharacterScript.sts.TEC]).ToString();

            _currTarget.ReceiveDamage(finalDMG, Color.white);

            if (_currTarget.gameObject.tag == "Player" && _currTarget.GetComponent<CharacterScript>().m_player != _char.m_player)
                return true;
        }

        return false;
    }

    static private void Ability(CharacterScript _char, GameObject _currTarget, string _name)
    {
        if (_currTarget.tag == "PowerUp" && UniqueActionProperties(_char.m_currAction, uniAct.MOBILITY) == -1)
            return;

        GameObject gO = _char.gameObject;

        CharacterScript targetScript = null;
        if (_currTarget.tag == "Player")
            targetScript = _currTarget.GetComponent<CharacterScript>();
        TileScript targetTile = _currTarget.GetComponent<ObjectScript>().m_tile.GetComponent<TileScript>();
        TileScript tileScript = _char.m_tile.GetComponent<TileScript>();

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
                StatusScript.NewStatus(_currTarget, _char, _char.m_currAction);
                break;
            case "ATK(Deplete)":
            case "ATK(Prismatic)":
            case "ATK(Syphon)":
                if (_name == "ATK(Prismatic)" && _char.m_tempStats[(int)CharacterScript.sts.TEC] < -2 || _name == "ATK(Deplete)" && _char.m_tempStats[(int)CharacterScript.sts.TEC] < -1 ||
                    _name == "ATK(Syphon)" && _char.m_tempStats[(int)CharacterScript.sts.TEC] < -2)
                    break;
                if (!_char.m_isAI)
                    SelectorInit(_char, targetScript, "Energy Selector");
                else if (_name == "ATK(Deplete)")
                    targetScript.m_player.RemoveRandomEnergy(2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                else if (_name == "ATK(Prismatic)") // REFACTOR
                    _char.m_player.GainRandomEnergyAI(2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                else if (_name == "ATK(Syphon)") // REFACTOR
                {
                    targetScript.m_player.RemoveRandomEnergy(2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                    _char.m_player.GainRandomEnergyAI(2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "ATK(Copy)":
            case "ATK(Hack)":
            case "SUP(Redirect)":
                if (!_char.m_isAI)
                {
                    PanelManagerScript.GetPanel("Choose Panel").m_slideScript.m_inView = true;
                    PanelManagerScript.m_history.Add(PanelManagerScript.GetPanel("Choose Panel"));
                }
                break;
            case "SUP(Delete)":
            case "SUP(Extension)":
                //case "Modification": Permanently add a temp status effect
                if (targetScript.GetComponents<StatusScript>().Length > 0 && !_char.m_isAI)
                    SelectorInit(_char, targetScript, "Status Selector");
                break;
            //case "Cleansing ATK":
            case "ATK(Salvage)":
                _char.HealHealth(2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                if (_char.m_tempStats[(int)CharacterScript.sts.TEC] > 0)
                    _char.m_player.RemoveRandomEnergy(_char.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "SUP(Restore)":
                targetScript.HealHealth(3 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                if (_char.m_tempStats[(int)CharacterScript.sts.TEC] > 0)
                    targetScript.m_player.RemoveRandomEnergy(_char.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            // Unique abilities
            case "ATK(Blast)":
                Knockback(_char, _currTarget.GetComponent<ObjectScript>(), _char.m_boardScript.m_selected, 2 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "ATK(Mod)":
                targetScript.m_stats[(int)CharacterScript.sts.DMG]++;
                StatusScript.ApplyStatus(_currTarget);
                break;
            case "SUP(Channel)":
                _char.m_player.m_energy[(int)PlayerScript.eng.BLU] += 2;
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
                targetScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 2 + _char.m_tempStats[(int)CharacterScript.sts.TEC];
                break;
            case "ATK(Synch)":
                targetScript.m_tempStats[(int)CharacterScript.sts.SPD] += 2 + _char.m_tempStats[(int)CharacterScript.sts.TEC];
                break;
            case "ATK(Dash)":
                if (TileScript.CheckForEmptyNeighbor(tileScript) && !_char.m_isAI)
                {
                    tileScript.ClearRadius();
                    _char.m_boardScript.m_isForcedMove = _char.gameObject;
                    _char.MovementSelection(3 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "ATK(Leak)":
                targetScript.m_player.RemoveRandomEnergy(1);
                break;
            case "ATK(Overclock)":
                _char.m_tempStats[(int)CharacterScript.sts.SPD] += 3 + _char.m_tempStats[(int)CharacterScript.sts.TEC];
                break;
            case "ATK(Lunge)":
                PullTowards(_char, _char, targetScript.m_tile, 100);
                break;
            case "ATK(Magnet)":
                PullTowards(_char, _currTarget.GetComponent<ObjectScript>(), _char.m_boardScript.m_selected, 100);
                break;
            case "ATK(Pull)":
                PullTowards(_char, _currTarget.GetComponent<ObjectScript>(), tileScript, 100);
                break;
            case "ATK(Push)":
                if (TileScript.CheckForEmptyNeighbor(targetTile) && !_char.m_isAI)
                {
                    tileScript.ClearRadius();
                    _char.m_boardScript.m_isForcedMove = _currTarget;
                    _currTarget.GetComponent<ObjectScript>().MovementSelection(3 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                }
                break;
            case "SUP(Reboot)":
                targetScript.Revive(5 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
                break;
            case "ATK(Smash)":
                Knockback(_char, _currTarget.GetComponent<ObjectScript>(), tileScript, 3 + _char.m_tempStats[(int)CharacterScript.sts.TEC]);
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

        return -1;
    }

    static private void Knockback(CharacterScript _char, ObjectScript _targetScript, TileScript _away, int _force)
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

    static private void PullTowards(CharacterScript _char, ObjectScript _targetScript, TileScript _towards, int _maxDis)
    {
        TileScript targetTileScript = _targetScript.m_tile.GetComponent<TileScript>();
        TileScript nei = null;

        while (_maxDis > 0)
        {
            TileScript[] neighbors = targetTileScript.m_neighbors;
            nei = null;

            for (int i = 0; i < neighbors.Length; i++)
            {
                if (checkTargetsTiles(_char, _targetScript, neighbors[i])) // For magnet attack.
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

    static public void DisableRandomAction(CharacterScript _char)
    {
        List<int> viableActs = new List<int>();
        for (int i = 0; i < _char.m_actions.Length; i++)
        {
            string currAct = _char.m_actions[i];
            if (!PlayerScript.CheckIfGains(DatabaseScript.GetActionData(currAct, DatabaseScript.actions.ENERGY)) &&
                _char.m_isDiabled[i] == 0)
                viableActs.Add(i);
        }
        if (viableActs.Count < 1)
            return;

        _char.m_isDiabled[viableActs[Random.Range(0, viableActs.Count)]] = 1; // 1 == temp, 2 == perm
    }

    static public void DisableSelectedAction(CharacterScript _char, int _ind)
    {
        _char.m_isDiabled[_ind] = 2; // 1 == temp, 2 == perm
        _char.m_boardScript.m_isForcedMove = null;
        PanelManagerScript.CloseHistory();
    }

    static public void SelectorInit(CharacterScript _char, CharacterScript _targetScript, string _pan)
    {
        string actName = DatabaseScript.GetActionData(_char.m_currAction, DatabaseScript.actions.NAME);
        PanelScript selector = PanelManagerScript.GetPanel("Status Selector");
        if (_pan == "Status Selector")
        {
            selector = PanelManagerScript.GetPanel("Status Selector");
            if (actName == "SUP(Delete)")
                selector.m_text[0].text = "Choose a status to remove";
            if (actName == "SUP(Extension)" || actName == "Modification")
                selector.m_text[0].text = "Choose a status to boost";
        }
        else if (_pan == "Energy Selector")
        {
            selector = PanelManagerScript.GetPanel("Energy Selector");

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

        _char.m_tile.GetComponent<TileScript>().ClearRadius();
        _char.m_boardScript.m_isForcedMove = _char.gameObject;
    }

    static public bool CheckIfAttack(string _action)
    {
        if (_action[0] == 'A' && _action[1] == 'T' && _action[2] == 'K')
            return true;
        else
            return false;
    }

    static private void EnergyConversion(CharacterScript _char, string _energy)
    {
        // Gain or use energy

        // Assign _energy symbols
        for (int i = 0; i < _energy.Length; i++)
        {
            if (_energy[i] == 'g')
                _char.m_player.m_energy[0] += 1;
            else if (_energy[i] == 'r')
                _char.m_player.m_energy[1] += 1;
            else if (_energy[i] == 'w')
                _char.m_player.m_energy[2] += 1;
            else if (_energy[i] == 'b')
                _char.m_player.m_energy[3] += 1;
            else if (_energy[i] == 'G')
                _char.m_player.m_energy[0] -= 1;
            else if (_energy[i] == 'R')
                _char.m_player.m_energy[1] -= 1;
            else if (_energy[i] == 'W')
                _char.m_player.m_energy[2] -= 1;
            else if (_energy[i] == 'B')
                _char.m_player.m_energy[3] -= 1;
        }

        _char.SetPopupSpheres(_energy);
        _char.m_player.SetEnergyPanel(_char);
    }

    static public int CheckActionLevel(string _actEng)
    {
        if (_actEng[0] == 'g' || _actEng[0] == 'r' || _actEng[0] == 'w' || _actEng[0] == 'b')
            return 0;

        return _actEng.Length;
    }

    static public int CheckActionColor(string _act)
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

    static public void SortActions(CharacterScript _char)
    {
        for (int i = 0; i < _char.m_actions.Length; i++)
        {
            int currEng = ConvertedCost(DatabaseScript.GetActionData(_char.m_actions[i], DatabaseScript.actions.ENERGY));

            for (int j = i + 1; j < _char.m_actions.Length; j++)
            {
                int indexedEng = ConvertedCost(DatabaseScript.GetActionData(_char.m_actions[j], DatabaseScript.actions.ENERGY));

                if (currEng > indexedEng ||
                    currEng == indexedEng && CheckActionColor(_char.m_actions[i]) > CheckActionColor(_char.m_actions[j]))
                {
                    string temp = _char.m_actions[i];
                    _char.m_actions[i] = _char.m_actions[j];
                    _char.m_actions[j] = temp;
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

    static public bool checkTargetsTiles(CharacterScript _char, ObjectScript _target, TileScript _tile)
    {
        if (_tile == null)
            return false;

        for (int i = 0; i < _char.m_targets.Count; i++)
        {
            TileScript tScript = _char.m_targets[i].GetComponent<ObjectScript>().m_tile;
            if (_tile.m_holding && _tile.m_holding.tag == "Environment" ||
                _tile == tScript && _char.m_targets[i].tag == _target.gameObject.tag ||
                _tile == _char.m_tile && _char.gameObject.tag == _target.gameObject.tag)
                return false;
        }

        return true;
    }

    static public void RetrieveActions(CharacterScript _char)
    {
        DatabaseScript db = _char.m_boardScript.GetComponent<DatabaseScript>();
        for (int i = 0; i < _char.m_actions.Length; i++)
        {
            if (_char.m_actions[i].Length > 20)
                continue;
            for (int j = 0; j < db.m_actions.Length; j++)
                if (_char.m_actions[i] == DatabaseScript.GetActionData(db.m_actions[j], DatabaseScript.actions.NAME))
                {
                    _char.m_actions[i] = db.m_actions[j];
                    break;
                }
        }
    }
}
