using Prowl.Editor.Drawers;
using Prowl.Editor.Preferences;
using Prowl.Editor.ProjectSettings;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using System.Reflection;
using static Prowl.Editor.EditorGUI;
using System.Threading.Channels;

namespace Prowl.Editor;

public class ProjectSettingsWindow : SingletonEditorWindow
{
    public ProjectSettingsWindow() : base("Project Settings") { }

    public ProjectSettingsWindow(Type settingToOpen) : base(settingToOpen) { }

    public override void RenderSideView()
    {
        RenderSideViewElement(PhysicsSetting.Instance);
        RenderSideViewElement(BuildProjectSetting.Instance);
    }
}

public class PreferencesWindow : SingletonEditorWindow
{
    public PreferencesWindow() : base("Preferences") { }

    public PreferencesWindow(Type settingToOpen) : base(settingToOpen) { }

    public override void RenderSideView()
    {
        RenderSideViewElement(GeneralPreferences.Instance);
        RenderSideViewElement(AssetPipelinePreferences.Instance);
        RenderSideViewElement(SceneViewPreferences.Instance);
    }
}

public abstract class SingletonEditorWindow : EditorWindow
{
    protected override double Width { get; } = 512;
    protected override double Height { get; } = 512;

    private Type? currentType;
    private object? currentSingleton;

    public SingletonEditorWindow(string title) : base() { Title = FontAwesome6.Gear + " " + title; }

    public SingletonEditorWindow(Type settingToOpen) : base() { currentType = settingToOpen; }

    protected override void Draw()
    {
        if (!Project.HasProject) return;

        // New UI
        g.CurrentNode.Layout(LayoutType.Row);

        elementCounter = 0;

        using (g.Node("SidePanel").Padding(5, 10, 10, 10).Width(Size.Percentage(0.2f)).ExpandHeight().Layout(LayoutType.Column).Clip().Enter())
        {
            g.DrawRectFilled(g.CurrentNode.LayoutData.Rect, GuiStyle.WindowBackground * 0.8f, 10);
            RenderSideView();
        }

        using (g.Node("ContentPanel").PaddingRight(28).Left(Offset.Percentage(0.2f)).Width(Size.Percentage(0.8f)).ExpandHeight().Enter())
        {
            RenderBody();
        }
    }

    public abstract void RenderSideView();

    private int elementCounter = 0;
    protected void RenderSideViewElement<T>(T elementInstance)
    {
        Type settingType = elementInstance.GetType();
        using (g.ButtonNode("Element" + elementCounter++, out var pressed, out var hovered).ExpandWidth().Height(GuiStyle.ItemHeight).Enter())
        {

            if (currentType == settingType)
                g.DrawRectFilled(g.CurrentNode.LayoutData.Rect, GuiStyle.Indigo, 10);
            else if (hovered)
                g.DrawRectFilled(g.CurrentNode.LayoutData.Rect, GuiStyle.Base5, 10);

            g.DrawText(settingType.Name, g.CurrentNode.LayoutData.Rect, false);

            if (pressed)
            {
                currentType = settingType;
                currentSingleton = elementInstance;
            }

        }
    }

    private void RenderBody()
    {
        if (currentType == null) return;

        // Draw Settings
        object setting = currentSingleton;

        if (PropertyGrid(currentSingleton.GetType().Name, ref setting, TargetFields.Serializable, PropertyGridConfig.NoBorder | PropertyGridConfig.NoBackground))
        {
            // Use reflection to find a method "protected void Save()" and Validate
            MethodInfo? validateMethod = setting.GetType().GetMethod("Validate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            validateMethod?.Invoke(setting, null);
            MethodInfo? saveMethod = setting.GetType().GetMethod("Save", BindingFlags.Instance | BindingFlags.Public);
            saveMethod?.Invoke(setting, null);
        }

        // Draw any Buttons
        //EditorGui.HandleAttributeButtons(setting);
    }
}
