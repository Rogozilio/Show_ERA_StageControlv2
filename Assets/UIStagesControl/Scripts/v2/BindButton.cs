using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[Serializable]
public class BindButton : MonoBehaviour
{
    public string actionName;
    
    private InputActionAsset _inputActionAsset;
    private InputAction _inputAction;

    [Inject]
    private void Construct(InputActions inputActions)
    {
        _inputActionAsset = inputActions.InputActionsAsset;
    }
    
    private void Awake()
    {
        _inputAction = _inputActionAsset.FindAction(actionName);
        _inputAction.performed += OnCombineButton;
    }
    
    private void OnEnable()
    {
        _inputAction.Enable();
    }
    
    private void OnDisable()
    {
        _inputAction.Disable();
    }
    
    public virtual void  OnCombineButton(InputAction.CallbackContext context)
    {
        Debug.Log("Action " + context.action.name + " not override");
    }
}