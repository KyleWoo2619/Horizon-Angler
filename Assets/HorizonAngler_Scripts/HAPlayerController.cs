using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class HAPlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;

        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Header("Ground Check Settings")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Fishing Settings")]
        public GameObject fishingPromptUI;
        public GameObject fishingMinigameUI;
        public Transform FishingCameraTarget;
        public GameObject fishingrod;
        private bool canFish = false;
        public bool canFishPondBoss = false;
        public bool canFishRiverBoss = false;
        public bool canFishOceanBoss = false;
        private bool shownLockedZoneNotification = false;
        private bool isFishing = false;
        private Vector3 fishingLookTarget;

        [Header("Shop Settings")]
        public GameObject shopInteractPromptUI;
        public GameObject shopUI;
        public Transform shopCameraTarget;
        public GameObject playerVisual;
        private bool canInteractWithShop = false;
        private bool isInShop = false;
        private Vector3 _originalShopCameraPosition;
        private Quaternion _originalShopCameraRotation;

        [Header("Camera Settings")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private bool _hasAnimator;
        public bool inFishZone = false;
        public InitiateMicrogames.FishZoneType currentZoneType;
        public bool inBossFishZone = false;

        // Animator Parameters
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDMotionSpeed;
        private int _animIDFishing;

        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
        
            _controller = GetComponent<CharacterController>();
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions["Interact"].performed += ctx =>
            {
                if (canFish && !isFishing)
                    StartFishing();
                else if (canInteractWithShop && !isInShop)
                    EnterShop();
            };
            _playerInput.actions["ExitFishing"].performed += ctx =>
            {
                if (isFishing)
                    EndFishing();
                if (isInShop)
                    ExitShop();
            };
#endif
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }

        private void Update()
        {
            if (isInShop)
                return; // Stop player movement if in shop
#if ENABLE_INPUT_SYSTEM
            if (_playerInput != null)
            {
                _moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
                _lookInput = _playerInput.actions["Look"].ReadValue<Vector2>();

                if (_moveInput.magnitude > 1f)
                    _moveInput.Normalize();
            }
#endif
            GroundedCheck();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            if (Time.timeScale == 0f)
                return;
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDFishing = Animator.StringToHash("Fishing");

            Debug.Log($"Speed Hash: {_animIDSpeed}");
            Debug.Log($"Grounded Hash: {_animIDGrounded}");
            Debug.Log($"MotionSpeed Hash: {_animIDMotionSpeed}");
            Debug.Log($"Fishing Hash: {_animIDFishing}");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );
            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (_lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _lookInput.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _lookInput.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(
                _cinemachineTargetYaw,
                float.MinValue,
                float.MaxValue
            );
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private void Move()
        {
            if (isFishing)
                return;

            float targetSpeed = MoveSpeed;
#if ENABLE_INPUT_SYSTEM
            if (_playerInput != null && _playerInput.actions["Sprint"].IsPressed())
            {
                targetSpeed = SprintSpeed;
            }
#endif
            if (_moveInput == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(
                _controller.velocity.x,
                0.0f,
                _controller.velocity.z
            ).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _moveInput.magnitude;

            if (
                currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * SpeedChangeRate
            );
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

            if (inFishZone)
            {
                Vector3 directionToLook = fishingLookTarget - transform.position;
                directionToLook.y = 0f;
                if (directionToLook != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * 5f
                    );
                }
            }
            else if (_moveInput != Vector2.zero)
            {
                _targetRotation =
                    Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                    + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime
                );
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection =
                Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime)
                    + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void StartFishing()
        {
            isFishing = true;
            fishingPromptUI.SetActive(false);
            fishingMinigameUI.SetActive(true);
            fishingrod.SetActive(true);

            if (_hasAnimator)
            {
                _animator.SetBool("Fishing", true);
                _animator.SetFloat(_animIDSpeed, 0f);

                // Force play Fishing Idle immediately
                _animator.Play("FishingIdle", 0, 0f);
            }

            _originalCameraPosition = CinemachineCameraTarget.transform.position;
            _originalCameraRotation = CinemachineCameraTarget.transform.rotation;

            CinemachineCameraTarget.transform.position = FishingCameraTarget.position;
            CinemachineCameraTarget.transform.rotation = FishingCameraTarget.rotation;

            _cinemachineTargetYaw = FishingCameraTarget.eulerAngles.y;
            _cinemachineTargetPitch = FishingCameraTarget.eulerAngles.x;

            LockCameraPosition = true;
        }

        private void EndFishing()
        {
            isFishing = false;
            fishingMinigameUI.SetActive(false);
            fishingrod.SetActive(false);

            if (inFishZone) // Only show if still inside fish zone
            {
                fishingPromptUI.SetActive(true);
            }
            else
            {
                fishingPromptUI.SetActive(false);
            }

            if (_hasAnimator)
            {
                _animator.SetBool("Fishing", false);
                _animator.SetFloat(_animIDSpeed, 0f);

                //  Force play back Idle immediately
                _animator.Play("Idle Walk Blend", 0, 0f);
            }

            CinemachineCameraTarget.transform.position = _originalCameraPosition;
            CinemachineCameraTarget.transform.rotation = _originalCameraRotation;

            LockCameraPosition = false;
        }

        private void OnTriggerStay(Collider other)
        {
            FishingZone fishingZone = other.GetComponent<FishingZone>();
            if (fishingZone != null)
            {
                inFishZone = true;
                currentZoneType = fishingZone.zoneType;

                bool allowedToFish = false;

                switch (currentZoneType)
                {
                    case InitiateMicrogames.FishZoneType.Pond:
                    case InitiateMicrogames.FishZoneType.River:
                    case InitiateMicrogames.FishZoneType.Ocean:
                        allowedToFish = true;
                        break;
                    case InitiateMicrogames.FishZoneType.BossPond:
                        allowedToFish = canFishPondBoss;
                        break;
                    case InitiateMicrogames.FishZoneType.BossRiver:
                        allowedToFish = canFishRiverBoss;
                        break;
                    case InitiateMicrogames.FishZoneType.BossOcean:
                        allowedToFish = canFishOceanBoss;
                        break;
                }

                canFish = allowedToFish;

                if (!allowedToFish)
                {
                    // Only show notification ONCE when entering
                    if (!shownLockedZoneNotification && FishingProgress.Instance != null)
                    {
                        FishingProgress.Instance.ShowNotification("You haven't unlocked this fishing zone yet!");
                        shownLockedZoneNotification = true;  // prevent repeats
                    }
                }

                if (!isFishing)
                {
                    fishingPromptUI.SetActive(allowedToFish);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            FishingZone fishingZone = other.GetComponent<FishingZone>();
            if (fishingZone != null)
            {
                inFishZone = false;
                canFish = false;
                fishingPromptUI.SetActive(false);
            }
            else if (other.CompareTag("ShopZone"))
            {
                canInteractWithShop = false;
                shopInteractPromptUI.SetActive(false);
            }
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index],
                        transform.TransformPoint(_controller.center),
                        FootstepAudioVolume
                    );
                }
            }
        }

        private void ApplyGravity()
        {
            if (Grounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;
                if (_verticalVelocity < -_terminalVelocity)
                {
                    _verticalVelocity = -_terminalVelocity;
                }
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        public void EnterShop()
        {
            isInShop = true;
            shopUI.SetActive(true);
            shopInteractPromptUI.SetActive(false);

            if (playerVisual != null)
                playerVisual.SetActive(false); // Hide player

            // Save original camera state
            _originalShopCameraPosition = CinemachineCameraTarget.transform.position;
            _originalShopCameraRotation = CinemachineCameraTarget.transform.rotation;

            // Move camera to shop view
            CinemachineCameraTarget.transform.position = shopCameraTarget.position;
            CinemachineCameraTarget.transform.rotation = shopCameraTarget.rotation;

            // Update the internal camera values to match new rotation
            _cinemachineTargetYaw = shopCameraTarget.eulerAngles.y;
            _cinemachineTargetPitch = shopCameraTarget.eulerAngles.x;

            // Lock camera while in shop
            LockCameraPosition = true;

            // Unlock mouse cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ExitShop()
        {
            isInShop = false;
            shopUI.SetActive(false);

            // Show player
            if (playerVisual != null)
                playerVisual.SetActive(true);

            // Restore camera to original position
            CinemachineCameraTarget.transform.position = _originalShopCameraPosition;
            CinemachineCameraTarget.transform.rotation = _originalShopCameraRotation;

            // Unlock camera
            LockCameraPosition = false;

            // Lock mouse cursor again for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Show interact prompt if still in shop zone
            if (canInteractWithShop)
            {
                shopInteractPromptUI.SetActive(true);
            }
        }
    }
}
