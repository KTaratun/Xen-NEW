using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    public enum trn { MOV, ACT };

    public int m_roundCount;
    public bool m_battle; // This checks to see if a battle has started
    public bool m_newTurn; // This tells the turn panels to slide
    public List<CharacterScript> m_priorityQueue; // This queue is chosen for the next round before normal rolls.
    public FieldScript m_field;
    public int m_livingPlayersInRound; // This is mainly for the turn panels. This tells them how far to move when I player finishes their turn.
    public float m_actionEndTimer; // This is used to delay camera movement when focusing on a target
    protected Camera m_camera;
    public List<CharacterScript> m_currRound; // A list of the players who have not taken a turn in the current round
    public bool[] m_hasActed;
    public CharacterScript m_currCharScript; // A pointer to the character that's turn is currently up

    protected SlidingPanelManagerScript m_panMan;
    protected BoardScript m_board;


    // Start is called before the first frame update
    virtual protected void Start()
    {
        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
        if (GameObject.Find("Board"))
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();
        
        m_camera = GameObject.Find("BoardCam/Main Camera").GetComponent<Camera>();

        m_roundCount = 0;
        m_newTurn = false;

        m_actionEndTimer = 0;

        m_hasActed = new bool[2];

        //if (m_field)
        //    m_battle = false;
        //else
            BattleOverview();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        GameInput();
        EndOfActionTimer();
        CheckEndTurn(false);
    }

    private void GameInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SlidingPanelScript rEndPan = m_panMan.GetPanel("Round End Panel");
            if (rEndPan.m_inView && rEndPan.transform.Find("Text").GetComponent<Text>().text == "BATTLE START" && !GameObject.Find("Network") ||
                rEndPan.m_inView && rEndPan.transform.Find("Text").GetComponent<Text>().text == "BATTLE START" && GameObject.Find("Network")) // && GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted
                StartCoroutine(StartBattle());
            else if (rEndPan.m_inView && rEndPan.transform.Find("Text").GetComponent<Text>().text[rEndPan.transform.Find("Text").GetComponent<Text>().text.Length - 1] == 'S')
            {
                m_battle = false;
                SceneManager.LoadScene("Menu");
            }
        }
    }

    virtual protected void BattleOverview()
    {
        BoardCamScript cam = m_camera.GetComponent<BoardCamScript>();
        cam.m_rotate = true;
        cam.m_zoomIn = false;
        m_board.m_camIsFrozen = true;
        cam.m_target = m_board.gameObject;
        SlidingPanelScript rEndPan = GameObject.Find("Round End Panel").GetComponent<SlidingPanelScript>();
        rEndPan.m_inView = true;
        rEndPan.GetComponentInChildren<Text>().text = "BATTLE START";
        m_battle = true;
    }

    virtual protected IEnumerator StartBattle()
    {
        m_board.RandomEnvironmentInit();

        if (!m_field)
            m_board.TeamInit();

        m_camera.GetComponent<BoardCamScript>().m_zoomIn = true;
        m_camera.GetComponent<BoardCamScript>().m_rotate = false;
        m_panMan.GetPanel("HUD Panel LEFT").m_inView = true;
        m_panMan.GetPanel("Round Panel").m_inView = true;

        yield return new WaitForSeconds(.1f);

        NewTurn(false);
    }

    public void NewTurn(bool _isForced)
    {
        //if (m_currRound.Count == 0 && GameObject.Find("Network") && !GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
        //    return;

        bool newRound = false;
        if (m_currRound.Count == 0)
        {
            NewRound();
            newRound = true;
        }

        newRound = CheckIfActivePlayerIsOK(newRound);
        SwitchActivePlayer(m_currRound[0].GetComponent<CharacterScript>());
        m_currRound.RemoveAt(0);

        if (!newRound && !_isForced)
        {
            m_newTurn = true;
            BoardCamScript camScript = m_camera.GetComponent<BoardCamScript>();
            camScript.m_target = m_currCharScript.gameObject;
        }

        //if (m_currCharScript.m_isAI)
        //    m_currCharScript.AITurn();

        m_board.m_camIsFrozen = true;
    }

    private bool CheckIfActivePlayerIsOK(bool _newRound)
    {
        while (m_currRound[0].m_effects[(int)StatusScript.effects.STUN] || !m_currRound[0].m_isAlive)
        {
            StatusScript.UpdateStatus(m_currRound[0], StatusScript.mode.TURN_END);
            m_currRound[0].m_turnPanels[0].SetActive(false);
            m_currRound[0].m_turnPanels.RemoveAt(0);

            if (m_currRound.Count == 0)
            {
                NewRound();
                _newRound = true;
            }

            m_currCharScript = m_currRound[0].GetComponent<CharacterScript>();
            m_currRound.RemoveAt(0);
        }

        return _newRound;
    }

    public void SwitchActivePlayer(CharacterScript _char)
    {
        if (m_currCharScript)
            EndTurn();

        _char.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(true);
        _char.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.green;

        if (_char.m_turnPanels.Count > 0)
            _char.m_turnPanels.RemoveAt(0);

        m_currCharScript = _char;
        m_panMan.m_confirmPanel.m_cScript = _char;

        PanelScript HUDLeftScript = m_panMan.GetPanel("HUD Panel LEFT");
        HUDLeftScript.m_cScript = _char;
        HUDLeftScript.PopulatePanel();
    }

    public void CheckEndTurn(bool _isForced)
    {
        if (!m_currCharScript || !m_currCharScript.m_anim)
            return;

        PlayerScript winningTeam = null;
        int numTeamsActive = 0;
        for (int i = 0; i < m_board.m_players.Length; i++)
        {
            PlayerScript p = m_board.m_players[i];
            for (int j = 0; j < p.m_characters.Count; j++)
            {
                CharacterScript c = p.m_characters[j];
                if (c.m_isAlive)
                {
                    numTeamsActive++;
                    winningTeam = p;
                    break;
                }
            }
        }

        if (numTeamsActive < 2)
        {
            GameOver(winningTeam);
            return;
        }

        if (m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Idle Melee") ||
            m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Death") || _isForced)
            if (m_hasActed[(int)trn.MOV] == true && m_hasActed[(int)trn.ACT] == true &&
                !m_board.m_isForcedMove && !m_board.m_camIsFrozen && m_actionEndTimer == 0 || _isForced)
            {
                if (GameObject.Find("Network") && GameObject.Find("Network").GetComponent<CustomDirect>().m_isStarted &&
                    m_currRound.Count != 0)
                {
                    CustomDirect s = GameObject.Find("Network").GetComponent<CustomDirect>();
                    s.SendMessageCUSTOM("NEWTURN~");
                }

                NewTurn(false);
            }
    }

    public void EndTurn()
    {
        print(m_currCharScript.m_name + " has ended their turn.\n");
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Button>().interactable = true;
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Pass").GetComponent<Button>().interactable = true;
        StatusScript.UpdateStatus(m_currCharScript, StatusScript.mode.TURN_END);
        m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(false);
        m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.white;
        m_hasActed[(int)trn.MOV] = false;
        m_hasActed[(int)trn.ACT] = false;
        m_currCharScript.m_currAction = null;
    }

    virtual public void NewRound()
    {
        List<CharacterScript> tempChars = new List<CharacterScript>();

        m_currCharScript = null;

        m_livingPlayersInRound = 0;
        for (int i = 0; i < m_board.m_characters.Count; i++)
            if (m_board.m_characters[i].GetComponent<CharacterScript>().m_isAlive)
            {
                StatusScript.UpdateStatus(m_board.m_characters[i], StatusScript.mode.ROUND_END);
                tempChars.Add(m_board.m_characters[i]);
                m_livingPlayersInRound++;
            }

        if (m_priorityQueue.Count > 0)
        {
            for (int i = 0; i < m_priorityQueue.Count; i++)
            {
                if (m_priorityQueue[i].m_isAlive)
                {
                    m_currRound.Add(m_priorityQueue[i]);
                    m_priorityQueue[i].m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                }
            }

            m_priorityQueue.Clear();
        }

        int numPool;
        do
        {
            numPool = 0;
            // Gather all characters speed to pull from
            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] < 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;
                else
                    numPool += charScript.m_tempStats[(int)CharacterScript.sts.SPD];
            }

            int randNum = Random.Range(0, numPool);
            int currNum = 0;

            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;

                currNum += charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    m_currRound.Add(tempChars[i]);

                    if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                    {
                        numPool -= charScript.m_tempStats[(int)CharacterScript.sts.SPD];
                        tempChars.RemoveAt(i);
                    }

                    charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    break;
                }
            }
        } while (m_currRound.Count < m_livingPlayersInRound && numPool > 0);

        for (int i = 0; i < m_board.m_characters.Count; i++)
        {
            CharacterScript charScript = m_board.m_characters[i].GetComponent<CharacterScript>();
            if (!charScript.m_effects[(int)StatusScript.effects.STUN] && charScript.m_isAlive)
                charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 10;
        }

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = m_panMan.GetPanel("Round Panel");
        roundPanScript.PopulatePanel();

        GameObject.Find("Turn Panel").GetComponent<TurnPanelScript>().NewTurnOrder();

        m_board.m_powerUp = m_board.SpawnItem(-1, -1, -1);

        SlidingPanelScript roundPan = m_panMan.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.GetComponentInChildren<Text>().text = "NEW ROUND";
    }

    public void GameOver(PlayerScript _winningTeam)
    {
        SlidingPanelScript roundPan = m_panMan.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.transform.Find("Text").GetComponent<Text>().text = _winningTeam.name + " WINS";
        m_camera.GetComponent<BoardCamScript>().m_rotate = true;
        m_currCharScript = null;
    }

    public void Pass()
    {
        m_hasActed[(int)trn.MOV] = true;
        m_hasActed[(int)trn.ACT] = true;
        m_panMan.CloseHistory();
        m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Pass").GetComponent<Image>().color = Color.white;
        m_board.m_currButton = null;
    }

    protected void EndOfActionTimer()
    {
        if (m_actionEndTimer > 0)
        {
            m_actionEndTimer += Time.deltaTime;
            if (m_actionEndTimer > 2)
            {
                m_actionEndTimer = 0;
                m_board.m_camIsFrozen = false;

                if (!m_board.m_selected)
                    m_panMan.GetPanel("HUD Panel RIGHT").ClosePanel();

                if (m_currRound.Count == m_livingPlayersInRound - 1 && m_hasActed[(int)trn.ACT] == false)
                    m_camera.GetComponent<BoardCamScript>().m_target = m_currCharScript.gameObject;
            }
        }
    }
}