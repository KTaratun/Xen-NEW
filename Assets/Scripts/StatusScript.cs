using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusScript : MonoBehaviour {

    public int[] m_statMod;
    public int m_mode; // mode 0: lifeSpan depletes on end of turn; mode 1: lifeSpan depletes on end of round
    public int m_lifeSpan;
    public Sprite m_sprite;
    public Color m_color;

	// Use this for initialization
	void Start () {
        
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
        statScripts[statScripts.Length - 1].StatusInit(_id);
        ApplyStatus(_character);
        charScript.UpdateStatusImages();
    }

    public void StatusInit(int _id)
    {
        switch (_id)
        {
            case 9: // Spot
                m_statMod[(int)CharacterScript.sts.RNG] = 3;
                m_mode = 0;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                m_color = Color.cyan;
                break;
            case 10: // Passage
                m_statMod[(int)CharacterScript.sts.MOV] = 3;
                m_mode = 0;
                m_lifeSpan = 1;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                m_color = Color.cyan;
                break;
            default:
                break;
        }
    }

    static public void ApplyStatus(GameObject _character)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        StatusScript[] statScripts = charScript.GetComponents<StatusScript>();

        for (int i = 0; i < charScript.m_stats.Length; i++)
        {
            charScript.m_tempStats[i] = charScript.m_stats[i];

            for (int j = 0; j < statScripts.Length; j++)
                charScript.m_tempStats[i] += statScripts[j].m_statMod[i];
        }
    }

    static public void UpdateStatus(GameObject _character, int _mode)
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
                    DestroyImmediate(statScripts[i]);
                    ApplyStatus(_character);
                }
            }
        }
    }
}
