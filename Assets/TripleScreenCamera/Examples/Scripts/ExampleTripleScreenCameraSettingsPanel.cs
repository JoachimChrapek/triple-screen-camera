using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FazApp.TripleScreenCamera.Examples
{
    public class ExampleTripleScreenCameraSettingsPanel : MonoBehaviour
    {
        [SerializeField]
        private TripleScreenCameraController tripleScreenCameraController;
        
        [Header("UI")]
        [SerializeField]
        private TMP_Dropdown screenSetupDropdown;
        [SerializeField]
        private TMP_Dropdown aspectRatioDropDown;
        [SerializeField]
        private TMP_InputField displayWidthInputField;
        [SerializeField]
        private TMP_InputField distanceFromCenterDisplayInputField;
        [SerializeField]
        private TMP_InputField lateralDisplayAngleInputField;
        [SerializeField]
        private TMP_InputField lateralDisplayMarginInputField;
        [SerializeField]
        private TMP_InputField nearClippingPlaneInputField;
        [SerializeField]
        private TMP_InputField farClippingPlaneInputField;
        [SerializeField]
        private Toggle autoUpdateToggle;
        
        
        private static readonly List<KeyValuePair<string, ScreenSetupType>> screenSetupOptions = new() {
            new KeyValuePair<string, ScreenSetupType>("Single Screen", ScreenSetupType.SingleScreen), 
            new KeyValuePair<string, ScreenSetupType>("Triple Screen", ScreenSetupType.TripleScreen)
        };
        
        private static readonly List<KeyValuePair<string, Vector2>> aspectRatioOptions = new() {
            new KeyValuePair<string, Vector2>("16:9", new Vector2(16.0f, 9.0f)), 
            new KeyValuePair<string, Vector2>("16:10", new Vector2(16.0f, 10.0f)), 
            new KeyValuePair<string, Vector2>("4:3", new Vector2(4.0f, 3.0f))
        };

        public void OnUpdateAfterSettingsChangeButtonClicked()
        {
            tripleScreenCameraController.UpdateAfterSettingsChange();
        }

        private void Awake()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            screenSetupDropdown.ClearOptions();
            screenSetupDropdown.AddOptions(screenSetupOptions.Select(o => o.Key).ToList());
            screenSetupDropdown.onValueChanged.AddListener(OnScreentSetupDropdownChanged);

            for (int i = 0; i < screenSetupOptions.Count; i++)
            {
                KeyValuePair<string, ScreenSetupType> screenSetupOption = screenSetupOptions[i];

                if (screenSetupOption.Value == tripleScreenCameraController.ScreenSetup)
                {
                    screenSetupDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }

            aspectRatioDropDown.ClearOptions();
            aspectRatioDropDown.AddOptions(aspectRatioOptions.Select(o => o.Key).ToList());
            aspectRatioDropDown.onValueChanged.AddListener(OnAspectRatioDropdownChanged);

            for (int i = 0; i < aspectRatioOptions.Count; i++)
            {
                KeyValuePair<string, Vector2> aspectRatioOption = aspectRatioOptions[i];

                if (aspectRatioOption.Value == tripleScreenCameraController.AspectRatio)
                {
                    aspectRatioDropDown.SetValueWithoutNotify(i);
                    break;
                }
            }

            InitializeFloatOption(displayWidthInputField, () => tripleScreenCameraController.DisplayWidth, x => tripleScreenCameraController.DisplayWidth = x / 1000.0f);
            InitializeFloatOption(distanceFromCenterDisplayInputField, () => tripleScreenCameraController.DistanceFromCenterDisplay, x => tripleScreenCameraController.DistanceFromCenterDisplay = x / 1000.0f);
            InitializeFloatOption(lateralDisplayAngleInputField, () => tripleScreenCameraController.LateralDisplaysAngle, x => tripleScreenCameraController.LateralDisplaysAngle = x, false);
            InitializeFloatOption(lateralDisplayMarginInputField, () => tripleScreenCameraController.LateralDisplaysMargin, x => tripleScreenCameraController.LateralDisplaysMargin = x / 1000.0f);
            InitializeFloatOption(nearClippingPlaneInputField, () => tripleScreenCameraController.NearClippingPlane, x => tripleScreenCameraController.NearClippingPlane = x, false, true);
            InitializeFloatOption(farClippingPlaneInputField, () => tripleScreenCameraController.FarClippingPlane, x => tripleScreenCameraController.FarClippingPlane = x, false, true);

            autoUpdateToggle.SetIsOnWithoutNotify(tripleScreenCameraController.AutoUpdate);
            autoUpdateToggle.onValueChanged.AddListener(OnAutoUpdateToggleChanged);
        }

        private void OnScreentSetupDropdownChanged(int optionIndex)
        {
            tripleScreenCameraController.ScreenSetup = screenSetupOptions[optionIndex].Value;
        }
        
        private void OnAspectRatioDropdownChanged(int optionIndex)
        {
            tripleScreenCameraController.AspectRatio = aspectRatioOptions[optionIndex].Value;
        }

        private void InitializeFloatOption(TMP_InputField inputField, Func<float> getter, Action<float> setter, bool multiplyValue = true, bool showHundreths = false)
        {
            inputField.SetTextWithoutNotify(GetFloatText(getter, multiplyValue, showHundreths));
            inputField.onValueChanged.AddListener(s => OnFloatInputFieldValueChanged(s, inputField, getter, setter, multiplyValue, showHundreths));
        }

        private void OnFloatInputFieldValueChanged(string newValue, TMP_InputField inputField, Func<float> getter, Action<float> setter, bool multiplyValue, bool showHundreths)
        {
            if (float.TryParse(newValue, out float newValueParsed))
            {
                setter.Invoke(newValueParsed);
            }
            
            inputField.SetTextWithoutNotify(GetFloatText(getter, multiplyValue, showHundreths));
        }

        private string GetFloatText(Func<float> getter, bool multiplyValue, bool showHundreths)
        {
            float value = getter.Invoke();

            if (multiplyValue)
            {
                value *= 1000.0f;
            }

            return showHundreths ? value.ToString("F2") : value.ToString("F0");
        }
        
        private void OnAutoUpdateToggleChanged(bool isToggled)
        {
            tripleScreenCameraController.AutoUpdate = isToggled;
        }
    }
}