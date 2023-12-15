using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIStagesControl : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public string first;
        public string second;

        public Data(string first, string second)
        {
            this.first = first;
            this.second = second;
        }
    }

    [Serializable]
    public struct DataStep
    {
        public int index;
        public List<Data> text;
        public Data GetText => index < text.Count ? text[index] : new Data("Шаг №", "Описание шага");
    }

    public GameObject nameLab;
    public GameObject stages;
    public GameObject stageInfo;
    public GameObject helpMessages;
    public UIInfoButton infoButton;
    public UIRestartButton restartButton;
    public UIInfoButton showHideInfoButton;

    [HideInInspector] public TextMeshProUGUI textNameLab;
    [HideInInspector] public TextMeshProUGUI textNumberStage;
    [HideInInspector] public TextMeshProUGUI textNameStage;
    [HideInInspector] public TextMeshProUGUI textNumberStep;
    [HideInInspector] public TextMeshProUGUI textDescStep;

    [HideInInspector] [SerializeField] private List<Data> _textStages;
    [HideInInspector] [SerializeField] private List<DataStep> _textSteps;

    private List<StageButton> _stageButtons;
    private int _indexActiveStage;
    private StageButton _activeStage;

    public string SetNameLab
    {
        set => textNameLab.text = value;
    }

    private void Awake()
    {
        textNameLab = nameLab.GetComponentInChildren<TextMeshProUGUI>();
        textNumberStage = stageInfo.GetComponentsInChildren<TextMeshProUGUI>()[0];
        textNameStage = stageInfo.GetComponentsInChildren<TextMeshProUGUI>()[1];
        textNumberStep = helpMessages.GetComponentsInChildren<TextMeshProUGUI>()[0];
        textDescStep = helpMessages.GetComponentsInChildren<TextMeshProUGUI>()[1];
        _stageButtons ??= new List<StageButton>();
    }

    private void Start()
    {
        var index = 0;
        var indexStage = 0;
        var indexSubStage = 0;

        foreach (Transform stage in stages.transform)
        {
            if (stage.TryGetComponent(out UIStageButton uiStageButton))
            {
                indexSubStage = 0;
                uiStageButton.index = index;
                index += uiStageButton.subStages.Count > 0 ? 0 : 1;
                uiStageButton.IndexStage = indexStage++;
                uiStageButton.AddActionOnClick = () =>
                {
                    if (uiStageButton.IsDisabled) return;
                    _indexActiveStage = uiStageButton.index;
                    _activeStage = uiStageButton;
                    restartButton.IsAlwaysShow = _activeStage.IsFinished;
                    DisableAllStage();
                    uiStageButton.ActivateStage();
                    IsUseRestartButton(uiStageButton.isUseRestartButton);
                    IsShowStage(uiStageButton.isShowStage && infoButton.isOpen);
                    IsShowSteps(uiStageButton.isShowSteps && infoButton.isOpen);
                    SetDataInfoButton(uiStageButton);
                    ShowInfoButton();
                    ShowTextStage();
                    ShowStep();
                };
                _stageButtons.Add(uiStageButton);
                foreach (var subStage in uiStageButton.subStages)
                {
                    subStage.index = index++;
                    subStage.IndexSubStage = ++indexSubStage;
                    subStage.AddActionOnClick = () =>
                    {
                        if (subStage.IsDisabled) return;
                        _indexActiveStage = subStage.index;
                        _activeStage = subStage;
                        restartButton.IsAlwaysShow = _activeStage.IsFinished;
                        IsUseRestartButton(subStage.isUseRestartButton);
                        IsShowStage(subStage.isShowStage && infoButton.isOpen);
                        IsShowSteps(subStage.isShowSteps && infoButton.isOpen);
                        SetDataInfoButton(uiStageButton, subStage);
                        ShowInfoButton();
                        ShowTextStage();
                        ShowStep();
                    };
                    _stageButtons.Add(subStage);
                }
            }
        }

        restartButton.AddActionOnClick = () =>
        {
            infoButton.gameObject.SetActive(true);
            _activeStage.IsFinished = false;
        };

        infoButton.AddActionOnClick = () =>
        {
            stageInfo.SetActive(infoButton.isOpen && _activeStage.isShowStage);
            helpMessages.SetActive(infoButton.isOpen && _activeStage.isShowSteps);
            ShowInfoButton();
            ShowTextStage();
            ShowStep();
        };

        _stageButtons[0].ActivateStage();
        _activeStage = _stageButtons[0];
        IsUseRestartButton(_stageButtons[0].isUseRestartButton);
        IsShowStage(_stageButtons[0].isShowStage && infoButton.isOpen);
        IsShowSteps(_stageButtons[0].isShowSteps && infoButton.isOpen);
        AlignmentRestartButton();
        ShowInfoButton();
        ShowTextStage();
        ShowStep();
    }

    private void Update()
    {
        //SwitchStagesWithKeyboard();
    }

    private void SwitchStagesWithKeyboard()
    {
        foreach (var stageButton in _stageButtons)
        {
            if(stageButton.GetType() != typeof(UIStageButton)) continue;
            var uiStageButton = stageButton as UIStageButton;
            if (Input.GetKeyDown(uiStageButton.keyboardButton))
            {
                if (!uiStageButton.IsActive) uiStageButton.OnClickInvoke();

                for (var i = 0; i < uiStageButton.subStages.Count; i++)
                {
                    if (uiStageButton.subStages[i].IsActive)
                    {
                        var isActiveNextSubStage = ActivateIsNotDisabledSubStage(uiStageButton, i + 1);
                        if (isActiveNextSubStage) return;
                        break;
                    }
                }

                ActivateIsNotDisabledSubStage(uiStageButton);
                return;
            }
        }
    }

    private bool ActivateIsNotDisabledSubStage(UIStageButton stage, int indexStartFind = 0)
    {
        for (var i = indexStartFind; i < stage.subStages.Count; i++)
        {
            if (stage.subStages[i].IsDisabled) continue;
            stage.subStages[i].OnClickInvoke();
            return true;
        }

        return false;
    }

    private void DisableAllStage()
    {
        foreach (var stageButton in _stageButtons)
        {
            stageButton.DeactivateStage();
        }
    }

    private void SetDataInfoButton(UIStageButton stage, UISubStageButton subStage = null)
    {
        infoButton.SetStageIndex = subStage ? stage.IndexStage + subStage.IndexSubStage : stage.IndexStage;
        infoButton.isSelectSubStage = subStage;
        infoButton.SwitchStage();
    }

    public void AddTextStage(string first, string second)
    {
        _textStages ??= new List<Data>();
        _textStages.Add(new Data(first.Trim(), second.Trim()));
    }

    public void ClearTextStages()
    {
        _textStages.Clear();
    }

    private void ShowTextStage()
    {
        var text = _textStages[_indexActiveStage];
        textNumberStage.text = text.first == String.Empty ? "Опыт №" : text.first;
        textNameStage.text = text.second == String.Empty ? "Название опыта" : text.second;
        AlignmentByMainLayout(textNameStage, 100f);
        AlignmentRestartButton();
    }

    public void AddStep(int indexStage, string first, string second)
    {
        _textSteps ??= new List<DataStep>();
        if (indexStage >= _textSteps.Count)
        {
            _textSteps.Add(new DataStep());
        }

        var textSteps = _textSteps[indexStage];
        textSteps.text ??= new List<Data>();
        _textSteps[indexStage] = textSteps;
        _textSteps[indexStage].text.Add(new Data(first.Trim(), second.Trim()));
    }

    public void ClearSteps()
    {
        _textSteps.Clear();
    }

    public void NextStep(bool isEnableNextStageAfterLastStep = false)
    {
        var step = _textSteps[_indexActiveStage];
        step.index = Math.Clamp(step.index + 1, 0, step.text.Count - 1);
        _textSteps[_indexActiveStage] = step;
        ShowStep();

        if (step.index == step.text.Count - 1 && step.index > 0)
            FinishStage(isEnableNextStageAfterLastStep);
    }

    public void ResetSteps()
    {
        var step = _textSteps[_indexActiveStage];
        step.index = 0;
        _textSteps[_indexActiveStage] = step;
        ShowStep();
    }

    public void FinishStage(bool isEnableNextStage = false)
    {
        if (isEnableNextStage)
            EnableNextStage();

        _activeStage.IsFinished = true;
        restartButton.IsAlwaysShow = true;
        ShowInfoButton();
    }

    public void EnableNextStage(bool isEnableAllSubStagesInStage = false)
    {
        EnableStage(isEnableAllSubStagesInStage);
    }

    private void EnableStage(bool isEnableAllSubStagesInStage, bool isDisabled = false)
    {
        for (var i = 0; i < _stageButtons.Count; i++)
        {
            if (_activeStage == _stageButtons[i])
            {
                 var nextIndex = i + 1 == _stageButtons.Count ? i : i + 1;
                _stageButtons[nextIndex].IsDisabled = isDisabled;
                if(isEnableAllSubStagesInStage)
                    EnableAllSubStages(nextIndex);
                return;
            }
        }
    }

    private void EnableAllSubStages(int nextIndex)
    {
        if(_stageButtons[nextIndex].GetType() != typeof(UIStageButton)) return;

        var uiStageButton = _stageButtons[nextIndex] as UIStageButton;
        foreach (var subStage in uiStageButton.subStages)
        {
            subStage.IsDisabled = false;
        }
    }

    private void ShowStep()
    {
        var text = _textSteps[_indexActiveStage].GetText;
        textNumberStep.text = text.first;
        textDescStep.text = text.second;
        AlignmentByMainLayout(textDescStep, 100f);
        AlignmentRestartButton();
    }

    private void ShowInfoButton()
    {
        var isStageFinished = infoButton.isOpen && restartButton.IsAlwaysShow;
        var isVisibleStageAndSteps = _activeStage.isShowStage || _activeStage.isShowSteps;
        infoButton.gameObject.SetActive(!isStageFinished && isVisibleStageAndSteps);
    }

    private void AlignmentByMainLayout(TextMeshProUGUI textMeshProUGUI, float minusValue)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        var mainLayoutRectTransform = (RectTransform)nameLab.transform.parent.transform;
        var width = mainLayoutRectTransform.sizeDelta.x;
        textMeshProUGUI.GetComponent<LayoutElement>().preferredWidth = width - minusValue;
    }

    private void AlignmentRestartButton()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        var mainLayoutRectTransform = (RectTransform)nameLab.transform.parent.transform;
        var width = mainLayoutRectTransform.sizeDelta.x;

        var rectRestartButton = (RectTransform)restartButton.transform;
        rectRestartButton.sizeDelta = new Vector2(width, rectRestartButton.sizeDelta.y);
    }

    private void IsUseRestartButton(bool isUseRestartButton)
    {
        restartButton.gameObject.SetActive(isUseRestartButton);
    }

    private void IsShowStage(bool isShowStage)
    {
        stageInfo.gameObject.SetActive(isShowStage);
    }

    private void IsShowSteps(bool isShowSteps)
    {
        helpMessages.gameObject.SetActive(isShowSteps);
    }
}

