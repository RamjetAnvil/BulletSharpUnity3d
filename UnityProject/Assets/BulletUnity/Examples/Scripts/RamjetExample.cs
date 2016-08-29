using System.Collections.Generic;
using BulletUnity;
using UnityEngine;

public class RamjetExample : MonoBehaviour
{
    [SerializeField] private List<GameObject> _physicsObjects;
    [SerializeField] private RamjetPhysicsWorld _physicsWorld;

    void Awake()
    {
        _physicsWorld.AddObjects(_physicsObjects);

        // Todo: instead of manually managed list, can we do a quick scan scene and add all?

        // call resolve constraints after adding all objects.

        // Think about dynamic removes and additions. That's a very local and ad hoc thing, not global.

        // WorldEntry.Resolve

        // GroupAdd GroupRemove

        // Add/Remove Single, Add/Remove Group

        // if body && !body.isInWorld Add
        // if constraint && !constraint.otherBody.isInWorld Add

        // It's either that, or we first aggregate, sort, and add. But that's mostly optimization.
        // I really dislike branching within loops within loops, though. I guess that's the
        // object oriented design rearing its head again.


        // Split up add objects phase into two phases: collision objects and constraints
        // Constraint groups
    }
}
