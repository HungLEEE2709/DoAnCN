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

    private Renderer[] renderers;
    private Canvas[] canvases;

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        renderers = GetComponentsInChildren<Renderer>();
        canvases = GetComponentsInChildren<Canvas>();
    }

    private void SetVisualsActive(bool active)
    {
        if (renderers != null)
        {
            foreach (var r in renderers) r.enabled = active;
        }
        if (canvases != null)
        {
            foreach (var c in canvases) c.enabled = active;
        }
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(active);
        }
    }

    private void Update()
    {
        if (_entityView == null || animator == null)
            return;

        Frame f = VerifiedFrame;

        if (!f.TryGet(_entityView.EntityRef, out enemyInfo))
            return;

        // ===== DEATH LOGIC =====
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
                // Hide visuals (keep script running)
                SetVisualsActive(false);
            }
            return;
        }

        // ===== RESPAWN / ALIVE LOGIC =====
        if (playedDeath)
        {
            // Just respawned
            playedDeath = false;
            SetVisualsActive(true);
            animator.SetBool("IsDead", false);
            animator.Play("Goblin_Idle", 0, 0f);
        }
        
        // Failsafe: If alive but hidden, show
        if (renderers.Length > 0 && !renderers[0].enabled)
        {
             SetVisualsActive(true);
        }

        // ===== MOVEMENT & ANIMATION =====
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

        if (enemyInfo.Health > FP._0)
        {
            float hpPercent = (enemyInfo.CurrentHealth / enemyInfo.Health).AsFloat;
            healthBar.SetValue(hpPercent);
        }

        animator.SetBool("IsAttack", enemyInfo.IsAttacking);
    }
}
