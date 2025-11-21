using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PlayerView : QuantumEntityViewComponent
{
    private Animator animator;
    private bool lastAttack = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {

        Frame frame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;

        var body = frame.Get<PhysicsBody2D>(_entityView.EntityRef);
        var info = frame.Get<PlayerInfo>(_entityView.EntityRef);

        // Move animation
        animator.SetBool("isRunning", body.Velocity != FPVector2.Zero);

        if (body.Velocity.X > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (body.Velocity.X < 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (info.IsAttacking && !lastAttack)
        {
            animator.SetTrigger("attack");
        }
        lastAttack = info.IsAttacking;

    }
}
