using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlateBoss : Boss
{
    private enum BossState
    {
        Idle,
        Tied,
        OneBroken,
        Free,
        Dead
    }

    private BossState _curBossState;

    private BossFSM _bossFSM;

    private GameObject[] _restraints = new GameObject[2];

    private GameObject[,] _chains = new GameObject[2, 10];

    private int _restraintIndex = 0;
    private int _restrainCount = 0;
    private int _chainCount = 0;
    private int _shortenChainIndex = 0;

    [SerializeField]
    private float _restraintDistance;
    [SerializeField]
    private float _unChainTime;
    private float _currentTime = 0;
    private float _shakeInterval;

    private bool _isShaked = false;

    void Start()
    {
        _currentTime = 0;
        _restraintIndex = 0;
        _restrainCount = _restraints.Length;
        _chainCount = _chains.GetLength(1);
        _shortenChainIndex = 0;
        _shakeInterval = _unChainTime / 2;
        _isShaked = false;
        originPos = Vector2.zero;
        isTied = true;

        ChainSetting();

        _curBossState = BossState.Idle;
        _bossFSM = new BossFSM(new BossIdleState(this));

        ChangeBossState(BossState.Tied);

        StartCoroutine(ShortenChain());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, bossSo.StopRadius);
    }

    protected override void Update()
    {
        base.Update();
        if(!wasDead)
        {
            ChangeState();

            if (_restraintIndex < _restrainCount)
            {
                TimeChecker(Time.deltaTime * (_restraintIndex + 1));
            }

            ChainsFollowBoss();

            _bossFSM.UpdateBossState();
        }
    }

    private void ChainSetting()
    {
        for (int i = 0; i < _restrainCount; i++)
        {
            _restraints[i] = ObjectPool.Instance.GetObject(ObjectPoolType.PrisonerRestraint);
            var rad = Mathf.Deg2Rad * i * 360 / _restrainCount;
            var x = _restraintDistance * Mathf.Cos(rad);
            var y = _restraintDistance * Mathf.Sin(rad);
            _restraints[i].transform.position = transform.GetChild(i).position + new Vector3(x, y, 0);
            _restraints[i].transform.rotation = Quaternion.identity;
            for (int j = 0; j < _chainCount; j++)
            {
                var xx = j * _restraintDistance / _chainCount * Mathf.Cos(rad);
                var yy = j * _restraintDistance / _chainCount * Mathf.Sin(rad);
                _chains[i, j] = ObjectPool.Instance.GetObject(ObjectPoolType.PrisonerChain, _restraints[i].transform.GetChild(0).transform);
                _chains[i, j].transform.position = transform.GetChild(i).position + new Vector3(xx, yy, 0);
                _chains[i, j].transform.rotation = Quaternion.identity;
            }
        }
    }

    private void ChainsFollowBoss()
    {
        for (int i = 0; i < _restrainCount; i++)
        {
            float angle = Mathf.Atan2(transform.position.y - _restraints[i].transform.position.y, transform.position.x - _restraints[i].transform.position.x) * Mathf.Rad2Deg;

            _restraints[i].transform.GetChild(0).rotation = Quaternion.AngleAxis(angle + 180 - i * 180, Vector3.forward);
        }
    }

    private IEnumerator ShortenChain()
    {
        while(_restraintIndex < 2)
        {
            if (_restraintIndex > 0)
            {
                if (Vector3.Distance(transform.position, _chains[_restraintIndex, _shortenChainIndex].transform.position) < 0.5f)
                {
                    if (_shortenChainIndex < _chainCount - 1)
                    {
                        _chains[_restraintIndex, _shortenChainIndex].gameObject.SetActive(false);
                        _shortenChainIndex++;
                    }
                    yield return null;
                }
                else if (Vector3.Distance(transform.position, _chains[_restraintIndex, _shortenChainIndex].transform.position) > 0.5f)
                {
                    if (_shortenChainIndex > 0)
                    {
                        _chains[_restraintIndex, _shortenChainIndex].gameObject.SetActive(true);
                        _shortenChainIndex--;
                    }
                    yield return null;
                }
            }

            yield return null;
        }
        
    }

    private void ChangeState()
    {
        if(dead && !wasDead)
        {
            if(_restraintIndex < 2)
                ReturnRestraintAndChains();
            StartCoroutine(CameraManager.Instance.CameraShake(0, 0));
            wasDead = true;
            isTied = false;
            isOneBroken = false;
            ChangeBossState(BossState.Dead);
        }
        else
        {
            if (!isRunning)
            {
                switch (_curBossState)
                {
                    case BossState.Tied:
                        if (_restraintIndex > 0)
                        {
                            isTied = false;
                            isOneBroken = true;
                            ChangeBossState(BossState.OneBroken);
                        }
                        break;
                    case BossState.OneBroken:
                        if (_restraintIndex > 1)
                        {
                            isOneBroken = false;
                            ChangeBossState(BossState.Free);
                        }
                        break;
                }
            }
        }
    }

    private void ChangeBossState(BossState nextBossState)
    {
        _curBossState = nextBossState;

        switch (_curBossState)
        {
            case BossState.Idle:
                _bossFSM.ChangeBossState(new BossIdleState(this));
                break;
            case BossState.Tied:
                _bossFSM.ChangeBossState(new PTiedState(this));
                break;
            case BossState.OneBroken:
                _bossFSM.ChangeBossState(new POneBrokenState(this));
                break;
            case BossState.Free:
                _bossFSM.ChangeBossState(new PFreeState(this));
                break;
            case BossState.Dead:
                _bossFSM.ChangeBossState(new BossDeadState(this));
                break;
        }
    }

    private void TimeChecker(float time)
    {
        if (_currentTime <= 0)
            _currentTime = 0;

        if (_currentTime >= _shakeInterval && !_isShaked)
        {
            StartCoroutine(CameraManager.Instance.CameraShake(3, 1));
            _isShaked = true;
        }


        if (_currentTime <= _unChainTime)
            _currentTime += time;
        else
        {
            StartCoroutine(CameraManager.Instance.CameraShake(3, 0.5f));
            StartCoroutine(UnChain(3));
            _currentTime = 0;
        }
    }

    private IEnumerator UnChain(float speed)
    {
        for (int i = 0; i < _chainCount; i++)
        {
            GameObject chainFragment = ObjectPool.Instance.GetObject(ObjectPoolType.ChainFragment, chainCollector.transform);
            chainFragment.GetComponent<BossBullet>().Attack(bossSo.Damage);
            chainFragment.transform.position = _chains[_restraintIndex, i].transform.position;
            chainFragment.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = chainFragment.GetComponent<Rigidbody2D>();
            Vector2 dir = new Vector2(Mathf.Cos(Mathf.PI * 2 * UnityEngine.Random.Range(0, 361) / 360), Mathf.Sin(Mathf.PI * 2 * UnityEngine.Random.Range(0, 361) / 360));
            rigid.velocity = dir.normalized * speed;
        }
        for(int i = 0; i < _chainCount; i++)
        {
            GameObject split = ObjectPool.Instance.GetObject(ObjectPoolType.BossBulletType0, chainCollector.transform);
            split.GetComponent<BossBullet>().Attack(bossSo.Damage);
            split.transform.position = _restraints[_restraintIndex].transform.position;
            split.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = split.GetComponent<Rigidbody2D>();
            Vector2 dir = new Vector2(Mathf.Cos(Mathf.PI * 2 * i / _chainCount), Mathf.Sin(Mathf.PI * 2 * i / _chainCount));
            rigid.velocity = dir.normalized * speed;
        }

        ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerRestraint, _restraints[_restraintIndex]);
        for (int i = 0; i < _chainCount; i++)
        {
            ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerChain, _chains[_restraintIndex, i]);
        }

        _restraintIndex++;
        if(_restraintIndex == 1)
        {
            originPos = _restraints[_restraintIndex].transform.position;
        }
        _isShaked = false;

        yield return null;
    }

    private void ReturnRestraintAndChains()
    {
        if(_restraintIndex > 0)
        {
            ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerRestraint, _restraints[_restraintIndex]);
            for (int j = 0; j < _chainCount; j++)
            {
                ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerChain, _chains[_restraintIndex, j]);
            }
        }
        else
        {
            for (int i = 0; i < _restrainCount; i++)
            {
                ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerRestraint, _restraints[i]);
                for (int j = 0; j < _chainCount; j++)
                {
                    ObjectPool.Instance.ReturnObject(ObjectPoolType.PrisonerChain, _chains[i, j]);
                }
            }
        }
        
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }

}