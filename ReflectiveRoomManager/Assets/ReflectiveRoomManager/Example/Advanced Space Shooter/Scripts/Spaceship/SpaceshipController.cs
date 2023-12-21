using TMPro;
using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.NETWORK.Room.Service;
using Random = UnityEngine.Random;

namespace Examples.SpaceShooter.Spaceship
{
    using Game;
    using Utilities;
    using PostProcess;

    [RequireComponent(typeof(NetworkIdentity))]
    public sealed class SpaceshipController : NetworkBehaviour
    {
        private const float IdleCameraDistanceSmooth = 0.85f;
        private static readonly Vector3[] RotationDirections = { Vector3.right, Vector3.up, Vector3.forward };
        public Transform CachedTransform { get; private set; }

        public Vector3 CameraOffsetVector => new(0.0f, Mathf.Sin(m_camera.Angle * Mathf.Deg2Rad) * m_camera.Offset,
            -m_camera.Offset);

        public float CurrentSpeed => Mathf.Lerp(m_spaceship.SpeedRange.x, m_spaceship.SpeedRange.y, SpeedFactor);

        public Vector4 RawInput { get; private set; }
        public Vector4 SmoothedInput { get; private set; }

        public float SpeedFactor => m_spaceship.AccelerationCurve.Evaluate(SmoothedInput.w);

        private Transform m_cachedCameraTransform;

        public bool IsEnableControl = true;
        public bool IsRunningMotor = true;

        [SerializeField] public GameObject _winContent;
        [SerializeField] public Button _exitButton;
        
        [field:SerializeField] public PostProcessManager PostProcessManager { get; private set; }
        [field:SerializeField] public FuelSystem FuelSystem { get; private set; }
        [field:SerializeField] public SpawnSystem SpawnSystem { get; private set; }
        
        [field:SerializeField] public DeadManager DeadManager { get; private set; }
        
        [field: SerializeField] public SpaceshipShooter Shooter { get; private set; }

        [field: SerializeField] public Damageable Damageable { get; private set; }
        
        [field: SerializeField] public Health Health { get; private set; }

        [SerializeField] private LayerMask _environmentLayer;
        
        [SerializeField] private GameObject _usernameCanvas;
        [SerializeField] private TMP_Text _usernameText;

        [SerializeField] private GameObject _explosionParticle;
        [SerializeField] private float _explosionScale = 10;
        
        [Header("Network options.")] [SerializeField]
        private GameObject[] NetworkObjects;

        //[SerializeField, Tooltip("Camera options.")]
        public CameraSettings m_camera = new()
        {
            Angle = 18.0f,
            Offset = 44.0f,
            PositionSmooth = 10.0f,
            RotationSmooth = 5.0f,
            OnRollCompensationFactor = 0.5f,
            LookAtPointOffset = new CameraLookAtPointOffsetSettings
            {
                OnIdle = new Vector2(0.0f, 10.0f),
                Smooth = new Vector2(30.0f, 30.0f),
                OnMaxSpeed = new Vector2(20.0f, -20.0f),
                OnTurn = new Vector2(30.0f, -30.0f)
            },
            normalCursor = null,
            aimingCursor = null,
            shakeAmount = 0.75f,
            shakeDuration = 0.02f
        };

        private float m_idleCameraDistance;
        private Quaternion m_initialAvatarRotation;
        private float m_initialCameraFOV;

        [SerializeField, Tooltip("Input options.")]
        private InputSettings m_input = new()
        {
            Mode = InputMode.KeyboardAndMouse,
            Response = new Vector4(6.0f, 6.0f, 6.0f, 0.75f),
            Keyboard = new KeyboardSettings
            {
                Sensitivity = 1.5f,
                SensitivityOnMaxSpeed = 1.0f
            },
            Mouse = new MouseSettings
            {
                ActiveArea = new Vector2(450.0f, 300.0f),
                MovementThreshold = 75.0f,
                Sensitivity = 1.0f,
                SensitivityOnMaxSpeed = 0.85f
            }
        };

        private Vector2 m_lookAtPointOffset;

        [SerializeField, Tooltip("Spaceship options.")]
        public SpaceshipSettings m_spaceship = new()
        {
            AccelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f),
            BankAngleSmooth = 2.5f,
            Maneuverability = new Vector3(75.0f, 75.0f, -50.0f),
            MaxBankAngleOnTurnAxis = 45.0f,
            MaxBankAngleOnTurnLeftRight = 60f,
            MaxBankAngleOnTurnUpDown = 60f,
            MaxBankAngleSideways = 30f,
            SidewaysSpeed = 25f,
            SpeedRange = new Vector2(30.0f, 600.0f),
        };

