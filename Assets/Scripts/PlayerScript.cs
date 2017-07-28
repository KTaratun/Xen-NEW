using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public enum eng { GRN, RED, WHT, BLU }

    public List<GameObject> characters;
    public GameObject energyPanel;
    public int num;
    public int[] energy;

	// Use this for initialization
	void Start ()
    {
        energy = new int[4];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetEnergyPanel()
    {
        Text[] text = energyPanel.GetComponentsInChildren<Text>();

        for (int i = 0; i < text.Length; i++)
            text[i].text = energy[i].ToString();
    }

    public bool CheckEnergy(string eng)
    {
        if (eng[0] == 'g' || eng[0] == 'r' || eng[0] == 'w' || eng[0] == 'b')
            return true;

        int[] engCheck =  new int[4];

        for (int i = 0; i < eng.Length; i++)
        {
            if (eng[i] == 'G')
                engCheck[0]++;
            else if (eng[i] == 'R')
                engCheck[1]++;
            else if (eng[i] == 'W')
                engCheck[2]++;
            else if (eng[i] == 'B')
                engCheck[3]++;
        }

        for (int i = 0; i < energy.Length; i++)
            if (engCheck[i] > energy[i])
                return false;

        return true;
    }
}
