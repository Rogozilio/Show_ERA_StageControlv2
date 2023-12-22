using System.Linq;
using ModestTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

[ExecuteAlways]
public class UIInfoButton2 : BindButton
{
    public Sprite origin;
    public Sprite normal;
    // public bool isOpen
    // {
    //     set
    //     {
    //         _uiRoot.IsDisplayStageInfo = value;
    //         _uiRoot.IsDisplayStep = value;
    //     }
    //     get => _uiRoot.IsDisplayStageInfo;
    // }
    
    
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(UIInfoButton2))]
public class UIInfoButtonEditor : Editor
{
    private SerializedProperty _imageOrigin;
    private SerializedProperty _imageNormal;
    
    private SerializedProperty _bindButtonProperty;
    private SerializedProperty _actionName;
    private InputActionAsset _inputActionAsset;

    private string[] _optionActions;
    
    private int _indexToolbar = 1;
    private readonly string[] _valueToolbar = {"Init", "Main", "Input"};

    private void OnEnable()
    {
        _inputActionAsset = FindObjectOfType<InputActionsInstaller>().inputActionAsset;

        _imageOrigin = serializedObject.FindProperty("origin");
        _imageNormal = serializedObject.FindProperty("normal");
        _actionName = serializedObject.FindProperty("actionName");

        if (_actionName.stringValue == string.Empty)
        {
            _actionName.stringValue = _inputActionAsset.actionMaps[0].actions[0].name;
        }

        _optionActions = _inputActionAsset.actionMaps[0].actions.Select(x => x.name).ToArray();

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawToolbar();

        switch (_indexToolbar)
        {
            case 0:
                DrawInit();
                break;
            case 1: break;
            case 2:
                DrawInputSettings();
                break;
        }
      
        serializedObject.ApplyModifiedProperties();
    }
    private void OpenInputActionsAsset(InputActionAsset inputActionsAsset)
    {
        if (inputActionsAsset != null)
        {
            AssetDatabase.OpenAsset(inputActionsAsset);
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.Space();
        
        _indexToolbar = GUILayout.Toolbar(_indexToolbar, _valueToolbar);
    }

    private void DrawInit()
    {
        EditorGUILayout.PropertyField(_imageOrigin);
        EditorGUILayout.PropertyField(_imageNormal);
    }

    private void DrawInputSettings()
    {
        EditorGUILayout.Space();

        var index = EditorGUILayout.Popup("Input Action", _optionActions.IndexOf(_actionName.stringValue), _optionActions);
        _actionName.stringValue = _optionActions[index];
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Open Input Actions"))
        {
            OpenInputActionsAsset(_inputActionAsset);
        }
    }
}
#endif