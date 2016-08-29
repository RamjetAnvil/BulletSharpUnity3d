using System;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;

namespace BulletUnity
{
    public class RamjetPhysicsWorld : MonoBehaviour
    {
        [SerializeField] private int _poolCapacity = 100;
        [SerializeField] private BPhysicsWorld _physicsWorld;

        private ObjectPool<WorldEntry> _worldEntryPool;
        private List<WorldEntry> _registeredObjects;

        void Awake()
        {
            Debug.Assert(_physicsWorld.worldType >= BPhysicsWorld.WorldType.RigidBodyDynamics,
                "World type must not be collision only");

            _worldEntryPool = new ObjectPool<WorldEntry>(_poolCapacity);
            _registeredObjects = new List<WorldEntry>(_poolCapacity);

            _physicsWorld._InitializePhysicsWorld();
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

        private static readonly List<IPhysicsComponent> PhysicsComponentCache = new List<IPhysicsComponent>();
        public void AddObject(GameObject go)
        {
            var entry = _worldEntryPool.Take();

            entry.Root = go;

            PhysicsComponentCache.Clear();
            go.GetComponentsInChildren(includeInactive: true, results: PhysicsComponentCache);

            entry.PhysicsComponents.Clear();
            for (int i = 0; i < PhysicsComponentCache.Count; i++)
            {
                entry.PhysicsComponents.Add(PhysicsComponentCache[i] as MonoBehaviour);
            }
            entry.PhysicsComponents.Sort(ExecutionOrderComparer.Default);
            entry.PhysicsComponents.Reverse();

            entry.CollisionObjects.Clear();
            go.GetComponentsInChildren(entry.CollisionObjects);
            for (int i = 0; i < entry.CollisionObjects.Count; i++)
            {
                var collisionObject = entry.CollisionObjects[i];
                collisionObject.AddObjectToBulletWorld(_physicsWorld);
            }

            entry.Constraints.Clear();
            go.GetComponentsInChildren(entry.Constraints);
            for (int i = 0; i < entry.Constraints.Count; i++)
            {
                var constraint = entry.Constraints[i];
                constraint.AddToBulletWorld(_physicsWorld);
            }

            _registeredObjects.Add(entry);
        }

        private static IList<WorldEntry> _groupedEntries = new List<WorldEntry>(128);
        public void AddObjects(IList<GameObject> gameObjects) {
            // Two loops, add collision objects, add constraints

            _groupedEntries.Clear();

            for (int i = 0; i < gameObjects.Count; i++) {
                var go = gameObjects[i];

                var entry = _worldEntryPool.Take();
                entry.Constraints.Clear();
                entry.CollisionObjects.Clear();

                entry.Root = go;

                PhysicsComponentCache.Clear();
                go.GetComponentsInChildren(includeInactive: true, results: PhysicsComponentCache);

                go.GetComponentsInChildren(entry.CollisionObjects);
                for (int j = 0; j < entry.CollisionObjects.Count; j++) {
                    var collisionObject = entry.CollisionObjects[j];
                    collisionObject.AddObjectToBulletWorld(_physicsWorld);
                }

                _groupedEntries.Add(entry);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                var entry = _groupedEntries[i];
                var go = entry.Root;

                
                go.GetComponentsInChildren(entry.Constraints);
                for (int j = 0; j < entry.Constraints.Count; j++)
                {
                    var constraint = entry.Constraints[j];
                    constraint.AddToBulletWorld(_physicsWorld);
                }
            }

            _registeredObjects.AddRange(_groupedEntries);
        }

        public void RemoveObject(GameObject go)
        {
            WorldEntry entry = null;
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

            if (entry != null)
            {
                for (int i = 0; i < entry.Constraints.Count; i++)
                {
                    var constraint = entry.Constraints[i];
                    constraint.RemoveFromBulletWorld();
                }
                for (int i = 0; i < entry.CollisionObjects.Count; i++)
                {
                    var collisionObject = entry.CollisionObjects[i];
                    collisionObject.RemoveObjectFromBulletWorld();
                }
                _worldEntryPool.Return(entry);
            }
        }

        [Serializable]
        private class WorldEntry
        {
            [SerializeField] public GameObject Root;
            public readonly List<MonoBehaviour> PhysicsComponents;
            public readonly List<BCollisionObject> CollisionObjects;
            public readonly List<BTypedConstraint> Constraints;

            public WorldEntry()
            {
                Root = null;
                PhysicsComponents = new List<MonoBehaviour>();
                CollisionObjects = new List<BCollisionObject>();
                Constraints = new List<BTypedConstraint>();
            }
        }

    }
}