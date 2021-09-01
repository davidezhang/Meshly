using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Xamin
{
    /// <summary>
    /// This class is the core of tasty pie menu.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CircleSelector : MonoBehaviour
    {
        [Range(2, 10)] private int buttonCount;
        private int startButCount;

        [Header("Customization")] public Color AccentColor = Color.red;
        public Color DisabledColor = Color.gray, BackgroundColor = Color.white;
        [Space(10)] public bool UseSeparators = true;
        [SerializeField] private GameObject separatorPrefab;

        [Header("Animations")] [Range(0.0001f, 1)]
        public float LerpAmount = .145f;

        public AnimationType OpenAnimation, CloseAnimation;
        public float Size = 1f;
        private Image _cursor, _background;
        private float _desiredFill;
        float radius = 120f;

        [Header("Sound")] public AudioClip SegmentChangedSound;
        public AudioClip SegmentClickedSound;

        [Header("Interaction")] public List<GameObject> Buttons = new List<GameObject>();
        public ButtonSource buttonSource;
        private readonly List<Xamin.Button> buttonsInstances = new List<Xamin.Button>();
        private Vector2 _menuCenter;
		public bool RaiseOnSelection;

        private GameObject _selectedSegment;
        private float _audioCoolDown;
        private bool _previousUseSeparators;

        public bool flip = true;

        [HideInInspector]
        public GameObject SelectedSegment
        {
            get { return _selectedSegment; }
            set
            {
                if (value == null) return;
                //Debug.Log(value.name);
                if (value == SelectedSegment) return;
                _selectedSegment = value;
                if (SegmentChangedSound == null || !(_audioCoolDown <= 0)) return;
                localAudioSource.PlayOneShot(SegmentChangedSound);
                _audioCoolDown = .05f;
            }
        }

        public bool selectOnlyOnHover;
        public float pieThickness = 85;

        public bool snap, tiltTowardsMouse;
        public float tiltAmount = 15;
        private bool opened;


        public enum ControlType
        {
            mouseAndTouch,
            gamepad,
            customVector
        }

        /// <summary>
        /// Button source
        /// <para>use prefabs in a menu where you want to add or remove elements at runtime</para>
        /// <para>use scene if you want a static menu that you can only modify on the editor</para>
        /// </summary>
        public enum ButtonSource
        {
            prefabs,
            scene
        }

        [Header("Controls")] public string activationButton = "Fire1";
        public ControlType controlType;
        public string gamepadAxisX, gamepadAxisY;
        public Vector2 CustomInputVector;


        public enum AnimationType
        {
            zoomIn,
            zoomOut
        }

        private AudioSource localAudioSource;

        private Dictionary<GameObject, Button> instancedButtons;

        /// <summary>
        /// Rearranges the buttons, can be called multiple times
        /// </summary>
        void Start()
        {
            instancedButtons = new Dictionary<GameObject, Button>();
            transform.localScale = Vector3.zero;
            Assert.IsNotNull(transform.Find("Cursor"));
            _cursor = transform.Find("Cursor").GetComponent<Image>();
            Assert.IsNotNull(transform.Find("Background"));
            _background = transform.Find("Background").GetComponent<Image>();
            buttonCount = Buttons.Count;
            foreach (Xamin.Button b in buttonsInstances)
                Destroy(b.gameObject);
            buttonsInstances.Clear();
            if (buttonCount > 0 && buttonCount < 11)
            {
                #region Arrange Buttons

                startButCount = buttonCount;
                _desiredFill = 1f / (float) buttonCount;
                float fillRadius = _desiredFill * 360f;
                float previousRotation = 0;
                foreach (Transform sep in transform.Find("Separators"))
                    Destroy(sep.gameObject);
                for (int i = 0; i < buttonCount; i++)
                {
                    //TIP   y=sin(angle)
                    //      x=cos(angle)
                    GameObject b;
                    if (buttonSource == ButtonSource.prefabs)
                        b = Instantiate(Buttons[i], Vector2.zero, transform.rotation) as GameObject;
                    else
                        b = Buttons[i];
                    b.transform.SetParent(transform.Find("Buttons"));
                    float bRot = previousRotation + fillRadius / 2;
                    previousRotation = bRot + fillRadius / 2;
                    GameObject separator =
                        Instantiate(separatorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    separator.transform.SetParent(transform.Find("Separators"));
                    separator.transform.localScale = Vector3.one;
                    separator.transform.localPosition = Vector3.zero;
                    separator.transform.localRotation = Quaternion.Euler(0, 0, previousRotation);

                    b.transform.localPosition = new Vector2(radius * Mathf.Cos((bRot - 90) * Mathf.Deg2Rad),
                        -radius * Mathf.Sin((bRot - 90) * Mathf.Deg2Rad));
                    b.transform.localScale = Vector3.one;
                    if (bRot > 360)
                        bRot -= 360;
                    b.name = bRot.ToString();
                    if (b.GetComponent<Button>())
                    {
                        instancedButtons[b] = b.GetComponent<Button>();
                        var but = instancedButtons[b];
                        instancedButtons[b] = but;
                        if (but.useCustomColor)
                            but.SetColor(but.customColor);
                        else
                            but.SetColor(but.useCustomColor ? but.customColor : AccentColor);
                    }
                    else
                        b.GetComponent<Image>().color = DisabledColor;

                    if (buttonSource == ButtonSource.prefabs)
                        buttonsInstances.Add(b.GetComponent<Button>());
                }

                #endregion
            }

            localAudioSource = GetComponent<AudioSource>();
            if (buttonsInstances.Count != 0)
                SelectedSegment = buttonsInstances[buttonsInstances.Count - 1].gameObject;
        }

        /// <summary>
        /// Open the menu at the center of the screen
        /// </summary>
        public void Open()
        {
            _menuCenter = new Vector2((float) Screen.width / 2f, (float) Screen.height / 2f);
            opened = true;
            transform.localScale = (OpenAnimation == AnimationType.zoomIn) ? Vector3.zero : Vector3.one * 10;
        }

        /// <summary>
        /// Open the menu at a desired location
        /// </summary>
        /// <param name="origin">Mouse or Touch screen point</param>
        public void Open(Vector2 origin)
        {
            Open();
            _menuCenter = origin;

            Vector2 relativeCenter = new Vector2(_menuCenter.x - Screen.width / 2f, _menuCenter.y - Screen.height / 2f);
            transform.localPosition = relativeCenter;
        }

        public bool isOpen()
        {
            return opened;
        }

        /// <summary>
        /// Closes the menu
        /// </summary>
        public void Close()
        {
            opened = false;
        }

        /// <summary>
        /// Useful for changing a button at runtime
        /// </summary>
        /// <param name="id">The button Id</param>
        /// <returns>The button with the specified id</returns>
        public Xamin.Button GetButtonWithId(string id)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                Xamin.Button b = (buttonSource == ButtonSource.prefabs)
                    ? buttonsInstances[i]
                    : Buttons[i].GetComponent<Xamin.Button>();
                if (b.id == id)
                    return b;
            }

            return null;
        }

        public float zRotation = 180;
        public bool rotateButtons = false;

        void ChangeSeparatorsState()
        {
            if (!transform.Find("Separators"))
            {
                Debug.LogError("Can't find Separators");
                return;
            }

            transform.Find("Separators").gameObject.SetActive(UseSeparators);
        }

        void Update()
        {
            if (_audioCoolDown > 0)
                _audioCoolDown -= Time.deltaTime;
            if (opened)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(Size, Size, Size), .2f);
                _background.color = BackgroundColor;
                if (UseSeparators != _previousUseSeparators)
                    ChangeSeparatorsState();
                if (transform.localScale.x >= Size - .2f)
                {
                    #region Check if should re-arrange

                    buttonCount = Buttons.Count;
                    if (startButCount != buttonCount && buttonSource == ButtonSource.prefabs)
                    {
                        Start();
                        return;
                    }

                    #endregion

                    #region Update the mouse rotation and extimate cursor rotation

                    _cursor.fillAmount = Mathf.Lerp(_cursor.fillAmount, _desiredFill, .2f);
                    //Cursor placement
                    Vector3 screenBounds = new Vector3(_menuCenter.x, _menuCenter.y, 0f);
                    Vector2 vector = (UnityEngine.Input.mousePosition - screenBounds);
//                    vector.y = vector.y*((flip) ? -1 : 1);
                    if (tiltTowardsMouse)
                    {
                        float x = vector.x / screenBounds.x, y = vector.y / screenBounds.y;
                        transform.localRotation = Quaternion.Slerp(transform.localRotation,
                            Quaternion.Euler((Vector3) (new Vector2(y, -x) * -tiltAmount) +
                                             Vector3.forward * zRotation), LerpAmount);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.Euler(Vector3.forward * zRotation);
                    }

                    float mouseRotation = zRotation + 57.29578f *
                        (controlType == ControlType.mouseAndTouch
                            ? Mathf.Atan2(vector.x, vector.y) 
                            : controlType == ControlType.gamepad
                                ? Mathf.Atan2(Input.GetAxis(gamepadAxisX), Input.GetAxis(gamepadAxisY))
                                : Mathf.Atan2(CustomInputVector.x, CustomInputVector.y));

                    if (mouseRotation < 0f)
                        mouseRotation += 360f;
                    float cursorRotation = -(mouseRotation - _cursor.fillAmount * 360f / 2f) + zRotation;

                    #endregion

                    #region Find and color the selected button

                    float mouseDistanceFromCenter = Vector2.Distance(_menuCenter, Input.mousePosition);
                    if (selectOnlyOnHover && controlType == ControlType.mouseAndTouch &&
                        mouseDistanceFromCenter > pieThickness ||
                        (selectOnlyOnHover && controlType == ControlType.gamepad &&
                         (Mathf.Abs(Input.GetAxisRaw(gamepadAxisX) + Mathf.Abs(Input.GetAxisRaw(gamepadAxisY)))) !=
                         0) ||
                        !selectOnlyOnHover)
                    {
                        _cursor.enabled = true;

                        float difference = float.MaxValue;
                        GameObject nearest = null;
                        for (int i = 0; i < buttonCount; i++)
                        {
                            GameObject b;
                            if (buttonSource == ButtonSource.prefabs)
                                b = buttonsInstances[i].gameObject;
                            else
                                b = Buttons[i];
                            b.transform.localScale = Vector3.one;
                            float rotation = System.Convert.ToSingle(b.name);
                            if (Mathf.Abs(rotation - mouseRotation) < difference)
                            {
                                nearest = b;
                                difference = Mathf.Abs(rotation - mouseRotation);
                            }

                            if (rotateButtons)
                                b.transform.localEulerAngles = new Vector3(0, 0, -zRotation);
                        }

                        SelectedSegment = nearest;

                        if (snap)
                            cursorRotation = -(System.Convert.ToSingle(SelectedSegment.name) -
                                               _cursor.fillAmount * 360f / 2f);
                        _cursor.transform.localRotation = Quaternion.Slerp(_cursor.transform.localRotation,
                            Quaternion.Euler(0, 0, cursorRotation), LerpAmount);

                        instancedButtons[SelectedSegment].SetColor(
                            Color.Lerp(instancedButtons[SelectedSegment].currentColor, BackgroundColor, LerpAmount));

                        for (int i = 0; i < Buttons.Count; i++)
                        {
                            Button b = (buttonSource == ButtonSource.prefabs)
                                ? buttonsInstances[i]
                                : instancedButtons[Buttons[i]];
                            if (b.gameObject != SelectedSegment)
                            {
                                if (b.unlocked)
                                    b.SetColor(Color.Lerp(b.currentColor,
                                        b.useCustomColor ? b.customColor : AccentColor, LerpAmount));
                                else
                                    b.SetColor(Color.Lerp(b.currentColor,
                                        DisabledColor, LerpAmount));
                            }
                        }

                        try
                        {
                            if (SelectedSegment && instancedButtons[SelectedSegment].unlocked)
                            {
                                _cursor.color = Color.Lerp(_cursor.color,
                                    instancedButtons[SelectedSegment].useCustomColor
                                        ? instancedButtons[SelectedSegment].customColor
                                        : AccentColor, LerpAmount);
                            }
                            else
                                _cursor.color = Color.Lerp(_cursor.color, DisabledColor, LerpAmount);
                        }
                        catch
                        {
                        }
                    }
                    else if (_cursor.enabled && SelectedSegment)
                    {
                        _cursor.enabled = false;
                        if (instancedButtons[SelectedSegment].unlocked)
                            instancedButtons[SelectedSegment].SetColor(instancedButtons[SelectedSegment].useCustomColor
                                ? instancedButtons[SelectedSegment].customColor
                                : AccentColor);
                        else
                            instancedButtons[SelectedSegment].SetColor(DisabledColor);

                        for (int i = 0; i < Buttons.Count; i++)
                        {
                            Button b = (buttonSource == ButtonSource.prefabs)
                                ? buttonsInstances[i]
                                : instancedButtons[Buttons[i]];
                            if (b.gameObject != SelectedSegment)
                            {
                                if (b.unlocked)
                                    b.SetColor(instancedButtons[SelectedSegment].useCustomColor
                                        ? instancedButtons[SelectedSegment].customColor
                                        : AccentColor);
                                else
                                    b.SetColor(DisabledColor);
                            }
                        }
                    }

                    //if (_cursor.isActiveAndEnabled)
                    //    CheckForInput();
                    //else if (Input.GetButtonUp(activationButton))
                    //    Close();

                    #endregion
                }

                _previousUseSeparators = UseSeparators;
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale,
                    (CloseAnimation == AnimationType.zoomIn) ? Vector3.zero : Vector3.one * 10, .05f);
                _cursor.color = Color.Lerp(_cursor.color, Color.clear, LerpAmount / 3f);
                _background.color = Color.Lerp(_background.color, Color.clear, LerpAmount / 3f);
            }
        }

        void CheckForInput()
        {
            #region Call the selected button action

            if (Input.GetButton(activationButton))
            {
                _cursor.rectTransform.localPosition = Vector3.Lerp(_cursor.rectTransform.localPosition,
                    new Vector3(0, 0, RaiseOnSelection ? -10 : 0), LerpAmount);
                if (instancedButtons[SelectedSegment].unlocked)
                {
                    SelectedSegment.transform.localScale = new Vector2(.8f, .8f);
                }
            }
            else
                _cursor.rectTransform.localPosition = Vector3.Lerp(_cursor.rectTransform.localPosition,
                    Vector3.zero, LerpAmount);

            if (Input.GetButtonUp(activationButton) && SelectedSegment)
            {
                if (instancedButtons[SelectedSegment].unlocked)
                {
                    instancedButtons[SelectedSegment].ExecuteAction();
                    localAudioSource.PlayOneShot(SegmentClickedSound);
                }

                Close();
            }

            #endregion
        }

        public void confirmSelect()
        {
            instancedButtons[SelectedSegment].ExecuteAction();
            localAudioSource.PlayOneShot(SegmentClickedSound);
        }
    }
}