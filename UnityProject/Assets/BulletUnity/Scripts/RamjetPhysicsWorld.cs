using System;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using UnityEngine.Assertions;

namespace BulletUnity
{
    public class RamjetPhysicsWorld : MonoBehaviour
    {
        [SerializeField] private int _maxObjects = 1024;
        [SerializeField] private BPhysicsWorld _physicsWorld;
        [SerializeField] private int _maxSubSteps = 5;
        [SerializeField] private float _fixedTimeStep = 0.01f;

        private ObjectPool<WorldEntry> _worldEntryPool;
        private IDictionary<int, WorldEntry> _registeredWorldEntries;
        private List<WorldEntry> _registeredObjects;

        private DynamicsWorld _world;

        void Awake()
        {
            Debug.Assert(_physicsWorld.worldType >= BPhysicsWorld.WorldType.RigidBodyDynamics,
                "World type must not be collision only");

            _worldEntryPool = new ObjectPool<WorldEntry>(_maxObjects);
            for (int i = 0; i < _maxObjects; i++) {
                var entryId = i;
                _worldEntryPool.Return(new WorldEntry(entryId, RemoveObject));
            }
            _registeredWorldEntries = new ArrayDictionary<int, WorldEntry>(_maxObjects);
            _registeredObjects = new List<WorldEntry>(_maxObjects);

            _physicsWorld._InitializePhysicsWorld();

            // http://bulletphysics.org/mediawiki-1.5.8/index.php/Simulation_Tick_Callbacks
            _world = (DynamicsWorld)_physicsWorld.world;
            _world.SetInternalTickCallback(OnWorldPreTick, _world.WorldUserInfo, true);
        }

        void Update()
        {
            _world.StepSimulation(Time.deltaTime, _maxSubSteps, _fixedTimeStep);
        }

        public void StepSimulation(float deltaTime)
        {
            _world.StepSimulation(deltaTime, _maxSubSteps, _fixedTimeStep);
        }

        private void OnWorldPreTick(DynamicsWorld world, float timeStep)
        {
            for (int i = 0; i < _registeredObjects.Count; i++)
            {
                var worldEntry = _registeredObjects[i];
                for (int j = 0; j < worldEntry.PhysicsObject.PhysicsComponents.Count; j++)
                {
                    var physicsBehaviour = worldEntry.PhysicsObject.PhysicsComponents[j];

                    Assert.IsNotNull(physicsBehaviour, "PhysicsBehaviours may not be removed while registered to a physics world");

                    if (physicsBehaviour.enabled)
                    {
                        (physicsBehaviour as IPhysicsComponent).PhysicsUpdate(timeStep);
                    }
                }
            }
        }

        // TODO If we really want to make this fast we need to decouple the creation of world entries
        //      from the registration/deregistration. That way we don't do unnecessary GetComponent() calls

        private static readonly List<IPhysicsComponent> PhysicsComponentCache = new List<IPhysicsComponent>();
        public IWorldEntry AddObject(PhysicsObject po)
        {
            for (int i = 0; i < po.CollisionObjects.Count; i++)
            {
                var collisionObject = po.CollisionObjects[i];
                collisionObject.AddObjectToBulletWorld(_physicsWorld);
            }

            for (int i = 0; i < po.Constraints.Count; i++)
            {
                var constraint = po.Constraints[i];
                constraint.AddToBulletWorld(_physicsWorld);
            }

            var entry = _worldEntryPool.Take();

            entry.PhysicsObject = po;

            _registeredWorldEntries[entry.Id] = entry;
            _registeredObjects.Add(entry);

            return entry;
        }

        private static readonly IList<WorldEntry> _groupedEntries = new List<WorldEntry>(128);
        public void AddObjects(IList<PhysicsObject> objects, IList<IWorldEntry> worldEntries) {
            // Two loops, add collision objects, add constraints

            _groupedEntries.Clear();

            for (int i = 0; i < objects.Count; i++)
            {
                var po = objects[i];

                var entry = _worldEntryPool.Take();
                entry.PhysicsObject = po;

                var collisionObjects = po.CollisionObjects;
                for (int j = 0; j < collisionObjects.Count; j++) {
                    var collisionObject = collisionObjects[j];
                    collisionObject.AddObjectToBulletWorld(_physicsWorld);
                }

                _groupedEntries.Add(entry);
            }

            for (int i = 0; i < objects.Count; i++)
            {
                var entry = _groupedEntries[i];

                var constraints = entry.PhysicsObject.Constraints;
                for (int j = 0; j < constraints.Count; j++)
                {
                    var constraint = constraints[j];
                    constraint.AddToBulletWorld(_physicsWorld);
                }
            }

            _registeredObjects.AddRange(_groupedEntries);

            for (int i = 0; i < _groupedEntries.Count; i++)
            {
                var worldEntry = _groupedEntries[i];
                _registeredWorldEntries[worldEntry.Id] = worldEntry;
                worldEntries.Add(worldEntry);
            }
        }
        
        public void RemoveObjects(IList<IWorldEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Dispose();
            }
            entries.Clear();
        }

        private void RemoveObject(WorldEntry worldEntry)
        {
            var isRemoved = _registeredWorldEntries.Remove(worldEntry.Id);

            Assert.IsTrue(isRemoved, "World entry isn't registered anymore");

            _worldEntryPool.Return(worldEntry);
            _registeredObjects.Remove(worldEntry);
        }

        public interface IWorldEntry : IDisposable
        {
            PhysicsObject PhysicsObject { get; }
        }
        
        [Serializable]
        private class WorldEntry : IWorldEntry
        {
            [SerializeField] public GameObject _root;

            // Real type: MonoBehaviour with IPhysicsComponent
            public readonly int Id;
            private PhysicsObject _physicsObject;

            private readonly Action<WorldEntry> _remove;

            public WorldEntry(int id, Action<WorldEntry> remove)
            {
                Id = id;
                _remove = remove;
                _physicsObject = null;
            }

            public void Dispose()
            {
                _remove(this);
                _root = null;
                _physicsObject = null;
            }

            public PhysicsObject PhysicsObject
            {
                get { return _physicsObject; }
                set
                {
                    _physicsObject = value;
                    _root = _physicsObject.gameObject;
                }
            }
        }

    }
}