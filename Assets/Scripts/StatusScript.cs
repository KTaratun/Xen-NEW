using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusScript : MonoBehaviour {

    public enum mode { TURN_END, ROUND_END, NONE }

    public enum effects {
        BLEED, // Damage over time
        BYPASS, // Ignore DEFENSE
        CAREFUL, // Extra radius
        CRUSH, // Take 1 extra DMG TODO
        DEFENSE, // Take 1 less DMG TODO
        DISRUPT, // Affects of attacks are negated
        HINDER, // Cannot use Non-attack actions
        IMMOBILE, // Cannot move
        REFLECT, // Unaffected by status effects
        REGEN, // Heal over time
        SCARRING, //Cannot be healed
        STUN, // Turn is skipped
        WARD, // Cannot use attack actions
        TOT }

    static public Color c_buffColor = Color.cyan;
    static public Color c_debuffColor = new Color(1, .7f, 0, 1);
    static public Color c_statusColor = Color.magenta;

    public string m_name;
    public string m_action;
    public CharacterScript m_charScript;
    public int[] m_statMod;
    public mode m_mode;
    public int m_lifeSpan;
    public Sprite m_sprite;
    public Color m_color;
    public string m_effect;

	// Use this for initialization
	void Start ()
    {
        m_name = "";
        m_action = "";
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

    static public StatusScript FindOpenStatus(CharacterScript _owner)
    {
        int ind = -1;
        for (int i = 0; i < _owner.m_statuses.Length; i++)
            if (_owner.m_statuses[i].m_lifeSpan < 1)
            {
                ind = i;
                break;
            }
        return _owner.m_statuses[ind];
    }

    static public void NewStatus(CharacterScript _owner, string _name,
        CharacterScript.sts _stat, int _value, mode _mode, int _lifeSpan, string _sprite, Color _color)
    {
        StatusScript status = FindOpenStatus(_owner);
        status.m_statMod[(int)_stat] = _value;
        status.m_mode = _mode;
        status.m_lifeSpan = _lifeSpan;
        status.m_sprite = Resources.Load<Sprite>(_sprite);
        status.m_color = _color;

        ApplyStatus(_owner.gameObject);
        _owner.UpdateStatusImages();
    }

    static public void NewStatus(CharacterScript _owner, string _name,
        effects _effect, mode _mode, int _lifeSpan, string _sprite, Color _color)
    {
        StatusScript status = FindOpenStatus(_owner);
        _owner.m_effects[(int)_effect] = true;
        status.m_mode = _mode;
        status.m_lifeSpan = _lifeSpan;
        status.m_sprite = Resources.Load<Sprite>(_sprite);
        status.m_color = _color;

        ApplyStatus(_owner.gameObject);
        _owner.UpdateStatusImages();
    }

    static public void ApplyStatus(GameObject _character)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        for (int i = 2; i < charScript.m_stats.Length; i++) // Don't do this for the first index since that would reset HP
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

    public void RemoveEffect()
    {
        switch (m_name)
        {
            case "ATK(Crash)":
                List<int> viableActs = new List<int>();
                for (int i = 0; i < m_charScript.m_isDiabled.Length; i++)
                    if (m_charScript.m_isDiabled[i] == 1)
                        viableActs.Add(i);
         
                if (viableActs.Count < 1)
                    return;

                m_charScript.m_isDiabled[viableActs[Random.Range(0, viableActs.Count)]] = 0;
                break;
            case "SUP(Explosive)":
                m_charScript.m_effects[(int)effects.CAREFUL] = false;
                break;
            case "ATK(Hinder)":
                m_charScript.m_effects[(int)effects.HINDER] = false;
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
            case "ATK(Ruin)":
                m_charScript.m_effects[(int)effects.SCARRING] = false;
                break;
            case "ATK(Rust)":
                m_charScript.m_effects[(int)effects.BLEED] = false;
                break;
            case "SUP(Secure)":
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
                _charScript.ReceiveDamage((int.Parse(tMesh.text) + 1).ToString(), Color.white);
                break;
            case "ATK(Maintain)":
                _charScript.HealHealth(int.Parse(tMesh.text) + 1);
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
