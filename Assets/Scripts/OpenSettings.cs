using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OpenSettings : MonoBehaviour
{
    public GameObject settingsObject;
    public GameObject empty1;
    public GameObject emptyExtraButtons;
    public Dropdown backgroundChooser;

    public GameObject scrollViewSettings;
    public GameObject scrollViewInstructions;

    public Toggle ctrlAltDel;
    public Toggle tab;
    public Toggle fs;
    public Toggle arrows;
    public Toggle esc;
    public Toggle altGr;
    public Toggle insertHome;
    public Toggle scrollPause;
    public Toggle enter;
    public Toggle doneAsEnter;

    public Text instructionsBtnText;
    public GameObject saveSettingsBtn;

    public GameObject arrowsEmpty;
    public GameObject fsEmpty;
    public GameObject tabShiftEmpty;
    public GameObject ctrlAltDelEmpty;
    public GameObject insertHomeEmpty;
    public GameObject scrollPauseEmpty;
    public GameObject escEmpty;
    public GameObject altGrEmpty;
    public GameObject enterEmpty;

    public Sprite[] backgrounds;
    public Material[] materials;

    private void Awake()
    {
        backgroundChooser.ClearOptions();
        List<Dropdown.OptionData> backgroundItems = new List<Dropdown.OptionData>();

        foreach (var item in backgrounds)
        {
            var itemOption = new Dropdown.OptionData(item.name, item);
            backgroundItems.Add(itemOption);
        }

        backgroundChooser.AddOptions(backgroundItems);
        backgroundChooser.value = PlayerPrefs.GetInt("background", 0);

        ctrlAltDel.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("ctrlaltdel", 0));
        tab.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("tab", 0));
        fs.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("f", 0));
        arrows.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("arrow", 0));
        esc.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("esc", 0));
        altGr.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("altgr", 0));
        insertHome.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("insert", 0));
        scrollPause.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("scroll", 0));
        enter.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("enter", 0));
        doneAsEnter.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("doneAsEnter", 0));

        ctrlAltDelEmpty.SetActive(ctrlAltDel.isOn);
        tabShiftEmpty.SetActive(tab.isOn);
        fsEmpty.SetActive(fs.isOn);
        arrowsEmpty.SetActive(arrows.isOn);
        escEmpty.SetActive(esc.isOn);
        altGrEmpty.SetActive(altGr.isOn);
        insertHomeEmpty.SetActive(insertHome.isOn);
        scrollPauseEmpty.SetActive(scrollPause.isOn);
        enterEmpty.SetActive(enter.isOn);
    }

    public void OpenSettingsPanel()
    {
        settingsObject.SetActive(!settingsObject.gameObject.activeSelf);

        if (empty1.activeSelf == true)
            emptyExtraButtons.SetActive(false);

        empty1.SetActive(!empty1.gameObject.activeSelf);

        if (empty1.transform.position.y < -250)
            emptyExtraButtons.SetActive(!settingsObject.gameObject.activeSelf);
    }

    public void SaveCloseSettingsPanel()
    {
        PlayerPrefs.SetInt("ctrlaltdel", ctrlAltDel.isOn ? 1 : 0);
        PlayerPrefs.SetInt("tab", tab.isOn ? 1 : 0);
        PlayerPrefs.SetInt("f", fs.isOn ? 1 : 0);
        PlayerPrefs.SetInt("arrow", arrows.isOn ? 1 : 0);
        PlayerPrefs.SetInt("esc", esc.isOn ? 1 : 0);
        PlayerPrefs.SetInt("altgr", altGr.isOn ? 1 : 0);
        PlayerPrefs.SetInt("insert", insertHome.isOn ? 1 : 0);
        PlayerPrefs.SetInt("scroll", scrollPause.isOn ? 1 : 0);
        PlayerPrefs.SetInt("enter", enter.isOn ? 1 : 0);
        PlayerPrefs.SetInt("doneAsEnter", doneAsEnter.isOn ? 1 : 0);
        PlayerPrefs.SetInt("background", backgroundChooser.value);

        settingsObject.SetActive(false);
        empty1.SetActive(true);

        if (empty1.transform.position.y < -250)
         emptyExtraButtons.SetActive(true);
    }

    public void ToggleButtons(string name)
    {
        if(name == "ctrlaltdel")
        {
            ctrlAltDelEmpty.SetActive(ctrlAltDel.isOn);
        } else if (name == "tab")
        {
            tabShiftEmpty.SetActive(tab.isOn);
        } else if (name == "f")
        {
            fsEmpty.SetActive(fs.isOn);
        } else if (name == "arrow")
        {
            arrowsEmpty.SetActive(arrows.isOn);
        } else if (name == "esc")
        {
            escEmpty.SetActive(esc.isOn);
        } else if (name == "altgr")
        {
            altGrEmpty.SetActive(altGr.isOn);
        } else if (name == "insert")
        {
            insertHomeEmpty.SetActive(insertHome.isOn);
        } else if (name == "scroll")
        {
            scrollPauseEmpty.SetActive(scrollPause.isOn);
        } else if (name == "enter")
        {
            enterEmpty.SetActive(enter.isOn);
        }
    }

    public void ChangeBackground()
    {
        var panelimage = GameObject.FindWithTag("MainPanel").GetComponent<Image>();
        panelimage.material = materials[backgroundChooser.value];
    }

    public void OpenInstructions()
    {
        if (scrollViewInstructions.activeSelf == false)
        {
            scrollViewInstructions.SetActive(true);
            scrollViewSettings.SetActive(false);
            saveSettingsBtn.SetActive(false);
            instructionsBtnText.text = "Back";
        } else
        {
            scrollViewInstructions.SetActive(false);
            scrollViewSettings.SetActive(true);
            saveSettingsBtn.SetActive(true);
            instructionsBtnText.text = "Instructions";
        }
    }
}
