using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManagerScript : MonoBehaviour {

    private SlidingPanelManagerScript m_panMan;
    private BoardScript m_board;

	// Use this for initialization
	void Start ()
    {
        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();

        if (GameObject.Find("Board"))
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // REFACTOR
        if (Input.GetMouseButtonDown(1))
                OnRightClick();
    }

    public void OnRightClick()
    {
        //AudioClip a = Resources.Load<AudioClip>("Sounds/Menu Sound 2");
        //m_audio.PlayOneShot(a);

        if (m_board && m_board.m_selected)
            m_board.m_selected = null;

        m_panMan.ClosePanelLast();

        if (m_board && m_board.m_currButton)
        {
            ButtonScript butt = m_board.m_currButton.GetComponent<ButtonScript>();
            butt.Select();
            butt.HoverFalse();

            m_board.m_highlightedTile.ClearRadius();
        }
    }

    //public void SetUniqueEnergy(string energy)
    //{
    //    GameObject panel = energyPanel[0];

    //    int numEle = 0;
    //    List<char> used = new List<char>();

    //    for (int i = 0; i < energy.Length; i++)
    //    {
    //        if (used.Contains(energy[i]))
    //            continue;

    //        used.Add(energy[i]);
    //        numEle++;
    //    }

    //    if (numEle== 1)
    //    {
    //        energyPanel[0].SetActive(true);
    //        panel = energyPanel[0];
    //    }
    //    else if (numEle == 2)
    //    {
    //        energyPanel[1].SetActive(true);
    //        panel = energyPanel[1];
    //    }
    //    else if (numEle == 3)
    //    {
    //        energyPanel[2].SetActive(true);
    //        panel = energyPanel[2];
    //    }

    //    Image[] orbs = panel.GetComponentsInChildren<Image>();

    //    for (int i = 0; i < num; i++)
    //    {
    //        if (energy[i] == 'g')
    //            orbs[i + 1].color = new Color(.5f, 1, .5f, 1);
    //        else if (energy[i] == 'r')
    //            orbs[i + 1].color = new Color(1, .5f, .5f, 1);
    //        else if (energy[i] == 'w')
    //            orbs[i + 1].color = new Color(1, 1, 1, 1);
    //        else if (energy[i] == 'b')
    //            orbs[i + 1].color = new Color(.5f, .5f, 1, 1);
    //        else if (energy[i] == 'G')
    //            orbs[i + 1].color = new Color(.1f, .9f, .1f, 1);
    //        else if (energy[i] == 'R')
    //            orbs[i + 1].color = new Color(.9f, .1f, .1f, 1);
    //        else if (energy[i] == 'W')
    //            orbs[i + 1].color = new Color(.9f, .9f, .9f, 1);
    //        else if (energy[i] == 'B')
    //            orbs[i + 1].color = new Color(.1f, .1f, .9f, 1);
    //    }
    //}

    static public void ResumeGame()
    {
        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        board.m_isForcedMove = null;
        board.m_camIsFrozen = false;
        //panman.CloseHistory();
    }

    public void ChangeScreen(string _screen)
    {
        SceneManager.LoadScene(_screen);
    }
}
