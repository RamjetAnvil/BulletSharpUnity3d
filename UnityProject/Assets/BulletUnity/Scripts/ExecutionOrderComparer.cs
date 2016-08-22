using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BulletUnity
{
    public class ExecutionOrderComparer : IComparer<MonoBehaviour>
    {
        public static readonly IComparer<MonoBehaviour> Default = new ExecutionOrderComparer();

        private ExecutionOrderComparer()
        {
        }

        public int Compare(MonoBehaviour x, MonoBehaviour y)
        {
            return GetExecutionOrder(x) - GetExecutionOrder(y);
        }

        private int GetExecutionOrder(MonoBehaviour m)
        {
            return MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(m));
        }
    }
}