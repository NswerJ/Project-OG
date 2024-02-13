using FSM_System;

public class BatEnemyRootState : FSM_State<EBatState>
{
    protected new BatStateController controller;
    protected EnemyDataSO _data => controller.EnemyData;

    public BatEnemyRootState(BatStateController controller) : base(controller)
    {
        this.controller = controller;
    }
}
