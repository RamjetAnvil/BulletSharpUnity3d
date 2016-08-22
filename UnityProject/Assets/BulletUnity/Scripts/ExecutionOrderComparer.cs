﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BulletUnity
{
    [ExecuteInEditMode]
    public class ExecutionOrderComparer : MonoBehaviour, IComparer<MonoBehaviour>
    {
        [SerializeField] private List<Entry> _executionOrderDb;

        #if UNITY_EDITOR
        void Awake()
        {
            var monoScripts = UnityEditor.MonoImporter.GetAllRuntimeMonoScripts()
                .Where(script => script.GetClass() != null);
            _executionOrderDb.Clear();
            foreach (var monoScript in monoScripts)
            {
                var executionOrder = UnityEditor.MonoImporter.GetExecutionOrder(monoScript);
                if (executionOrder != 0)
                {
                    _executionOrderDb.Add(new Entry(monoScript.GetClass(), executionOrder));
                }
            }
        }
        #endif

        public int Compare(MonoBehaviour x, MonoBehaviour y)
        {
            return GetExecutionOrder(x) - GetExecutionOrder(y);
        }

        private int GetExecutionOrder(MonoBehaviour m)
        {
            var mType = m.GetType().AssemblyQualifiedName;
            for (int i = 0; i < _executionOrderDb.Count; i++)
            {
                var entry = _executionOrderDb[i];
                if (entry.Type == mType)
                {
                    return entry.ExecutionOrder;
                }
            }
            return 0;
        }

        [Serializable]
        private struct Entry
        {
            [SerializeField] public string Type;
            [SerializeField] public int ExecutionOrder;

            public Entry(Type type, int executionOrder)
            {
                Type = type.AssemblyQualifiedName;
                ExecutionOrder = executionOrder;
            }
        }
    }
}