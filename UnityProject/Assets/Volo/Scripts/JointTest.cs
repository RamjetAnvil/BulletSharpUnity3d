using UnityEngine;
using BulletUnity;

public class JointTest : MonoBehaviour, IPhysicsComponent {
    [SerializeField] private float _maxMotorForce = 100f;
    [SerializeField] private BConeTwistConstraint _joint;

    void Start () {
        _joint.motorEnabled = true;
        _joint.maxMotorImpulse = _maxMotorForce;
        _joint.motorTarget = Quaternion.identity; // Todo: bug, this is not initialized to identity automatically, messing up physics
    }

    public void PhysicsUpdate(float deltaTime) {
        float inputTwist = Input.GetAxis("Horizontal");
        float inputSwing = Input.GetAxis("Vertical");
        inputTwist = inputTwist * 66f;
        inputSwing = inputSwing * 45f;


        Debug.Log(inputTwist);

        Quaternion inputRotation = Quaternion.Euler(inputTwist, inputSwing, 0f);

        _joint.motorTarget = inputRotation;
    }
}
