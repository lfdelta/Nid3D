using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections;
using TeamUtility.IO;


[RequireComponent(typeof(Button))]
public class RebindInput : MonoBehaviour, IPointerDownHandler {
  
  public enum RebindType {Keyboard, GamepadButton, GamepadAxis}
  public Color _scanningState;

  public Text _keyDescription;
  public string _inputConfigName;
  public string _axisConfigName;
  public string _cancelButton;
  public float _timeout;
  public bool _changePositiveKey;
  public bool _changeAltKey;
  public bool _allowAnalogButton;
  [Range(0, AxisConfiguration.MaxJoysticks)]
  public int _joystick = 0;
  public RebindType _rebindType;

  private AxisConfiguration _axisConfig;
  private Color _normalState;
  private Button _button;
  private Image _image;
  private static string[] _axisNames = new string[] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };

  private void Awake() {
    _image = GetComponent<Image> ();
    _button = GetComponent<Button> ();
    ColorBlock clrs = _button.colors;
    _normalState = clrs.normalColor;
    _image.color = _normalState;
    clrs.pressedColor = _scanningState;
    _button.colors = clrs;

    InitializeAxisConfig();

    //  The axis config needs to be reinitialized because loading can invalidate
    //  the input configurations
    InputManager.Instance.Loaded += InitializeAxisConfig;
    InputManager.Instance.ConfigurationDirty += HandleConfigurationDirty;
  }

  private void OnDestroy() {
    if (InputManager.Instance != null) {
      InputManager.Instance.Loaded -= InitializeAxisConfig;
      InputManager.Instance.ConfigurationDirty -= HandleConfigurationDirty;
    }
  }

  // set up the keyDescription text
  private void InitializeAxisConfig() {
    _axisConfig = InputManager.GetAxisConfiguration(_inputConfigName, _axisConfigName);
    if (_axisConfig != null) {
      if (_rebindType == RebindType.Keyboard || _rebindType == RebindType.GamepadButton) {
        if (_changePositiveKey) {
          if (_changeAltKey)
            _keyDescription.text = _axisConfig.altPositive == KeyCode.None ? "" : _axisConfig.altPositive.ToString();
          else
            _keyDescription.text = _axisConfig.positive == KeyCode.None ? "" : _axisConfig.positive.ToString();
        } else {
          if (_changeAltKey)
            _keyDescription.text = _axisConfig.altNegative == KeyCode.None ? "" : _axisConfig.altNegative.ToString();
          else
            _keyDescription.text = _axisConfig.negative == KeyCode.None ? "" : _axisConfig.negative.ToString();
        }
      } else {
        _keyDescription.text = _axisNames[_axisConfig.axis];
      }
    } else {
      _keyDescription.text = "";
      Debug.LogError(string.Format(@"Input configuration '{0}' does not exist or axis '{1}' does not exist", _inputConfigName, _axisConfigName));
    }
  }

  private void HandleConfigurationDirty(string configName) {
    if (configName == _inputConfigName)
      InitializeAxisConfig();
  }

  public void OnPointerDown(PointerEventData data) {
    StartCoroutine(StartInputScanDelayed());
  }

  // if not already scanning, call the appropriate Handle..Scan function
  private IEnumerator StartInputScanDelayed() {
    yield return null;

    if (!InputManager.IsScanning && _axisConfig != null) {
      _image.color = _scanningState;
      _keyDescription.text = "...";

      ScanSettings settings;
      settings.joystick = _joystick;
      settings.cancelScanButton = _cancelButton;
      settings.timeout = _timeout;
      settings.userData = null;
      if (_rebindType == RebindType.GamepadAxis) {
        settings.scanFlags = ScanFlags.JoystickAxis;
        InputManager.StartScan(settings, HandleJoystickAxisScan);
      } else if (_rebindType == RebindType.GamepadButton) {
        settings.scanFlags = ScanFlags.JoystickButton;
        if (_allowAnalogButton) {
          settings.scanFlags = settings.scanFlags | ScanFlags.JoystickAxis;
        }
        InputManager.StartScan(settings, HandleJoystickButtonScan);
      } else {
        settings.scanFlags = ScanFlags.Key;
        InputManager.StartScan(settings, HandleKeyScan);
      }
    }
  }

  private bool HandleKeyScan(ScanResult result) {
    //  When you return false you tell the InputManager that it should keep scaning for other keys
    if (!IsKeyValid(result.key))
      return false;
    
    //  The key is KeyCode.None when the timeout has been reached or the scan has been canceled
    if (result.key != KeyCode.None) {
      //  If the key is KeyCode.Backspace clear the current binding
      result.key = (result.key == KeyCode.Backspace) ? KeyCode.None : result.key;
      if (_changePositiveKey) {
        if (_changeAltKey)
          _axisConfig.altPositive = result.key;
        else
          _axisConfig.positive = result.key;
      } else {
        if (_changeAltKey)
          _axisConfig.altNegative = result.key;
        else
          _axisConfig.negative = result.key;
      }
      _keyDescription.text = (result.key == KeyCode.None) ? "" : result.key.ToString();
    } else {
      KeyCode currentKey = GetCurrentKeyCode();
      _keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
    }

    _image.color = _normalState;
    return true;
  }

  private bool IsKeyValid(KeyCode key) {
    bool isValid = true;

    if (_rebindType == RebindType.Keyboard) {
      if ((int)key >= (int)KeyCode.JoystickButton0)
        isValid = false;
      else if (key == KeyCode.LeftApple || key == KeyCode.RightApple)
        isValid = false;
      else if (key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
        isValid = false;
    } else {
      isValid = false;
    }

    return isValid;
  }

  private bool HandleJoystickButtonScan(ScanResult result) {
    if (result.scanFlags == ScanFlags.JoystickButton) {
      //  When you return false you tell the InputManager that it should keep scaning for other keys
      if (!IsJoytickButtonValid(result.key))
        return false;

      //  The key is KeyCode.None when the timeout has been reached or the scan has been canceled
      if (result.key != KeyCode.None) {
        //  If the key is KeyCode.Backspace clear the current binding
        result.key = (result.key == KeyCode.Backspace) ? KeyCode.None : result.key;
        _axisConfig.type = InputType.Button;
        if (_changePositiveKey) {
          if (_changeAltKey)
            _axisConfig.altPositive = result.key;
          else
            _axisConfig.positive = result.key;
        } else {
          if (_changeAltKey)
            _axisConfig.altNegative = result.key;
          else
            _axisConfig.negative = result.key;
        }
        _keyDescription.text = (result.key == KeyCode.None) ? "" : result.key.ToString();
      } else {
        if (_axisConfig.type == InputType.Button) {
          KeyCode currentKey = GetCurrentKeyCode();
          _keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
        } else {
          _keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
        }
      }
      _image.color = _normalState;
    } else {
      //  The axis is negative when the timeout has been reached or the scan has been canceled
      if (result.joystickAxis >= 0) {
        _axisConfig.type = InputType.AnalogButton;
        _axisConfig.invert = result.joystickAxisValue < 0.0f;
        _axisConfig.SetAnalogButton(_joystick, result.joystickAxis);
        _keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
      } else {
        if (_axisConfig.type == InputType.AnalogButton) {
          _keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
        } else {
          KeyCode currentKey = GetCurrentKeyCode();
          _keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
        }
      }
      _image.color = _normalState;
    }

    return true;
  }

  private bool IsJoytickButtonValid(KeyCode key) {
    bool isValid = true;

    if (_rebindType == RebindType.GamepadButton) {
      //  Allow KeyCode.None to pass because it means that the scan has been canceled or the timeout has been reached
      //  Allow KeyCode.Backspace to pass so it can clear the current binding
      if ((int)key < (int)KeyCode.JoystickButton0 && key != KeyCode.None && key != KeyCode.Backspace)
        isValid = false;
    } else {
      isValid = false;
    }

    return isValid;
  }

  private bool HandleJoystickAxisScan(ScanResult result) {
    //  The axis is negative when the timeout has been reached or the scan has been canceled
    if (result.joystickAxis >= 0)
      _axisConfig.SetAnalogAxis(_joystick, result.joystickAxis);

    _image.color = _normalState;
    _keyDescription.text = _axisNames[_axisConfig.axis];
    return true;
  }

  private KeyCode GetCurrentKeyCode()  {
    if (_rebindType == RebindType.GamepadAxis)
      return KeyCode.None;

    if (_changePositiveKey) {
      if (_changeAltKey)
        return _axisConfig.altPositive;
      else
        return _axisConfig.positive;
    } else {
      if (_changeAltKey)
        return _axisConfig.altNegative;
      else
        return _axisConfig.negative;
    }
  }


  // onPress: if (rebindable) dostuff;
  // check: if (analog input && axisConfigName == horiz or vert && positive == false) SetRebindable(false);
  // gamepad toggle button: call ToggleRebindable for negative axis rebindings in the panel
  //   change inputconfigname and rebindtype for all buttons

  /* void SetRebindable (bool b) {
    _button.interactable = b;
    private bool _rebindable = b;
  }*/
}