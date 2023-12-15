using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStageButton : StageButton
{
    public KeyCode keyboardButton;
    public List<UISubStageButton> subStages;
    
    private byte _indexStage;

    public int IndexStage
    {
        set => _indexStage = (byte)value;
        get => _indexStage;
    }

    public override bool IsDisabled
    {
        set
        {
            base.IsDisabled = value;

            if(value || subStages.Count == 0) return;

            subStages[0].IsDisabled = false;
        }
    }

    private void Awake()
    {
        base.Awake();
        var isAllDisabled = true;
        foreach (var subStage in subStages)
        {
            subStage.Awake();
            subStage.StageParent = this;
            subStage.AddActionOnClick = () =>
            {
                DeactivateSubStages();
                subStage.ActivateStage();
            };
            if (!subStage.IsDisabled)
                isAllDisabled = false;
        }

        if (subStages.Count > 0)
            IsDisabled = isAllDisabled;
        DisableSubStages();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (subStages.Count <= 0) return;
        
        foreach (var subStage in subStages)
        {
            if(subStage.IsDisabled) continue;
                
            subStage.OnPointerClick(eventData);
        }

    }

    public override void ActivateStage()
    {
        base.ActivateStage();
        _image.color = Color.white;
        ActivateSubStages();
    }

    public override void DeactivateStage()
    {
        base.DeactivateStage();
        _image.color = new Color32(91, 163, 75, 255);
        DeactivateSubStages();
    }
    
    private void ActivateSubStages()
    {
        EnableSubStages();
    }

    private void EnableSubStages()
    {
        if(!_isActiveStage) return;
        foreach (var subStage in subStages)
        {
            subStage.gameObject.SetActive(true);
        }
    }

    private void DeactivateSubStages()
    {
        foreach (var subStage in subStages)
        {
            subStage.DeactivateStage();
        }
        DisableSubStages();
    }
    
    private void DisableSubStages()
    {
        if(_isActiveStage) return;
        foreach (var subStage in subStages)
        {
            subStage.gameObject.SetActive(false);
        }
    }
}