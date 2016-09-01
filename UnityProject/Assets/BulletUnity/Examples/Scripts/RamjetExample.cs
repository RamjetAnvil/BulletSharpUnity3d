using System.Collections;
using System.Collections.Generic;
using BulletUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RamjetExample : MonoBehaviour
{
    [SerializeField] private RamjetPhysicsWorld _physicsWorld;
    [SerializeField] private string _physicsSceneName;

    private List<PhysicsObject> _physicsObjects;

    void Awake() {
        _physicsObjects = new List<PhysicsObject>(128);
    }

    private IEnumerator Start() {
        yield return SceneManager.LoadSceneAsync(_physicsSceneName, LoadSceneMode.Additive);
        var scene = SceneManager.GetSceneByName(_physicsSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Couldn't load scene: " + _physicsSceneName);
            yield break;
        }

        var rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++) {
            var rootObject = rootObjects[i];
            rootObject.GetComponentsInChildren(_physicsObjects);
        }

        var worldEntries = new List<RamjetPhysicsWorld.IWorldEntry>(_physicsObjects.Count);
        _physicsWorld.AddObjects(_physicsObjects, worldEntries);
    }
}
