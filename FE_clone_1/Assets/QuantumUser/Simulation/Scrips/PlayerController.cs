using UnityEngine.Networking;
using UnityEngine;

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
                // thời gian attack (20 frame ~ 1/3 giây nếu 60fps)
                f.Info->AttackTimer = 20;
                f.Info->IsAttacking = true;
                Log.Debug("PLAYER ATTACK START");
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
