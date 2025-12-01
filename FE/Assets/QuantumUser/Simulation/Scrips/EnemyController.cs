namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class EnemyController : SystemMainThreadFilter<EnemyController.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PhysicsBody2D* Body;
            public Transform2D* Transform;
            public EnemyInfo* EnemyInfo;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Khởi tạo gốc di chuyển
            if (filter.EnemyInfo->MaxDistance == FP._0)
            {
                filter.EnemyInfo->Origin = filter.Transform->Position;
                filter.EnemyInfo->MaxDistance = FP._2;   // enemy đi trong bán kính 2
            }

            FP speed = FP._2;

            // Nếu chưa có hướng thì cho đi sang phải
            if (filter.EnemyInfo->Direction == FPVector2.Zero)
            {
                filter.EnemyInfo->Direction = FPVector2.Right;
            }

            // Áp velocity
            filter.Body->Velocity = filter.EnemyInfo->Direction * speed;

            // Tính khoảng cách X từ vị trí hiện tại tới điểm gốc
            FP distance = filter.Transform->Position.X - filter.EnemyInfo->Origin.X;

            // Nếu đi quá phải → quay trái
            if (distance > filter.EnemyInfo->MaxDistance)
            {
                filter.EnemyInfo->Direction = FPVector2.Left;
            }
            // Nếu đi quá trái → quay phải
            else if (distance < -filter.EnemyInfo->MaxDistance)
            {
                filter.EnemyInfo->Direction = FPVector2.Right;
            }
        }
    }
}
