﻿using System.Collections.Generic;
using UnityEngine;
using BulletSharp;

namespace BulletUnity
{
    public class RamjetPhysicsWorld : MonoBehaviour
    {
        [SerializeField] private int _poolCapacity = 100;
        [SerializeField] private BPhysicsWorld _physicsWorld;

        private ObjectPool<WorldEntry> _worldEntryPool;
        private IList<WorldEntry> _registeredObjects;

        void Awake()
        {
            _worldEntryPool = new ObjectPool<WorldEntry>(_poolCapacity);
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

        private static readonly List<IPhysicsComponent> PhysicsComponentCache = new List<IPhysicsComponent>();
        public void AddObject(GameObject go)
        {
            var worldEntry = _worldEntryPool.Take();

            worldEntry.Root = go;

            PhysicsComponentCache.Clear();
            go.GetComponentsInChildren(includeInactive: true, results: PhysicsComponentCache);

            worldEntry.PhysicsComponents.Clear();
            for (int i = 0; i < PhysicsComponentCache.Count; i++)
            {
                worldEntry.PhysicsComponents.Add(PhysicsComponentCache[i] as MonoBehaviour);
            }
            worldEntry.PhysicsComponents.Sort(ExecutionOrderComparer.Default);
//            foreach (var behaviour in _updateBehaviours)
//            {
//                Debug.Log(behaviour.GetType());
//            }
            worldEntry.PhysicsComponents.Reverse();

            worldEntry.CollisionObjects.Clear();
            go.GetComponentsInChildren(worldEntry.CollisionObjects);

            for (int i = 0; i < worldEntry.CollisionObjects.Count; i++)
            {
                var collisionObject = worldEntry.CollisionObjects[i];
                collisionObject.AddObjectToBulletWorld(_physicsWorld);
            }
            _registeredObjects.Add(worldEntry);
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
                    _worldEntryPool.Return(entry);
                    break;
                }
            }

            if (entry != null)
            {
                for (int i = 0; i < entry.CollisionObjects.Count; i++)
                {
                    var collisionObject = entry.CollisionObjects[i];
                    collisionObject.RemoveObjectFromBulletWorld();
                }
            }
        }

        private class WorldEntry
        {
            public GameObject Root;
            public readonly List<MonoBehaviour> PhysicsComponents;
            public readonly List<BCollisionObject> CollisionObjects;

            public WorldEntry()
            {
                Root = null;
                PhysicsComponents = new List<MonoBehaviour>();
                CollisionObjects = new List<BCollisionObject>();
            }
        }

    }
}