using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class EnemyView : QuantumEntityViewComponent
{
    public Animator animator;
    public Transform visual;

    private PhysicsBody2D body;
    private EnemyInfo enemyInfo;
    private HealthBar healthBar;

    private bool playedDeath = false;
    private bool wasAttacking = false;

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Update()
    {
        if (_entityView == null || animator == null)
            return;

        Frame f = VerifiedFrame;

        if (f.TryGet(_entityView.EntityRef, out body))
        {
            float vx = body.Velocity.X.AsFloat;
            float vy = body.Velocity.Y.AsFloat;

            float speed = new Vector2(vx, vy).magnitude;
            animator.SetFloat("Speed", speed);

            if (vx < 0)
                visual.localScale = new Vector3(-1, 1, 1);
            else if (vx > 0)
                visual.localScale = new Vector3(1, 1, 1);
        }

        if (!f.TryGet(_entityView.EntityRef, out enemyInfo))
            return;

        if (enemyInfo.Health > FP._0)
        {
            float hpPercent = (enemyInfo.CurrentHealth / enemyInfo.Health).AsFloat;
            healthBar.SetValue(hpPercent);
        }
        else
        {
            healthBar.SetValue(0);
        }

        if (enemyInfo.IsDead)
        {
            if (!playedDeath)
            {

                animator.SetBool("IsDead", true);
                animator.SetBool("IsAttack", false);
                animator.SetFloat("Speed", 0);

                animator.Play("Goblin_Dying", 0, 0f);

                playedDeath = true;
            }

            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.IsName("Goblin_Dying") && st.normalizedTime >= 1f)
            {
                Destroy(gameObject);


            }

            return;
        }

        // ===== ENEMY ATTACK SOUND =====
        if (enemyInfo.IsAttacking && !wasAttacking)
        {
            // Enemy bắt đầu tấn công → phát âm thanh
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyAttackSound();
            }
        }
        wasAttacking = enemyInfo.IsAttacking;

        animator.SetBool("IsAttack", enemyInfo.IsAttacking);
    }
}
