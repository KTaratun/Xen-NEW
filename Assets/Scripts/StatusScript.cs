using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusScript : MonoBehaviour {

    public enum mode { TURN_END, ROUND_END }
    public enum effects { SCARRING, BLEED, BYPASS, HINDER, IMMOBILE, WARD, STUN, DELAY, TOT }

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
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void NewStatus(GameObject _character, string _name)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        _character.AddComponent<StatusScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        statScripts[statScripts.Length - 1].m_statMod = new int[(int)CharacterScript.sts.TOT];
        statScripts[statScripts.Length - 1].StatusInit(charScript, _name);
        ApplyStatus(_character);
        charScript.UpdateStatusImages();
    }

    public void StatusInit(CharacterScript _charScript, string _action)
    {
        string actName = DatabaseScript.GetActionData(_action, DatabaseScript.actions.NAME);
        string actEffect = DatabaseScript.GetActionData(_action, DatabaseScript.actions.EFFECT);

        Color buffColor = Color.cyan;
        Color debuffColor = new Color(1, .7f, 0, 1);
        Color statusColor = Color.magenta;

        m_name = actName;
        m_action = _action;
        m_charScript = _charScript;
        m_effect = actEffect;

        switch (actName)
        {
            case "Spot":
                m_statMod[(int)CharacterScript.sts.RNG] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                m_color = buffColor;
                break;
            case "Passage":
                m_statMod[(int)CharacterScript.sts.MOV] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = buffColor;
                break;
            case "Explosive":
                m_statMod[(int)CharacterScript.sts.RAD] = 1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Radius Symbol");
                m_color = buffColor;
                break;
            case "Winding ATK":
                m_statMod[(int)CharacterScript.sts.EVA] = -2;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Evasion Symbol");
                m_color = debuffColor;
                break;
            case "Weakening ATK":
                m_statMod[(int)CharacterScript.sts.DEF] = -1;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = debuffColor;
                break;
            case "Scarring ATK":
                m_charScript.m_effects[(int)effects.SCARRING] = true;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = statusColor;
                break;
            case "Boost":
                m_statMod[(int)CharacterScript.sts.DMG] = 2;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = buffColor;
                break;
            case "AIM":
                m_statMod[(int)CharacterScript.sts.HIT] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Hit Symbol");
                m_color = buffColor;
                break;
            case "Bleed ATK":
                m_charScript.m_effects[(int)effects.BLEED] = true;
                m_statMod[(int)CharacterScript.sts.HP] = 2;
                m_mode = mode.TURN_END;
                m_lifeSpan = 3;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = debuffColor;
                break;
            case "Critical":
                m_statMod[(int)CharacterScript.sts.CRT] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Critical Symbol");
                m_color = buffColor;
                break;
            case "Hindering ATK":
                m_charScript.m_effects[(int)effects.HINDER] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Ability Symbol");
                m_color = statusColor;
                break;
            case "Unnerving ATK":
                m_statMod[(int)CharacterScript.sts.CRT] = -2;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Critical Symbol");
                m_color = debuffColor;
                break;
            case "Defensive ATK":
                m_statMod[(int)CharacterScript.sts.DMG] = -1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = debuffColor;
                break;
            case "Blinding ATK":
                m_statMod[(int)CharacterScript.sts.HIT] = -3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Hit Symbol");
                m_color = debuffColor;
                break;
            case "Fortifying ATK":
                m_statMod[(int)CharacterScript.sts.DEF] = 1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = buffColor;
                break;
            case "Immobilizing ATK":
                m_charScript.m_effects[(int)effects.IMMOBILE] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = statusColor;
                break;
            case "Ward ATK":
                m_charScript.m_effects[(int)effects.WARD] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = statusColor;
                break;
            case "Bolstering ATK":
                m_statMod[(int)CharacterScript.sts.EVA] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Evasion Symbol");
                m_color = buffColor;
                break;
            case "Prepare":
                m_statMod[(int)CharacterScript.sts.EVA] = 5;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Evasion Symbol");
                m_color = buffColor;
                break;
            case "Protect":
                m_statMod[(int)CharacterScript.sts.DEF] = 2;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = buffColor;
                break;
            case "Stun ATK":
                m_charScript.m_effects[(int)effects.STUN] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Action Symbol");
                m_color = statusColor;
                break;
            case "Delay ATK":
                m_charScript.m_effects[(int)effects.DELAY] = true;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
                m_color = statusColor;
                break;
            default:
                break;
        }
    }

    static public void ApplyStatus(GameObject _character)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        for (int i = 1; i < charScript.m_stats.Length; i++) // Don't do this for the first index since that would reset HP
        {
            charScript.m_tempStats[i] = charScript.m_stats[i];

            for (int j = 0; j < statScripts.Length; j++)
                charScript.m_tempStats[i] += statScripts[j].m_statMod[i];
        }
    }

    static public void UpdateStatus(GameObject _character, mode _mode)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_mode == _mode)
            {
                statScripts[i].m_lifeSpan--;
                if (statScripts[i].m_lifeSpan <= 0)
                    statScripts[i].DestroyStatus(_character);
                else
                    PerformEffect(statScripts[i], charScript);
            }
        }
    }

    public void DestroyStatus(GameObject _character)
    {
        RemoveEffect();
        DestroyImmediate(this);
        ApplyStatus(_character);
        _character.GetComponent<CharacterScript>().UpdateStatusImages();
    }

    static public void DestroyAll(GameObject _character)
    {
        StatusScript[] statScripts = _character.GetComponents<StatusScript>();

        for (int i = 0; i < statScripts.Length; i++)
            statScripts[i].DestroyStatus(_character);
    }

    public void RemoveEffect()
    {
        switch (m_name)
        {
            case "Scarring ATK":
                m_charScript.m_effects[(int)effects.SCARRING] = false;
                break;
            case "Bleed ATK":
                m_charScript.m_effects[(int)effects.BLEED] = false;
                break;
            case "Hindering ATK":
                m_charScript.m_effects[(int)effects.HINDER] = false;
                break;
            case "Immobilize ATK":
                m_charScript.m_effects[(int)effects.IMMOBILE] = false;
                break;
            case "Ward ATK":
                m_charScript.m_effects[(int)effects.WARD] = false;
                break;
            case "Stun ATK":
                m_charScript.m_effects[(int)effects.STUN] = false;
                break;
            case "Delay ATK":
                m_charScript.m_effects[(int)effects.DELAY] = false;
                break;
            default:
                break;
        }
    }

    static public void PerformEffect(StatusScript _status, CharacterScript _charScript)
    {
        switch (_status.m_name)
        {
            case "Bleed ATK":
                TextMesh tMesh = _charScript.m_popupText.GetComponent<TextMesh>();
                _charScript.ReceiveDamage((int.Parse(tMesh.text) + _status.m_statMod[(int)CharacterScript.sts.HP]).ToString(), Color.white);
                break;
            default:
                break;
        }
    }
}
