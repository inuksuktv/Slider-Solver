using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SliderCount : MonoBehaviour
{
    TextMeshProUGUI _text;
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.normalizedValue = Random.Range(0, 0.25f);
    }

    void Start()
    {
        Transform text = transform.Find("CountText");
        if (text.TryGetComponent<TextMeshProUGUI>(out var theText)) {
            _text = theText;
        }
        UpdateText();
        _slider.onValueChanged.AddListener(delegate { UpdateText(); });
        _slider.onValueChanged.AddListener(delegate { TitleGrid.Instance.UpdateValues(); });
    }

    void UpdateText()
    {
        _text.text = _slider.value.ToString();
    }
}
