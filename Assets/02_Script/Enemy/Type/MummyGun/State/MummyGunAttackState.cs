using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MummyGunAttackState : MummyGunRootState
{
    Transform targetTrm;
    public MummyGunAttackState(MummyGunStateController controller) : base(controller)
    {
        targetTrm = GameManager.Instance.player.transform;
    }

    protected override void EnterState()
    {
        controller.StopImmediately();
        controller.ChangeColor(Color.red);
        
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.3f);

        Sequence seq = DOTween.Sequence();
      
        Tween shakeTween =
            controller.transform.DOShakeScale(0.2f);

        seq.Append(shakeTween).AppendCallback(()=> Shoot());
        seq.OnComplete(() =>
        {
            StartCoroutine(AttackEndEvt());
        });
        seq.Play();
    }

    private void Shoot()
    {
        Vector2 dir =  (targetTrm.position - controller.attackPoint.position).normalized;
        controller.InstantiateBullet(dir, EEnemyBulletSpeedType.Linear, EEnemyBulletCurveType.None);
    }

    private IEnumerator AttackEndEvt()
    {
        yield return new WaitForSeconds(0.3f);

        Debug.Log($"Cool : {_data.name}");
        _data.SetCoolDown();
        controller.ChangeState(EMummyGunState.Idle);
    }

    protected override void ExitState()
    {

    }

    protected override void UpdateState()
    {

    }
}
