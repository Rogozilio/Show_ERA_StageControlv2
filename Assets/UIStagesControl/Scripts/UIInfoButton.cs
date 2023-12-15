using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInfoButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite open;
    public Sprite close;
    public Sprite closeZero;
    [Space] public bool isOpen;
    public KeyCode keyboardButton;

    private int _stageIndex;
    private bool _isSelectSubStage;
    private Image _image;
    private UnityEvent _onClick;
    private RectTransform _rectTransform;
    
    private float sizeX => _isSelectSubStage ? 36f : 37f;
    public bool isSelectSubStage
    {
        set => _isSelectSubStage = value;
    }
    public int SetStageIndex
    {
        set => _stageIndex = value;
    }

    public UnityAction AddActionOnClick
    {
        set => _onClick.AddListener(value);
    }

    public void Awake()
    {
        _image = GetComponent<Image>();
        _onClick = new UnityEvent();
        _onClick.AddListener(SwitchStage);
        _rectTransform = (RectTransform)transform;
        SwitchStage();
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyboardButton))
        {
            OnPointerClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Color32 color = _image.color;
        color.a = 255;
        _image.color = color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color32 color = _image.color;
        color.a = 160;
        _image.color = color;
    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        isOpen = !isOpen;
        _onClick?.Invoke();
    }
    
    public void OnPointerClickAction()
    {
        OnPointerClick();
    }

    public void SwitchStage()
    {
        if (isOpen)
        {
            _image.sprite = open;
            _rectTransform.sizeDelta = new Vector2(36, _rectTransform.sizeDelta.y);
            _rectTransform.anchoredPosition = new Vector2(6, _rectTransform.anchoredPosition.y);
        }
        else
        {
            _image.sprite = _stageIndex == 0 ? closeZero : close;
            _rectTransform.sizeDelta = new Vector2(sizeX, _rectTransform.sizeDelta.y);
            _rectTransform.anchoredPosition = new Vector2( 36 * _stageIndex + (_isSelectSubStage ? 1 : 0),
                _rectTransform.anchoredPosition.y);
        }
    }
}