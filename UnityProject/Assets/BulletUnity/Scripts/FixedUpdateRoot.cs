using System.Collections.Generic;
using UnityEngine;

namespace BulletUnity
{
    public class FixedUpdateRoot : MonoBehaviour
    {
        [SerializeField] private ExecutionOrderComparer _executionOrderComparer;

        private List<MonoBehaviour> _updateBehaviours;

        void Awake()
        {
            _updateBehaviours = new List<MonoBehaviour>();
            foreach (var physicsComponent in GetComponentsInChildren<IPhysicsComponent>())
            {
                _updateBehaviours.Add(physicsComponent as MonoBehaviour);
            }
            _updateBehaviours.Sort(_executionOrderComparer);
            foreach (var behaviour in _updateBehaviours)
            {
                Debug.Log(behaviour.GetType());
            }
            _updateBehaviours.Reverse();
        }

        void FixedUpdate()
        {
            PhysicsUpdate(Time.fixedDeltaTime);
        }

        public void PhysicsUpdate(float deltaTime)
        {
            for (int i = _updateBehaviours.Count - 1; i >= 0; i--)
            {
                var physicsBehaviour = _updateBehaviours[i];
                if (physicsBehaviour == null)
                {
                    _updateBehaviours.RemoveAt(i);
                }
                else if (physicsBehaviour.enabled)
                {
                    (physicsBehaviour as IPhysicsComponent).PhysicsUpdate(deltaTime);
                }
            }
        }
    }
}