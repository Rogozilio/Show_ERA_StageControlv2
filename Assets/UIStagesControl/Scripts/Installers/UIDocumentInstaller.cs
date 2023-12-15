using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class UIDocumentInstaller : MonoInstaller
{
    public GameObject uiStageControl;
    public PanelSettings panelSettings;
    public VisualTreeAsset sourceAsset;
    public override void InstallBindings()
    {
        var uiDocument = Container.InstantiateComponent<UIDocument>(uiStageControl);
        uiDocument.panelSettings = panelSettings;
        uiDocument.visualTreeAsset = sourceAsset;

        Container.Bind<UIRoot>().AsSingle().WithArguments(uiDocument);
    }
}