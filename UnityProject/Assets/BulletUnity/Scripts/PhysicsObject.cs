using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BulletUnity
{
    public class PhysicsObject : MonoBehaviour
    {
        private List<MonoBehaviour> _physicsComponents;
        private BCollisionObject[] _collisionObjects;
        private BTypedConstraint[] _constraints;

        private static readonly List<IPhysicsComponent> PhysicsComponentCache = new List<IPhysicsComponent>();
        void Awake()
        {
            PhysicsComponentCache.Clear();
            gameObject.GetComponentsInChildren(includeInactive: true, results: PhysicsComponentCache);

            _physicsComponents = new List<MonoBehaviour>();
            for (int i = 0; i < PhysicsComponentCache.Count; i++)
            {
                _physicsComponents.Add(PhysicsComponentCache[i] as MonoBehaviour);
            }
            _physicsComponents.Sort(ExecutionOrderComparer.Default);

            _collisionObjects = gameObject.GetComponentsInChildren<BCollisionObject>();
            _constraints = gameObject.GetComponentsInChildren<BTypedConstraint>();
        }

        public IList<MonoBehaviour> PhysicsComponents
        {
            get { return _physicsComponents; }
        }

        public IList<BCollisionObject> CollisionObjects
        {
            get { return _collisionObjects; }
        }

        public IList<BTypedConstraint> Constraints
        {
            get { return _constraints; }
        }
    }
}
