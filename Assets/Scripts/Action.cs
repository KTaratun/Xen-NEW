using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : StateMachineBehaviour {

    public enum aniAct { ACT_TIME, MELEE_ANI, RANGED_ANI}

    bool m_hasDoneAction;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_hasDoneAction = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AnimationProperties(stateInfo, aniAct.ACT_TIME))
        {
            if (AnimationProperties(stateInfo, aniAct.MELEE_ANI))
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Melee Hit Sound 1.2"));
            else if (stateInfo.fullPathHash == Animator.StringToHash("Base.Ability"))
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Ability Sound 1"));

            animator.GetComponentInParent<CharacterScript>().Action();
            m_hasDoneAction = true;
        }
        else if (AnimationProperties(stateInfo, aniAct.RANGED_ANI))
        {
            BoardScript board = animator.GetComponentInParent<ObjectScript>().m_boardScript;

            if (stateInfo.fullPathHash == Animator.StringToHash("Base.Throw"))
            {
                board.m_grenade.GetComponent<ObjectScript>().MovingStart(board.m_selected, true, false);
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Grenade Shot 1"));
            }
            else if (stateInfo.fullPathHash == Animator.StringToHash("Base.Ranged"))
            {
                board.m_laser.GetComponent<ObjectScript>().MovingStart(board.m_selected, true, false);
                animator.GetComponentInParent<CharacterScript>().m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Gun Sound 1"));
            }

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

    private bool AnimationProperties(AnimatorStateInfo _stateInfo, aniAct _aniAct)
    {
        if (_aniAct == aniAct.ACT_TIME)
        {
            if (_stateInfo.fullPathHash == Animator.StringToHash("Base.Melee") &&
            _stateInfo.normalizedTime > 0.3777777777777778 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Ability") &&
            _stateInfo.normalizedTime > 0.5 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Kick") &&
            _stateInfo.normalizedTime > 0.48 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Stab") &&
            _stateInfo.normalizedTime > 0.42 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Slash") &&
            _stateInfo.normalizedTime > 0.38 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Sweep") &&
            _stateInfo.normalizedTime > 0.3 && !m_hasDoneAction)
                return true;
        }
        else if (_aniAct == aniAct.MELEE_ANI)
        {
            if (_stateInfo.fullPathHash == Animator.StringToHash("Base.Melee") ||
                _stateInfo.fullPathHash == Animator.StringToHash("Base.Kick") ||
                _stateInfo.fullPathHash == Animator.StringToHash("Base.Stab") ||
                _stateInfo.fullPathHash == Animator.StringToHash("Base.Slash") ||
                _stateInfo.fullPathHash == Animator.StringToHash("Base.Sweep"))
                return true;
        }
        else if (_aniAct == aniAct.RANGED_ANI)
        {
            if (_stateInfo.fullPathHash == Animator.StringToHash("Base.Throw") &&
            _stateInfo.normalizedTime > 0.5 && !m_hasDoneAction ||
            _stateInfo.fullPathHash == Animator.StringToHash("Base.Ranged") &&
            _stateInfo.normalizedTime > 0.72 && !m_hasDoneAction)
                return true;
        }

        return false;
    }
}
