using UnityEngine;

public class SpinObject : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0f, 90f, 0f); // degrees per second
    public float currentMultiplier=1;

    void Update()
    {
        transform.Rotate(rotationSpeed * currentMultiplier * Time.deltaTime);
    }

    public void SetSpinMultiplier(float multiplier)
    {
        currentMultiplier = multiplier;
    }

}
