using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BulletSharp;

namespace BulletUnity
{
    public class RamjetPhysicsWorld : MonoBehaviour
    {
        [SerializeField] private BPhysicsWorld _physicsWorld;

        private IList<WorldEntry> _registeredObjects;

        void Awake()
        {
            _registeredObjects = new List<WorldEntry>();
        }

        void FixedUpdate()
        {
            SimulateStep(Time.fixedDeltaTime);
        }

        // TODO Add simulate step overloads
        public void SimulateStep(float deltaTime)
        {
            for (int i = 0; i < _registeredObjects.Count; i++)
            {
                var worldEntry = _registeredObjects[i];
                for (int j = 0; j < worldEntry.PhysicsComponents.Count; j++)
                {
                    var physicsBehaviour = worldEntry.PhysicsComponents[j];
                    if (physicsBehaviour == null)
                    {
                        worldEntry.PhysicsComponents.RemoveAt(i);
                    }
                    else if (physicsBehaviour.enabled)
                    {
                        (physicsBehaviour as IPhysicsComponent).PhysicsUpdate(deltaTime);
                    }
                }
            }

            (_physicsWorld.world as DynamicsWorld).StepSimulation(deltaTime);
        }

        public void AddObject(GameObject go)
        {
            var physicsBehaviours = new List<MonoBehaviour>(go.GetComponentsInChildren<IPhysicsComponent>().Cast<MonoBehaviour>());
            physicsBehaviours.Sort(ExecutionOrderComparer.Default);
//            foreach (var behaviour in _updateBehaviours)
//            {
//                Debug.Log(behaviour.GetType());
//            }
            physicsBehaviours.Reverse();

            var collisionObjects = new List<BCollisionObject>(go.GetComponentsInChildren<BCollisionObject>());
            // TODO Pool world entries
            var worldEntry = new WorldEntry(go, physicsBehaviours, collisionObjects);

            for (int i = 0; i < worldEntry.CollisionObjects.Count; i++)
            {
                var collisionObject = worldEntry.CollisionObjects[i];
                collisionObject.AddObjectToBulletWorld(_physicsWorld);
            }
            _registeredObjects.Add(worldEntry);
        }

        public void RemoveObject(GameObject go)
        {
            WorldEntry? entry = null;
            for (int i = _registeredObjects.Count - 1; i >= 0; i--)
            {
                var o = _registeredObjects[i];
                if (o.Root == go)
                {
                    entry = o;
                    _registeredObjects.RemoveAt(i);
                    break;
                }
            }

            if (entry.HasValue)
            {
                for (int i = 0; i < entry.Value.CollisionObjects.Count; i++)
                {
                    var collisionObject = entry.Value.CollisionObjects[i];
                    collisionObject.RemoveObjectFromBulletWorld();
                }
            }
        }

        private struct WorldEntry
        {
            public readonly GameObject Root;
            public readonly IList<MonoBehaviour> PhysicsComponents;
            public readonly IList<BCollisionObject> CollisionObjects;

            public WorldEntry(
                GameObject root,
                IList<MonoBehaviour> physicsComponents,
                IList<BCollisionObject> collisionObjects)
            {
                Root = root;
                PhysicsComponents = physicsComponents;
                CollisionObjects = collisionObjects;
            }
        }

    }
}