using UnityEngine;

public class UISubStageButton : StageButton
{
    public Sprite activate;
    public Sprite deactivate;
    
    private byte _indexSubStage;
    private UIStageButton _stageParent;

    public UIStageButton StageParent
    {
        set => _stageParent = value;
        get => _stageParent;
    }
    
    public override bool IsDisabled
    {
        set
        {
            base.IsDisabled = value;
            if(!_stageParent || _stageParent.IsDisabled == value) return;
            _stageParent.IsDisabled = value;
        }
    }

    public void Awake()
    {
        base.Awake();
    }

    public int IndexSubStage
    {
        set => _indexSubStage = (byte)value;
        get => _indexSubStage;
    }
    public override void ActivateStage()
    {
        base.ActivateStage();
        _image.sprite = activate;
    }

    public override void DeactivateStage()
    {
        base.DeactivateStage();
        _image.sprite = deactivate;
    }
}
