using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupScript : ObjectScript {

    public enum powerups { GREEN_ENG, RED_ENG, WHITE_ENG, BLUE_ENG, ALL_ENG,
                        DMG, TEC, MOV, RNG,
                        HP, SPD,
                        WEAK, HURT, SLOW}
    public enum typ { ENERGY, STAT, RESTORATIVE, NEGATIVE};

    // Types of powerups: 
    // ENERGY: 2 energy of a single color, 1 energy of all colors
    // STAT: +1 permanent buff to DMG, SPD, RNG or TEC
    // RESTORATIVE: +3 health or spd

    public powerups m_name;
    public Sprite m_sprite;
    public Transform m_transform;
    private bool m_upward;
    public Color m_color;
    public string m_effect;
    public ParticleSystem m_particle;

    // Use this for initialization
    void Start ()
    {
        CreateRandom();
        m_upward = true;
	}
	
	// Update is called once per frame
	new void Update ()
    {
        base.Update();
        m_transform.LookAt(2 * m_transform.position - m_boardScript.m_camera.transform.position);

        if (m_upward)
            transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + 0.002f, transform.position.z), transform.rotation);
        else
            transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - 0.002f, transform.position.z), transform.rotation);

        if (m_upward && transform.position.y > -36.5 || !m_upward && transform.position.y < -36.7)
            m_upward = !m_upward;
    }

    private void OnMouseDown()
    {
        m_boardScript.m_highlightedTile.OnMouseDown();
    }

    public void CreateRandom()
    {
        typ type = (typ)Random.Range(0, 4);
        MeshRenderer mRend = GetComponentInChildren<MeshRenderer>();

        if (type == typ.ENERGY)
        {
            int energy = Random.Range(0, 5);
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");

            if (energy == 0)
            {
                m_name = powerups.GREEN_ENG;
                m_color = CharacterScript.c_green;
                mRend.material.color = CharacterScript.c_green;
                m_effect = "Gain 2 Green energy.";
            }
            else if (energy == 1)
            {
                m_name = powerups.RED_ENG;
                m_color = CharacterScript.c_red;
                mRend.material.color = CharacterScript.c_red;
                m_effect = "Gain 2 Red energy.";
            }
            else if (energy == 2)
            {
                m_name = powerups.WHITE_ENG;
                m_color = CharacterScript.c_white;
                mRend.material.color = CharacterScript.c_white;
                m_effect = "Gain 2 White energy.";
            }
            else if (energy == 3)
            {
                m_name = powerups.BLUE_ENG;
                m_color = CharacterScript.c_blue;
                mRend.material.color = CharacterScript.c_blue;
                m_effect = "Gain 2 Blue energy.";
            }
            else if (energy == 4)
            {
                m_name = powerups.ALL_ENG;
                m_color = Color.magenta;
                mRend.material.color = Color.magenta;
                m_effect = "Gain 1 Energy of each color.";
            }
        }
        else if (type == typ.STAT)
        {
            int stat = Random.Range(0, 4);
            m_color = StatusScript.c_buffColor;

            if (stat == 0)
            {
                m_name = powerups.DMG;
                m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Damage Symbol");
                m_effect = "Receive +1 Damage for the fight.";
            }
            if (stat == 1)
            {
                m_name = powerups.TEC;
                m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Tech Symbol");
                m_effect = "Receive +1 Tech for the fight.";
            }
            else if (stat == 2)
            {
                m_name = powerups.MOV;
                m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Move Symbol");
                m_effect = "Receive +1 Move for the fight.";
            }
            else if (stat == 3)
            {
                m_name = powerups.RNG;
                m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Range Symbol");
                m_effect = "Receive +1 Range for the fight.";
            }

            mRend.material.color = StatusScript.c_buffColor;
        }
        else if (type == typ.RESTORATIVE)
        {
            int res = Random.Range(0, 2);
            m_color = StatusScript.c_buffColor;

            if (res == 0)
            {
                m_name = powerups.HP;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
                m_effect = "Heal 3 Health.";
            }
            else if (res == 1)
            {
                m_name = powerups.SPD;
                m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
                m_effect = "Receive 3 Speed.";
            }

            mRend.material.color = StatusScript.c_buffColor;
        }
        else if (type == typ.NEGATIVE)
        {
            int neg = Random.Range(0, 3);
            m_color = StatusScript.c_debuffColor;

            if (neg == 0)
            {
                m_name = powerups.HURT;
                m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
                m_effect = "Receive -3 Health.";
            }
            else if (neg == 1)
            {
                m_name = powerups.SLOW;
                m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
                m_effect = "Receive -3 Speed.";
            }
            else if (neg == 2)
            {
                m_name = powerups.WEAK;
                m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
                mRend.material = Resources.Load<Material>("Symbols/Materials/Defense Symbol");
                m_effect = "Receive -1 Defense for the fight.";
            }

            mRend.material.color = StatusScript.c_debuffColor;
        }

        m_particle.startColor = m_color;
    }

    public void OnPickup(CharacterScript _char)
    {
        if (m_name == powerups.GREEN_ENG)
            _char.m_player.m_energy[(int)PlayerScript.eng.GRN] += 2;
        else if (m_name == powerups.RED_ENG)
            _char.m_player.m_energy[(int)PlayerScript.eng.RED] += 2;
        else if (m_name == powerups.WHITE_ENG)
            _char.m_player.m_energy[(int)PlayerScript.eng.WHT] += 2;
        else if (m_name == powerups.BLUE_ENG)
            _char.m_player.m_energy[(int)PlayerScript.eng.BLU] += 2;
        else if (m_name == powerups.ALL_ENG)
            for (int i = 0; i < _char.m_player.m_energy.Length; i++)
                _char.m_player.m_energy[i]++;
        else if (m_name == powerups.DMG)
            _char.m_stats[(int)CharacterScript.sts.DMG]++;
        else if (m_name == powerups.TEC)
            _char.m_stats[(int)CharacterScript.sts.TEC]++;
        else if (m_name == powerups.MOV)
            _char.m_stats[(int)CharacterScript.sts.MOV]++;
        else if (m_name == powerups.RNG)
            _char.m_stats[(int)CharacterScript.sts.RNG]++;
        else if (m_name == powerups.HP)
            _char.HealHealth(3);
        else if (m_name == powerups.SPD)
            _char.m_tempStats[(int)CharacterScript.sts.SPD] += 3;
        else if (m_name == powerups.WEAK)
            _char.m_stats[(int)CharacterScript.sts.DEF]--;
        else if (m_name == powerups.HURT)
            _char.ReceiveDamage(3.ToString(), Color.white);
        else if (m_name == powerups.SLOW)
            _char.m_tempStats[(int)CharacterScript.sts.SPD] -= 3;

        StatusScript.ApplyStatus(_char.gameObject);

        if (m_boardScript.m_currCharScript == _char)
            PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();

        Destroy(gameObject);
    }
}
