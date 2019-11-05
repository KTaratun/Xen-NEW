using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : ObjectScript {

    public enum weaps { SWORD, HGUN };
    public enum sts { HP, SPD, DMG, DEF, MOV, RNG, TEC, RAD, TOT };
    public enum trn { MOV, ACT };
    public enum bod { HIPS, LEFT_UP_LEG, RIGHT_UP_LEG, SPINE, LEFT_LEG, RIGHT_LEG,
        SPINE_1, LEFT_FOOT, RIGHT_FOOT, SPINE_2, LEFT_TOE_BASE, RIGHT_TOE_BASE,
        LEFT_SHOULDER, NECK, RIGHT_SHOULDER, LEFT_TOE_END, RIGHT_TOE_END, LEFT_ARM,
        HEAD, RIGHT_ARM, LEFT_FORE_ARM, HEAD_TOP_END, RIGHT_FORE_ARM, LEFT_HAND,
        RIGHT_HAND}
    public enum prtcles { GUN_SHOT, PROJECTILE_HIT, CHAR_MARK, GAIN_STATUS}

    static public Color c_green = new Color(.45f, .7f, .4f, 1);
    static public Color c_red = new Color(.8f, .1f, .15f, 1);
    static public Color c_white = new Color(.8f, .8f, .8f, 1);
    static public Color c_blue = new Color(.45f, .4f, 1, 1);

    // General
    public string m_color;
    public int m_level;
    public int m_exp;
    public int m_gender;
    public string[] m_accessories;

    // Combat
    public PlayerScript m_player;
    public Color m_teamColor;
    public int[] m_stats;
    public int[] m_tempStats;
    public string[] m_actions;
    public string m_currAction;
    public List<GameObject> m_targets;

    // State Info
    public bool m_isAlive;
    public bool m_isFree;
    public int[] m_isDiabled;
    public bool m_isAI;
    public int[] m_hasActed;
    public StatusScript[] m_statuses;
    public int m_currStatus;
    public bool[] m_effects;

    // GUI
    public List<GameObject> m_turnPanels;
    public GameObject m_statusSymbol;
    public GameObject m_healthBar;
    public GameObject m_popupText;
    public GameObject[] m_popupSpheres;
    public GameObject[] m_colorDisplay;

    // References
    public List<GameObject> m_body;
    public GameObject[] m_weapons;
    public Animator m_anim;
    public GameObject[] m_particles;
    public AudioSource m_audio;



    // Use this for initialization
    new void Start ()
    {
        base.Start();
        // Stats
        if (m_stats.Length == 0)
            InitializeStats();

        m_tempStats = new int[(int)sts.TOT];
        for (int i = 0; i < m_stats.Length; i++)
            m_tempStats[i] = m_stats[i];

        // Status
        for (int i = 0; i < m_statuses.Length; i++)
            m_statuses[i] = gameObject.AddComponent<StatusScript>();

        // Audio
        m_audio = gameObject.AddComponent<AudioSource>();

        // Animation
        if (!m_anim)
            m_anim = GetComponentInChildren<Animator>();

        m_popupText.SetActive(false);
        m_isFree = false;

        InitBones();
        InitColors();

        // If characters have just action names, find full action data
        if (m_actions.Length > 0)
            Invoke("RetrieveActions", .1f);

        if (!m_isAlive)
            CharInit();

        //if (m_player)
        //    m_player.m_characters.Add(this);
    }

    public void CharInit()
    {
        m_isDiabled = new int[m_actions.Length];

        m_isAlive = true;
        m_effects = new bool[(int)StatusScript.effects.TOT];

        Renderer rend = transform.GetComponentInChildren<Renderer>();
        rend.materials[0].color = m_teamColor;

        if (m_hasActed.Length == 0)
            m_hasActed = new int[2];
    }

    public void InitializeStats()
    {
        m_stats = new int[(int)sts.TOT];

        m_stats[(int)sts.HP] = 12;
        m_stats[(int)sts.SPD] = 10;
        m_stats[(int)sts.MOV] = 5;


        //m_accessories = new string[2];
        //m_accessories[0] = "Ring of DEATH";
      
        m_level = 1;
    }

    private void InitBones()
    {
        List<GameObject> bodyParts = new List<GameObject>();
        bodyParts.Add(m_body[0]);

        while (bodyParts.Count > 0)
        {
            for (int i = 0; i < bodyParts[0].transform.childCount; i++)
            {
                if (!bodyParts[0].transform.GetChild(i).gameObject.activeSelf)
                    continue;
                m_body.Add(bodyParts[0].transform.GetChild(i).gameObject);
                bodyParts.Add(bodyParts[0].transform.GetChild(i).gameObject);
            }
            bodyParts.RemoveAt(0);
        }
    }

    private void InitColors()
    {
        SetPopupSpheres("");
        // Set up the spheres that pop up over the characters heads when they gain or lose energy
        for (int i = 0; i < m_popupSpheres.Length; i++)
        {
            MeshRenderer[] meshRends = m_popupSpheres[i].GetComponentsInChildren<MeshRenderer>();

            for (int j = 0; j < meshRends.Length; j++)
                meshRends[j].material.color = new Color(meshRends[j].material.color.r, meshRends[j].material.color.g, meshRends[j].material.color.b, 0);
        }

        //if (m_boardScript && m_boardScript.m_currCharScript != this)
        //{
        //    Renderer rend = transform.GetComponentInChildren<Renderer>();
        //    rend.materials[0].color = Color.red;
        //}
    }


    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (m_boardScript)
        {
            UpdateOverheadItems();

            if (m_particles[(int)prtcles.GUN_SHOT].activeSelf)
            {
                ParticleSystem[] pS = m_particles[(int)prtcles.GUN_SHOT].GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < pS.Length; i++)
                    pS[i].transform.SetPositionAndRotation(pS[i].transform.position, Quaternion.Euler(-90, 0, 0));
            }

            //if (m_boardScript.m_currCharScript == this && Input.GetKeyDown(KeyCode.P))
            //    GetComponentAITurn();
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
    override public void MovingStart(TileScript _newScript, bool _isForced, bool _isNetForced)
    {
        base.MovingStart(_newScript, _isForced, _isNetForced);

        if (m_isAlive)
            m_anim.Play("Running Melee", -1, 0);

        if (!_isForced)
        {
            m_hasActed[(int)trn.MOV] += 2;
            PanelManagerScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[(int)trn.MOV].interactable = false;
            if (m_boardScript.m_currButton)
                m_boardScript.m_currButton.GetComponent<Image>().color = Color.white;
        }

        print(m_name + " has started moving.\n");
    }

    override public void MovingFinish()
    {
        if (m_tile.m_holding && m_tile.m_holding.tag == "PowerUp")
            m_tile.m_holding.GetComponent<PowerupScript>().OnPickup(this);

        base.MovingFinish();

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

        print(m_name + " has finished moving.\n");
    }


    // Health/Damage
    override public void ReceiveDamage(string _dmg, Color _color)
    {
        TextMesh textMesh = m_popupText.GetComponent<TextMesh>();
        m_popupText.SetActive(true);
        textMesh.color = _color;

        if (int.Parse(_dmg) <= 0)
        {
            _dmg = "0";
            m_anim.Play("Block", -1, 0);
        }
        else
        {
            m_anim.Play("Hit", -1, 0);
            m_particles[(int)prtcles.PROJECTILE_HIT].transform.LookAt(m_boardScript.m_currCharScript.transform);
            m_particles[(int)prtcles.PROJECTILE_HIT].SetActive(false);
            m_particles[(int)prtcles.PROJECTILE_HIT].SetActive(true);
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Explosion Sound 3"));
        }

        int parsedDMG;
        if (int.TryParse(_dmg, out parsedDMG))
        {
            if (m_tempStats[(int)sts.HP] - parsedDMG <= 0)
                Dead();
            else
                m_tempStats[(int)sts.HP] -= parsedDMG;
        }

        int res = 0;
        if (int.TryParse(_dmg, out res))
            textMesh.text = (int.Parse(textMesh.text) + int.Parse(_dmg)).ToString();
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

        if (_hp < 0)
            _hp = 0;

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
            PanelScript actPanScript = PanelManagerScript.GetPanel("HUD Panel LEFT");
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


    // Utilities
    public void HighlightCharacter()
    {
        if (PanelManagerScript.GetPanel("HUD Panel RIGHT").m_slideScript.m_inView)
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
            PanelScript hudPanScript = PanelManagerScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_cScript = this;
            hudPanScript.PopulatePanel();
        }
    }

    public void SetPopupSpheres(string _energy)
    {
        GameObject[] colorSpheres = m_popupSpheres;
        if (_energy.Length == 0)
        {
            if (m_color == "")
                return;
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


    // The only reason why this exists is because statusscript needs a way to get to HUD LEFT
    public void UpdateStatusImages()
    {
        if (this == m_boardScript.m_currCharScript)
            PanelManagerScript.GetPanel("HUD Panel LEFT").PopulatePanel();
    }

    public void PlayAnimation(prtcles _ani, Color _color)
    {
        if (_ani == prtcles.GAIN_STATUS)
        {
            m_particles[(int)prtcles.GAIN_STATUS].SetActive(false);
            m_particles[(int)prtcles.GAIN_STATUS].SetActive(true);
            ParticleSystem[] ps = m_particles[(int)prtcles.GAIN_STATUS].GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < ps.Length; i++)
                ps[i].startColor = _color;
        }
    }

    //public void SparkRandomly()
    //{
    //    int randPart = Random.Range(0, m_body.Count);
    //    m_particle.transform.SetPositionAndRotation(m_body[randPart].transform.position, m_particle.transform.rotation);
    //}


    // MOVE TO AICONTROLLER
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
                MovingStart(mostViable.m_position, false, false);
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
                MovingStart(mostViable.m_position, false, false);
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
                m_effects[(int)StatusScript.effects.HINDER] && !ActionScript.CheckIfAttack(name) ||
                m_effects[(int)StatusScript.effects.WARD] && ActionScript.CheckIfAttack(name) ||
                m_isDiabled[i] > 0)
                continue;
            else if (_target != this)
            {
                for (int j = 0; j < _path.Count; j++)
                {
                    if (j > m_tempStats[(int)sts.MOV])
                        break;

                    ActionScript.ActionTargeting(this, _path[j]);

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
                    ActionScript.ActionTargeting(this, m_tile);

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
                if (ActionScript.CheckIfAttack(name) && _target.m_player == m_player || !ActionScript.CheckIfAttack(name) && _target != m_player)
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
}
