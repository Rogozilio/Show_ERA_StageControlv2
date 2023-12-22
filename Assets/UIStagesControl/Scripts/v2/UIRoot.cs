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
        set => _root.Q<Label>("NameLab").text = value.ToUpper();
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

    public bool IsDisplayStageInfo
    {
        set => _root.Q<VisualElement>("StageInfo").style.display
            = value ? DisplayStyle.Flex : DisplayStyle.None;
        get => _root.Q<VisualElement>("StageInfo").style.display.value == DisplayStyle.Flex;
    }

    public bool IsDisplayHelpMassage
    {
        set => _root.Q<VisualElement>("HelpMessage").style.display
            = value ? DisplayStyle.Flex : DisplayStyle.None;
        get => _root.Q<VisualElement>("HelpMessage").style.display.value == DisplayStyle.Flex;
    }

    public bool IsBtnInfoArrowUp
    {
        set => _root.Q<VisualElement>("ImageArrow").transform.rotation
            = Quaternion.Euler(_root.Q<VisualElement>("ImageArrow").transform.rotation.x
                , _root.Q<VisualElement>("ImageArrow").transform.rotation.y, value ? 180 : 0);
    }

    public byte StageCount => (byte)(_root.Q<VisualElement>("Stages").childCount - 1);

    private Button GetActiveStage => _root.Q<Button>(className: "stageActive");
    private Button GetActiveSubStage => _root.Q<Button>(className: "subStageActive");
    public byte GetIndexActiveStage => byte.Parse(GetActiveStage.name.Split("age")[1] ?? "0");
    public byte GetIndexActiveSubStage => byte.Parse(GetActiveSubStage?.name.Split("_")[1] ?? "0");

    private void DeactivateActiveStage()
    {
        var stage = GetActiveStage;
        stage.RemoveFromClassList("stageActive");
        stage.Children().FirstOrDefault()?.RemoveFromClassList("stageTextActive");

        IsDisplaySubStages(stage, false);
    }

    private void DeactivateActiveSubStage()
    {
        var subStage = GetActiveSubStage;
        subStage?.RemoveFromClassList("subStageActive");
        subStage?.Children().ToList()[0].RemoveFromClassList("subStageTextActive");
    }

    public byte SubStageCount(int indexStage)
    {
        return (byte)(_root.Q<VisualElement>("Stage" + indexStage).childCount);
    }

    public void IsDisplaySubStages(VisualElement element, bool value)
    {
        GetSubStages(element)?.ForEach(x => x.style.display
            = value ? DisplayStyle.Flex : DisplayStyle.None);
    }

    private List<Button> GetStages()
    {
        var stages = _root.Query<Button>(null, "stage").ToList();
        return stages.Count > 0 ? stages : null;
    }

    private List<Button> GetSubStages(VisualElement stage)
    {
        var subStages = stage.Query<Button>(null, "subStage").ToList();
        return subStages.Count > 0 ? subStages : null;
    }

    public bool IsOpenInfoButton => IsDisplayStageInfo;

    public void SetInfoButton(bool isOpen)
    {
        if (!isOpen) ClickInfoButton(isOpen);

        _root.Q<Button>("InfoButton").clicked += () =>
        {
            isOpen = !isOpen;
            ClickInfoButton(isOpen);
        };
    }

    private void ClickInfoButton(bool isOpen)
    {
        var infoButton = _root.Q<Button>("InfoButton");

        IsDisplayStageInfo = isOpen;
        IsDisplayHelpMassage = isOpen;
        IsBtnInfoArrowUp = isOpen;

        if (isOpen)
            infoButton.AddToClassList("InfoButtonNormal");
        else if (GetIndexActiveStage == 0)
            infoButton.RemoveFromClassList("InfoButtonNormal");
        else
            infoButton.AddToClassList("InfoButtonNormal");

        NewPositionInfoButton();
    }

    public void AddStage()
    {
        var newStage = new Button();
        newStage.AddToClassList("stage");
        newStage.name = "Stage" + StageCount;
        newStage.clicked += () =>
        {
            ActivateStage(newStage);
            ClickInfoButton(IsOpenInfoButton);
        };

        var newStageLabel = new Label();
        newStageLabel.AddToClassList("stageText");
        newStageLabel.text = StageCount.ToString();
        newStage.Insert(0, newStageLabel);
        Debug.Log(newStage.name);

        if (StageCount == 0)
        {
            newStage.AddToClassList("stage0");
            newStageLabel.text = "O";
        }

        _root.Q<VisualElement>("Stages").Insert(StageCount, newStage);
    }

    public void AddSubStage(int indexStage)
    {
        var newSubStage = new Button();
        var indexSubStage = SubStageCount(indexStage);
        newSubStage.AddToClassList("subStage");
        newSubStage.name = "subStage" + indexStage + "_" + indexSubStage;
        newSubStage.clicked += () =>
        {
            ActiveSubStage(newSubStage.parent, indexSubStage - 1);
            ClickInfoButton(IsOpenInfoButton);
        };

        var newStageLabel = new Label();
        newStageLabel.AddToClassList("subStageText");
        newStageLabel.text = indexSubStage.ToString();
        newSubStage.Insert(0, newStageLabel);

        _root.Q<Button>("Stage" + indexStage).Insert(indexSubStage, newSubStage);
    }

    public void ActivateStage(VisualElement stage)
    {
        DeactivateActiveStage();

        stage.AddToClassList("stageActive");
        stage.Children().FirstOrDefault()?.AddToClassList("stageTextActive");
        IsDisplaySubStages(stage, true);

        ActiveSubStage(stage);
    }

    public void ActiveSubStage(VisualElement stage, int index = 0)
    {
        var subStages = GetSubStages(stage);

        DeactivateActiveSubStage();

        if (subStages == null) return;

        subStages[index]?.AddToClassList("subStageActive");
        subStages[index]?.Children().ToList()[0].AddToClassList("subStageTextActive");
    }

    private void NewPositionInfoButton()
    {
        var infoButton = _root.Q<Button>("InfoButton");

        var indexStage = GetIndexActiveStage;
        var indexSubStage = GetIndexActiveSubStage;

        infoButton.style.left = IsOpenInfoButton
            ? 10
            : 61f * indexStage + 63f * indexSubStage;
    }

    public void ActivateStageByIndex(int index)
    {
        var indexStage = 0;
        foreach (var stage in _root.Q<VisualElement>("Stages").Children())
        {
            stage.RemoveFromClassList("stageActive");
            stage.Children().FirstOrDefault()?.RemoveFromClassList("stageTextActive");
            IsDisplaySubStages(stage, false);
            if (index == indexStage++)
            {
                stage.AddToClassList("stageActive");
                stage.Children().FirstOrDefault()?.AddToClassList("stageTextActive");
                IsDisplaySubStages(stage, true);

                ActiveSubStage(stage);
            }
        }
    }
}