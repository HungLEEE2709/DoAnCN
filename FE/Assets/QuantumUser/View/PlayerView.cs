using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PlayerView : QuantumEntityViewComponent
{
    private Animator animator;
    private bool lastAttack = false;

    public float cameraSmooth = 0.1f;
    public Vector3 cameraOffset = new Vector3(0, 1.5f, -10f);

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (QuantumRunner.Default == null ||
            QuantumRunner.Default.Game == null ||
            QuantumRunner.Default.Game.Frames == null)
            return;

        Frame frame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;

        var body = frame.Get<PhysicsBody2D>(_entityView.EntityRef);
        var info = frame.Get<PlayerInfo>(_entityView.EntityRef);

        // ===== RUNNING ANIMATION =====
        animator.SetBool("isRunning", body.Velocity != FPVector2.Zero);

        if (body.Velocity.X > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (body.Velocity.X < 0)
            transform.localScale = new Vector3(1, 1, 1);

        // ===== ATTACK + KI UI =====
        if (info.IsAttacking && !lastAttack)
        {
            PlayerUI ui = FindObjectOfType<PlayerUI>();
            if (ui != null)
            {
                if (ui.currentKi < 3)
                {
                    lastAttack = false;
                    return;
                }

                animator.SetTrigger("attack");
                ui.UseKi(3);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }
        }

        lastAttack = info.IsAttacking;

        // ===== SYNC HP → UI =====
        {
            PlayerUI ui = FindObjectOfType<PlayerUI>();
            ui.SetHealthFromQuantum(info.CurrentHealth.AsFloat);
        }

        // ===== CAMERA FOLLOW LOCAL PLAYER =====
        if (QuantumRunner.Default.Game.PlayerIsLocal(info.PlayerRef) && Camera.main != null)
        {
            Vector3 targetPos = new Vector3(
                transform.position.x,
                transform.position.y + cameraOffset.y,
                cameraOffset.z
            );

            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                targetPos,
                cameraSmooth
            );
        }
    }
}
