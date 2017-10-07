﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : StateMachineBehaviour {

    bool m_hasDoneAction;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_hasDoneAction = false;
        animator.GetComponentInParent<CharacterScript>().m_audio.volume = 1.0f;


        //for (int i = 0; i < animator.GetComponentInParent<CharacterScript>().m_weapons.Length; i++)
        //    animator.GetComponentInParent<CharacterScript>().m_weapons[i].SetActive(false);
        //
        //    if (stateInfo.nameHash == Animator.StringToHash("Base.Melee"))
        //    animator.GetComponentInParent<CharacterScript>().m_weapons[(int)CharacterScript.weaps.SWORD].SetActive(true);
        //else if (stateInfo.nameHash == Animator.StringToHash("Base.Range"))
        //    animator.GetComponentInParent<CharacterScript>().m_weapons[(int)CharacterScript.weaps.HGUN].SetActive(true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.fullPathHash == Animator.StringToHash("Base.Melee") && 
            stateInfo.normalizedTime > 0.3777777777777778 && !m_hasDoneAction ||
            stateInfo.fullPathHash == Animator.StringToHash("Base.Ranged") &&
            stateInfo.normalizedTime > 0.72 && !m_hasDoneAction ||
            stateInfo.fullPathHash == Animator.StringToHash("Base.Throw") &&
            stateInfo.normalizedTime > 0.7857142857142857 && !m_hasDoneAction ||
            stateInfo.fullPathHash == Animator.StringToHash("Base.Ability") &&
            stateInfo.normalizedTime > 0.5 && !m_hasDoneAction)
        {
            if (stateInfo.fullPathHash == Animator.StringToHash("Base.Melee"))
            {
                animator.GetComponentInParent<CharacterScript>().m_audio.volume = 0.6f;
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Melee Hit Sound 1.2"));
            }
            else if (stateInfo.fullPathHash == Animator.StringToHash("Base.Ranged"))
            {
                animator.GetComponentInParent<CharacterScript>().m_audio.volume = 0.5f;
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Gun Sound 1"));
            }
            else if (stateInfo.fullPathHash == Animator.StringToHash("Base.Ability"))
            {
                animator.GetComponentInParent<CharacterScript>().m_audio.volume = 0.5f;
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Ability Sound 1"));
            }

            animator.GetComponentInParent<CharacterScript>().Action();
            m_hasDoneAction = true;
        }
    }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    for (int i = 0; i < animator.GetComponentInParent<CharacterScript>().m_weapons.Length; i++)
    //        animator.GetComponentInParent<CharacterScript>().m_weapons[i].SetActive(false);
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}