        [SerializeField, Tooltip("Shooting options.")]
        public ShootingSettings m_shooting = new()
        {
            mode2D = false,
            bulletSettings = new BulletSettings
            {
                BulletBarrels = new List<GameObject>(),
                Bullet = null,
                BulletSpeed = 300f,
                BulletFireDelay = 0.15f,
                BulletDamage = 15f,
                BulletLifetime = 7f,
                TargetDistance = 300f
            },
        };

        //singleton
        public static SpaceshipController instance;

        #region Client Callbacks

        public bool IsInitialized;

        [SyncVar(hook = nameof(OnChangedUsername))]
        public string Username;

        private void OnChangedUsername(string _, string newValue)
        {
            _usernameText.text = newValue;
        }

        [Command]
        private void CMD_SetUsername(string userName)
        {
            Username = userName;
        }
        
        public override void OnStartClient()
        {
            Init();

            if (!isOwned) return;

            var username = PlayerPrefs.HasKey("username") ? PlayerPrefs.GetString("username") : "Undefined";
            
            CMD_SetUsername(username);
            
            _usernameCanvas.SetActive(false);
            
            CMD_AddPlayer();
        }

        [Command(requiresAuthority = false)]
        private void CMD_AddPlayer()
        {
            var leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();
            leaderboardManager.AddPlayer(Username);
        }
        
        public override void OnStopClient()
        {
            RoomClient.ExitRoom();
        }

        #endregion

        [TargetRpc]
        public void RPC_OpenWinPanel()
        {
            _winContent.SetActive(true);
            _exitButton.onClick.AddListener(RoomClient.ExitRoom);
        }
        
        public void Init()
        {
            foreach (var obj in NetworkObjects)
            {
                obj.SetActive(isOwned);
            }

            if (isOwned)
            {
                if (instance == null)
                {
                    instance = this;
                }
                else
                {
                    Debug.LogError("Singleton pattern violated! Two player controlled spaceships present in the scene");
                }
            }

            RawInput = Vector4.zero;
            SmoothedInput = Vector4.zero;
            CachedTransform = transform;
            m_cachedCameraTransform = m_camera.TargetCamera.transform;
            m_idleCameraDistance = CameraOffsetVector.magnitude;
            m_initialAvatarRotation = m_spaceship.Avatar.localRotation;
            m_initialCameraFOV = m_camera.TargetCamera.fieldOfView;
            m_lookAtPointOffset = m_camera.LookAtPointOffset.OnIdle;

            m_cachedCameraTransform.position = CachedTransform.position + CameraOffsetVector;

            if (!isOwned) return;

            //Health.OnDeath += CMD_OnDeath;

            if (m_camera.normalCursor != null)
            {
                Cursor.SetCursor(m_camera.normalCursor,
                    new Vector2(m_camera.normalCursor.width * 0.5f, m_camera.normalCursor.height * 0.5f),
                    CursorMode.Auto);
            }

            IsInitialized = true;
        }

        private Coroutine shooting;
        private bool isShooting;

        private Coroutine firing;
        private bool isFiring;

        //global bullet barrel variable
        private int b;

        private float _fireDelayTimer;
        
        [ServerCallback]
        public void OnDeath()
        {
            StartCoroutine(OnDeathCoroutine());
        }

        [ServerCallback]
        private IEnumerator OnDeathCoroutine()
        {
            var gameManager = gameObject.RoomContainer().GetSingleton<GameManager>();
            
            var modType = gameManager.GetModType();
            
            DeadManager.ShowGameOver(modType);

            SetShipActiveState(false);

            OnExplosion();

            if (modType == ModType.ShrinkingArea) yield break;
            
            yield return new WaitForSeconds(3f);
            
            SetShipActiveState(true);
            
            DeadManager.CloseGameOver();

            Health.Reset();
            
            ResetFuel();
            
            SpawnSystem?.SpawnPlayer();
        }

        [ClientRpc]
        private void SetShipActiveState(bool state)
        {
            transform.GetChild(0).gameObject.SetActive(state);
        }

        [ClientRpc]
        private void OnExplosion() => StartCoroutine(OnExplosionCor());

