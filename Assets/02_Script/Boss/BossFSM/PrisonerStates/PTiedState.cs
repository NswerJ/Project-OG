using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTiedState : BossBaseState
{
    public PTiedState(Boss boss) : base(boss)
    {
        _willChange = false;
    }

    public override void OnBossStateExit()
    {
        _willChange = true;
        _boss.StopCoroutine(RandomPattern(_boss.bossSo.PatternChangeTime));
    }

    public override void OnBossStateOn()
    {
        _boss.StartCoroutine(RandomPattern(_boss.bossSo.PatternChangeTime));
    }

    public override void OnBossStateUpdate()
    {
        
    }

    private GameObject CheckPlayerCircleCastG(float radius)
    {
        RaycastHit2D[] hit = Physics2D.CircleCastAll(_boss.transform.position, radius, Vector2.zero);
        foreach (var h in hit)
        {
            if (h.collider.gameObject.tag == "Player")
            {
                return h.collider.gameObject;
            }
        }

        return null;
    }

    public override IEnumerator RandomPattern(float waitTime)
    {
        Debug.Log("tied");

        yield return new WaitForSeconds(waitTime);

        int rand = Random.Range(1, 3);

        switch (rand)
        {
            case 1:
                _boss.StartCoroutine(OmnidirAttack(20, 3, 1, 1));
                break;
            case 2:
                _boss.StartCoroutine(SoundAttack(3, 1));
                break;
        }
    }

    // 전방향으로 공격한다
    private IEnumerator OmnidirAttack(int bulletCount, float speed, float time, int burstCount)
    {
        GameObject[,] bullets = new GameObject[burstCount, bulletCount];
        for (int i = 0; i < burstCount; i++)
        {
            for (int j = 0; j < bulletCount; j++)
            {
                bullets[i, j] = ObjectPool.Instance.GetObject(ObjectPoolType.BossBulletType0, _boss.transform);
                bullets[i, j].GetComponent<BossBullet>().Attack(_boss.bossSo.Damage);
                bullets[i, j].transform.position = _boss.transform.position;
                bullets[i, j].transform.rotation = Quaternion.identity;

                Rigidbody2D rigid = bullets[i, j].GetComponent<Rigidbody2D>();
                Vector2 dir = new Vector2(Mathf.Cos(Mathf.PI * 2 * j / bulletCount), Mathf.Sin(Mathf.PI * 2 * j / bulletCount));
                rigid.velocity = dir.normalized * speed;

                // 원기둥의 경우 총알이 발사 방향으로 회전
                //Vector3 rotation = Vector3.forward * 360 * i / bulletCount + Vector3.forward * 90;
                //bullet.transform.Rotate(rotation);
            }

            yield return new WaitForSeconds(time);
        }

        yield return new WaitForSeconds(time);

        for (int i = 0; i < burstCount; i++)
        {
            for (int j = 0; j < bulletCount; j++)
            {
                ObjectPool.Instance.ReturnObject(ObjectPoolType.BossBulletType0, bullets[i, j]);
            }

            yield return new WaitForSeconds(time);
        }

        if(!_willChange)
            _boss.StartCoroutine(RandomPattern(_boss.bossSo.PatternChangeTime));
    }

    // 범위안에 플레이어가 있으면 피해를 준다
    private IEnumerator SoundAttack(int radius, float waitTime)
    {
        GameObject warning = ObjectPool.Instance.GetObject(ObjectPoolType.WarningType1, _boss.transform);
        warning.transform.localScale = warning.transform.localScale * radius * 2;
        warning.transform.position = _boss.transform.position;
        warning.transform.rotation = Quaternion.identity;

        yield return new WaitForSeconds(waitTime);

        ObjectPool.Instance.ReturnObject(ObjectPoolType.WarningType1, warning);

        GameObject p = CheckPlayerCircleCastG(radius);

        if (p)
        {
            Debug.Log("데미지 줌");
            if (p.TryGetComponent<IHitAble>(out var IhitAble))
            {
                IhitAble.Hit(_boss.bossSo.Damage);
            }
        }

        if (!_willChange)
            _boss.StartCoroutine(RandomPattern(_boss.bossSo.PatternChangeTime));
    }
}
