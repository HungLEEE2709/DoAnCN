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

        if (QuantumRunner.Default == null ||
            QuantumRunner.Default.Game == null ||
            QuantumRunner.Default.Game.Frames == null)
        {
            return;
        }

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
            // 🔥 Lấy PlayerUI
            PlayerUI ui = FindObjectOfType<PlayerUI>();

            // ❌ Không đủ Ki → không cho đánh
            if (ui.currentKi < 3)
            {
                // Tắt attack bên Quantum luôn
                info.IsAttacking = false;
                lastAttack = false;
                return;
            }

            // ✔ Có đủ Ki → chạy animation
            animator.SetTrigger("attack");

            // 🔥 Trừ Ki
            ui.UseKi(3);
        }


        lastAttack = info.IsAttacking;
    }
}
