using FSM_System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETestEnemyState
{
    Idle,
    Move,
    Jump,
    Dash,
}

public class TestEnemyController : FSM_Controller<ETestEnemyState>
{
    [field: SerializeField] public TestEnemyDataSO EnemyData { get; protected set; }

    [SerializeField] public GameObject grid;
    [SerializeField] public EnemyBullet bullet;

    protected override void Awake()
    {
        EnemyData = Instantiate(EnemyData);

        var idleState = new TestEnemyRootState(this);
        var idleToMove = new TestEnemyTransitionToMoveOrIdle(this, ETestEnemyState.Move);
        
        idleState
            .AddTransition<ETestEnemyState>(idleToMove);
        
        var moveState = new TestEnemyMoveState(this);
        var moveToIdle = new TestEnemyTransitionToMoveOrIdle(this, ETestEnemyState.Idle);
        var goToDash = new TestEnemyDashTransition(this, ETestEnemyState.Dash);
        //var goToJump = new TestEnemyJumpTransition(this, ETestEnemyState.Jump);

        moveState
            .AddTransition<ETestEnemyState>(moveToIdle)
            .AddTransition<ETestEnemyState>(goToDash);
            //.AddTransition<ETestEnemyState>(goToJump);

        var dashState = new TestEnemyDashState(this);
        //var jumpState = new TestEnemyJumpState(this);
        
        AddState(idleState, ETestEnemyState.Idle);
        AddState(moveState, ETestEnemyState.Move);
        AddState(dashState, ETestEnemyState.Dash);
        //AddState(dashState, ETestEnemyState.Jump);
    }

    //Debug
    public void InstantiateDebugGrid(Vector2 pos)
    {
        Destroy(Instantiate(grid, pos, Quaternion.identity), 2);
    }

    public void InstantiateBullet(Vector2 dir, EEnemyBulletSpeedType speedType, EEnemyBulletCurveType curveType = EEnemyBulletCurveType.None)
    {
        Instantiate(bullet, transform.position, Quaternion.identity).Shoot(dir, speedType, curveType);
    }
}
