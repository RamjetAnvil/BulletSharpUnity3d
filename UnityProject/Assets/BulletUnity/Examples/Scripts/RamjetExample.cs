using System.Collections.Generic;
using BulletUnity;
using UnityEngine;

public class RamjetExample : MonoBehaviour
{
    [SerializeField] private List<GameObject> _physicsObjects;
    [SerializeField] private RamjetPhysicsWorld _physicsWorld;

    void Awake()
    {
        for (int i = 0; i < _physicsObjects.Count; i++)
        {
            var physicsObject = _physicsObjects[i];
            _physicsWorld.AddObject(physicsObject);
        }
    }
}
