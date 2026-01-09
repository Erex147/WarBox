using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace WarBox.UI.Windows;

public class WarBoxAutoLayoutWindow : AutoLayoutWindow<WarBoxAutoLayoutWindow>
{
    private void Update()
    {
        if (!Initialized || !IsOpened) return;
    }

    protected override void Init()
    {
        GetLayoutGroup().spacing = 3;
    }
    public override void OnFirstEnable()
    {
        base.OnFirstEnable();
    }
    public override void OnNormalEnable()
    {
        base.OnNormalEnable();
    }
    public override void OnNormalDisable()
    {
        base.OnNormalDisable();
    }
}