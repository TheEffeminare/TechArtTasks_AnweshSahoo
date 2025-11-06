using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleBackgroundSwap : MonoBehaviour
{
    public Image background;         // your blue/green pill Image
    public Sprite onSprite;          // green pill
    public Sprite offSprite;         // blue pill

    Toggle t;

    void Awake() {
        t = GetComponent<Toggle>();
        if (!background) background = GetComponentInChildren<Image>(); // fallback
        Apply(t.isOn);
        t.onValueChanged.AddListener(Apply);
    }

    void OnDestroy() => t.onValueChanged.RemoveListener(Apply);

    void Apply(bool isOn) {
        if (background) background.sprite = isOn ? onSprite : offSprite;
    }
}
