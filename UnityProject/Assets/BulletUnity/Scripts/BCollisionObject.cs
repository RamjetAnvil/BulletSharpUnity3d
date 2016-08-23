using BulletUnity.Debugging;
using UnityEngine;
using System;
using System.Collections;
using BulletSharp;

namespace BulletUnity
{
    public class BCollisionObject : MonoBehaviour, IDisposable
    {

        public interface BICollisionCallbackEventHandler
        {
            void OnVisitPersistentManifold(PersistentManifold pm);
            void OnFinishedVisitingManifolds();
        }

        private IWorldRegistrar _worldRegistrar;

        protected CollisionObject m_collisionObject;
        protected BCollisionShape m_collisionShape;
        private BPhysicsWorld _currentWorld = null;
        private bool _isInWorld = false;
        [SerializeField]
        protected BulletSharp.CollisionFlags m_collisionFlags = BulletSharp.CollisionFlags.None;
        [SerializeField]
        protected BulletSharp.CollisionFilterGroups m_groupsIBelongTo = BulletSharp.CollisionFilterGroups.DefaultFilter; // A bitmask
        [SerializeField]
        protected BulletSharp.CollisionFilterGroups m_collisionMask = BulletSharp.CollisionFilterGroups.AllFilter; // A colliding object must match this mask in order to collide with me.

        protected virtual IWorldRegistrar WorldRegistrar {
            get 
            {
                // TODO Remove lazy init
                if (_worldRegistrar == null) 
                {
                    _worldRegistrar = new CollisionObjectRegistrar(this);
                }
                return _worldRegistrar;
            }
        }

        public bool IsInWorld 
        {
            get 
            {
                return _isInWorld; 
            }
        }

        public BPhysicsWorld CurrentWorld {
            get { return _currentWorld; }
        }

