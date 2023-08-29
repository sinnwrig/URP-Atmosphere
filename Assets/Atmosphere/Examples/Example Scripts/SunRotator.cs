using UnityEngine;

public class SunRotator : MonoBehaviour
{
    public enum RotationAxis
    {
        X = 0, Y = 1, Z = 2
    }

    
    public RotationAxis axis = RotationAxis.X;
    public bool isLocal = true;
    public float rotationSpeed = 10f;


    Quaternion AddRotation(Quaternion rotation) 
    {
        float angle = rotationSpeed * Time.deltaTime;

        Vector3 axis = Vector3.right;

        if (this.axis == RotationAxis.Y) {
            axis = Vector3.up;
        } else if (this.axis == RotationAxis.Z) {
            axis = Vector3.forward;
        }

        return rotation * Quaternion.AngleAxis(angle, axis);
    }


    void Update() 
    {
        if (isLocal) {
            transform.localRotation = AddRotation(transform.localRotation);
        } else {
            transform.rotation = AddRotation(transform.rotation);
        }
    }
}
