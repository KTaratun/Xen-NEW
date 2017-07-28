using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterScript : MonoBehaviour {

    public enum sts { HP, SPD, HIT, EVA, CRT, DMG, DEF, MOV, RNG, TOT };
    enum trn { MOV, ACT };

    public GameObject tile;
    public BoardScript boardScript;
    public GameObject turnPanel;
    public GameObject popupText;
    public GameObject player;
    public string[] actions;
    public string currAction;
    public int[] stats;
    public int[] tempStats;
    public string[] accessories;
    public string color;

	// Use this for initialization
	void Start ()
    {
        popupText.SetActive(false);

        accessories = new string[2];
        accessories[0] = "Ring of DEATH";
        stats = new int[(int)sts.TOT];
        stats[(int)sts.HP] = 12;
        stats[(int)sts.SPD] = 10;
        stats[(int)sts.MOV] = 5;
        tempStats = new int[(int)sts.TOT];

        for (int i = 0; i < tempStats.Length; i++)
            tempStats[i] = stats[i];
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Move towards new tile
        if (tile && transform.position != tile.transform.position)
        {
            // Determine how much the character will be moving this update
            float charAcceleration = 0.04f;
            float charSpeed = 0.03f;
            float charMovement = Vector3.Distance(transform.position, tile.transform.position) * charAcceleration + charSpeed;
            transform.SetPositionAndRotation(new Vector3(transform.position.x + transform.forward.x * charMovement, transform.position.y, transform.position.z + transform.forward.z * charMovement), transform.rotation);

            float snapDistance = 0.02f;
            if (Vector3.Distance(transform.position, tile.transform.position) < snapDistance)
                transform.SetPositionAndRotation(tile.transform.position, new Quaternion());
        }

        // If it's my turn and I did all my actions, reset everything and end my turn
        if (boardScript)
        {
            PanelScript mainPanelScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
            if (boardScript.currPlayer == gameObject && mainPanelScript.buttons[(int)PanelScript.butts.MOV_BUTT].interactable == false 
                && mainPanelScript.buttons[(int)PanelScript.butts.ACT_BUTT].interactable == false)
            {
                boardScript.NewTurn();
                mainPanelScript.buttons[(int)PanelScript.butts.MOV_BUTT].interactable = true;
                mainPanelScript.buttons[(int)PanelScript.butts.ACT_BUTT].interactable = true;
            }
        }

        if (popupText.activeSelf)
        {
            float fadeSpeed = .01f;
            popupText.transform.LookAt(2 * popupText.transform.position - boardScript.camera.transform.position);
            TextMesh textMesh = popupText.GetComponent<TextMesh>();
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, textMesh.color.a - fadeSpeed);

            if (textMesh.color.a <= 0)
            {
                textMesh.color = Color.white;
                popupText.SetActive(false);
            }
        }
    }

    private void OnMouseDown()
    {
        TileScript tileScript = tile.GetComponent<TileScript>();
        tileScript.OnMouseDown();
    }

    public void MovementSelection()
    {
        FetchTilesWithinRange(stats[(int)sts.MOV], new Color (0, 0, 1, 0.5f), true);
        PanelScript mainPanScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanScript.inView = false;
    }

    public void Movement(TileScript selectedScript, TileScript newScript)
    {
        newScript.holding = selectedScript.holding;
        selectedScript.holding = null;
        tile = newScript.gameObject;
        boardScript.currTile = tile;
        transform.LookAt(tile.transform);
        PanelScript mainPanelScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
    }

    public void ActionSelection()
    {
        PanelScript mainPanScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript aPScript = boardScript.panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        mainPanScript.inView = false;
        aPScript.inView = true;

        aPScript.character = gameObject;
        aPScript.cScript = this;
        aPScript.PopulateActionButtons(actions);
    }

    public void ActionTargeting()
    {;
        string[] actsSeparated = currAction.Split('|');
        string[] rng = actsSeparated[5].Split(':');

        FetchTilesWithinRange(int.Parse(rng[1]), new Color(1, 0, 0, 0.5f), false);

        PanelScript actionPanScript = boardScript.panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        actionPanScript.inView = false;
    }

    public void Action(GameObject[] targets)
    {
        PanelScript mainPanelScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;

        string[] actsSeparated = currAction.Split('|');
        string[] engPreSplit = actsSeparated[2].Split(':');
        string eng = engPreSplit[1];
        string[] hit = actsSeparated[3].Split(':');
        string[] dmg = actsSeparated[4].Split(':');
        string[] crt = actsSeparated[6].Split(':');

        for (int i = 0; i < targets.Length; i++)
        {
            CharacterScript targetScript = targets[i].GetComponent<CharacterScript>();
            targetScript.popupText.SetActive(true);
            TextMesh textMesh = targetScript.popupText.GetComponent<TextMesh>();

            int roll = Random.Range(0, 20);

            if (roll >= int.Parse(crt[1]) + tempStats[(int)sts.CRT])
            {
                targetScript.tempStats[(int)sts.HP] -= int.Parse(dmg[1]) * 2;
                textMesh.text = (int.Parse(dmg[1]) * 2).ToString();
                textMesh.color = Color.red;
                EnergyConversion(eng);

                return;
            }

            textMesh.color = Color.white;

            if (roll < int.Parse(hit[1]) - tempStats[(int)sts.HIT] + targetScript.tempStats[(int)sts.EVA])
                textMesh.text = "MISS";
            else
            {
                targetScript.tempStats[(int)sts.HP] -= int.Parse(dmg[1]);
                textMesh.text = dmg[1];
                EnergyConversion(eng);
            }
        }
    }

    private void EnergyConversion(string energy)
    {
        PlayerScript playScript = player.GetComponent<PlayerScript>();
        // Assign energy symbols
        for (int i = 0; i < energy.Length; i++)
        {
            if (energy[i] == 'g')
                playScript.energy[0] += 1;
            else if (energy[i] == 'r')
                playScript.energy[1] += 1;
            else if (energy[i] == 'w')
                playScript.energy[2] += 1;
            else if (energy[i] == 'b')
                playScript.energy[3] += 1;
            else if (energy[i] == 'G')
                playScript.energy[0] -= 1;
            else if (energy[i] == 'R')
                playScript.energy[1] -= 1;
            else if (energy[i] == 'W')
                playScript.energy[2] -= 1;
            else if (energy[i] == 'B')
                playScript.energy[3] -= 1;
        }

        playScript.SetEnergyPanel();
    }

    private void FetchTilesWithinRange(int range, Color color, bool isMove)
    {
        TileScript parentScript = tile.GetComponent<TileScript>();

        // REFACTOR: Maybe less lists?
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        // Start with current tile in oddGen
        oddGen.Add(tile.GetComponent<TileScript>());

        for (int i = 0; i < range; i++)
        {
            // Alternate between gens. Unload current gen and load up the gen and then swap next iteration
            if (oddGen.Count > 0)
            {
                workingList = oddGen;
                storingList = evenGen;
            }
            else if (evenGen.Count > 0)
            {
                workingList = evenGen;
                storingList = oddGen;
            }

            while (workingList.Count > 0)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (!workingList[0].neighbors[k])
                        continue;

                    TileScript tScript = workingList[0].neighbors[k].GetComponent<TileScript>();

                    if (isMove && tScript.holding || !isMove && workingList[0].neighbors[k] == boardScript.selected)
                        continue;

                    if (workingList[0].neighbors[k])
                    {
                        Renderer tR = workingList[0].neighbors[k].GetComponent<Renderer>();
                        if (tR.material.color != color)
                        {
                            tR.material.color = color;
                            storingList.Add(workingList[0].neighbors[k].GetComponent<TileScript>());
                            parentScript.radius.Add(workingList[0].neighbors[k]);
                        }
                    }
                }
                workingList.RemoveAt(0);
            }
        }
    }

    public void Pass()
    {
        PanelScript mainPanelScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        mainPanelScript.buttons[(int)PanelScript.butts.MOV_BUTT].interactable = false;
        mainPanelScript.buttons[(int)PanelScript.butts.ACT_BUTT].interactable = false;
        mainPanelScript.inView = false;
    }

    public void Status()
    {
        PanelScript sPScript = boardScript.panels[(int)BoardScript.pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript mainPanScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript auxPanScript = boardScript.panels[(int)BoardScript.pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();

        if (mainPanScript.inView)
        {
            mainPanScript.inView = false;
            sPScript.parent = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL];
        }
        else if (auxPanScript.inView)
        {
            auxPanScript.inView = false;
            sPScript.parent = boardScript.panels[(int)BoardScript.pnls.AUXILIARY_PANEL];
        }

        sPScript.inView = true;
        sPScript.character = gameObject;
        sPScript.cScript = this;
        sPScript.PopulateText();
    }
}
