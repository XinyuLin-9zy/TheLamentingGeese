using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utage;

[AddComponentMenu("Accessibility/UI/Accessible Button")]
public class AccessibleButton : Button
{
    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.RemoveListener(HandleClick);
        onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        UguiView view = GetComponentInParent<UguiView>();
        if (view == null)
        {
            return;
        }

        Transform t = transform;
        while (t != null && t != view.transform)
        {
            string pname = t.parent ? t.parent.name : "";

            if (view is UtageUguiTitle title)
            {
                string buttonName = IsTitleButtonName(t.name) ? t.name : pname;
                if (buttonName == "Start") { title.OnTapStart(); return; }
                if (buttonName == "Load") { title.OnTapLoad(); return; }
                if (buttonName == "Config") { title.OnTapConfig(); return; }
                if (buttonName == "Gallery") { title.OnTapGallery(); return; }
                if (buttonName == "Archive") { title.OnTapArchive(); return; }
                if (buttonName == "PlotMap") { title.OnTapPlotMap(); return; }
                if (buttonName == "ExtraStory") { title.OnTapExtraStory(); return; }
                if (buttonName == "Exit") { title.OnTapExit(); return; }
                if (buttonName == "Download" || buttonName == "Button-Download") { title.OnTapDownLoad(); return; }
            }

            t = t.parent;
        }
    }

    static bool IsTitleButtonName(string name)
    {
        return name == "Start"
            || name == "Load"
            || name == "Config"
            || name == "Gallery"
            || name == "Archive"
            || name == "PlotMap"
            || name == "ExtraStory"
            || name == "Exit"
            || name == "Download"
            || name == "Button-Download";
    }
}