        [ClientRpc]
        private void ResetFuel() => FuelSystem.ResetFuel();
        
        private IEnumerator OnExplosionCor()
        {
            var particle = Instantiate(_explosionParticle, transform.position, Quaternion.identity);
            particle.transform.localScale *= _explosionScale;

            yield return new WaitForSeconds(2f);
            
            particle.Destroy();
        }

        private PhysicsScene _physicsScene;

        [ClientCallback]
        private void Start()
        {
            _physicsScene = gameObject.scene.GetPhysicsScene();
        }

        [ClientCallback]
        private void LateUpdate()
        {
            if (!isOwned || !IsEnableControl || !IsRunningMotor || Health.IsDead) return;

            Ray ray;
            if (m_camera.TargetCamera.targetTexture == null)
            {
                ray = m_camera.TargetCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                //EVERYTHING MUST BE A FLOAT
                ray = m_camera.TargetCamera.ScreenPointToRay(Input.mousePosition /
                                                             (Screen.height /
                                                              (float)m_camera.TargetCamera.pixelHeight));
            }
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if (!isOwned || !IsInitialized || !IsRunningMotor) return;

            UpdateCamera();
            UpdateOrientationAndPosition();
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned || !IsEnableControl || !IsInitialized || Health.IsDead) return;

            if (!IsRunningMotor)
            {
                RawInput = Vector4.zero;
                SmoothedInput = Vector4.zero;

                return;
            }
            
