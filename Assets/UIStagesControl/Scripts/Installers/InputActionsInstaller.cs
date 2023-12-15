using UnityEngine.InputSystem;
using Zenject;

public class InputActionsInstaller : MonoInstaller
{
    public InputActionAsset inputActionAsset;

    public override void InstallBindings()
    {
        Container.Bind<InputActions>().AsSingle().OnInstantiated<InputActions>((context, inputActions) =>
            inputActions.InputActionsAsset = inputActionAsset);
    }
}