using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{
    public Color EnabledColor;
    public Color DisabledColor;

    private void Start() {
       GetComponent<Toggle>().onValueChanged.Invoke(false);
    }

    public void SetColor(bool enabled) {
        if (enabled) {
            GetComponent<Image>().color = EnabledColor;
        } else {
            GetComponent<Image>().color = DisabledColor;
        }
    }
}