//-----CUSTOM EDITOR-----

#if UNITY_EDITOR
[CustomEditor(typeof(UIStagesControl))]
public class UIStagesControlEditor : Editor
{
    public enum ItemStageControl
    {
        None,
        NameLab,
        StageButton,
        SubStageButton,
        RestartButton
    }

    private SerializedProperty _propertyTextNameLab;
    private SerializedProperty _propertyDataStage;
    private SerializedProperty _propertyDataSteps;
    private SerializedProperty _propertyRestartButton;
    private SerializedProperty _propertyShowHideInfoButton;

    private ItemStageControl _itemStageControl;

    private List<UIStageButton> _stages;
    private List<UISubStageButton> _subStages;
    private int _indexSelectStage;
    private int _indexSelectSubStage;
    private int _indexSelectGeneralStage;

    private GameObject _prefabStage;
    private GameObject _prefabSubStage;
    private Texture2D _openShowHideInfoButton;
    private Texture2D _closeShowHideInfoButton;

    private GUILayoutOption[] _sizeStageButton;
    private GUILayoutOption[] _sizeSubStageButton;
    private GUIStyle _styleSubButton;

    private bool _isFoldoutOptionsText;

    private void OnEnable()
    {
        var data = (UIStagesControl)target;

        data.textNameLab = data.nameLab.GetComponentInChildren<TextMeshProUGUI>();
        data.textNumberStage = data.stageInfo.GetComponentsInChildren<TextMeshProUGUI>()[0];
        data.textNameStage = data.stageInfo.GetComponentsInChildren<TextMeshProUGUI>()[1];
        data.textNumberStep = data.helpMessages.GetComponentsInChildren<TextMeshProUGUI>()[0];
        data.textDescStep = data.helpMessages.GetComponentsInChildren<TextMeshProUGUI>()[1];

        _propertyTextNameLab = serializedObject.FindProperty("textNameLab");
        _propertyDataStage = serializedObject.FindProperty("_textStages");
        _propertyDataSteps = serializedObject.FindProperty("_textSteps");
        _propertyRestartButton = serializedObject.FindProperty("restartButton");
        _propertyShowHideInfoButton = serializedObject.FindProperty("showHideInfoButton");

        _prefabStage = Resources.Load("Stage") as GameObject;
        _prefabSubStage = Resources.Load("SubStage") as GameObject;
        _openShowHideInfoButton = Resources.Load("OpenShowHideInfoButton") as Texture2D;
        _closeShowHideInfoButton = Resources.Load("CloseShowHideInfoButton") as Texture2D;

        _stages ??= new List<UIStageButton>();
        _stages.AddRange(data.GetComponentsInChildren<UIStageButton>());
        _subStages ??= new List<UISubStageButton>();
        _subStages.AddRange(data.GetComponentsInChildren<UISubStageButton>(true));
        if (!Application.isPlaying)
        {
            _propertyDataStage.arraySize = data.GetComponentsInChildren<StageButton>().Length;
            _propertyDataSteps.arraySize = _propertyDataStage.arraySize;
        }

        _sizeStageButton = new[] { GUILayout.Width(30f), GUILayout.Height(30f) };
        _sizeSubStageButton = new[] { GUILayout.Width(25f), GUILayout.Height(25f) };

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        if (!IsAllInit())
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Please, set all field above.", MessageType.Warning);
            return;
        }

