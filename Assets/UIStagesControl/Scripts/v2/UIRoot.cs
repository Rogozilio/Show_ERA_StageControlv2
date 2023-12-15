using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIRoot
{
    private VisualElement _root;

    public UIRoot(UIDocument uiDocument)
    {
        _root = uiDocument.rootVisualElement;
    }

    public string NameLab
    {
        set => _root.Q<Label>("NameLab").text = value;
        get => _root.Q<Label>("NameLab").text;
    }

    public string NumberStage
    {
        set => _root.Q<Label>("NumberStage").text = value;
        get => _root.Q<Label>("NumberStage").text;
    }

    public string NameStage
    {
        set => _root.Q<Label>("NameStage").text = value;
        get => _root.Q<Label>("NameStage").text;
    }

    public string NumberStep
    {
        set => _root.Q<Label>("NumberStep").text = value;
        get => _root.Q<Label>("NumberStep").text;
    }

    public string DescStep
    {
        set => _root.Q<Label>("DescStep").text = value;
        get => _root.Q<Label>("DescStep").text;
    }

    public bool IsDisplayStage
    {
        set => _root.Q<VisualElement>("StageInfo").style.display
            = value ? DisplayStyle.Flex : DisplayStyle.None;
        get => _root.Q<VisualElement>("StageInfo").style.display.value == DisplayStyle.Flex;
    }

    public bool IsDisplayStep
    {
        set => _root.Q<VisualElement>("HelpMessage").style.display
            = value ? DisplayStyle.Flex : DisplayStyle.None;
        get => _root.Q<VisualElement>("HelpMessage").style.display.value == DisplayStyle.Flex;
    }

    public Button BtnInfo => _root.Q<Button>("InfoButton");

    public StyleBackground BtnInfoBackground
    {
        set => BtnInfo.style.backgroundImage = value;
    }

    public bool IsBtnInfoArrowUp
    {
        set => _root.Q<VisualElement>("ImageArrow").transform.rotation
            = Quaternion.Euler(_root.Q<VisualElement>("ImageArrow").transform.rotation.x
                , _root.Q<VisualElement>("ImageArrow").transform.rotation.y, value ? 180 : 0);
    }

    public byte StageCount => (byte)(_root.Q<VisualElement>("Stages").childCount - 1);

    public byte SubStageCount(int indexStage)
    {
        return (byte)(_root.Q<VisualElement>("Stage" + indexStage).childCount);
    }

    public void AddStage()
    {
        var newStage = new Button();
        newStage.AddToClassList("stage");
        newStage.name = "Stage" + StageCount;

        var newStageLabel = new Label();
        newStage.AddToClassList("stageText");
        newStageLabel.text = StageCount.ToString();
        newStage.Insert(0, newStageLabel);
        Debug.Log(newStage.name);
        
        _root.Q<VisualElement>("Stages").Insert(StageCount, newStage);
    }

    public void AddSubStage(int indexStage)
    {
        var newStage = new Button();
        newStage.AddToClassList("subStage");
        newStage.name = "subStage" + indexStage + "_" + ((SubStageCount(indexStage) + 1));
        
        var newStageLabel = new Label();
        newStage.AddToClassList("subStageText");
        newStageLabel.text = (SubStageCount(indexStage)).ToString();
        newStage.Insert(0, newStageLabel);
        
        _root.Q<Button>("Stage" + indexStage).Insert(SubStageCount(indexStage), newStage);
    }

    public void ActivateStage(int index)
    {
        var indexStage = 0;
        foreach (var stage in _root.Q<VisualElement>("Stages").Children())
        {
            stage.RemoveFromClassList("stageActive");
            stage.Children().FirstOrDefault()?.RemoveFromClassList("textActive");
            if (index == indexStage++)
            {
                stage.AddToClassList("stageActive");
                stage.Children().FirstOrDefault()?.AddToClassList("textActive");
            }
        }
    }
}