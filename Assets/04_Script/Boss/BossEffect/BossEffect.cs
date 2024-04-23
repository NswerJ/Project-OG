using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEffect : MonoBehaviour
{
    protected virtual void OnDisable()
    {
        ObjectPool.Instance.ReturnObject(gameObject, 2);
    }
}