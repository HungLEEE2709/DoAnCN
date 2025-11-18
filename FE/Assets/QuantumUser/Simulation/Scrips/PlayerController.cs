namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerController : SystemMainThreadFilter<PlayerController.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PhysicsBody2D* Body;
            public Transform2D* Transform;
            public PlayerInfo* PlayerInfo;
        }
        public override void Update(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.PlayerInfo->PlayerRef);
            filter.Body->Velocity = input->Direction * filter.PlayerInfo->Speed;
        }
    }
}
