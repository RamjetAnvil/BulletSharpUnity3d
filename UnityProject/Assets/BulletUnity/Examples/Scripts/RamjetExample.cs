using System.Collections;
using System.Collections.Generic;
using BulletUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RamjetExample : MonoBehaviour
{
    [SerializeField] private RamjetPhysicsWorld _physicsWorld;
    [SerializeField] private string _physicsSceneName;

    private List<GameObject> _physicsObjects;

    void Awake() {
        _physicsObjects = new List<GameObject>(128);
    }

    private IEnumerator Start() {
        yield return SceneManager.LoadSceneAsync(_physicsSceneName, LoadSceneMode.Additive);
        var scene = SceneManager.GetSceneByName(_physicsSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Couldn't load scene: " + _physicsSceneName);
            yield break;
        }

        scene.GetRootGameObjects(_physicsObjects);
        _physicsWorld.AddObjects(_physicsObjects);
    }
}
