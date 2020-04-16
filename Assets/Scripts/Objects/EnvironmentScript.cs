using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScript : ObjectScript
{
    new public TileScript m_otherTile;
    // Start is called before the first frame update
    new void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        
    }

    override public void PlaceRandomly(BoardScript bScript)
    {
        m_boardScript = bScript;

        // Set up position
        TileScript script;
        bool isPlacable = false;
        int randX;
        int randZ;

        do
        {
            randX = Random.Range(0, m_boardScript.m_width - 1);
            randZ = Random.Range(0, m_boardScript.m_height - 1);

            script = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].GetComponent<TileScript>();

            if (!script.m_holding)
            {
                if (m_width <= 1)
                    isPlacable = true;
                else if (m_width == 2 && script.m_neighbors[(int)m_facing] && !script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding)
                {
                    isPlacable = true;
                    script.m_neighbors[(int)m_facing].GetComponent<TileScript>().m_holding = gameObject;
                }
            }
        } while (!isPlacable);

        script.m_holding = gameObject;
        transform.position = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width].transform.position;
        m_tile = m_boardScript.m_tiles[randX + randZ * m_boardScript.m_width];
    }
}
