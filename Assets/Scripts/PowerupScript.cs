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

    public Sprite m_sprite;
    public Transform m_transform;
    private bool m_upward;
    public Color m_color;
    public string m_effect;
    public ParticleSystem m_particle;

    // Use this for initialization
    protected new void Start ()
    {
        base.Start();
	}

    public void Init(int _ind)
    {
        if (_ind == -1)
            _ind = Random.Range(0, 13);

        name = _ind.ToString();

        PopulatePowerUp(_ind);

        m_upward = true;
    }

    // Update is called once per frame
    new void Update ()
    {
        base.Update();
        m_transform.LookAt(2 * m_transform.position - m_boardScript.m_camera.transform.position);

        float bobSpeed = .03f;

        if (m_upward)
            transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + bobSpeed, transform.position.z), transform.rotation);
        else
            transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - bobSpeed, transform.position.z), transform.rotation);

        if (m_upward && transform.position.y > 4 || !m_upward && transform.position.y < 0)
            m_upward = !m_upward;
    }

    private void OnMouseDown()
    {
        m_boardScript.m_highlightedTile.OnMouseDown();
    }

    private void PopulatePowerUp(int _ind)
    {
        MeshRenderer mRend = GetComponentInChildren<MeshRenderer>();

        if (_ind == 0)
        {
            m_name = powerups.GREEN_ENG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            m_color = CharacterScript.c_green;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
            m_effect = "Gain 2 Green energy.";
        }
        else if (_ind == 1)
        {
            m_name = powerups.RED_ENG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            m_color = CharacterScript.c_red;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
            m_effect = "Gain 2 Red energy.";
        }
        else if (_ind == 2)
        {
            m_name = powerups.WHITE_ENG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            m_color = CharacterScript.c_white;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
            m_effect = "Gain 2 White energy.";
        }
        else if (_ind == 3)
        {
            m_name = powerups.BLUE_ENG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            m_color = CharacterScript.c_blue;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
            m_effect = "Gain 2 Blue energy.";
        }
        else if (_ind == 4)
        {
            m_name = powerups.ALL_ENG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
            m_color = Color.magenta;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
            m_effect = "Gain 1 Energy of each color.";
        }
        else if (_ind == 5)
        {
            m_name = powerups.DMG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Damage Symbol");
            m_effect = "Receive +1 Damage for the fight.";
        }
        else if (_ind == 6)
        {
            m_name = powerups.TEC.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Tech Symbol");
            m_effect = "Receive +1 Tech for the fight.";
        }
        else if (_ind == 7)
        {
            m_name = powerups.MOV.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Move Symbol");
            m_effect = "Receive +1 Move for the fight.";
        }
        else if (_ind == 8)
        {
            m_name = powerups.RNG.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Range Symbol");
            m_effect = "Receive +1 Range for the fight.";
        }
        else if (_ind == 9)
        {
            m_name = powerups.HP.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
            m_effect = "Heal 3 Health.";
        }
        else if (_ind == 10)
        {
            m_name = powerups.SPD.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
            m_color = StatusScript.c_buffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
            m_effect = "Receive 3 Speed.";
        }
        else if (_ind == 11)
        {
            m_name = powerups.HURT.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
            m_color = StatusScript.c_debuffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
            m_effect = "Receive -3 Health.";
        }
        else if (_ind == 12)
        {
            m_name = powerups.SLOW.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
            m_color = StatusScript.c_debuffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
            m_effect = "Receive -3 Speed.";
        }
        else if (_ind == 13)
        {
            m_name = powerups.WEAK.ToString();
            m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
            m_color = StatusScript.c_debuffColor;
            mRend.material = Resources.Load<Material>("Symbols/Materials/Defense Symbol");
            m_effect = "Receive -1 Defense for the fight.";
        }

        mRend.material.color = m_color;
        m_particle.startColor = m_color;
        
    }

    //public void RandomPowerUp()
    //{
    //    typ type = (typ)Random.Range(0, 4);
    //    MeshRenderer mRend = GetComponentInChildren<MeshRenderer>();
    //
    //    if (type == typ.ENERGY)
    //    {
    //        int energy = Random.Range(0, 5);
    //        m_sprite = Resources.Load<Sprite>("Symbols/Energy Symbol");
    //        mRend.material = Resources.Load<Material>("Symbols/Materials/Energy Symbol");
    //
    //        if (energy == 0)
    //        {
    //            m_name = powerups.GREEN_ENG;
    //            m_color = CharacterScript.c_green;
    //            mRend.material.color = CharacterScript.c_green;
    //            m_effect = "Gain 2 Green energy.";
    //        }
    //        else if (energy == 1)
    //        {
    //            m_name = powerups.RED_ENG;
    //            m_color = CharacterScript.c_red;
    //            mRend.material.color = CharacterScript.c_red;
    //            m_effect = "Gain 2 Red energy.";
    //        }
    //        else if (energy == 2)
    //        {
    //            m_name = powerups.WHITE_ENG;
    //            m_color = CharacterScript.c_white;
    //            mRend.material.color = CharacterScript.c_white;
    //            m_effect = "Gain 2 White energy.";
    //        }
    //        else if (energy == 3)
    //        {
    //            m_name = powerups.BLUE_ENG;
    //            m_color = CharacterScript.c_blue;
    //            mRend.material.color = CharacterScript.c_blue;
    //            m_effect = "Gain 2 Blue energy.";
    //        }
    //        else if (energy == 4)
    //        {
    //            m_name = powerups.ALL_ENG;
    //            m_color = Color.magenta;
    //            mRend.material.color = Color.magenta;
    //            m_effect = "Gain 1 Energy of each color.";
    //        }
    //    }
    //    else if (type == typ.STAT)
    //    {
    //        int stat = Random.Range(0, 4);
    //        m_color = StatusScript.c_buffColor;
    //
    //        if (stat == 0)
    //        {
    //            m_name = powerups.DMG;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Damage Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Damage Symbol");
    //            m_effect = "Receive +1 Damage for the fight.";
    //        }
    //        if (stat == 1)
    //        {
    //            m_name = powerups.TEC;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Tech Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Tech Symbol");
    //            m_effect = "Receive +1 Tech for the fight.";
    //        }
    //        else if (stat == 2)
    //        {
    //            m_name = powerups.MOV;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Move Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Move Symbol");
    //            m_effect = "Receive +1 Move for the fight.";
    //        }
    //        else if (stat == 3)
    //        {
    //            m_name = powerups.RNG;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Range Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Range Symbol");
    //            m_effect = "Receive +1 Range for the fight.";
    //        }
    //
    //        mRend.material.color = StatusScript.c_buffColor;
    //    }
    //    else if (type == typ.RESTORATIVE)
    //    {
    //        int res = Random.Range(0, 2);
    //        m_color = StatusScript.c_buffColor;
    //
    //        if (res == 0)
    //        {
    //            m_name = powerups.HP;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
    //            m_effect = "Heal 3 Health.";
    //        }
    //        else if (res == 1)
    //        {
    //            m_name = powerups.SPD;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
    //            m_effect = "Receive 3 Speed.";
    //        }
    //
    //        mRend.material.color = StatusScript.c_buffColor;
    //    }
    //    else if (type == typ.NEGATIVE)
    //    {
    //        int neg = Random.Range(0, 3);
    //        m_color = StatusScript.c_debuffColor;
    //
    //        if (neg == 0)
    //        {
    //            m_name = powerups.HURT;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Life Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Life Symbol");
    //            m_effect = "Receive -3 Health.";
    //        }
    //        else if (neg == 1)
    //        {
    //            m_name = powerups.SLOW;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Speed Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Speed Symbol");
    //            m_effect = "Receive -3 Speed.";
    //        }
    //        else if (neg == 2)
    //        {
    //            m_name = powerups.WEAK;
    //            m_sprite = Resources.Load<Sprite>("Symbols/Defense Symbol");
    //            mRend.material = Resources.Load<Material>("Symbols/Materials/Defense Symbol");
    //            m_effect = "Receive -1 Defense for the fight.";
    //        }
    //
    //        mRend.material.color = StatusScript.c_debuffColor;
    //    }
    //
    //    m_particle.startColor = m_color;
    //}

    public void OnPickup(CharacterScript _char)
    {
        if (m_name == powerups.GREEN_ENG.ToString())
            _char.m_player.m_energy[(int)PlayerScript.eng.GRN] += 2;
        else if (m_name == powerups.RED_ENG.ToString())
            _char.m_player.m_energy[(int)PlayerScript.eng.RED] += 2;
        else if (m_name == powerups.WHITE_ENG.ToString())
            _char.m_player.m_energy[(int)PlayerScript.eng.WHT] += 2;
        else if (m_name == powerups.BLUE_ENG.ToString())
            _char.m_player.m_energy[(int)PlayerScript.eng.BLU] += 2;
        else if (m_name == powerups.ALL_ENG.ToString())
            for (int i = 0; i < _char.m_player.m_energy.Length; i++)
                _char.m_player.m_energy[i]++;
        else if (m_name == powerups.DMG.ToString())
            _char.m_stats[(int)CharacterScript.sts.DMG]++;
        else if (m_name == powerups.TEC.ToString())
            _char.m_stats[(int)CharacterScript.sts.TEC]++;
        else if (m_name == powerups.MOV.ToString())
            _char.m_stats[(int)CharacterScript.sts.MOV]++;
        else if (m_name == powerups.RNG.ToString())
            _char.m_stats[(int)CharacterScript.sts.RNG]++;
        else if (m_name == powerups.HP.ToString())
            _char.HealHealth(3);
        else if (m_name == powerups.SPD.ToString())
            _char.m_tempStats[(int)CharacterScript.sts.SPD] += 3;
        else if (m_name == powerups.WEAK.ToString())
            _char.m_stats[(int)CharacterScript.sts.DEF]--;
        else if (m_name == powerups.HURT.ToString())
            _char.ReceiveDamage(3.ToString(), Color.white);
        else if (m_name == powerups.SLOW.ToString())
            _char.m_tempStats[(int)CharacterScript.sts.SPD] -= 3;

        StatusScript.ApplyStatus(_char.gameObject);
        _char.PlayAnimation(CharacterScript.prtcles.GAIN_STATUS, m_color);

        if (m_boardScript.m_currCharScript == _char)
            PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();

        Destroy(gameObject);
    }
}
