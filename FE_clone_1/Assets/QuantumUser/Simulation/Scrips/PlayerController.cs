namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerController : SystemMainThreadFilter<PlayerController.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PhysicsBody2D* Body;
            public PlayerInfo* Info;
        }

        public override void Update(Frame frame, ref Filter f)
        {
            var input = frame.GetPlayerInput(f.Info->PlayerRef);

            // MOVE
            f.Body->Velocity = input->Direction * f.Info->Speed;

            // ATTACK START
            if (input->Attack.WasPressed)
            {
                f.Info->AttackTimer = 10;
                f.Info->IsAttacking = true;
            }

            // ATTACK UPDATE
            if (f.Info->AttackTimer > 0)
            {
                f.Info->AttackTimer--;
            }
            else
            {
                f.Info->IsAttacking = false;
            }
        }

    }
}
