using Photon.Deterministic;

namespace Quantum
{
    public unsafe class CollisionSystem :
        SystemSignalsOnly,
        ISignalOnCollisionEnter2D,
        ISignalOnTriggerEnter2D
    {

        public void OnCollisionEnter2D(Frame f, CollisionInfo2D info)
        {
            // Log.Debug("Collision Detected");
            EntityRef a = info.Entity;
            EntityRef b = info.Other;
            ProcessCollision(f, a, b);
        }

        public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
        {
            // Log.Debug("Trigger Detected");
            EntityRef a = info.Entity;
            EntityRef b = info.Other;
            ProcessCollision(f, a, b);
        }

        private void ProcessCollision(Frame f, EntityRef a, EntityRef b)
        {
            // Removed Player Attack Logic (Now handled in PlayerController via AoE)
        }

        // HandlePlayerAttack method removed
    }
}
