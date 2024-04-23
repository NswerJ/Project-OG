using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageTransition : MonoBehaviour
{
    [SerializeField]
    private AudioClip _stageTransitionClip;

    Animator _stageAnimator;
    Image _transitionImage;

    private int _startHash = Animator.StringToHash("Start");
    private int _endHash = Animator.StringToHash("End");
    private int _valueHash = Animator.StringToHash("Value");

    private void Awake()
    {
        _stageAnimator = GetComponent<Animator>();
        _transitionImage = GetComponent<Image>();
    }

    public void StartTransition()
    {
        RandomTransitionValue();

        if(_stageTransitionClip != null)
            SoundManager.Instance.SFXPlay("Transition", _stageTransitionClip, 1f);

        //_stageAnimator.ResetTrigger(_startHash);
        _transitionImage.color = Color.white;
        _stageAnimator.SetTrigger(_startHash);
    }

    public void EndTransition()
    {
        RandomTransitionValue();

        //_stageAnimator.ResetTrigger(_endHash);
        _stageAnimator.SetTrigger(_endHash);

    }

    public void EndTransitionEvent()
    {
        _transitionImage.color = Color.clear;
    }

    private void RandomTransitionValue()
    {
        _stageAnimator.SetInteger(_valueHash, Random.Range(0, 3));
    }
}