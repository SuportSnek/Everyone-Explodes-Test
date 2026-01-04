using UnityEngine;
using UnityEngine.UI;

public class UITimeSlider : MonoBehaviour 
{
    [SerializeField] private Slider slider;

    public void SetNormalized(float value)
    {
        slider.value = Mathf.Clamp01(value);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
