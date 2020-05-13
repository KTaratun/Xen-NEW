using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TutorialManagerScript : GameManagerScript
{
    private SlidingPanelScript m_dialogPan;
    private Image m_highlightedButton;
    private bool m_brighter;

    private List<CharacterScript> m_mainChars;
    private List<CharacterScript> m_enemies;
    private ConfirmationPanelScript m_confirm;
    private int m_errorNum = -1;

    private bool m_unlock = false;
    private GameObject m_tileHighlight;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        Button button = GameObject.Find("Dialog Panel").GetComponentInChildren<Button>();
        button.onClick.AddListener(() => Next());

        m_mainChars = new List<CharacterScript>();
        m_enemies = new List<CharacterScript>();

        m_confirm = GameObject.Find("Confirmation Panel").GetComponent<ConfirmationPanelScript>();
        m_confirm.m_errorCheck = CheckForError;

        m_tileHighlight = GameObject.Find("Tile Highlight");
        m_tileHighlight.SetActive(false);
    }

    // Update is called once per frame
    new void Update()
    {
        EndOfActionTimer();

        if (m_highlightedButton)
            MakeButtonFlash();
    }

    public bool CheckForError()
    {
        if (m_errorNum > -1 && Vector3.Distance(m_board.m_selected.transform.position, m_tileHighlight.transform.position) > 1)
            return true;

        return false;
        //if (m_errorNum == 0)
        //{
        //    if (Vector3.Distance(m_board.m_selected.transform.position, m_enemies[0].transform.position) > 10)
        //    {
        //        m_dialogPan.GetComponentInChildren<Text>().text = "Try to select the tile right in front of the closer target.";
        //        return true;
        //    }
        //}
        //else if (m_errorNum == 1)
        //{
        //    if (m_currCharScript.m_currAction)
        //    {
        //        if (m_board.m_selected.m_holding && m_board.m_selected.m_holding.name != "Red")
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Red needs the DEF boost right now.";
        //            return true;
        //        }
        //        else
        //            m_errorNum = -1;
        //    }
        //    else if (m_currCharScript.m_tile.m_id == 0)
        //    {
        //        if (Vector3.Distance(m_board.m_selected.transform.position, m_mainChars[0].transform.position) > 20)
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try to get within range of Red, then hit him with some fortification.";
        //            return true;
        //        }
        //        else if (TileLinkScript.CheckIfBlocked(m_board.m_selected, m_mainChars[0].m_tile))
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Looks like the opponent is blocking Red from this spot. Choose another one then use SUP(Fortify) on Red.";
        //            return true;
        //        }
        //    }

        //    return false;
        //}
        //else if (m_errorNum == 2)
        //{
        //    if (m_currCharScript.m_currAction)
        //    {
        //        if (m_currCharScript.m_currAction.m_name != "ATK(Magnet)")
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try using ATK(Magnet).";
        //            return true;
        //        }

        //        TileScript tarTile = m_mainChars[0].m_tile.m_neighbors[(int)TileLinkScript.nbors.TOP].m_neighbors[(int)TileLinkScript.nbors.LEFT].m_neighbors[(int)TileLinkScript.nbors.LEFT];

        //        if (m_board.m_selected != tarTile)
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try targeting the board where we can hit all of the enemies without hitting our own units.";
        //            return true;
        //        }
        //        else
        //            m_errorNum = -1;
        //    }
        //    else if (!m_hasActed[(int)trn.ACT])
        //    {
        //        TileScript moveTile = m_mainChars[0].m_tile.m_neighbors[(int)TileLinkScript.nbors.TOP].m_neighbors[(int)TileLinkScript.nbors.RIGHT].m_neighbors[(int)TileLinkScript.nbors.RIGHT];

        //        if (m_board.m_selected != moveTile)
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Let's move to that green energy so we can use ATK(Magnet).";
        //            return true;
        //        }
        //    }

        //    return false;
        //}
        //else if (m_errorNum == 3)
        //{
        //    if (m_currCharScript.m_currAction)
        //    {
        //        if (m_currCharScript.m_currAction.m_name != "SUP(Queue)")
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try using SUP(Queue).";
        //            return true;
        //        }

        //        if (m_board.m_selected.m_holding && m_board.m_selected.m_holding.name != "Red")
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "We need to hit Red with that to ensure he goes first next round.";
        //            return true;
        //        }
        //        else
        //            m_errorNum = -1;
        //    }
        //    else if (Vector3.Distance(m_board.m_selected.transform.position, m_mainChars[0].transform.position) >= 30)
        //    {
        //        m_dialogPan.GetComponentInChildren<Text>().text = "We need to be closer in order to target Red with SUP(Queue).";
        //        return true;
        //    }

        //    return false;
        //}
        //else if (m_errorNum == 4)
        //{
        //    if (m_currCharScript.m_currAction)
        //    {
        //        if (m_currCharScript.m_currAction.m_name != "ATK(Slash)")
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try using ATK(Slash).";
        //            return true;
        //        }
        //    }
        //    else if (!m_hasActed[(int)trn.MOV])
        //    {
        //        TileScript moveTile = m_enemies[2].m_tile.m_neighbors[(int)TileLinkScript.nbors.TOP];

        //        if (m_board.m_selected != moveTile)
        //        {
        //            m_dialogPan.GetComponentInChildren<Text>().text = "Try positioning Red so he can hit all of the targets this turn.";
        //            return true;
        //        }

        //        return false;
        //    }
        //}

        return false;
    }

    public void Next()
    {
        //m_clicks++;
        m_unlock = true;
    }

    override protected void BattleOverview()
    {
        BoardCamScript cam = m_camera.GetComponent<BoardCamScript>();
        //cam.m_rotate = true;

        cam.transform.RotateAround(cam.transform.parent.transform.position, Vector3.up, 70);
        cam.m_zoomIn = false;
        m_board.m_camIsFrozen = true;
        cam.m_target = m_board.gameObject;
        StartCoroutine(IntroMessage());
        m_battle = true;
    }

    override protected IEnumerator StartBattle()
    {
        //m_board.RandomEnvironmentInit();

        m_board.TeamInit();

        m_camera.GetComponent<BoardCamScript>().m_zoomIn = true;
        m_camera.GetComponent<BoardCamScript>().m_rotate = false;
        m_panMan.GetPanel("HUD Panel LEFT").m_inView = true;
        m_panMan.GetPanel("Round Panel").m_inView = true;

        yield return new WaitForSeconds(.1f);

        NewTurn(false);
    }

    public IEnumerator IntroMessage()
    {
        m_dialogPan = GameObject.Find("Dialog Panel").GetComponent<SlidingPanelScript>();
        m_dialogPan.m_inView = true;
        m_dialogPan.GetComponentInChildren<Button>().interactable = false;

        m_dialogPan.GetComponentInChildren<Text>().text = "Xen2 is a tactical turn-based game using an enegry system to coordinate big moves among your units.";
        yield return new WaitUntil(() => !m_board.m_camIsFrozen);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        //////
        //////

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "There are 4 types of energy that can be generated and used by your units that all have different properties.";

        //////
        //////

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "Each attack in Xen2 either gains or uses energy. Energy can also be picked up on the stage for additional boosts.";

        //////
        //////

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "Plan out each round based on what actions are available and which actions you want to set up for.";

        //////
        //////

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;
        m_dialogPan.GetComponentInChildren<Button>().interactable = false;

        TutorialInit();

        yield return new WaitUntil(() => !m_board.m_camIsFrozen);

        StartCoroutine(RedTutorial());
    }
    private void TutorialInit()
    {
        // Init teams
        m_board.m_players[0].name = "Team " + 0;
        m_board.m_players[0].GetComponent<PlayerScript>().m_bScript = m_board;
        m_board.m_players[0].GetComponent<PlayerScript>().m_energy = new int[4];
        m_board.m_players[1].name = "Team " + 1;
        m_board.m_players[1].GetComponent<PlayerScript>().m_bScript = m_board;
        m_board.m_players[1].GetComponent<PlayerScript>().m_energy = new int[4];

        BoardInit();

        // Set Camera
        m_camera.GetComponent<BoardCamScript>().m_zoomIn = true;
        m_camera.GetComponent<BoardCamScript>().m_rotate = false;
        m_board.m_camIsFrozen = true;
        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[0].gameObject;
        m_currCharScript = m_mainChars[0];

        // HUD Set up
        m_panMan.GetPanel("HUD Panel LEFT").m_cScript = m_mainChars[0];
        m_panMan.GetPanel("HUD Panel LEFT").PopulatePanel();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Pass").GetComponent<Button>().interactable = false;
        ToggleMove();

        GameObject.Find("Turn Panel").GetComponent<TurnPanelScript>().NewTurnOrder();
    }
    private void BoardInit()
    {
        // Define team colors
        Color mainCharColor = new Color(0.9058824f, 0.6375825f, 0.3215686f);
        Color enemyCharColor = new Color(0.5489212f, 0.3215686f, 0.9058824f);

        // Set up the teams
        m_mainChars.Add(m_board.SpawnCharacter("Red", 0, mainCharColor, 2, 0, (int)TileLinkScript.nbors.BOTTOM, "R", new string[] { "ATK(AUX)", "ATK(Slash)" }, 14));
        m_mainChars.Add(m_board.SpawnCharacter("White", 0, mainCharColor, 0, 0, (int)TileLinkScript.nbors.BOTTOM, "W", new string[] { "SUP(Fortify)", "ATK(Leg)" }, 12));
        m_mainChars.Add(m_board.SpawnCharacter("Green", 0, mainCharColor, 4, 0, (int)TileLinkScript.nbors.BOTTOM, "G", new string[] { "SUP(Accelerate)", "ATK(Magnet)" }, 12));
        m_mainChars.Add(m_board.SpawnCharacter("Blue", 0, mainCharColor, 3, 0, (int)TileLinkScript.nbors.BOTTOM, "B", new string[] { "SUP(Channel)", "SUP(Queue)" }, 12));

        m_enemies.Add(m_board.SpawnCharacter("Target", 1, enemyCharColor, 2, 4, (int)TileLinkScript.nbors.TOP, "R", new string[] { "ATK(AUX)" }, 10));
        m_enemies.Add(m_board.SpawnCharacter("Target", 1, enemyCharColor, 0, 5, (int)TileLinkScript.nbors.TOP, "R", new string[] { "ATK(AUX)" }, 10));
        m_enemies.Add(m_board.SpawnCharacter("Target", 1, enemyCharColor, 0, 9, (int)TileLinkScript.nbors.TOP, "G", new string[] { "ATK(Aim)" }, 10));

        // Set up the power ups
        m_board.SpawnItem(3, m_enemies[0].m_tile.m_x, m_enemies[0].m_tile.m_z - 1);

        // Set up the environment
        TileScript tarTile = m_mainChars[1].m_tile.m_neighbors[(int)TileLinkScript.nbors.RIGHT].m_neighbors[(int)TileLinkScript.nbors.TOP].m_neighbors[(int)TileLinkScript.nbors.TOP];
        m_board.SpawnEnvironmentOBJ("Small Rock1", tarTile.m_x, tarTile.m_z, 0);

        // Add the characters to the round in which I want to see them in the turn order
        m_currRound.Add(m_mainChars[0]);
        m_currRound.Add(m_enemies[0]);
        m_currRound.Add(m_enemies[1]);
        m_currRound.Add(m_mainChars[1]);
        m_currRound.Add(m_enemies[2]);
        m_currRound.Add(m_mainChars[2]);
        m_currRound.Add(m_mainChars[3]);

        GameObject.Find("Confirmation Panel").GetComponent<PanelScript>().m_cScript = m_mainChars[0];
        m_mainChars[0].m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(true);
        m_mainChars[0].m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.green;
    }


    private IEnumerator RedTutorial()
    {
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        m_dialogPan.GetComponentInChildren<Text>().text = "Red units have greater destructive force and can enhance special abilities.";
        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Let's go in for an attack. Looks like there is energy in front of our target too. This can help set up Blue.";
        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        StartCoroutine(RedTurn1());

        yield return new WaitUntil(() => m_hasActed[(int)trn.ACT] == true);

        m_tileHighlight.SetActive(false);

        //////
        //////

        StartCoroutine(RedEnemeyTurns1());

        //////
        //////

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        StartCoroutine(WhiteTutorial());
    }
    private IEnumerator RedTurn1()
    {
        m_dialogPan.GetComponentInChildren<Button>().interactable = false;

        ToggleMove();
        m_highlightedButton = m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Image>();
        m_tileHighlight.SetActive(true);
        m_tileHighlight.transform.position = m_enemies[0].m_tile.m_neighbors[(int)TileLinkScript.nbors.BOTTOM].transform.position;
        m_errorNum = 0;

        m_dialogPan.GetComponentInChildren<Text>().text = "Move to the energy draw and attack. (Press either the highlighted HUD icon or the current character)";
        yield return new WaitUntil(() => Vector3.Distance(m_mainChars[0].transform.position, m_enemies[0].transform.position) <= 10 && m_highlightedButton);

        m_tileHighlight.transform.position = m_enemies[0].transform.position;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;
        m_errorNum = -1;
        m_highlightedButton.color = Color.white;
        m_highlightedButton = null;

        //////
        //////

        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = true;
        m_highlightedButton = m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Image>();
        m_dialogPan.GetComponentInChildren<Text>().text = "Now that you are in position, select and use your attack on the left. (Hovering over target will preview HP after hit.)";
    }
    private IEnumerator RedEnemeyTurns1()
    {
        m_dialogPan.ClosePanel();
        yield return new WaitForSeconds(2f);
        m_highlightedButton.color = PanelScript.b_isDisallowed;
        m_highlightedButton = null;

        SwitchActivePlayer(m_enemies[0]);
        m_newTurn = true;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;

        AIAttack(m_enemies[0], m_enemies[0].m_actions[0], m_mainChars[0]);
        yield return new WaitForSeconds(2f);

        SwitchActivePlayer(m_enemies[1]);
        m_newTurn = true;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;

        m_enemies[1].MovingStart(m_mainChars[0].m_tile.m_neighbors[(int)TileLinkScript.nbors.LEFT], false, false);
        yield return new WaitForSeconds(2f);
        AIAttack(m_enemies[1], m_enemies[1].m_actions[0], m_mainChars[0]);
        yield return new WaitForSeconds(3f);

        m_dialogPan.PopulatePanel();
        m_dialogPan.GetComponentInChildren<Text>().text = "You received some red energy from that attack. We'll use a stronger attack with Red when we're able to.";
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;
    }



    private IEnumerator WhiteTutorial()
    {
        m_dialogPan.GetComponentInChildren<Text>().text = "For now, it looks like Red is in trouble. Let's get White over to help out.";

        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[1].gameObject;

        SwitchActivePlayer(m_mainChars[1]);
        m_newTurn = true;

        ToggleMove();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;

        yield return new WaitUntil(() => !m_board.m_camIsFrozen);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "White units specialize in protection and hindering targets.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "Move in and target Red with SUP(Fortify) to give him some more protection.";
        m_tileHighlight.SetActive(true);
        m_tileHighlight.transform.position = m_mainChars[0].m_tile.m_neighbors[(int)TileLinkScript.nbors.BOTTOM].m_neighbors[(int)TileLinkScript.nbors.BOTTOM].transform.position;
        m_errorNum = 1;

        ToggleMove();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = true;

        yield return new WaitUntil(() => Vector3.Distance(m_mainChars[1].transform.position, m_tileHighlight.transform.position) <= 1);
        m_tileHighlight.transform.position = m_mainChars[0].transform.position;

        yield return new WaitUntil(() => m_mainChars[0].m_tempStats[(int)CharacterScript.sts.DEF] > 0);

        //////
        //////

        m_tileHighlight.SetActive(false);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;
        m_dialogPan.GetComponentInChildren<Text>().text = "Hovering over characters will make their profiles appear on the right side of the screen.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "If you click a character while hovering over them, you will lock their profile in place until you deselect it.";

        yield return new WaitUntil(() => m_panMan.GetPanel("HUD Panel RIGHT").m_inView && m_board.m_selected && m_board.m_selected.m_holding && m_board.m_selected.m_holding.name == "Red");
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "With the profile selected, hover over the status icon in the bottom of Red's profile to confirm his status.";

        yield return new WaitUntil(() => m_panMan.GetPanel("StatusViewer Panel").m_inView && 
            m_panMan.GetPanel("StatusViewer Panel").transform.Find("Main View/Duration").GetComponent<Text>().text == "Duration: 2");
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Additionally, if you need help with what these icons do, you can hover over any stat to get a description.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        StartCoroutine(GreenTutorial());
    }

    private IEnumerator GreenTutorial()
    {
        EndTurn();

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        m_dialogPan.ClosePanel();

        SwitchActivePlayer(m_enemies[2]);
        m_newTurn = true;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;

        m_enemies[2].MovingStart(m_enemies[1].m_tile.m_neighbors[(int)TileLinkScript.nbors.LEFT].m_neighbors[(int)TileLinkScript.nbors.TOP].m_neighbors[(int)TileLinkScript.nbors.TOP], false, false);
        yield return new WaitForSeconds(2f);
        AIAttack(m_enemies[2], m_enemies[2].m_actions[0], m_mainChars[0]);
        yield return new WaitForSeconds(3f);

        m_dialogPan.GetComponentInChildren<Button>().interactable = true;
        m_dialogPan.PopulatePanel();

        m_dialogPan.GetComponentInChildren<Text>().text = "Looks like Red barely survived because of White's intervention. Now let's get some payback with Green.";


        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_board.SpawnItem(0, m_enemies[0].m_tile.m_x + 2, m_enemies[0].m_tile.m_z);

        m_dialogPan.GetComponentInChildren<Text>().text = "This green energy could benefit us right now and set us up for a magnet attack. Let's grab that first.";


        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        SwitchActivePlayer(m_mainChars[2]);
        m_newTurn = true;

        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;
        ToggleMove();
        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[2].gameObject;
        m_dialogPan.GetComponentInChildren<Text>().text = "Green units specialize in board control by repositioning other units. They also excel at range.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Use WASD or hold middle click to move around the camera. Pressing F will snap to the current character.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Scrolling the mouse wheel will adjust the camera's zoom. Holding right click and moving the mouse will rotate.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Holding right click and scrolling the mouse will adjust the pitch of the camera.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;

        m_dialogPan.GetComponentInChildren<Text>().text = "Now that we got that out of the way, let's grab that energy and target all of them with a magnet attack.";

        ToggleMove();
        m_errorNum = 2;

        yield return new WaitUntil(() => m_hasActed[(int)trn.ACT] == true);

        StartCoroutine(BlueTutorial());
    }

    private IEnumerator BlueTutorial()
    {
        yield return new WaitForSeconds(2f);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;
        m_dialogPan.GetComponentInChildren<Text>().text = "That magnet did a good job at rounding all of them up. Now we just need Red to use his ATK(Slash).";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        SwitchActivePlayer(m_mainChars[3]);
        m_newTurn = true;
        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[3].gameObject;

        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(1).GetComponent<Button>().interactable = false;
        ToggleMove();
        
        m_dialogPan.GetComponentInChildren<Text>().text = "First Blue is up. Since we got that blue energy from before, we can use his SUP(Queue) on Red.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Blue units work with energy gathering/transmuting, affecting speed and hiding/revealing state info.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "SUP(Queue) ensures each target hit will be picked first in the next round.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "It also gives a SPD boost. A character's speed will affect where they are placed in a new round.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "If your SPD is high enough, it is possible to override another player's turn and go twice in the same round.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////
        
        m_dialogPan.GetComponentInChildren<Button>().interactable = false;

        m_dialogPan.GetComponentInChildren<Text>().text = "Let's move in and use SUP(Queue) on Red now.";

        ToggleMove();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(1).GetComponent<Button>().interactable = true;

        m_errorNum = 3;

        yield return new WaitUntil(() => m_hasActed[(int)trn.ACT] == true);

        StartCoroutine(FinalTutorial());
    }

    private IEnumerator FinalTutorial()
    {
        yield return new WaitForSeconds(2f);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;
        m_dialogPan.GetComponentInChildren<Text>().text = "Great! Not only did Red go first because of Queue's primary effect, but Red replaced an enemies turn!";

        SwitchActivePlayer(m_mainChars[0]);
        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[0].gameObject;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(0).GetComponent<Button>().interactable = false;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(1).GetComponent<Button>().interactable = false;
        ToggleMove();
        
        m_currRound.Clear();
        m_currRound.Add(m_mainChars[0]);
        m_currRound.Add(m_enemies[1]);
        m_currRound.Add(m_enemies[0]);
        m_currRound.Add(m_mainChars[2]);
        m_currRound.Add(m_mainChars[1]);
        m_currRound.Add(m_enemies[2]);
        m_currRound.Add(m_mainChars[0]);

        GameObject.Find("Turn Panel").GetComponent<TurnPanelScript>().NewTurnOrder();

        m_mainChars[0].m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(true);
        m_mainChars[0].m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.green;

        m_mainChars[0].m_tempStats[(int)CharacterScript.sts.SPD] -= 10;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Whenever this happens, the character going twice loses 10 SPD and the one that got replaced gains 10.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "Make going twice in a round count, since the order is going to be against you in the next round.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "You can confirm that Red is going twice by checking the turn order at the top of the screen.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////
        
        m_dialogPan.GetComponentInChildren<Text>().text = "The current player going is not listed in the turn order, but yet we can see Red at the end of the list.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////
        
        m_dialogPan.GetComponentInChildren<Text>().text = "If you hover over characters listed in the turn order, it will highlight where they are on the board.";

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.GetComponentInChildren<Text>().text = "If you click one of the characters listed, the camera will take you to their location on the board. Try it now.";

        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        yield return new WaitUntil(() => EventSystem.current.currentSelectedGameObject && 
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().tag == "Turn Panel");
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_camera.GetComponent<BoardCamScript>().m_target = m_mainChars[0].gameObject;
        m_dialogPan.GetComponentInChildren<Button>().interactable = false;
        m_dialogPan.GetComponentInChildren<Text>().text = "Now, let's finish this with a ATK(Slash) on all of the targets.";

        ToggleMove();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").transform.GetChild(1).GetComponent<Button>().interactable = true;

        m_errorNum = 4;

        yield return new WaitUntil(() => m_hasActed[(int)trn.ACT] == true);
        m_dialogPan.GetComponentInChildren<Button>().interactable = true;

        yield return new WaitUntil(() => m_unlock == true);
        m_unlock = false;

        //////
        //////

        m_dialogPan.ClosePanel();
        m_panMan.GetPanel("Round End Panel").m_inView = true;
        m_panMan.GetPanel("Round End Panel").GetComponentInChildren<Text>().text = "TUTORIAL OVER";
    }



    // Utilities

    private void MakeButtonFlash()
    {
        if (!m_highlightedButton)
            return;

        float speedMod = 0.3f;

        if (m_brighter)
        {
            m_highlightedButton.color = new Color(m_highlightedButton.color.r + speedMod * Time.deltaTime,
                m_highlightedButton.color.g + speedMod * Time.deltaTime,
                m_highlightedButton.color.b + speedMod * 5 * Time.deltaTime, m_highlightedButton.color.a);

            if (m_highlightedButton.color.b >= 1)
                m_brighter = false;
        }
        else
        {
            m_highlightedButton.color = new Color(m_highlightedButton.color.r - speedMod * Time.deltaTime,
                m_highlightedButton.color.g - speedMod * Time.deltaTime,
                m_highlightedButton.color.b - speedMod * 5 * Time.deltaTime, m_highlightedButton.color.a);

            if (m_highlightedButton.color.b <= .5f)
                m_brighter = true;
        }
    }

    private void AIAttack(CharacterScript _AI, ActionScript _act, CharacterScript _target)
    {
        m_board.m_selected = _target.m_tile;
        m_currCharScript = _AI;
        _AI.m_currAction = _act;
        _AI.m_targets.Add(_target.gameObject);
        _act.ActionStart(false);
    }

    private void ToggleMove()
    {
        if (m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Button>().interactable == false)
        {
            m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Button>().interactable = true;
            m_board.m_moveLocked = false;
        }
        else
        {
            m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Button>().interactable = false;
            m_board.m_moveLocked = true;
        }
    }
}
