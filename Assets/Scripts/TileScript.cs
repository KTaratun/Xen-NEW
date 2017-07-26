using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    public enum nbors {left, right, top, bottom};

    //public GameObject camera;
    public GameObject holding;
    public GameObject[] neighbors;
    public List<GameObject> radius;
    public int x;
    public int z;
    public BoardScript boardScript;

	// Use this for initialization
	void Start ()
    {
        radius = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnMouseDown()
    {
        // REFACTOR: Disallow the clicking of tiles if any menu is up
        PanelScript mainPanScript = boardScript.panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript ActionPanScript = boardScript.panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        PanelScript StsPanScript = boardScript.panels[(int)BoardScript.pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript AuxPanScript = boardScript.panels[(int)BoardScript.pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();
        if (mainPanScript.inView == true || ActionPanScript.inView == true || StsPanScript.inView == true || AuxPanScript.inView == true)
            return;

        boardScript.selected = gameObject;

        Renderer renderer = GetComponent<Renderer>();
        CharacterScript currPlayerScript = boardScript.currPlayer.GetComponent<CharacterScript>();
        TileScript currTileScript = boardScript.currTile.GetComponent<TileScript>();
        if (renderer.material.color == Color.blue) // If tile is blue when clicked, perform movement code
        {
            currPlayerScript.Movement(currTileScript, GetComponent<TileScript>());
            ClearRadius(currTileScript);

            return;
        }
        else if (renderer.material.color == new Color(1, 0, 0, 1) && holding) // Otherwise if color is red, perform action code
        {
            GameObject[] targets = new GameObject[1];
            targets[0] = holding;
            currPlayerScript.Action(targets);
            ClearRadius(currTileScript);

            return;
        }

        if (holding && holding.tag == "Player")
        {
            Renderer holdingR = holding.GetComponent<Renderer>();
            if (holdingR.material.color == Color.green)
            {
                mainPanScript.inView = true;

                // Assign character to panels
                mainPanScript.character = holding;
                CharacterScript cScript = holding.GetComponent<CharacterScript>();
                mainPanScript.cScript = cScript;
                mainPanScript.SetButtons();
            }
            else
            {
                AuxPanScript.inView = true;

                // Assign character to panels
                AuxPanScript.character = holding;
                CharacterScript cScript = holding.GetComponent<CharacterScript>();
                AuxPanScript.cScript = cScript;
                AuxPanScript.SetButtons();
            }
        }
    }

    // Reset all tiles to their original color
    private void ClearRadius(TileScript _tS)
    {
        if (_tS.radius.Count > 0)
        {
            for (int i = 0; i < _tS.radius.Count; i++)
            {
                Renderer sRend = _tS.radius[i].GetComponent<Renderer>();
                sRend.material.color = new Color(1, 1, 1, 0f);
            }
            _tS.radius.Clear();
        }
    }
}
