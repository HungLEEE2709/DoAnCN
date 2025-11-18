namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public class PlayerView : QuantumEntityViewComponent
    {
        private Animator animator;
        private PhysicsBody2D body;

        private bool isAttacking = false;
        private float attackTimer = 0f;
        private float attackDuration = 0.1f; 

        private readonly int IdleState = Animator.StringToHash("IdleP1");
        private readonly int RunState = Animator.StringToHash("RunP1");
        private readonly int AttackState = Animator.StringToHash("Atk1P1");

        public override void OnInitialize()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            body = PredictedFrame.Get<PhysicsBody2D>(_entityView.EntityRef);
            FPVector2 velocity = body.Velocity;

            bool isMoving = velocity != FPVector2.Zero;


            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && !isAttacking && !isMoving)
            {
                isAttacking = true;
                attackTimer = 0f;

                animator.speed = 0.1f;  
                animator.Play(AttackState);
                return;
            }

            if (isAttacking)
            {
                attackTimer += UnityEngine.Time.deltaTime;

                if (attackTimer < attackDuration)
                    return;   


                isAttacking = false;
                animator.speed = 1f;
            }

            if (!isMoving)
                animator.Play(IdleState);
            else
                animator.Play(RunState);


            if (velocity.X > 0)
                animator.transform.localScale = new Vector3(-1, 1, 1);
            else if (velocity.X < 0)
                animator.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
