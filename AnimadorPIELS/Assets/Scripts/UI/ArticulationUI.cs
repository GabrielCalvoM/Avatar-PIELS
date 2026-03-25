using UnityEngine;
using UnityEngine.UI;

public class ArticulationUI : MonoBehaviour
{
    [SerializeField] GameObject button;
    [SerializeField] GameObject rotator;

    public void OnButtonToggled()
    {
        bool isOn = button.GetComponent<Toggle>().isOn;
        rotator.SetActive(isOn);

        Color newColor = button.GetComponent<Image>().color;
        newColor.a = isOn ? 0 : 1;
        button.GetComponent<Image>().color = newColor;
    }
}