            UpdateInput();
        }

        private void UpdateCamera()
        {
            Vector2 focalPointOnMoveOffset = Vector2.Lerp(m_camera.LookAtPointOffset.OnTurn,
                m_camera.LookAtPointOffset.OnMaxSpeed, SpeedFactor);

            m_lookAtPointOffset.x = Mathf.Lerp(
                m_lookAtPointOffset.x,
                Mathf.Lerp(
                    m_camera.LookAtPointOffset.OnIdle.x,
                    focalPointOnMoveOffset.x * Mathf.Sign(SmoothedInput.y),
                    Mathf.Abs(SmoothedInput.y)),
                m_camera.LookAtPointOffset.Smooth.x * Time.deltaTime);

            m_lookAtPointOffset.y = Mathf.Lerp(
                m_lookAtPointOffset.y,
                Mathf.Lerp(
                    m_camera.LookAtPointOffset.OnIdle.y,
                    focalPointOnMoveOffset.y * Mathf.Sign(SmoothedInput.x),
                    Mathf.Abs(SmoothedInput.x)),
                m_camera.LookAtPointOffset.Smooth.y * Time.deltaTime);

            var lookTargetPosition = CachedTransform.position + CachedTransform.right * m_lookAtPointOffset.x +
                                         CachedTransform.up * m_lookAtPointOffset.y;

            var lookTargetUpVector = (CachedTransform.up + CachedTransform.right * (SmoothedInput.z * m_camera.OnRollCompensationFactor)).normalized;

            var targetCameraRotation = Quaternion.LookRotation(lookTargetPosition -
                                                                      m_cachedCameraTransform.position,
                lookTargetUpVector);

            m_cachedCameraTransform.rotation = Quaternion.Slerp(m_cachedCameraTransform.rotation,
                targetCameraRotation, m_camera.RotationSmooth * Time.deltaTime);

            var cameraOffset = CachedTransform.TransformDirection(CameraOffsetVector);

            m_cachedCameraTransform.position = Vector3.Lerp(m_cachedCameraTransform.position,
                CachedTransform.position + cameraOffset, m_camera.PositionSmooth * Time.deltaTime);

            var idleCameraDistance = cameraOffset.magnitude + (cameraOffset.normalized * (m_spaceship.SpeedRange.x * Time.deltaTime) / m_camera.PositionSmooth).magnitude;

            m_idleCameraDistance = Mathf.Lerp(m_idleCameraDistance, idleCameraDistance,
                IdleCameraDistanceSmooth * Time.deltaTime);
            
            var baseFrustumHeight =
                2.0f * m_idleCameraDistance * Mathf.Tan(m_initialCameraFOV * 0.5f * Mathf.Deg2Rad);
            m_camera.TargetCamera.fieldOfView = 2.0f * Mathf.Atan(baseFrustumHeight * 0.5f / Vector3.Distance(
                CachedTransform.position, m_cachedCameraTransform.position)) * Mathf.Rad2Deg;
        }

        private void UpdateInput()
        {
            var currentKeyboardSensitivity = Mathf.Lerp(m_input.Keyboard.Sensitivity,
                m_input.Keyboard.SensitivityOnMaxSpeed, SpeedFactor);

            //Calc raw input.
            var currentRawInput = Vector4.zero;
            switch (m_input.Mode)
            {
                case InputMode.Keyboard:
                    currentRawInput.x = Input.GetAxis(m_input.Keyboard.InputNames.AxisX) * currentKeyboardSensitivity;
                    currentRawInput.y = Input.GetAxis(m_input.Keyboard.InputNames.AxisY) * currentKeyboardSensitivity;
                    break;

                case InputMode.KeyboardAndMouse:
                    float currentMouseSensitivity = Mathf.Lerp(m_input.Mouse.Sensitivity,
                        m_input.Mouse.SensitivityOnMaxSpeed, SpeedFactor);

                    Vector2 mouseOffsetFromScreenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f) -
                                                          (Vector2)Input.mousePosition;

                    if (Mathf.Abs(mouseOffsetFromScreenCenter.y) > m_input.Mouse.MovementThreshold)
                    {
                        float verticalOffsetFromCenter = mouseOffsetFromScreenCenter.y -
                                                         Mathf.Sign(mouseOffsetFromScreenCenter.y) *
                                                         m_input.Mouse.MovementThreshold;

                        currentRawInput.x = Mathf.Clamp(verticalOffsetFromCenter / (m_input.Mouse.ActiveArea.y -
                            m_input.Mouse.MovementThreshold), -1.0f, 1.0f) * currentMouseSensitivity;
                    }

                    if (Mathf.Abs(mouseOffsetFromScreenCenter.x) > m_input.Mouse.MovementThreshold)
                    {
                        float horizontalOffsetFromCenter = mouseOffsetFromScreenCenter.x -
                                                           Mathf.Sign(mouseOffsetFromScreenCenter.x) *
                                                           m_input.Mouse.MovementThreshold;

                        currentRawInput.y = -Mathf.Clamp(horizontalOffsetFromCenter / (m_input.Mouse.ActiveArea.x -
                            m_input.Mouse.MovementThreshold), -1.0f, 1.0f) * currentMouseSensitivity;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!m_shooting.mode2D)
            {
                currentRawInput.z = Input.GetAxis(m_input.Keyboard.InputNames.AxisZ) * currentKeyboardSensitivity;
            }

            currentRawInput.w = Input.GetButton(m_input.Keyboard.InputNames.Throttle) ? 1.0f : 0.0f;

            //Calc smoothed input.
            Vector4 currentSmoothedInput = Vector4.zero;
            for (int i = 0; i < 4; ++i)
            {
                currentSmoothedInput[i] = Mathf.Lerp(SmoothedInput[i], currentRawInput[i],
                    m_input.Response[i] * Time.deltaTime);
            }

            RawInput = currentRawInput;
            SmoothedInput = currentSmoothedInput;
        }

        private void UpdateOrientationAndPosition()
        {
            for (int i = 0; i < 3; ++i)
            {
                CachedTransform.localRotation *= Quaternion.AngleAxis(SmoothedInput[i] *
                                                                      m_spaceship.Maneuverability[i] * Time.deltaTime,
                    RotationDirections[i]);
            }

            if (!Health.IsDead)
            {
                if (Input.GetAxis("Stop") == 0f && Input.GetKey(KeyCode.W))
                {
                    var speed = CurrentSpeed * Time.deltaTime;
                    var dir = CachedTransform.forward * speed;
                    
                    if(CanMove(dir, speed))
                        CachedTransform.localPosition += dir;
                }
            }

            //left right
            m_spaceship.Avatar.localRotation = Quaternion.Slerp(
                m_spaceship.Avatar.localRotation,
                m_initialAvatarRotation *
                Quaternion.AngleAxis(SmoothedInput.y * m_spaceship.MaxBankAngleOnTurnLeftRight, Vector3.up),
                m_spaceship.BankAngleSmooth * Time.deltaTime);

            //around axis
            m_spaceship.Avatar.localRotation = Quaternion.Slerp(
                m_spaceship.Avatar.localRotation,
                m_initialAvatarRotation * Quaternion.AngleAxis(-SmoothedInput.y * m_spaceship.MaxBankAngleOnTurnAxis,
                    Vector3.forward),
                m_spaceship.BankAngleSmooth * Time.deltaTime);

            //up and down
            m_spaceship.Avatar.localRotation = Quaternion.Slerp(
                m_spaceship.Avatar.localRotation,
                m_initialAvatarRotation * Quaternion.AngleAxis(SmoothedInput.x * m_spaceship.MaxBankAngleOnTurnUpDown,
                    Vector3.right),
                m_spaceship.BankAngleSmooth * Time.deltaTime);

            if (Health.IsDead) return;
            
            if (Input.GetAxis("Sideways") != 0f && !m_shooting.mode2D)
            {
                var speed = (Time.deltaTime * m_spaceship.SidewaysSpeed);
                var dir = CachedTransform.right * Input.GetAxis("Sideways");

                if (!CanMove(dir, speed)) return;

                CachedTransform.localPosition += dir * speed;
                m_spaceship.Avatar.localRotation = Quaternion.Slerp(
                    m_spaceship.Avatar.localRotation,
                    m_initialAvatarRotation *
                    Quaternion.AngleAxis(-Input.GetAxis("Sideways") * m_spaceship.MaxBankAngleSideways,
                        Vector3.forward),
                    m_spaceship.BankAngleSmooth * Time.deltaTime);
            }
        }

        private bool CanMove(Vector3 dir, float distance)
        {
            var halfExtents = new Vector3(3, 1, 3);
            
            DrawUtilities.DrawBoxCastBox(transform.position, halfExtents, transform.rotation, dir, distance, Color.cyan);
            
            return !_physicsScene.BoxCast(transform.position, halfExtents, dir, out _, transform.rotation, distance,
                _environmentLayer);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            IsEnableControl = hasFocus;

            RawInput = Vector4.zero;
            SmoothedInput = Vector4.zero;
        }

        private Coroutine _shakeCor;

        private float shakePercentage;
        private float startAmount;
        private float startDuration;

        private bool isRunning;

        [TargetRpc]
        public void RPC_Shake()
        {
            if (isRunning)
            {
                m_camera.TargetCamera.transform.localRotation = Quaternion.identity;
                if (_shakeCor != null) StopCoroutine(_shakeCor);
            }

            _shakeCor = StartCoroutine(Shaking());
        }

        private IEnumerator Shaking()
        {
            isRunning = true;
            startAmount = m_camera.shakeAmount;
            startDuration = m_camera.shakeDuration;

            while (m_camera.shakeDuration > 0.01f)
            {
                Vector3 rotationAmount = Random.insideUnitSphere * m_camera.shakeAmount;
                rotationAmount += m_camera.TargetCamera.transform.localEulerAngles;

                shakePercentage = m_camera.shakeDuration / startDuration;

                m_camera.shakeAmount = startAmount * shakePercentage;
                m_camera.shakeDuration = Mathf.Lerp(m_camera.shakeDuration, 0, Time.deltaTime);

                m_camera.TargetCamera.transform.localRotation = Quaternion.Euler(rotationAmount);

                yield return null;
            }

            m_camera.shakeAmount = startAmount;
            m_camera.shakeDuration = startDuration;
            isRunning = false;
        }

        [Serializable]
        public struct CameraLookAtPointOffsetSettings
        {
            [Tooltip(
                "Offset of the look-at point (relative to the spaceship) when flying straight with a minimum speed.")]
            public Vector2 OnIdle;

            [Tooltip(
                "Offset of the look-at point (relative to the spaceship) when flying or turning with a maximum speed.")]
            public Vector2 OnMaxSpeed;

            [Tooltip("Offset of the look-at point (relative to the spaceship) when turning with a minimum speed.")]
            public Vector2 OnTurn;

            [Tooltip("How fast the look-at point interpolates to the desired value. Higher = faster.")]
            public Vector2 Smooth;
        }

        [Serializable]
        public struct CameraSettings
        {
            [Tooltip("Angle of the camera. 0 = behind, 90 = top-down.")]
            public float Angle;

            [Tooltip("Look-at point options.")] public CameraLookAtPointOffsetSettings LookAtPointOffset;

            [Tooltip("Distance between the camera and the spaceship.")]
            public float Offset;

            [Tooltip("Tilt of the camera when the spaceship is doing a roll. 0 = no tilt.")]
            public float OnRollCompensationFactor;

            [Tooltip("How fast the camera follows the spaceship's position. Higer = faster.")]
            public float PositionSmooth;

            [Tooltip("How fast the camera follows the spaceship's rotation. Higer = faster.")]
            public float RotationSmooth;

            [Tooltip("Camera object.")] public Camera TargetCamera;
            [Tooltip("Standard cursor")] public Texture2D normalCursor;

            [Tooltip("Cursor when aiming at target")]
            public Texture2D aimingCursor;

            [Tooltip("Shake amount when hit")] public float shakeAmount;
            [Tooltip("Shake duration when hit")] public float shakeDuration;
        }

        private enum InputMode
        {
            Keyboard,
            KeyboardAndMouse
        }

        [Serializable]
        private struct InputSettings
        {
            [Tooltip("Keyboard options.")] public KeyboardSettings Keyboard;
            [Tooltip("Input mode.")] public InputMode Mode;
            [Tooltip("Mouse options.")] public MouseSettings Mouse;

            [Tooltip("How fast the input interpolates to the desired value. Higher = faster.")]
            public Vector4 Response;
        }

        [Serializable]
        private struct KeyboardInputNames
        {
            [Tooltip("Rotation around x-axis (vertical movement).")]
            public string AxisX;

            [Tooltip("Rotation around y-axis (horizontal movement).")]
            public string AxisY;

            [Tooltip("Rotation around z-axis (roll).")]
            public string AxisZ;

            [Tooltip("Speed control.")] public string Throttle;
        }

        [Serializable]
        private struct KeyboardSettings
        {
            [Tooltip("Names of input axes (from InputManager).")]
            public KeyboardInputNames InputNames;

            [Tooltip("Keyboard sensitivity when flying with a minimum speed.")]
            public float Sensitivity;

            [Tooltip("Keyboard sensitivity when flying with a maximum speed.")]
            public float SensitivityOnMaxSpeed;
        }

        [Serializable]
        private struct MouseSettings
        {
            [Tooltip("Mouse input is set to a maximum when the cursor is out of bounds of that area.")]
            public Vector2 ActiveArea;

            [Tooltip("How far the cursor should be moved from the center of the screen to make the spaceship turn.")]
            public float MovementThreshold;

            [Tooltip("Mouse sensitivity when flying with a minimum speed.")]
            public float Sensitivity;

            [Tooltip("Mouse sensitivity when flying with a maximum speed.")]
            public float SensitivityOnMaxSpeed;
        }

        [Serializable]
        public struct SpaceshipSettings
        {
            [Tooltip("Defines how speed changes over time.")]
            public AnimationCurve AccelerationCurve;

            [Tooltip("The spaceship's model.")] public Transform Avatar;

            [Tooltip("How fast the spaceship tilts when doing a sideways turns. Higher = faster.")]
            public float BankAngleSmooth;

            [Tooltip("How fast the spaceship turns. Higher = faster.")]
            public Vector3 Maneuverability;

            [Tooltip("Maximum tilt of the spaceship when doing a sideways turns.")]
            public float MaxBankAngleOnTurnAxis;

            [Tooltip("Maximum turn to left/right")]
            public float MaxBankAngleOnTurnLeftRight;

            [Tooltip("Maximum turn up/down")] public float MaxBankAngleOnTurnUpDown;

            [Tooltip("Maximum tilt when going sideways")]
            public float MaxBankAngleSideways;

            [Tooltip("Speed when going sideways")] public float SidewaysSpeed;

            [Tooltip("Minimum and maximum speed of the spaceship.")]
            public Vector2 SpeedRange;
        }

        [Serializable]
        public struct ShootingSettings
        {
            [Tooltip("2D shooting mode")] public bool mode2D;
            [Tooltip("The bullet settings.")] public BulletSettings bulletSettings;
        }

        [Serializable]
        public struct BulletSettings
        {
            [Tooltip("The origin point(s) of bullets.")]
            public List<GameObject> BulletBarrels;

            [Tooltip("The bullet prefab.")] public GameObject Bullet;
            [Tooltip("The bullet speed.")] public float BulletSpeed;
            [Tooltip("The bullet firing delay.")] public float BulletFireDelay;

            [Tooltip("The bullet count for each fire.")]
            public float BulletCount;

            [Tooltip("The bullet damage.")] public float BulletDamage;

            [Tooltip("How long before the bullet disappears.")]
            public float BulletLifetime;

            [Tooltip("The dostance from the cursors position on the screen in 3D space.")]
            public float TargetDistance;
        }
    }
}