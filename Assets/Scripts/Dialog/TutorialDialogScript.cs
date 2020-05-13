using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogScript : MonoBehaviour
{
    private Text m_text;

    // Start is called before the first frame update
    void Start()
    {
        if (!m_text)
            m_text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator IntroMessage()
    {
        Start();

        m_text.text = "Xen2 is a tactical turn-based game using an enegry system to coordinate big moves amongst your units.";

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        m_text.text = "There are 4 types of energy that can be generated and used by your units that all have different properties.";

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));


    }

    public void RedTutorialStart()
    {
        m_text.text = "Red units have greater destructive force.";
    }
}
