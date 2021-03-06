/**
 * Copyright (c) 2018 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.ComponentModel;

public class LEDSensor : MonoBehaviour, Comm.BridgeClient
{
    public enum LEDModeTypes
    {
        [Description("c")]
        None,
        [Description("a")]
        All,
        [Description("b")]
        Blink,
        [Description("f")]
        Fade,
        [Description("r")]
        Right,
        [Description("l")]
        Left
    };
    private LEDModeTypes currentLEDMode = LEDModeTypes.None;

    public enum LEDColorTypes
    {
        [Description("w")]
        White,
        [Description("g")]
        Green,
        [Description("r")]
        Red,
        [Description("b")]
        Blue,
        [Description("o")]
        Orange,
        [Description("R")]
        Rainbow
    }
    private LEDColorTypes currentLEDColor = LEDColorTypes.White;
    private List<Color> ledColors = new List<Color>() { Color.white, Color.green, Color.red, Color.blue, new Color(1f, 0.45f, 0f), Color.white};
    private Color lerpedColor = Color.white;

    public string ledServiceName = "/central_controller/effects";
    //public float publishRate = 1f;
    private Comm.Bridge Bridge;

    public Renderer ledMatRight;
    public Renderer ledMatLeft;
    public Texture fadeLEDEmitTexture;

    private List<Light> ledLightsRight = new List<Light>();
    private List<Light> ledLightsLeft = new List<Light>();

    private float fadeRate = 0.25f;
    private float fadeOffset = 0f;

    private bool isBlinkOn = true;
    private float blinkRate = 0.5f;
    private float currentBlinkTime = 0f;

    private int rainbowIndex = 0;
    private float rainbowRate = 3f;
    private float currentRainbowRate = 0f;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "LEDLightL" && child.GetComponent<Light>() != null)
                ledLightsLeft.Add(child.GetComponent<Light>());
            if (child.name == "LEDLightR" && child.GetComponent<Light>() != null)
                ledLightsRight.Add(child.GetComponent<Light>());
        }
        AddUIElements();
    }
    
    private void Update()
    {
        ApplyLEDMode(currentLEDMode);
    }

    private void SetLEDMode(LEDModeTypes modeIndex)
    {
        currentLEDMode = modeIndex;
    }

    private void SetLEDColor(LEDColorTypes colorIndex)
    {
        currentLEDColor = colorIndex;
    }

    private void ApplyLEDMode(LEDModeTypes mode)
    {
        if (currentLEDColor == LEDColorTypes.Rainbow)
        {
            lerpedColor = Color.Lerp(lerpedColor, ledColors[rainbowIndex], rainbowRate);
            ledColors[(int)LEDColorTypes.Rainbow] = lerpedColor;
            if (currentRainbowRate < 1)
            {
                currentRainbowRate += Time.deltaTime / rainbowRate;
            }
            else
            {
                currentRainbowRate = 0f;
                rainbowIndex = rainbowIndex < ledColors.Count - 2 ? rainbowIndex + 1 : rainbowIndex = 0;
            }
        }

        switch (mode)
        {
            case LEDModeTypes.None:
                ledMatRight.material.DisableKeyword("_EMISSION");
                ledMatLeft.material.DisableKeyword("_EMISSION");
                ledMatRight.material.SetTexture("_EmissionMap", null);
                ledMatLeft.material.SetTexture("_EmissionMap", null);
                foreach (var item in ledLightsRight)
                    item.enabled = false;
                foreach (var item in ledLightsLeft)
                    item.enabled = false;
                break;
            case LEDModeTypes.All:
                ledMatRight.material.EnableKeyword("_EMISSION");
                ledMatLeft.material.EnableKeyword("_EMISSION");
                ledMatRight.material.SetTexture("_EmissionMap", null);
                ledMatLeft.material.SetTexture("_EmissionMap", null);
                ledMatRight.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                ledMatLeft.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                foreach (var item in ledLightsRight)
                {
                    item.color = ledColors[(int)currentLEDColor];
                    item.enabled = true;
                }
                foreach (var item in ledLightsLeft)
                {
                    item.color = ledColors[(int)currentLEDColor];
                    item.enabled = true;
                }
                break;
            case LEDModeTypes.Blink:
                ledMatRight.material.SetTexture("_EmissionMap", null);
                ledMatLeft.material.SetTexture("_EmissionMap", null);
                if (currentBlinkTime < 1)
                {
                    currentBlinkTime += Time.deltaTime / blinkRate;
                    if (isBlinkOn)
                    {
                        ledMatRight.material.EnableKeyword("_EMISSION");
                        ledMatLeft.material.EnableKeyword("_EMISSION");
                        ledMatRight.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                        ledMatLeft.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                        foreach (var item in ledLightsRight)
                        {
                            item.color = ledColors[(int)currentLEDColor];
                            item.enabled = true;
                        }
                        foreach (var item in ledLightsLeft)
                        {
                            item.color = ledColors[(int)currentLEDColor];
                            item.enabled = true;
                        }
                    }
                    else
                    {
                        ledMatRight.material.DisableKeyword("_EMISSION");
                        ledMatLeft.material.DisableKeyword("_EMISSION");
                        foreach (var item in ledLightsRight)
                            item.enabled = false;
                        foreach (var item in ledLightsLeft)
                            item.enabled = false;
                    }
                }
                else
                {
                    currentBlinkTime = 0f;
                    isBlinkOn = !isBlinkOn;
                }
                break;
            case LEDModeTypes.Fade:
                fadeOffset = Time.time * -fadeRate;
                ledMatRight.material.EnableKeyword("_EMISSION");
                ledMatLeft.material.EnableKeyword("_EMISSION");
                ledMatRight.material.SetTexture("_EmissionMap", fadeLEDEmitTexture);
                ledMatLeft.material.SetTexture("_EmissionMap", fadeLEDEmitTexture);
                ledMatRight.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 2f);
                ledMatLeft.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 2f);
                ledMatRight.material.SetTextureOffset("_MainTex", new Vector2(0, fadeOffset));
                ledMatLeft.material.SetTextureOffset("_MainTex", new Vector2(0, fadeOffset));
                foreach (var item in ledLightsRight)
                {
                    item.color = ledColors[(int)currentLEDColor];
                    item.enabled = true;
                }
                foreach (var item in ledLightsLeft)
                {
                    item.color = ledColors[(int)currentLEDColor];
                    item.enabled = true;
                }
                break;
            case LEDModeTypes.Right:
                ledMatRight.material.SetTexture("_EmissionMap", null);
                ledMatLeft.material.DisableKeyword("_EMISSION");
                foreach (var item in ledLightsLeft)
                    item.enabled = false;
                if (currentBlinkTime < 1)
                {
                    currentBlinkTime += Time.deltaTime / blinkRate;
                    if (isBlinkOn)
                    {
                        ledMatRight.material.EnableKeyword("_EMISSION");
                        ledMatRight.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                        foreach (var item in ledLightsRight)
                        {
                            item.color = ledColors[(int)currentLEDColor];
                            item.enabled = true;
                        }
                    }
                    else
                    {
                        ledMatRight.material.DisableKeyword("_EMISSION");
                        foreach (var item in ledLightsRight)
                            item.enabled = false;
                    }
                }
                else
                {
                    currentBlinkTime = 0f;
                    isBlinkOn = !isBlinkOn;
                }
                break;
            case LEDModeTypes.Left:
                ledMatLeft.material.SetTexture("_EmissionMap", null);
                ledMatRight.material.DisableKeyword("_EMISSION");
                foreach (var item in ledLightsRight)
                    item.enabled = false;
                if (currentBlinkTime < 1)
                {
                    currentBlinkTime += Time.deltaTime / blinkRate;
                    if (isBlinkOn)
                    {
                        ledMatLeft.material.EnableKeyword("_EMISSION");
                        ledMatLeft.material.SetColor("_EmissionColor", ledColors[(int)currentLEDColor] * 1f);
                        foreach (var item in ledLightsLeft)
                        {
                            item.color = ledColors[(int)currentLEDColor];
                            item.enabled = true;
                        }
                    }
                    else
                    {
                        ledMatLeft.material.DisableKeyword("_EMISSION");
                        foreach (var item in ledLightsLeft)
                            item.enabled = false;
                    }
                }
                else
                {
                    currentBlinkTime = 0f;
                    isBlinkOn = !isBlinkOn;
                }
                break;
            default:
                break;
        }
    }

    public void GetSensors(List<UnityEngine.Component> sensors)
    {
        // this is not a sensor
    }

    public void OnBridgeAvailable(Comm.Bridge bridge)
    {
        Bridge = bridge;
        Bridge.OnConnected += () =>
        {
            Bridge.AddService<Ros.Srv.String, Ros.Srv.String>(ledServiceName, msg =>
            {
                var response = new Ros.Srv.String();

                if (msg.str == null || msg.str.Length == 0)
                {
                    response.str = "Invalid input!";
                    return response;
                }

                switch(msg.str[0])
                {
                    case 'c':
                        SetLEDMode(LEDModeTypes.None);
                        break;
                    case 'f':
                        SetLEDMode(LEDModeTypes.Fade);
                        break;
                    case 'b':
                        SetLEDMode(LEDModeTypes.Blink);
                        break;
                    case 'a':
                        SetLEDMode(LEDModeTypes.All);
                        break;
                    case 'r':
                        SetLEDMode(LEDModeTypes.Right);
                        break;
                    case 'l':
                        SetLEDMode(LEDModeTypes.Left);
                        break;
                    default:
                        response.str += "Invalid code for LED mode! ";
                        break;
                }

                if (msg.str.Length > 1)
                {
                    switch(msg.str[1])
                    {
                        case 'g':
                            SetLEDColor(LEDColorTypes.Green);
                            break;
                        case 'r':
                            SetLEDColor(LEDColorTypes.Red);
                            break;
                        case 'b':
                            SetLEDColor(LEDColorTypes.Blue);
                            break;
                        case 'w':
                            SetLEDColor(LEDColorTypes.White);
                            break;
                        case 'o':
                            SetLEDColor(LEDColorTypes.Orange);
                            break;
                        case 'R':
                            SetLEDColor(LEDColorTypes.Rainbow);
                            break;
                        default:
                            response.str += "Invalid code for LED color!";
                            return response;
                    }
                }
                if (response.str == null)
                {
                    response.str = "LED color/mode changed";
                }
                return response;
            });
        };
    }

    private void AddUIElements() // TODO combine with tweakables prefab for all sensors issues on start though
    {
        List<string> tempModeList = System.Enum.GetNames(typeof(LEDModeTypes)).ToList();
        for (int i = 0; i < tempModeList.Count; i++)
        {
            tempModeList[i] = tempModeList[i].Insert(0, "LED Mode: ");
        }
        var ledModeDropdown = GetComponentInParent<UserInterfaceTweakables>().AddDropdown("LEDMode", "LED Mode: ", tempModeList);
        ledModeDropdown.onValueChanged.AddListener(x => SetLEDMode((LEDModeTypes)x));

        List<string> tempColorList = System.Enum.GetNames(typeof(LEDColorTypes)).ToList();
        for (int i = 0; i < tempColorList.Count; i++)
        {
            tempColorList[i] = tempColorList[i].Insert(0, "LED Color: ");
        }
        var ledColorDropdown = GetComponentInParent<UserInterfaceTweakables>().AddDropdown("LEDColor", "LED Color: ", tempColorList);
        ledColorDropdown.onValueChanged.AddListener(x => SetLEDColor((LEDColorTypes)x));
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
