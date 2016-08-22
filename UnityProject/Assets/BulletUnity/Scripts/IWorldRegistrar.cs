using System;

namespace BulletUnity 
{
    public interface IWorldRegistrar : IDisposable {
        bool AddTo(BPhysicsWorld unityWorld);
        void RemoveFrom(BPhysicsWorld world);
    }
}
