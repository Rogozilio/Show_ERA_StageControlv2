using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageButton : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int index;
    [HideInInspector] public bool isUseRestartButton = true;
    [HideInInspector] public bool isShowStage = true;
    [HideInInspector] public bool isShowSteps = true;
    [HideInInspector] [SerializeField] private bool isFinished;
    [HideInInspector] [SerializeField] protected bool isDisabled;
    [HideInInspector][SerializeField] private UnityEvent onClick;

    protected bool _isActiveStage;
    protected Image _image;
    protected TextMeshProUGUI _textMeshPro;
    
    public UnityAction AddActionOnClick
    {
        set
        {
            onClick ??= new UnityEvent();
            onClick.AddListener(value);
        }
    }

    public bool IsActive => _isActiveStage;

    public bool IsFinished
    {
        set => isFinished = value;
        get => isFinished;
    }
    public virtual bool IsDisabled
    {
        set
        {
            isDisabled = value;
            _textMeshPro ??= GetComponentInChildren<TextMeshProUGUI>();
            ChangeAlphaText();
        }
        get => isDisabled;
    }

    protected void Awake()
    {
        _image = transform.GetComponent<Image>();
        _textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        onClick ??= new UnityEvent();
        ChangeAlphaText();
    }

    private void ChangeAlphaText()
    {
        var color = _textMeshPro.color;
        color.a = isDisabled ? 0.5f : 1f;
        _textMeshPro.color = color;
    }
    

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
    
    public virtual void ActivateStage()
    {
        if(_isActiveStage) return;
        _isActiveStage = true;
        _textMeshPro.color = new Color32(40, 118, 32, 255);
    }

    public virtual void DeactivateStage()
    {
        if(!_isActiveStage) return;
        _isActiveStage = false;
        _textMeshPro.color = Color.white;
    }

    public void OnClickInvoke()
    {
        onClick?.Invoke();
    }
}
