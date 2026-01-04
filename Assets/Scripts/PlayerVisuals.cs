using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Visual Roots")]
    [SerializeField] private GameObject firstPersonRoot;
    [SerializeField] private GameObject thirdPersonRoot;

    void Awake()
    {

    }

    public void SetFirstPersonVisible(bool visible)
    {
        firstPersonRoot.SetActive(visible);
    }

    public void SetThirdPersonVisible(bool visible)
    {
        thirdPersonRoot.SetActive(visible);
    }
}
