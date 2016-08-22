using BulletUnity.Debugging;
using UnityEngine;
using System.Collections;
using BulletSharp.SoftBody;
using System;
using BulletSharp;
using System.Collections.Generic;
//using BulletSharp.Math;

namespace BulletUnity
{

    public class BSoftBody : BCollisionObject 
    {
        private IWorldRegistrar _worldRegistrar;

        //common Soft body settings class used for all softbodies, parameters set based on type of soft body
        [SerializeField]
        private SBSettings _softBodySettings = new SBSettings();      //SoftBodyEditor will display this when needed
        public SBSettings SoftBodySettings
        {
            get { return _softBodySettings; }
            set { _softBodySettings = value; }
        }

        //protected SoftBody m_BSoftBody;
//
//        SoftRigidDynamicsWorld _world;
//        protected SoftRigidDynamicsWorld World
//        {
//            get { return _world = _world ?? (SoftRigidDynamicsWorld)BPhysicsWorld.Get().world; }
//        }

        protected override IWorldRegistrar WorldRegistrar {
            get {
                if (_worldRegistrar == null) {
                    _worldRegistrar = new SoftbodyRegistrar(this);
                }
                return _worldRegistrar;
            }
        }

        //for converting to/from unity mesh
        protected UnityEngine.Vector3[] verts = new UnityEngine.Vector3[0];
        protected UnityEngine.Vector3[] norms = new UnityEngine.Vector3[0];
        protected int[] tris = new int[1];

        protected override void Awake()
        {
            //disable warning
        }

        protected override CollisionObject _BuildCollisionObject(BPhysicsWorld world)
        {
            return null;
        }

        public void BuildSoftBody(BPhysicsWorld world)
        {
            _BuildCollisionObject(world);
        }

        public void DumpDataFromBullet()
        {
            if (IsInWorld)
            {
                SoftBody m_BSoftBody = (SoftBody)m_collisionObject;
                if (verts.Length != m_BSoftBody.Nodes.Count)
                {
                    verts = new Vector3[m_BSoftBody.Nodes.Count];
                }
                if (norms.Length != verts.Length)
                {
                    norms = new Vector3[m_BSoftBody.Nodes.Count];
                }
                for (int i = 0; i < m_BSoftBody.Nodes.Count; i++)
                {
                    verts[i] = m_BSoftBody.Nodes[i].Position.ToUnity();
                    norms[i] = m_BSoftBody.Nodes[i].Normal.ToUnity();
                }
            }
        }


        void Update()
        {
            DumpDataFromBullet();  //Get Bullet data
            UpdateMesh(); //Update mesh based on bullet data
            //Make coffee
        }

        /// <summary>
        /// Update Mesh (or line renderer) at runtime, call from Update 
        /// </summary>
        public virtual void UpdateMesh()
        {

        }

        public class SoftbodyRegistrar : IWorldRegistrar 
        {
            private readonly BSoftBody _softbody;

            public SoftbodyRegistrar(BSoftBody softbody) 
            {
                _softbody = softbody;
            }

            public bool AddTo(BPhysicsWorld unityWorld) 
            {
                var world = unityWorld.world;
                if (!(world is BulletSharp.SoftBody.SoftRigidDynamicsWorld))
                {
                    if (unityWorld.debugType >= BDebug.DebugType.Debug) 
                    {
                        Debug.LogFormat("The Physics World must be a BSoftBodyWorld for adding soft bodies");
                    }
                    return false;
                }
                if (!unityWorld.isDisposed)
                {
                    if (unityWorld.debugType >= BDebug.DebugType.Debug) 
                    {
                        Debug.LogFormat("Adding softbody {0} to world", _softbody);
                    }

                    var collisionObject = _softbody._BuildCollisionObject(unityWorld) as SoftBody;
                    if (collisionObject != null)
                    {
                        ((SoftRigidDynamicsWorld)world).AddSoftBody(collisionObject);
                    }
                    return true;
                }
                return false;
            }

            public void RemoveFrom(BPhysicsWorld unityWorld) 
            {
                if (unityWorld && _softbody.IsInWorld)
                {
                    if (!unityWorld.isDisposed && unityWorld.world is SoftRigidDynamicsWorld) 
                    {
                        var bulletSoftbody = (SoftBody) _softbody.m_collisionObject;
                        if (unityWorld.debugType >= BDebug.DebugType.Debug) 
                        {
                            Debug.LogFormat("Removing softbody {0} from world", bulletSoftbody.UserObject);
                        }
                        ((SoftRigidDynamicsWorld)unityWorld.world).RemoveSoftBody(bulletSoftbody);
                    }
                }
            }

            public void Dispose() 
            {
                SoftBody m_BSoftBody = _softbody.m_collisionObject as SoftBody;
                if (m_BSoftBody != null)
                {
                    m_BSoftBody.Dispose();
                }
                Debug.Log("Destroying SoftBody " + _softbody.name);
            }
        }
    }
}