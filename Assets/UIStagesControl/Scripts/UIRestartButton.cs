using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRestartButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] [SerializeField] private UnityEvent onClick;
    private Image[] _images;
    private TextMeshProUGUI _text;

    private bool _isAlwaysShow;
    private float[] _originColorAlphas;
    private Coroutine _coroutineShowHide;

    public UnityAction AddActionOnClick
    {
        set => onClick.AddListener(value);
    }

    public bool IsAlwaysShow
    {
        set
        {
            _isAlwaysShow = value;
            if(!gameObject.activeInHierarchy) return;
            if(value)
                OnPointerEnter(null);
            else
                OnPointerExit(null);
        }
        get => _isAlwaysShow;
    }

    private void Awake()
    {
        _images = GetComponentsInChildren<Image>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _originColorAlphas = new float[_images.Length];
        onClick ??= new UnityEvent();

        for (var i = 0; i < _originColorAlphas.Length; i++)
        {
            _originColorAlphas[i] = _images[i].color.a;
        }

        SetAllAlpha(0f);
    }

    private void OnEnable()
    {
        SetAllAlpha(_isAlwaysShow ? 1f : 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _isAlwaysShow = false;
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_coroutineShowHide != null)
            StopCoroutine(_coroutineShowHide);
        _coroutineShowHide = StartCoroutine(ShowHideButton());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(_isAlwaysShow) return;
        
        if (_coroutineShowHide != null)
            StopCoroutine(_coroutineShowHide);
        _coroutineShowHide = StartCoroutine(ShowHideButton(false));
    }

    private IEnumerator ShowHideButton(bool isShow = true)
    {
        var sign = isShow ? 1 : -1;
        var step = 0.1f * sign;
        var condition = isShow ? 1 : 0;

        while (Math.Abs(_text.color.a - condition) > 0.01f)
        {
            var minAlpha = isShow ? _text.color.a : 0;
            var maxAlpha = isShow ? 1 : _text.color.a;
            var newAlpha = Math.Clamp(_text.color.a + step, minAlpha, maxAlpha);
            SetAlphaText(_text, newAlpha);
            for (var i = 0; i < _images.Length; i++)
            {
                minAlpha = isShow ? _images[i].color.a : 0;
                maxAlpha = isShow ? _originColorAlphas[i] : _images[i].color.a;
                newAlpha = Math.Clamp(_images[i].color.a + step, minAlpha, maxAlpha);
                SetAlphaImages(_images[i], newAlpha);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void SetAllAlpha(float value)
    {
        foreach (var image in _images)
        {
            SetAlphaImages(image, value);
        }

        SetAlphaText(_text, value);
    }

    private void SetAlphaImages(Image image, float value)
    {
        var color = image.color;
        color.a = value;
        image.color = color;
    }

    private void SetAlphaText(TextMeshProUGUI text, float value)
    {
        var color = text.color;
        color.a = value;
        text.color = color;
    }
}