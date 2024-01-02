// Decompiled with JetBrains decompiler
// Type: KepRemapPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

#nullable disable
public class KepRemapPanel : MonoBehaviour
{
  public List<RemappableKey> remappableKeys = new List<RemappableKey>();
  public List<GameObject> keySlots = new List<GameObject>();
  public GameObject keyRemapSlotPrefab;
  public RectTransform keyRemapContainer;
  public float maxVertical;
  public float horizontalOffset;
  public float verticalOffset;
  public int currentVertical;
  public int currentHorizontal;
  public GameObject sectionTextPrefab;

  public void ResetKeybindsUI()
  {
    this.UnloadKeybindsUI();
    this.LoadKeybindsUI();
  }

  private void OnDisable() => this.UnloadKeybindsUI();

  public void UnloadKeybindsUI()
  {
    for (int index = 0; index < this.keySlots.Count; ++index)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.keySlots[index]);
    this.keySlots.Clear();
  }

  public void LoadKeybindsUI()
  {
    this.currentVertical = 0;
    this.currentHorizontal = 0;
    Vector2 vector2 = new Vector2(this.horizontalOffset * (float) this.currentHorizontal, this.verticalOffset * (float) this.currentVertical);
    bool flag = false;
    int num1 = 0;
    for (int index1 = 0; index1 < this.remappableKeys.Count; ++index1)
    {
      if (!((UnityEngine.Object) this.remappableKeys[index1].currentInput == (UnityEngine.Object) null))
      {
        GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.keyRemapSlotPrefab, (Transform) this.keyRemapContainer);
        this.keySlots.Add(gameObject1);
        gameObject1.GetComponentInChildren<TextMeshProUGUI>().text = this.remappableKeys[index1].ControlName;
        gameObject1.GetComponent<RectTransform>().anchoredPosition = vector2;
        SettingsOption componentInChildren = gameObject1.GetComponentInChildren<SettingsOption>();
        componentInChildren.rebindableAction = this.remappableKeys[index1].currentInput;
        componentInChildren.rebindableActionBindingIndex = this.remappableKeys[index1].rebindingIndex;
        componentInChildren.gamepadOnlyRebinding = this.remappableKeys[index1].gamepadOnly;
        string controlName = this.remappableKeys[index1].ControlName;
        ReadOnlyArray<InputControl> controls = this.remappableKeys[index1].currentInput.action.controls;
        // ISSUE: variable of a boxed type
        __Boxed<int> count1 = (ValueType) controls.Count;
        Debug.Log((object) string.Format("{0}: rebind controls length: {1}", (object) controlName, (object) count1));
        Debug.Log((object) string.Format("{0}: rebind control binding index is {1}", (object) this.remappableKeys[index1].ControlName, (object) this.remappableKeys[index1].rebindingIndex));
        int index2 = 0;
        InputBinding binding;
        while (true)
        {
          int num2 = index2;
          controls = this.remappableKeys[index1].currentInput.action.controls;
          int count2 = controls.Count;
          if (num2 < count2)
          {
            // ISSUE: variable of a boxed type
            __Boxed<int> local1 = (ValueType) index2;
            ReadOnlyArray<InputBinding> bindings = this.remappableKeys[index1].currentInput.action.bindings;
            ref ReadOnlyArray<InputBinding> local2 = ref bindings;
            InputAction action = this.remappableKeys[index1].currentInput.action;
            controls = this.remappableKeys[index1].currentInput.action.controls;
            InputControl control = controls[index2];
            int bindingIndexForControl = action.GetBindingIndexForControl(control);
            binding = local2[bindingIndexForControl];
            string humanReadableString = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            Debug.Log((object) string.Format("control #{0}: ${1}", (object) local1, (object) humanReadableString));
            ++index2;
          }
          else
            break;
        }
        int rebindingIndex = this.remappableKeys[index1].rebindingIndex;
        int index3 = Mathf.Max(rebindingIndex, 0);
        TextMeshProUGUI currentlyUsedKeyText = componentInChildren.currentlyUsedKeyText;
        binding = componentInChildren.rebindableAction.action.bindings[index3];
        string humanReadableString1 = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        currentlyUsedKeyText.text = humanReadableString1;
        Debug.Log((object) string.Format("bindingIndex of {0} : {1}; display bindingIndex: {2}", (object) componentInChildren.currentlyUsedKeyText.text, (object) rebindingIndex, (object) index3));
        if (!flag && index1 + 1 < this.remappableKeys.Count && this.remappableKeys[index1 + 1].gamepadOnly)
        {
          num1 = (int) ((double) this.maxVertical + 2.0);
          this.currentVertical = 0;
          this.currentHorizontal = 0;
          GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.sectionTextPrefab, (Transform) this.keyRemapContainer);
          gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -this.verticalOffset * (float) num1);
          gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = "REBIND CONTROLLERS";
          this.keySlots.Add(gameObject2);
          flag = true;
        }
        else
        {
          ++this.currentVertical;
          if ((double) this.currentVertical > (double) this.maxVertical)
          {
            this.currentVertical = 0;
            ++this.currentHorizontal;
          }
        }
        vector2 = new Vector2(this.horizontalOffset * (float) this.currentHorizontal, -this.verticalOffset * (float) (this.currentVertical + num1));
      }
    }
  }

  private void OnEnable() => this.LoadKeybindsUI();
}
