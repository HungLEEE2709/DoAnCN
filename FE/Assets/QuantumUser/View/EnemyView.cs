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

        if (!f.TryGet(_entityView.EntityRef, out enemyInfo))
            return;

        // ========== DEATH HANDLING ==========
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

        // ========== HEALTH BAR ==========
        if (enemyInfo.Health > FP._0)
        {
            float hpPercent = (enemyInfo.CurrentHealth / enemyInfo.Health).AsFloat;
            healthBar.SetValue(hpPercent);
        }
        else
        {
            healthBar.SetValue(0);
        }

        // ========== ATTACK ANIMATION ==========
        // Chỉ trigger animation khi BẮT ĐẦU attack, sau đó để animation tự chạy
        if (enemyInfo.IsAttacking)
        {
            // Đang attack - set parameter
            animator.SetBool("IsAttack", true);
            
            // Nếu mới bắt đầu attack (wasAttacking = false)
            if (!wasAttacking)
            {
                // Force play animation từ đầu
                animator.Play("Goblin_Attack", 0, 0f);
                
                // Phát âm thanh attack
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyAttackSound();
                }
            }
        }
        else
        {
            // KHÔNG đang attack - tắt parameter
            animator.SetBool("IsAttack", false);
        }
        
        wasAttacking = enemyInfo.IsAttacking;

        // ========== MOVEMENT & VELOCITY ==========
        if (f.TryGet(_entityView.EntityRef, out body))
        {
            float vx = body.Velocity.X.AsFloat;
            float vy = body.Velocity.Y.AsFloat;
            float speed = new Vector2(vx, vy).magnitude;

            // Set Speed parameter cho Animator
            animator.SetFloat("Speed", speed);

            // Flip sprite dựa trên hướng di chuyển
            if (vx < 0)
                visual.localScale = new Vector3(-1, 1, 1);
            else if (vx > 0)
                visual.localScale = new Vector3(1, 1, 1);

            // ========== FIX WALKING ANIMATION ==========
            // Chỉ play walking/idle khi KHÔNG đang attack hoặc dead
            if (!enemyInfo.IsAttacking && !enemyInfo.IsDead)
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
                
                if (speed > 0.1f)
                {
                    // Đang di chuyển - play walking animation
                    if (!currentState.IsName("Goblin_Walking"))
                    {
                        animator.Play("Goblin_Walking", 0);
                    }
                }
                else
                {
                    // Đứng yên - play idle animation
                    if (!currentState.IsName("Goblin_Idle"))
                    {
                        animator.Play("Goblin_Idle", 0);
                    }
                }
            }
        }
    }
}
