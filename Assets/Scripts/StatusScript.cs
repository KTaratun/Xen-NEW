using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusScript : MonoBehaviour {

    public enum mode { TURN_END, ROUND_END, NONE }
    public enum effects { 
        SCARRING, // Cannot be healed
        BLEED, // Damage each turn
        REGEN, // Heal each turn
        IMMOBILE, // Cannot move
        WARD, // Cannot ATK
        STUN, // Turn skipped
        RAGE, // Cannot use SUP
        REFLECT, // Not affected by opponent's abilities
        NULLIFY, // Not affected by friendly abilities
        CAREFUL, // Bigger radius
        BASIC, // Cannot use 2 ENG actions
        TOT }

    static public Color c_buffColor = Color.cyan;
    static public Color c_debuffColor = new Color(1, .7f, 0, 1);
    static public Color c_statusColor = Color.magenta;

    public string m_name;
    public ActionScript m_action;
    public CharacterScript m_charScript;
    public int[] m_statMod;
    public mode m_mode;
    public int m_lifeSpan;
    public Sprite m_sprite;
    public Color m_color;
    public string m_effect;

    private ActionLoaderScript m_actLoad;

	// Use this for initialization
	void Start ()
    {
        if (GameObject.Find("Scene Manager"))
            m_actLoad = GameObject.Find("Scene Manager").GetComponent<ActionLoaderScript>();

        m_name = "";
        m_charScript = null;

        if (m_statMod == null)
            m_statMod = new int[(int)CharacterScript.sts.TOT];
        else
            for (int i = 0; i < m_statMod.Length; i++)
                m_statMod[i] = 0;

        m_mode = mode.NONE;
        m_lifeSpan = 0;
        m_sprite = null;
        m_color = Color.white;
        m_effect = "";
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void NewStatus(GameObject _owner, CharacterScript _caster, ActionScript _Action)
    {
        CharacterScript charScript = _owner.GetComponent<CharacterScript>();

        int ind = -1;
        for (int i = 0; i < charScript.m_statuses.Length; i++)
            if (charScript.m_statuses[i].m_lifeSpan < 1)
            {
                ind = i;
                break;
            }

        charScript.m_statuses[ind].StatusInit(charScript, _caster, _Action);
        ApplyStatus(_owner);
        charScript.UpdateStatusImages();
    }

    public void StatusInit(CharacterScript _ownerScript, CharacterScript _casterScript, ActionScript _action)
    {
        m_name = _action.m_name;
        m_action = _action;
        m_charScript = _ownerScript;
        int tecVal = _casterScript.m_tempStats[(int)CharacterScript.sts.TEC];
        m_effect = m_actLoad.ModifyActions(tecVal, _action.m_effect);

        switch (_action.m_name)
        {
            case "SUP(Accelerate)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.MOV] = 2 + tecVal;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Arm)":
                if (tecVal < -2)
                    return;
                m_statMod[(int)CharacterScript.sts.DMG] = -(3 + tecVal);
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Body)":
                if (tecVal < -1)
                    return;
                m_statMod[(int)CharacterScript.sts.DEF] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = c_debuffColor;
                break;
            case "SUP(Boost)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.TEC] = 1 + tecVal;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Caution)":
                if (tecVal < -1)
                    return;
                m_statMod[(int)CharacterScript.sts.DEF] = 1 + tecVal;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Break)":
                if (tecVal < 0)
                    return;
                m_charScript.m_effects[(int)effects.RAGE] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
                m_color = c_statusColor;

                for (int i = 0; i < m_charScript.m_actions.Count; i++)
                    if (!m_charScript.m_actions[i].CheckIfAttack())
                        m_charScript.m_actions[i].m_isDisabled++;

                break;
            case "SUP(Charge)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.DMG] = 1 + tecVal;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Crash)":
                if (tecVal < 0)
                    return;
                m_charScript.m_effects[(int)effects.BASIC] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Action Symbol");
                m_color = c_statusColor;

                for (int i = 0; i < m_charScript.m_actions.Count; i++)
                    if (m_charScript.m_actions[i].m_energy.Length > 1)
                        m_charScript.m_actions[i].m_isDisabled++;

                break;
            case "SUP(Defrag)":
                if (tecVal < -1)
                    return;
                m_charScript.m_effects[(int)effects.REFLECT] = true;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                m_color = c_statusColor;
                break;
            case "ATK(Disrupt)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.TEC] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                m_color = c_debuffColor;
                break;
            case "SUP(Explosive)":
                if (tecVal < -1)
                    return;
                m_charScript.m_effects[(int)effects.CAREFUL] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2 + tecVal;
                m_statMod[(int)CharacterScript.sts.RAD] = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Radius Symbol");
                m_color = c_buffColor;
                break;
            case "SUP(Fortify)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.DEF] = 1 + tecVal;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Immobilize)":
                if (tecVal < -3)
                    return;
                m_statMod[(int)CharacterScript.sts.MOV] = -(4 + tecVal);
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Leg)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.MOV] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Lock)":
                m_charScript.m_effects[(int)effects.STUN] = true;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Action Symbol");
                m_color = c_statusColor;
                break;
            case "ATK(Maintain)":
                if (tecVal < -2)
                    return;
                m_charScript.m_effects[(int)effects.REGEN] = true;
                // Perform effect: = 1
                m_mode = mode.TURN_END;
                m_lifeSpan = 3 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Mind)":
                if (tecVal < -2)
                    return;
                m_statMod[(int)CharacterScript.sts.TEC] = -(3 + tecVal);
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Nullify)":
                if (tecVal < -1)
                    return;
                m_charScript.m_effects[(int)effects.NULLIFY] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                m_color = c_statusColor;
                break;
            //case "Protect":
            //    m_charScript.m_effects[(int)effects.PROTECT] = true;
            //    m_mode = mode.ROUND_END;
            //    m_lifeSpan = 2;
            //    m_sprite = Resources.Load<Sprite>("Symbols/Hit Symbol");
            //    m_color = c_statusColor;
            //    break;
            //case "SUP(Passage)":
            //    if (tecVal < -3)
            //        return;
            //    m_statMod[(int)CharacterScript.sts.MOV] = 4 + tecVal;
            //    m_mode = mode.TURN_END;
            //    m_lifeSpan = 1;
            //    m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
            //    m_color = c_buffColor;
            //    break;
            case "ATK(Rev)":
                if (tecVal < -2)
                    return;
                m_statMod[(int)CharacterScript.sts.DMG] = 1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 3 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = c_buffColor;
                break;
            case "ATK(Ruin)":
                if (tecVal < 0)
                    return;
                m_charScript.m_effects[(int)effects.SCARRING] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Rust)":
                if (tecVal < -2)
                    return;
                m_charScript.m_effects[(int)effects.BLEED] = true;
                // Perform effect = 2
                m_mode = mode.TURN_END;
                m_lifeSpan = 3 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = c_statusColor;
                break;
            case "ATK(Sight)":
                if (tecVal < 0)
                    return;
                m_statMod[(int)CharacterScript.sts.RNG] = -(1 + tecVal);
                m_mode = mode.ROUND_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Smoke)":
                if (tecVal < -1)
                    return;
                m_statMod[(int)CharacterScript.sts.MOV] = -(2 + tecVal);
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = c_debuffColor;
                break;
            case "SUP(Spot)":
                if (tecVal < -3)
                    return;
                m_statMod[(int)CharacterScript.sts.RNG] = 2 + tecVal;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                m_color = c_buffColor;
                break;
            //case "ATK(Target)":
            //    if (tecVal < -1)
            //        return;
            //    m_statMod[(int)CharacterScript.sts.RNG] = 1;
            //    m_mode = mode.TURN_END;
            //    m_lifeSpan = 4 + tecVal;
            //    m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
            //    m_color = c_buffColor;
            //    break;
            case "ATK(Ward)":
                if (tecVal < -2)
                    return;
                m_statMod[(int)CharacterScript.sts.DMG] = -3 - tecVal;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Weaken)":
                if (tecVal < -1)
                    return;
                m_statMod[(int)CharacterScript.sts.DEF] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2 + tecVal;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = c_debuffColor;
                break;
            case "ATK(Whole)":
                m_statMod[(int)CharacterScript.sts.DMG] = -1;
                m_statMod[(int)CharacterScript.sts.TEC] = -1;
                m_statMod[(int)CharacterScript.sts.MOV] = -1;
                m_statMod[(int)CharacterScript.sts.RNG] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Critical Symbol");
                m_color = c_debuffColor;
                break;
            default:
                break;
        }

        _ownerScript.PlayAnimation(CharacterScript.prtcles.GAIN_STATUS, m_color);
    }

    static public void ApplyStatus(GameObject _character)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        for (int i = 1; i < charScript.m_stats.Length; i++) // Don't do this for the first index since that would reset HP
        {
            charScript.m_tempStats[i] = charScript.m_stats[i];

            for (int j = 0; j < statScripts.Length; j++)
                if (statScripts[j].m_lifeSpan > 0)
                    charScript.m_tempStats[i] += statScripts[j].m_statMod[i];
                else
                    continue;
        }
    }

    static public void UpdateStatus(GameObject _character, mode _mode)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.m_statuses;
        bool hasBeenDestroyed = false;

        if (statScripts[0] == null)
            return;

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_mode == _mode && statScripts[i].m_lifeSpan > 0)
            {
                statScripts[i].m_lifeSpan--;
                if (statScripts[i].m_lifeSpan <= 0)
                {
                    hasBeenDestroyed = true;
                    statScripts[i].DestroyStatus(_character, false);
                }
                else
                    PerformEffect(statScripts[i], charScript);
            }
        }

        if (!hasBeenDestroyed)
            return;

        AlignSymbols(charScript);
    }

    public void DestroyStatus(GameObject _character, bool _align)
    {
        RemoveEffect();
        Start();
        ApplyStatus(_character);

        if (_align)
            AlignSymbols(_character.GetComponent<CharacterScript>());
    }

    static private void AlignSymbols(CharacterScript _charScript)
    {
        StatusScript[] cStatuses = _charScript.m_statuses;

        for (int i = 0; i < cStatuses.Length - 1; i++)
        {
            if (cStatuses[i].m_lifeSpan > 0)
                continue;

            for (int j = i + 1; j < cStatuses.Length; j++)
                if (cStatuses[j].m_lifeSpan > 0)
                {
                    cStatuses[i].m_name = cStatuses[j].m_name;
                    cStatuses[i].m_action = cStatuses[j].m_action;
                    cStatuses[i].m_charScript = cStatuses[j].m_charScript;
                    cStatuses[i].m_effect = cStatuses[j].m_effect;
                    cStatuses[i].m_mode = cStatuses[j].m_mode;
                    cStatuses[i].m_lifeSpan = cStatuses[j].m_lifeSpan;
                    cStatuses[i].m_sprite = cStatuses[j].m_sprite;
                    cStatuses[i].m_color = cStatuses[j].m_color;
                    
                    for (int k = 0; k < cStatuses[i].m_statMod.Length; k++)
                        cStatuses[i].m_statMod[k] = cStatuses[j].m_statMod[k];

                    cStatuses[j].Start();

                    break;
                }
        }

        _charScript.UpdateStatusImages();
    }

    static public void DestroyAll(GameObject _character)
    {
        StatusScript[] statScripts = _character.GetComponents<StatusScript>();

        for (int i = 0; i < statScripts.Length; i++)
            statScripts[i].DestroyStatus(_character, false);
    }

    static public void DestroyAllDebuffs(GameObject _character)
    {
        StatusScript[] statScripts = _character.GetComponents<StatusScript>();

        for (int i = 0; i < statScripts.Length; i++)
            if (statScripts[i].m_color == c_debuffColor)
                statScripts[i].DestroyStatus(_character, false);
    }

    public void RemoveEffect()
    {
        switch (m_name)
        {
            case "ATK(Break)":
                m_charScript.m_effects[(int)effects.RAGE] = false;

                for (int i = 0; i < m_charScript.m_actions.Count; i++)
                    if (!m_charScript.m_actions[i].CheckIfAttack())
                        m_charScript.m_actions[i].m_isDisabled--;
                break;
            case "ATK(Crash)":
                m_charScript.m_effects[(int)effects.BASIC] = false;

                for (int i = 0; i < m_charScript.m_actions.Count; i++)
                    if (m_charScript.m_actions[i].m_energy.Length > 1)
                        m_charScript.m_actions[i].m_isDisabled--;
                break;
            case "SUP(Explosive)":
                m_charScript.m_effects[(int)effects.CAREFUL] = false;
                break;
            case "ATK(Immobilize)":
                m_charScript.m_effects[(int)effects.IMMOBILE] = false;
                break;
            case "ATK(Lock)":
                m_charScript.m_effects[(int)effects.STUN] = false;
                break;
            case "ATK(Maintain)":
                m_charScript.m_effects[(int)effects.REGEN] = false;
                break;
            case "ATK(Nullify)":
                m_charScript.m_effects[(int)effects.NULLIFY] = false;
                break;
            case "ATK(Ruin)":
                m_charScript.m_effects[(int)effects.SCARRING] = false;
                break;
            case "ATK(Rust)":
                m_charScript.m_effects[(int)effects.BLEED] = false;
                break;
            case "SUP(Defrag)":
                m_charScript.m_effects[(int)effects.REFLECT] = false;
                break;
            case "ATK(Ward)":
                m_charScript.m_effects[(int)effects.WARD] = false;
                break;
            default:
                break;
        }
    }

    static public void PerformEffect(StatusScript _status, CharacterScript _charScript)
    {
        TextMesh tMesh = _charScript.m_popupText.GetComponent<TextMesh>();
        switch (_status.m_name)
        {
            case "ATK(Rust)":
                int damage = 2;
                _charScript.ReceiveDamage(tMesh.text + damage.ToString(), Color.white);
                break;
            case "ATK(Maintain)":
                int health = 1;
                _charScript.HealHealth(int.Parse(tMesh.text) + health);
                break;
            default:
                break;
        }
    }

    public void InvertEffect()
    {
        if (m_color == c_statusColor)
            return;
        else if (m_color == c_buffColor)
            m_color = c_debuffColor;
        else if (m_color == c_debuffColor)
            m_color = c_buffColor;

        for (int i = 0; i < m_statMod.Length; i++)
            if (m_statMod[i] != 0)
            {
                m_statMod[i] -= m_statMod[i] * 2;
                break;
            }

        string newString = "";
        for (int i = 0; i < m_effect.Length; i++)
        {
            if (m_effect[i] == '-')
                newString += '+';
            else if (m_effect[i] == '+')
                newString += '-';
            else
                newString += m_effect[i];
        }
        m_effect = newString;

        ApplyStatus(m_charScript.gameObject);
    }

    static public void ShareStatus()
    {

    }
}
