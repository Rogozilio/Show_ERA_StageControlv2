using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class UIStageControl2 : MonoBehaviour
{
    [Serializable]
    public struct HelpMassageData
    {
        public string number;
        public string description;

        public HelpMassageData(string number, string description)
        {
            this.number = number;
            this.description = description;
        }
    }

    [Serializable]
    public struct Data
    {
        public int index;
        public string numberStage;
        public string nameStage;
        public List<HelpMassageData> helpMassageData;
        public UnityEvent action;
        public bool isDisabled;
        public bool isUseRestartButton;
        public bool isShowStage;
        public bool isShowSteps;
    }

    [Serializable]
    public struct StageData
    {
        public Data stage;
        public List<Data> subStages;
    }

    [Inject] private UIRoot _uiRoot;

    public string nameLab;

    public List<StageData> stages;
    [SerializeField] private UnityEvent _onRestartButton;

    private void Awake()
    {
        InitUIStages();
        
        _uiRoot.ActivateStage(0);
    }

    private void InitUIStages()
    {
        for (var i = 0; i < stages.Count; i++)
        {
            _uiRoot.AddStage();

            foreach (var subStage in stages[i].subStages)
            {
                _uiRoot.AddSubStage(i);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIStageControl2))]
public class UIStagesControl2Editor : Editor
{
    public enum ItemStageControl
    {
        None,
        NameLab,
        StageButton,
        SubStageButton,
        RestartButton
    }

    private SerializedProperty _propertyNameLab;
    private SerializedProperty _propertyStages;
    private SerializedProperty _propertyRestartButton;
    private SerializedProperty _propertyShowHideInfoButton;

    private ItemStageControl _itemStageControl;

    private int _indexSelectStage;
    private int _indexSelectSubStage;
    private int _indexSelectGeneralStage;

    private Texture2D _openShowHideInfoButton;
    private Texture2D _closeShowHideInfoButton;

    private GUILayoutOption[] _sizeStageButton;
    private GUILayoutOption[] _sizeSubStageButton;
    private GUIStyle _styleSubButton;

    private bool _isFoldoutOptionsText;

    private UIStageControl2 _uiStageControl;

    private void OnEnable()
    {
        _uiStageControl = (UIStageControl2)target;

        _propertyNameLab = serializedObject.FindProperty("nameLab");
        _propertyStages = serializedObject.FindProperty("stages");
        _propertyRestartButton = serializedObject.FindProperty("_onRestartButton");
        _propertyShowHideInfoButton = serializedObject.FindProperty("showHideInfoButton");

        _openShowHideInfoButton = Resources.Load("OpenShowHideInfoButton") as Texture2D;
        _closeShowHideInfoButton = Resources.Load("CloseShowHideInfoButton") as Texture2D;

        _sizeStageButton = new[] { GUILayout.Width(30f), GUILayout.Height(30f) };
        _sizeSubStageButton = new[] { GUILayout.Width(25f), GUILayout.Height(25f) };

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        _uiStageControl = (UIStageControl2)target;

        _styleSubButton = new GUIStyle(GUI.skin.button)
            { fontSize = 20, margin = new RectOffset(0, 0, 4, 0), padding = new RectOffset(3, 0, 0, 3) };
        DrawButtons();
        DrawOptions();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawButtons()
    {
        DrawButtonNameLab();
        GUILayout.Space(1);
        DrawButtonsStages();
        GUILayout.Space(1);
        DrawRestartButton();
        GUILayout.Space(1);
        //DrawShowHideInfoButton();
    }

    private void DrawButtonNameLab()
    {
        EditorGUILayout.BeginHorizontal();
        var text = _propertyNameLab.stringValue;
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

        for (var i = 0; i < _uiStageControl.stages.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor =
                _itemStageControl is ItemStageControl.StageButton or ItemStageControl.SubStageButton &&
                i == _indexSelectStage
                    ? Color.magenta
                    : Color.green;
            if (GUILayout.Button(i.ToString(), _sizeStageButton))
            {
                _itemStageControl = _uiStageControl.stages[i].subStages.Count > 0
                    ? ItemStageControl.SubStageButton
                    : ItemStageControl.StageButton;
                _indexSelectStage = i;
                _indexSelectSubStage = indexSubStage;
                _indexSelectGeneralStage = _indexSelectStage + _indexSelectSubStage - countStagesWithSubStages;
            }

            for (var j = 0; j < _uiStageControl.stages[i].subStages.Count; j++)
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

            if (_uiStageControl.stages[i].subStages.Count > 0)
                countStagesWithSubStages++;

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        DrawButtonPlusStage();

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        for (var i = 0; i < _uiStageControl.stages.Count; i++)
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
        GUI.enabled = _uiStageControl.stages.Count > 1;
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("-", new GUIStyle(GUI.skin.button) { fontSize = 30, padding = new RectOffset(0, 0, 0, 5) },
                _sizeStageButton))
        {
            _propertyStages.arraySize--;
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
            _propertyStages.arraySize++;
            _itemStageControl = ItemStageControl.None;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawButtonMinusSubStage(int indexStage)
    {
        GUI.enabled = _uiStageControl.stages[indexStage].subStages.Count > 0;
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("-", _styleSubButton, _sizeSubStageButton))
        {
            _propertyStages.GetArrayElementAtIndex(indexStage).FindPropertyRelative("subStages").arraySize--;
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
            _propertyStages.GetArrayElementAtIndex(indexStage).FindPropertyRelative("subStages").arraySize++;
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
        EditorGUILayout.PropertyField(_propertyNameLab);
    }

    private void DrawOptionsStageOrSubStageButton()
    {
        SerializedProperty stage = default;
        if (_indexSelectSubStage == 0)
        {
            stage = _propertyStages.GetArrayElementAtIndex(_indexSelectStage).FindPropertyRelative("stage");
        }
        else
        {
            stage = _propertyStages.GetArrayElementAtIndex(_indexSelectStage).FindPropertyRelative("subStages")
                .GetArrayElementAtIndex(_indexSelectSubStage);
        }
        
        var stageName = _itemStageControl == ItemStageControl.StageButton
            ? stage.FindPropertyRelative("nameStage").stringValue
            : _uiStageControl.stages[_indexSelectStage].stage.nameStage + " " + stage.FindPropertyRelative("nameStage").stringValue;
        EditorGUILayout.LabelField(stageName + " options: ");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(stage.FindPropertyRelative("isDisabled"));
        if (stage.FindPropertyRelative("isDisabled").boolValue) GUI.enabled = false;
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(stage.FindPropertyRelative("isUseRestartButton"));
        EditorGUILayout.PropertyField(stage.FindPropertyRelative("isShowStage"));
        EditorGUILayout.PropertyField(stage.FindPropertyRelative("isShowSteps"));
        EditorGUI.indentLevel--;
        GUI.enabled = true;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);
        EditorGUILayout.PropertyField(stage.FindPropertyRelative("action"));
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        ShowOptionTextStageAndSteps();
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
            // EditorGUILayout.PropertyField(_propertyDataStage.GetArrayElementAtIndex(_indexSelectGeneralStage)
            //     .FindPropertyRelative("first"), new GUIContent("Number"));
            // EditorGUILayout.PropertyField(_propertyDataStage.GetArrayElementAtIndex(_indexSelectGeneralStage)
            //     .FindPropertyRelative("second"), new GUIContent("Name"));
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Steps: ");
            // var stepsStage = _propertyDataSteps.GetArrayElementAtIndex(_indexSelectGeneralStage)
            //     .FindPropertyRelative("text");
            // EditorGUI.indentLevel++;
            // for (var i = 0; i < stepsStage.arraySize; i++)
            // {
            //     EditorGUILayout.PropertyField(stepsStage.GetArrayElementAtIndex(i).FindPropertyRelative("first"),
            //         new GUIContent((i + 1) + " number"));
            //     EditorGUILayout.PropertyField(stepsStage.GetArrayElementAtIndex(i).FindPropertyRelative("second"),
            //         new GUIContent((i + 1) + " description"));
            //     GUILayout.Space(3);
            // }
            //
            // EditorGUI.indentLevel--;
            // EditorGUILayout.BeginHorizontal();
            // GUI.enabled = stepsStage.arraySize > 0;
            // GUILayout.Space(25);
            // if (GUILayout.Button("Remove"))
            // {
            //     stepsStage.arraySize--;
            // }
            //
            // GUI.enabled = true;
            // if (GUILayout.Button("Add"))
            // {
            //     stepsStage.arraySize++;
            // }
            //
            // EditorGUILayout.EndHorizontal();
            // EditorGUI.indentLevel--;
        }
    }

    private void DrawOptionsRestartButton()
    {
        EditorGUILayout.PropertyField(_propertyRestartButton);
    }
}
#endif