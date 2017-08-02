using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusScript : MonoBehaviour {

    public enum mode { TURN_END, ROUND_END }
    public enum effects { SCARRING, BLEED, BYPASS, TOT }

    public int m_id;
    public CharacterScript m_charScript;
    public int[] m_statMod;
    public mode m_mode;
    public int m_lifeSpan;
    public Sprite m_sprite;
    public Color m_color;

	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void NewStatus(GameObject _character, int _id)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        _character.AddComponent<StatusScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        statScripts[statScripts.Length - 1].m_statMod = new int[(int)CharacterScript.sts.TOT];
        statScripts[statScripts.Length - 1].StatusInit(charScript, _id);
        ApplyStatus(_character);
        charScript.UpdateStatusImages();
    }

    public void StatusInit(CharacterScript _charScript, int _id)
    {
        Color buffColor = Color.cyan;
        Color debuffColor = new Color(1, .7f, 0, 1);
        Color statusColor = Color.magenta;
        m_id = _id;
        m_charScript = _charScript;

        switch (_id)
        {
            case 9: // Spot
                m_statMod[(int)CharacterScript.sts.RNG] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                m_color = buffColor;
                break;
            case 10: // Passage
                m_statMod[(int)CharacterScript.sts.MOV] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = buffColor;
                break;
            case 14: // Explosive
                m_statMod[(int)CharacterScript.sts.RAD] = 1;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Radius Symbol");
                m_color = buffColor;
                break;
            case 18: // Winding ATK
                m_statMod[(int)CharacterScript.sts.EVA] = -2;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Evasion Symbol");
                m_color = debuffColor;
                break;
            case 22: // Weakening ATK
                m_statMod[(int)CharacterScript.sts.DEF] = -1;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                m_color = debuffColor;
                break;
            case 24: // Scarring ATK
                m_charScript.m_effects[(int)effects.SCARRING] = true;
                m_mode = mode.ROUND_END;
                m_lifeSpan = 2;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = statusColor;
                break;
            case 25: // Boost
                m_statMod[(int)CharacterScript.sts.DMG] = 2;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                m_color = buffColor;
                break;
            case 26: // AIM
                m_statMod[(int)CharacterScript.sts.HIT] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Hit Symbol");
                m_color = buffColor;
                break;
            case 28: // Bleed ATK
                m_charScript.m_effects[(int)effects.BLEED] = true;
                m_statMod[(int)CharacterScript.sts.HP] = 2;
                m_mode = mode.TURN_END;
                m_lifeSpan = 3;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                m_color = debuffColor;
                break;
            case 30: // Critical
                m_statMod[(int)CharacterScript.sts.CRT] = 3;
                m_mode = mode.TURN_END;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Critical Symbol");
                m_color = buffColor;
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

            for (int j = 1; j < statScripts.Length; j++)
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
                {
                    statScripts[i].RemoveEffect();
                    DestroyImmediate(statScripts[i]);
                    // TO DO: Slide all status effects down in hud
                    ApplyStatus(_character);
                }
                else
                    PerformEffect(statScripts[i], charScript);
            }
        }
    }

    public void RemoveEffect()
    {
        switch (m_id)
        {
            case 24: // Scarring ATK
                m_charScript.m_effects[(int)effects.SCARRING] = false;
                break;
            case 28: // Bleed ATK
                m_charScript.m_effects[(int)effects.BLEED] = false;
                break;
            default:
                break;
        }
    }

    static public void PerformEffect(StatusScript _status, CharacterScript _charScript)
    {
        switch (_status.m_id)
        {
            case 28: // Bleed ATK
                TextMesh tMesh = _charScript.m_popupText.GetComponent<TextMesh>();
                _charScript.ReceiveDamage((int.Parse(tMesh.text) + _status.m_statMod[(int)CharacterScript.sts.HP]).ToString(), Color.white);
                break;
            default:
                break;
        }
    }
}