        public BulletSharp.CollisionFlags collisionFlags
        {
            get { return m_collisionFlags; }
            set {
                m_collisionFlags = value;
                if (m_collisionObject != null && value != m_collisionFlags)
                {
                     m_collisionObject.CollisionFlags = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups groupsIBelongTo
        {
            get { return m_groupsIBelongTo; }
            set
            {
                if (m_collisionObject != null && value != m_groupsIBelongTo)
                {
                    Debug.LogError("Cannot change the collision group once a collision object has been created");
                } else 
                {
                    m_groupsIBelongTo = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups collisionMask
        {
            get { return m_collisionMask; }
            set
            {
                if (m_collisionObject != null && value != m_collisionMask)
                {
                    Debug.LogError("Cannot change the collision mask once a collision object has been created");
                } else
                {
                    m_collisionMask = value;
                }
            }
        }

        BICollisionCallbackEventHandler m_onCollisionCallback;
        public virtual BICollisionCallbackEventHandler collisionCallbackEventHandler
        {
            get { return m_onCollisionCallback; }
        }

        public virtual void AddOnCollisionCallbackEventHandler(BICollisionCallbackEventHandler myCallback)
        {
            if (_currentWorld == null) 
            {
                throw new Exception(String.Format("BCollisionObject {0} is not in any world. Please add it to a physics world first.", name));   
            } 
            if (m_onCollisionCallback != null)
            {
                throw new Exception(String.Format("BCollisionObject {0} already has a collision callback. You must remove it before adding another. ", name));
            }
            m_onCollisionCallback = myCallback;
            _currentWorld.RegisterCollisionCallbackListener(m_onCollisionCallback);
        }

        public virtual void RemoveOnCollisionCallbackEventHandler()
        {
            if (_currentWorld != null && m_onCollisionCallback != null)
            {
                _currentWorld.DeregisterCollisionCallbackListener(m_onCollisionCallback);
            }
            m_onCollisionCallback = null;
        }

        //called by Physics World just before rigid body is added to world.
        //the current rigid body properties are used to rebuild the rigid body.
        protected virtual CollisionObject _BuildCollisionObject(BPhysicsWorld world)
        {
            RemoveObjectFromBulletWorld();

            if (transform.localScale != UnityEngine.Vector3.one)
            {
                Debug.LogError("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
            }

            m_collisionShape = GetComponent<BCollisionShape>();
            if (m_collisionShape == null)
            {
                Debug.LogError("There was no collision shape component attached to this BRigidBody. " + name);
                return null;
            }

            CollisionShape cs = m_collisionShape.GetCollisionShape();
            //rigidbody is dynamic if and only if mass is non zero, otherwise static


            if (m_collisionObject == null)
            {
                m_collisionObject = new CollisionObject();
                m_collisionObject.CollisionShape = cs;
                m_collisionObject.UserObject = this;

                BulletSharp.Math.Matrix worldTrans;
                BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
                BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = transform.position.ToBullet();
                m_collisionObject.WorldTransform = worldTrans;
                m_collisionObject.CollisionFlags = m_collisionFlags;
            }
            else {
                m_collisionObject.CollisionShape = cs;
                BulletSharp.Math.Matrix worldTrans;
                BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
                BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = transform.position.ToBullet();
                m_collisionObject.WorldTransform = worldTrans;
                m_collisionObject.CollisionFlags = m_collisionFlags;
            }
            return m_collisionObject;
        }

        public virtual CollisionObject GetCollisionObject()
        {
            Debug.Assert(IsInWorld, "Cannot retrieve a collision object for an object that is not in a world");
            return m_collisionObject;
        }

        //Don't try to call functions on other objects such as the Physics world since they may not exit.
        protected virtual void Awake()
        {
            m_collisionShape = GetComponent<BCollisionShape>();
            if (m_collisionShape == null)
            {
                Debug.LogError("A BCollisionObject component must be on an object with a BCollisionShape component.");
            }
        }

        public void AddObjectToBulletWorld(BPhysicsWorld world) 
        {
            _currentWorld = world;
            if (enabled) 
            {
                WorldRegistrar.AddTo(world);
                _isInWorld = true;
            }
        }

        public void RemoveObjectFromBulletWorld() 
        {
            if (_currentWorld != null) 
            {
                WorldRegistrar.RemoveFrom(_currentWorld);
            }
            _isInWorld = false;
        }

        //OnEnable and OnDisable are called when a game object is Activated and Deactivated. 
        //Unfortunately the first call comes before Awake and Start. We suppress this call so that the component
        //has a chance to initialize itself. Objects that depend on other objects such as constraints should make
        //sure those objects have been added to the world first.
        //don't try to call functions on world before Start is called. It may not exist.
        protected virtual void OnEnable()
        {
            if (_currentWorld != null && !_isInWorld)
            {
                AddObjectToBulletWorld(_currentWorld);
            }
        }

        // when scene is closed objects, including the physics world, are destroyed in random order. 
        // There is no way to distinquish between scene close destruction and normal gameplay destruction.
        // Objects cannot depend on world existing when they Dispose of themselves. World may have been destroyed first.
        protected virtual void OnDisable() 
        {
            RemoveObjectFromBulletWorld();
        }

        protected virtual void OnDestroy()
        {
            RemoveObjectFromBulletWorld();
        }

        public void Dispose() 
        {
            RemoveObjectFromBulletWorld();
            WorldRegistrar.Dispose();
            GC.SuppressFinalize(this);
        }
        
        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            if (_isInWorld)
            {
                BulletSharp.Math.Matrix newTrans = m_collisionObject.WorldTransform;
                BulletSharp.Math.Quaternion q = rotation.ToBullet();
                BulletSharp.Math.Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = position.ToBullet();
                m_collisionObject.WorldTransform = newTrans;
            }
        }

        public virtual void SetPosition(Vector3 position)
        {
            SetPositionAndRotation(position, transform.rotation);
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            SetPositionAndRotation(transform.position, rotation);
        }

        private class CollisionObjectRegistrar : IWorldRegistrar 
        {
            private readonly BCollisionObject _object;

            public CollisionObjectRegistrar(BCollisionObject o) 
            {
                _object = o;
            }

            public bool AddTo(BPhysicsWorld unityWorld) 
            {
                if (!unityWorld.isDisposed)
                {
                    if (unityWorld.debugType >= BDebug.DebugType.Debug) Debug.LogFormat("Adding collision object {0} to world", _object);

                    var world = unityWorld.world;
                    var collisionObject = _object._BuildCollisionObject(unityWorld);
                    if (collisionObject != null)
                    {
                        world.AddCollisionObject(collisionObject, _object.groupsIBelongTo, _object.collisionMask);
                        if (_object is BGhostObject)
                        {
                            unityWorld.InitializeGhostPairCallback();
                        }
                        if (_object is BCharacterController && world is DynamicsWorld)
                        {
                            unityWorld.AddAction(((BCharacterController)_object).GetKinematicCharacterController());
                        }
                    }
                    return true;
                }
                return false;
            }

            public void RemoveFrom(BPhysicsWorld unityWorld) 
            {
                if (!unityWorld.isDisposed) 
                {
                    var bulletCollisionObject = _object.m_collisionObject;
                    if (unityWorld.debugType >= BDebug.DebugType.Debug) Debug.LogFormat("Removing collisionObject {0} from world", bulletCollisionObject.UserObject);
                    unityWorld.world.RemoveCollisionObject(bulletCollisionObject);
                    //TODO handle removing kinematic character controller action
                }
            }

            public void Dispose() {
                _object.m_collisionObject.Dispose();
            }
        }
    }
}
