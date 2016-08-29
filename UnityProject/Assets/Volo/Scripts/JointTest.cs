using UnityEngine;
using BulletUnity;

public class JointTest : MonoBehaviour, IPhysicsComponent {
    [SerializeField] private float _maxMotorForce = 100f;
    [SerializeField] private BConeTwistConstraint _joint;

    void Start () {
        _joint.motorEnabled = true;
        _joint.maxMotorImpulse = _maxMotorForce;
    }

    public void PhysicsUpdate(float deltaTime) {
        float inputTwist = Input.GetAxis("Horizontal");
        float inputSwing = Input.GetAxis("Vertical");
        inputTwist = inputTwist * 66f;
        inputSwing = inputSwing * 45f;

        Quaternion inputRotation = Quaternion.Euler(inputTwist, inputSwing, 0f);

        _joint.motorTarget = inputRotation;
    }
}
