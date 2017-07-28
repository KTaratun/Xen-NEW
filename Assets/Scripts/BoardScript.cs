using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    public enum pnls { MAIN_PANEL, ACTION_PANEL, STATUS_PANEL, HUD_LEFT_PANEL, HUD_RIGHT_PANEL, ROUND_PANEL, AUXILIARY_PANEL, TOTAL_PANEL }

    public Camera camera; // Why it do that squiggle?
    public int roundCount;
    public GameObject[] panels; // All movable GUI on screen
    private GameObject[] tiles; // All tiles on the board
    public GameObject tile; // A reference to the tile prefab
    public GameObject character; // A reference to the character prefab
    public int width; // Width of the board
    public int height; // Height of the board
    public GameObject selected; // Currently selected tile/character
    private GameObject highlightedCharacter;
    private GameObject oldTile; // A pointer to the previously hovered over tile
    public GameObject currTile; // The tile of the player who's turn is currently up
    public GameObject currPlayer; // A pointer to the player that's turn is currently up
    public List<GameObject> characters; // A list of all players within the game
    public GameObject[] players;
    public List<GameObject> currRound; // A list of the players who have taken a turn in the current round

	// Use this for initialization
	void Start ()
    {
        roundCount = 0;
        Renderer r = GetComponent<Renderer>();
        // If the user didn't assign a tile size in the editor, auto adjust the tiles to fit the board
        if (height == 0 && width == 0)
        {
            width = (int) (r.bounds.size.x / tile.GetComponent<Renderer>().bounds.size.x);
            height = (int)(r.bounds.size.z / tile.GetComponent<Renderer>().bounds.size.z);
        }

        InitBoardTiles();
        AssignNeighbors();
        CharacterInit();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(1))
            OnRightClick();

        Hover();

        if (currPlayer == null)
        {
            NewRound();
            NewTurn();
        }
    }

    public void OnRightClick()
    {
        CharacterScript charScript = currPlayer.GetComponent<CharacterScript>();
        TileScript currPScript = charScript.tile.GetComponent<TileScript>();

        if (currPScript.radius.Count == 0)
            return;

        Renderer sRend = currPScript.radius[0].GetComponent<Renderer>();
        if (sRend.material.color == new Color(1, 0, 0, 0.5f))
        {
            PanelScript actPan = panels[(int)pnls.ACTION_PANEL].GetComponent<PanelScript>();
            actPan.inView = true;
        }
        else if (sRend.material.color == new Color(0, 0, 1, 0.5f))
        {
            PanelScript panScript = panels[(int)pnls.MAIN_PANEL].GetComponent<PanelScript>();
            panScript.inView = true;
        }

        for (int i = 0; i < currPScript.radius.Count; i++)
        {
            sRend = currPScript.radius[i].GetComponent<Renderer>();
            sRend.material.color = new Color(1, 1, 1, 0f);
        }
        currPScript.radius.Clear();
    }

    public void InitBoardTiles()
    {
        Renderer r = GetComponent<Renderer>();
        tiles = new GameObject[width * height];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject newTile = Instantiate(tile);

                Renderer ntR = newTile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newTile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.1f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newTile.GetComponent<TileScript>();
                tScript.x = x;
                tScript.z = z;
                tScript.boardScript = GetComponent<BoardScript>();

                tiles[x + z * width] = newTile;
            }
        }
    }

    public void AssignNeighbors()
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tempTile = tiles[x + z * width];
                TileScript tScript = tempTile.GetComponent<TileScript>();

                tScript.neighbors = new GameObject[4];

                if (x != 0)
                    tScript.neighbors[(int)TileScript.nbors.left] = tiles[(x + z * width) - 1];
                if (x < width - 1)
                    tScript.neighbors[(int)TileScript.nbors.right] = tiles[(x + z * width) + 1];
                if (z != 0)
                    tScript.neighbors[(int)TileScript.nbors.bottom] = tiles[(x + z * width) - width];
                if (z < height - 1)
                    tScript.neighbors[(int)TileScript.nbors.top] = tiles[(x + z * width) + width];
            }
        }
    }

    public void CharacterInit()
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 6; j++)
            {
                string key = i.ToString() + ',' + j.ToString() + ",name";
                string name = PlayerPrefs.GetString(key);
                if (name.Length > 0)
                {
                    // Set up character
                    GameObject newChar = Instantiate(character);
                    CharacterScript cScript = newChar.GetComponent<CharacterScript>();

                    cScript.name = name;
                    key = i.ToString() + ',' + j.ToString() + ",color";
                    string color = PlayerPrefs.GetString(key);
                    cScript.color = color;
                    
                    key = i.ToString() + ',' + j.ToString() + ",actions";
                    string[] acts = PlayerPrefs.GetString(key).Split(';');
                    cScript.actions = acts;

                    // Set up position
                    TileScript script;
                    int randX;
                    int randZ;
                    
                    do
                    {
                        randX = Random.Range(0, width - 1);
                        randZ = Random.Range(0, height - 1);
                    
                        script = tiles[randX + randZ * width].GetComponent<TileScript>();
                    } while (script.holding);
                    
                    script.holding = newChar;
                    newChar.transform.SetPositionAndRotation(tiles[randX + randZ * width].transform.position, new Quaternion());
                    cScript.tile = tiles[randX + randZ * width];
                    cScript.boardScript = GetComponent<BoardScript>();

                    characters.Add(newChar);

                    // Link to player
                    cScript.player = players[i];
                    PlayerScript playScript = players[i].GetComponent<PlayerScript>();
                    playScript.characters.Add(newChar);
                }
            }
    }

    public void Hover()
    {
        Ray ray;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit))
            return;

        if (hit.collider.gameObject.tag == "Tile" || hit.collider.gameObject.tag == "Player" && hit.collider.gameObject != currPlayer)
        {
            if (hit.collider.gameObject != oldTile) // REFACTOR ALL THE CODE IN THIS IF STATEMENT
            {
                if (oldTile)
                {
                    Renderer oTR = oldTile.GetComponent<Renderer>();
                    if (oTR.material.color.a != 0)
                        oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

                    // Hacky fix for when you spawn colored tiles for range and your cursor starts on one of the tiles
                    if (oTR.material.color.r == 0 && oTR.material.color.b == 1 && oTR.material.color.a == 0f)
                        oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
                }

                Renderer hR = hit.collider.GetComponent<Renderer>();
                hR.material.color = new Color(hR.material.color.r, hR.material.color.g, hR.material.color.b, hR.material.color.a + 0.5f);
            }
            oldTile = hit.collider.gameObject;
        }
        
        PanelScript hudPanScript = panels[(int)pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
        if (hit.collider.gameObject.tag == "Player")
        {
            CharacterScript colScript = hit.collider.gameObject.GetComponent<CharacterScript>();
            if (hit.collider.gameObject != currPlayer)
            {
                // Change color of turn panel to indicate where the character is in the turn order
                if (colScript.turnPanel)
                {
                    Image turnPanImage = colScript.turnPanel.GetComponent<Image>();
                    turnPanImage.color = Color.cyan;
                }
                // Reveal right HUD with highlighted character's data
                highlightedCharacter = hit.collider.gameObject;
                hudPanScript.character = hit.collider.gameObject;
                hudPanScript.cScript = colScript;
                hudPanScript.PopulateText();
                hudPanScript.inView = true;
            }

            if (oldTile)
            {
                Renderer oTR = oldTile.GetComponent<Renderer>();
                if (oTR.material.color.a != 0)
                    oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

                // Hacky fix for when you spawn colored tiles for range and your cursor starts on one of the tiles
                if (oTR.material.color.r == 0 && oTR.material.color.b == 1 && oTR.material.color.a == 0f)
                    oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
            }

            Renderer hR = colScript.tile.GetComponent<Renderer>();
            hR.material.color = new Color(hR.material.color.r, hR.material.color.g, hR.material.color.b, hR.material.color.a + 0.5f);

            oldTile = colScript.tile;
        }
        else if (highlightedCharacter)
        {
            CharacterScript colScript = highlightedCharacter.GetComponent<CharacterScript>();
            if (colScript.turnPanel)
            {
                Image turnPanImage = colScript.turnPanel.GetComponent<Image>();
                turnPanImage.color = Color.white;
            }
            hudPanScript.inView = false;
        }
    }

    public void NewTurn()
    {
        if (currRound.Count == 0)
            NewRound();

        // Turn previous character back to original color
        if (currPlayer)
        {
            Renderer oldRenderer = currPlayer.GetComponent<Renderer>();
            oldRenderer.material.color = Color.white;
        }

        currPlayer = currRound[0];
        CharacterScript charScript = currPlayer.GetComponent<CharacterScript>();
        currTile = charScript.tile;

        PanelScript HUDLeftScript = panels[(int)pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
        HUDLeftScript.character = currPlayer;
        HUDLeftScript.cScript = currPlayer.GetComponent<CharacterScript>();
        HUDLeftScript.PopulateText();
        PlayerScript playScript = charScript.player.GetComponent<PlayerScript>();
        if (playScript)
            playScript.SetEnergyPanel();
 
        currRound.Remove(currPlayer);

        CameraScript camScript = camera.GetComponent<CameraScript>();
        camScript.target = currPlayer;

        Renderer charRenderer = currPlayer.GetComponent<Renderer>();
        charRenderer.material.color = Color.green;
    }

    public void NewRound()
    {
        int numPool;
        do
        {
            numPool = 0;
            // Gather all characters speed to pull from
            for (int i = 0; i < characters.Count; i++)
            {
                CharacterScript charScript = characters[i].GetComponent<CharacterScript>();
                numPool += charScript.tempStats[(int)CharacterScript.sts.SPD];
            }

            int randNum = Random.Range(0, numPool);
            int currNum = 0;

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterScript charScript = characters[i].GetComponent<CharacterScript>();
                if (charScript.tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.stats[(int)CharacterScript.sts.HP] <= 0)
                    continue;

                currNum += charScript.tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    if (charScript.tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                        numPool -= charScript.tempStats[(int)CharacterScript.sts.SPD];

                    charScript.tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    currRound.Add(characters[i]);
                }
            }
        } while (currRound.Count < characters.Count && numPool > 0);

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterScript charScript = characters[i].GetComponent<CharacterScript>();
            charScript.tempStats[(int)CharacterScript.sts.SPD] += 10;
        }

        roundCount++;
        PanelScript roundPanScript = panels[(int)pnls.ROUND_PANEL].GetComponent<PanelScript>();
        roundPanScript.PopulateText();
    }
}