        _styleSubButton = new GUIStyle(GUI.skin.button)
            { fontSize = 20, margin = new RectOffset(0, 0, 4, 0), padding = new RectOffset(3, 0, 0, 3) };
        DrawButtons();
        DrawOptions();

        serializedObject.ApplyModifiedProperties();
    }

    private bool IsAllInit()
    {
        var data = (UIStagesControl)target;

        return data.nameLab &&
               data.stages &&
               data.stageInfo &&
               data.helpMessages &&
               data.infoButton &&
               _propertyRestartButton.objectReferenceValue &&
               _propertyShowHideInfoButton.objectReferenceValue;
    }

    private void DrawButtons()
    {
        DrawButtonNameLab();
        GUILayout.Space(1);
        DrawButtonsStages();
        GUILayout.Space(1);
        DrawRestartButton();
        GUILayout.Space(1);
        DrawShowHideInfoButton();
    }

    private void DrawButtonNameLab()
    {
        EditorGUILayout.BeginHorizontal();
        var text = new SerializedObject(_propertyTextNameLab.objectReferenceValue).FindProperty("m_text").stringValue;
        GUI.backgroundColor = _itemStageControl == ItemStageControl.NameLab ? Color.magenta : Color.green;
        if (GUILayout.Button(text, GUILayout.ExpandWidth(true)))
        {
            _itemStageControl = ItemStageControl.NameLab;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawButtonsStages()
    {
        DrawButtonMinusStage();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        var indexSubStage = 0;
        var countStagesWithSubStages = 0;

        for (var i = 0; i < _stages.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor =
                _itemStageControl is ItemStageControl.StageButton or ItemStageControl.SubStageButton &&
                i == _indexSelectStage
                    ? Color.magenta
                    : Color.green;
            if (GUILayout.Button(i.ToString(), _sizeStageButton))
            {
                _itemStageControl = _stages[i].subStages.Count > 0
                    ? ItemStageControl.SubStageButton
                    : ItemStageControl.StageButton;
                _indexSelectStage = i;
                _indexSelectSubStage = indexSubStage;
                _indexSelectGeneralStage = _indexSelectStage + _indexSelectSubStage - countStagesWithSubStages;
            }

            for (var j = 0; j < _stages[i].subStages.Count; j++)
            {
                GUI.backgroundColor = _itemStageControl == ItemStageControl.SubStageButton &&
                                      indexSubStage == _indexSelectSubStage
                    ? Color.magenta
                    : Color.green;
                if (GUILayout.Button((j + 1).ToString(),
                        new GUIStyle(GUI.skin.button) { margin = new RectOffset(0, 0, 4, 0) }, _sizeSubStageButton))
                {
                    _itemStageControl = ItemStageControl.SubStageButton;
                    _indexSelectStage = i;
                    _indexSelectSubStage = indexSubStage;
                    _indexSelectGeneralStage = _indexSelectStage + _indexSelectSubStage - countStagesWithSubStages;
                }

                indexSubStage++;
                GUI.backgroundColor = Color.white;
            }

            if (_stages[i].subStages.Count > 0)
                countStagesWithSubStages++;

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        DrawButtonPlusStage();

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        for (var i = 0; i < _stages.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(1);
            DrawButtonPlusSubStage(i);
            DrawButtonMinusSubStage(i);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRestartButton()
    {
        GUI.backgroundColor = _itemStageControl == ItemStageControl.RestartButton ? Color.magenta : Color.green;
        if (GUILayout.Button("RESTART STAGE BUTTON"))
        {
            _itemStageControl = ItemStageControl.RestartButton;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawShowHideInfoButton()
    {
        var propertyShowHideInfoButton = new SerializedObject(_propertyShowHideInfoButton.objectReferenceValue);
        var isOpen = propertyShowHideInfoButton.FindProperty("isOpen").boolValue;
        var spriteShowHideInfoButton = isOpen ? _openShowHideInfoButton : _closeShowHideInfoButton;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button(spriteShowHideInfoButton, GUIStyle.none, _sizeStageButton))
        {
            propertyShowHideInfoButton.FindProperty("isOpen").boolValue = !isOpen;
            propertyShowHideInfoButton.ApplyModifiedProperties();
            var obj = _propertyShowHideInfoButton.objectReferenceValue as UIInfoButton;
            obj.Awake();
            obj.SwitchStage();
            var uiStagesControl = (UIStagesControl)target;
            uiStagesControl.stageInfo.SetActive(!isOpen);
            uiStagesControl.helpMessages.SetActive(!isOpen);
        }

        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.white;
    }

    private void DrawButtonMinusStage()
    {
        GUI.enabled = _stages.Count > 1;
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("-", new GUIStyle(GUI.skin.button) { fontSize = 30, padding = new RectOffset(0, 0, 0, 5) },
                _sizeStageButton))
        {
            DestroyImmediate(_stages[^1].gameObject);
            _stages.Remove(_stages[^1]);
            _propertyDataStage.arraySize--;
            _propertyDataSteps.arraySize--;
            _itemStageControl = ItemStageControl.None;
        }

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }

    private void DrawButtonPlusStage()
    {
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fontSize = 30, padding = new RectOffset(0, 0, 0, 5) },
                _sizeStageButton))
        {
            GameObject newStage = Instantiate(_prefabStage, _stages[0].transform.parent);
            newStage.name = "Stage " + _stages.Count;
            newStage.GetComponentInChildren<TextMeshProUGUI>().text = _stages.Count.ToString();
            newStage.GetComponentInChildren<UIStageButton>().keyboardButton = KeyCode.Alpha0 + _stages.Count;
            newStage.transform.SetSiblingIndex(_stages.Count);
            _stages.Add(newStage.GetComponent<UIStageButton>());
            _propertyDataStage.arraySize++;
            _propertyDataSteps.arraySize++;
            _itemStageControl = ItemStageControl.None;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawButtonMinusSubStage(int indexStage)
    {
        GUI.enabled = _stages[indexStage].subStages.Count > 0;
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("-", _styleSubButton, _sizeSubStageButton))
        {
            var lastSubStage = _stages[indexStage].subStages[^1];
            DestroyImmediate(lastSubStage.gameObject);
            var propertySubStages = new SerializedObject(_stages[indexStage]);
            propertySubStages.FindProperty("subStages").arraySize--;
            propertySubStages.ApplyModifiedProperties();
            _subStages.Clear();
            _subStages.AddRange(((UIStagesControl)target).GetComponentsInChildren<UISubStageButton>(true));
            _propertyDataStage.arraySize--;
            _propertyDataSteps.arraySize--;
            _itemStageControl = ItemStageControl.None;
        }

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }

    private void DrawButtonPlusSubStage(int indexStage)
    {
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("+", _styleSubButton, _sizeSubStageButton))
        {
            var subStage = Instantiate(_prefabSubStage, _stages[indexStage].transform);
            var numberSubStage = _stages[indexStage].subStages.Count + 1;
            subStage.name = "SubStage " + numberSubStage;
            subStage.GetComponentInChildren<TextMeshProUGUI>().text = numberSubStage.ToString();
            var propertySubStages = new SerializedObject(_stages[indexStage]);
            propertySubStages.FindProperty("subStages").arraySize++;
            propertySubStages.FindProperty("subStages")
                .GetArrayElementAtIndex(propertySubStages.FindProperty("subStages").arraySize - 1)
                .objectReferenceValue = subStage.GetComponent<UISubStageButton>();
            propertySubStages.ApplyModifiedProperties();
            _subStages.Clear();
            _subStages.AddRange(((UIStagesControl)target).GetComponentsInChildren<UISubStageButton>(true));
            _propertyDataStage.arraySize++;
            _propertyDataSteps.arraySize++;
            _itemStageControl = ItemStageControl.None;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawOptions()
    {
        EditorGUILayout.Space();
        switch (_itemStageControl)
        {
            case ItemStageControl.NameLab:
                DrawOptionsNameLab();
                break;
            case ItemStageControl.StageButton:
                DrawOptionsStageOrSubStageButton();
                break;
            case ItemStageControl.SubStageButton:
                DrawOptionsStageOrSubStageButton();
                break;
            case ItemStageControl.RestartButton:
                DrawOptionsRestartButton();
                break;
        }
    }

    private void DrawOptionsNameLab()
    {
        var obj = new SerializedObject(_propertyTextNameLab.objectReferenceValue);
        obj.FindProperty("m_text").stringValue =
            EditorGUILayout.TextField("Name lab", obj.FindProperty("m_text").stringValue);
        obj.ApplyModifiedProperties();
    }

    private void DrawOptionsStageOrSubStageButton()
    {
        StageButton stage = _itemStageControl == ItemStageControl.StageButton
            ? _stages[_indexSelectStage]
            : _subStages[_indexSelectSubStage];
        var stageName = _itemStageControl == ItemStageControl.StageButton
            ? stage.name
            : _stages[_indexSelectStage].name + " " + stage.name;
        var propertyStage = new SerializedObject(stage);
        var isDisabled = propertyStage.FindProperty("isDisabled");
        var isUseRestartButton = propertyStage.FindProperty("isUseRestartButton");
        var isShowStage = propertyStage.FindProperty("isShowStage");
        var isShowSteps = propertyStage.FindProperty("isShowSteps");
        var action = propertyStage.FindProperty("onClick");
        EditorGUILayout.LabelField(stageName + " options: ");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(isDisabled);
        if (isDisabled.boolValue) GUI.enabled = false;
        stage.IsDisabled = isDisabled.boolValue;
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(isUseRestartButton);
        EditorGUILayout.PropertyField(isShowStage);
        EditorGUILayout.PropertyField(isShowSteps);
        EditorGUI.indentLevel--;
        GUI.enabled = true;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);
        EditorGUILayout.PropertyField(action);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        ShowOptionTextStageAndSteps();
        propertyStage.ApplyModifiedProperties();
    }

    private void ShowOptionTextStageAndSteps()
    {
        _isFoldoutOptionsText = EditorGUILayout.Foldout(_isFoldoutOptionsText, "Options Text Stage: ", true);
        if (_isFoldoutOptionsText)
        {
            EditorGUILayout.HelpBox("Entering data manually is not recommended. Use the API to write data from a file!",
                MessageType.Warning);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Number and Name Stage: ");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_propertyDataStage.GetArrayElementAtIndex(_indexSelectGeneralStage)
                .FindPropertyRelative("first"), new GUIContent("Number"));
            EditorGUILayout.PropertyField(_propertyDataStage.GetArrayElementAtIndex(_indexSelectGeneralStage)
                .FindPropertyRelative("second"), new GUIContent("Name"));
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Steps: ");
            var stepsStage = _propertyDataSteps.GetArrayElementAtIndex(_indexSelectGeneralStage)
                .FindPropertyRelative("text");
            EditorGUI.indentLevel++;
            for (var i = 0; i < stepsStage.arraySize; i++)
            {
                EditorGUILayout.PropertyField(stepsStage.GetArrayElementAtIndex(i).FindPropertyRelative("first"),
                    new GUIContent((i + 1) + " number"));
                EditorGUILayout.PropertyField(stepsStage.GetArrayElementAtIndex(i).FindPropertyRelative("second"),
                    new GUIContent((i + 1) + " description"));
                GUILayout.Space(3);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = stepsStage.arraySize > 0;
            GUILayout.Space(25);
            if (GUILayout.Button("Remove"))
            {
                stepsStage.arraySize--;
            }

            GUI.enabled = true;
            if (GUILayout.Button("Add"))
            {
                stepsStage.arraySize++;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
    }

    private void DrawOptionsRestartButton()
    {
        EditorGUILayout.LabelField("Restart button options: ");
        var propertyRestartButton = new SerializedObject(_propertyRestartButton.objectReferenceValue);
        EditorGUILayout.PropertyField(propertyRestartButton.FindProperty("onClick"));
        propertyRestartButton.ApplyModifiedProperties();
    }
}
#endif