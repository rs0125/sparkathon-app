using UnityEngine;
using TMPro;

public class KeyboardAdjust : MonoBehaviour
{
    public RectTransform inputBar;
    public TMP_InputField inputField;
    public float positiondiff = 400;

    private Vector2 originalPos;

    void Start()
    {
        originalPos = inputBar.anchoredPosition;

        inputField.onSelect.AddListener(OnInputFocused);
        inputField.onDeselect.AddListener(OnInputUnfocused);
    }

    void OnInputFocused(string _)
    {
        inputBar.anchoredPosition = originalPos + new Vector2(0, positiondiff); // move up 400px
    }

    void OnInputUnfocused(string _)
    {
        inputBar.anchoredPosition = originalPos; // reset to original
    }
}