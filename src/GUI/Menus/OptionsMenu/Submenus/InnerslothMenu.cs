using System;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.Options;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class InnerslothMenu : MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro titleHeader;
    private GameObject anchorObject;

    public InnerslothMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Innersloth");
        anchorObject.transform.localPosition += new Vector3(2f, 2f);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1);

        titleHeader = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        titleHeader.font = CustomOptionContainer.GetGeneralFont();
        titleHeader.transform.localPosition = new Vector3(7.06f, -1.85f, 0);
        titleHeader.name = "MainTitle";
    }

    private void Start()
    {
        titleHeader.text = "Among Us Data";
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        anchorObject.SetActive(false);
    }

    public virtual void Open()
    {
        anchorObject.SetActive(true);
    }

    public virtual void Close()
    {
        anchorObject.SetActive(false);
    }

}