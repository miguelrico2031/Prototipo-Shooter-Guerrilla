using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("References")] [Tooltip("Reference to the main camera used for the player")]
        public Camera PlayerCamera;

        [Tooltip("Audio source for footsteps, jump, etc...")]
        public AudioSource AudioSource;

        [Header("General")] [Tooltip("Force applied downward when in the air")]
        public float GravityDownForce = 20f;

        [Tooltip("Physic layers checked to consider the player grounded")]
        public LayerMask GroundCheckLayers = -1;

        [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
        public float GroundCheckDistance = 0.05f;

        [Header("Movement")] [Tooltip("Max movement speed when grounded (when not sprinting)")]
        public float MaxSpeedOnGround = 10f;

        [Tooltip(
            "Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
        public float MovementSharpnessOnGround = 15;

        [Tooltip("Max movement speed when crouching")] [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;

        [Tooltip("Max movement speed when not grounded")]
        public float MaxSpeedInAir = 10f;

        [Tooltip("Acceleration speed when in the air")]
        public float AccelerationSpeedInAir = 25f;

        [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
        public float SprintSpeedModifier = 2f;

        [Tooltip("Height at which the player dies instantly when falling off the map")]
        public float KillHeight = -50f;

        [Header("Rotation")] [Tooltip("Rotation speed for moving the camera")]
        public float RotationSpeed = 200f;

        [Range(0.1f, 1f)] [Tooltip("Rotation speed multiplier when aiming")]
        public float AimingRotationMultiplier = 0.4f;

        [Header("Jump")] [Tooltip("Force applied upward when jumping")]
        public float JumpForce = 9f;

        [Header("Stance")] [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
        public float CameraHeightRatio = 0.9f;

        [Tooltip("Height of character when standing")]
        public float CapsuleHeightStanding = 1.8f;

        [Tooltip("Height of character when crouching")]
        public float CapsuleHeightCrouching = 0.9f;

        [Tooltip("Speed of crouching transitions")]
        public float CrouchingSharpness = 10f;

        [Header("Audio")] [Tooltip("Amount of footstep sounds played when moving one meter")]
        public float FootstepSfxFrequency = 1f;

        [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
        public float FootstepSfxFrequencyWhileSprinting = 1f;

        [Tooltip("Sound played for footsteps")]
        public AudioClip FootstepSfx;

        [Tooltip("Sound played when jumping")] public AudioClip JumpSfx;
        [Tooltip("Sound played when landing")] public AudioClip LandSfx;

        [Tooltip("Sound played when taking damage froma fall")]
        public AudioClip FallDamageSfx;

        [Header("Fall Damage")]
        [Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
        public bool RecievesFallDamage;

        [Tooltip("Minimun fall speed for recieving fall damage")]
        public float MinSpeedForFallDamage = 10f;

        [Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
        public float MaxSpeedForFallDamage = 30f;

        [Tooltip("Damage recieved when falling at the mimimum speed")]
        public float FallDamageAtMinSpeed = 10f;

        [Tooltip("Damage recieved when falling at the maximum speed")]
        public float FallDamageAtMaxSpeed = 50f;

        public UnityAction<bool> OnStanceChanged;

        public Vector3 CharacterVelocity { get; set; }
        public bool IsGrounded { get; private set; }
        public bool HasJumpedThisFrame { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsCrouching { get; private set; }

        public float RotationMultiplier
        {
            get
            {
                if (m_WeaponsManager.IsAiming)
                {
                    return AimingRotationMultiplier;
                }

                return 1f;
            }
        }

       

        Health m_Health;
        PlayerInputHandler m_InputHandler;
        CharacterController m_Controller;
        PlayerWeaponsManager m_WeaponsManager;
        Actor m_Actor;
        Vector3 m_GroundNormal;
        Vector3 m_CharacterVelocity;
        Vector3 m_LatestImpactSpeed;
        float m_LastTimeJumped = 0f;
        float m_CameraVerticalAngle = 0f;
        float m_FootstepDistanceCounter;
        float m_TargetCharacterHeight;

        const float k_JumpGroundingPreventionTime = 0.2f;
        const float k_GroundCheckDistanceInAir = 0.07f;



        private float _jumpForce;
        
        
        
        
        
        
        
        
        [Header("Propulsion")]
        
        // [SerializeField] private float _propulsionCamMaxAngle;
        [SerializeField] private float _maxPropulsionDistance;
        [SerializeField] private float _propulsionForce;
        [SerializeField] private float _propulsionCooldown;
        
        
        [Header("Hook")]
        
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _hookAimPoint;
        [SerializeField] private AnimationCurve hookVelocityCurve;
        [SerializeField] private AudioSource _hookAudioSource;
        [SerializeField] private AudioClip _kickSound;
        [SerializeField] private AudioClip _hookThrowSound;
        [SerializeField] private AudioClip _hookLandSound;
        [SerializeField] private float _hookSpeed;
        [SerializeField] private float _hookAnimSpeed;
        [SerializeField] private float _hookDistanceThreshold;
        
        
        [Header("Hook Jump/Kick")]
        
        [SerializeField] private Gradient _jumpThresholdColor;
        [SerializeField] private Animator _legAnimator;
        [SerializeField][Range(0f, 1f)] private float _horizontalJumpMultiplier;
        [SerializeField] private float _hookJumpDistanceThreshold;
        [SerializeField] private float _hookJumpForceMultiplier;
        [SerializeField] private float _hookJumpDamage;
        [SerializeField] private float _timeSlowMultiplier;
        [SerializeField] private float _timeSlowDuration;
        [SerializeField] private float _camShakeDuration;

        [Header("Progression")]
        public bool IsHookEnabled;
        public bool IsHookJumpEnabled;
        
        
        private bool _isHooked;
        private bool _isOnHookAnim;
        private bool _isHookingEnemy;
        private Vector3 _hookEnd;
        private Vector3 _hookOffset;
        private Transform _enemyHooked;
        private float _hookAnimT;
        private float _distanceToHookEnd;
        private Gradient _defaultColor;
        private CameraShake _camShake;
        private Vector3 _hookStartPos;
        private float _totalDistance;
        private bool _canPropulse = true;
        
        
        
        

        void Awake()
        {
            ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
            if (actorsManager != null)
                actorsManager.SetPlayer(gameObject);
            
            GetComponent<PlayerWeaponsManager>().OnShoot.AddListener(OnShoot);
        }

        private void OnDisable()
        {
            GetComponent<PlayerWeaponsManager>().OnShoot.RemoveListener(OnShoot);
        }

        void Start()
        {
            // fetch components on the same gameObject
            m_Controller = GetComponent<CharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller,
                this, gameObject);

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler,
                this, gameObject);

            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerCharacterController>(
                m_WeaponsManager, this, gameObject);

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerCharacterController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerCharacterController>(m_Actor, this, gameObject);

            m_Controller.enableOverlapRecovery = true;

            m_Health.OnDie += OnDie;

            // force the crouch state to false when starting
            SetCrouchingState(false, true);
            UpdateCharacterHeight(true);

            _defaultColor = _lineRenderer.colorGradient;
            _jumpForce = JumpForce;
            _camShake = PlayerCamera.GetComponent<CameraShake>();
        }

        void Update()
        {
            // check for Y kill
            if (!IsDead && transform.position.y < KillHeight)
            {
                m_Health.Kill();
            }

            HandleHookInput();

            HasJumpedThisFrame = false;

            bool wasGrounded = IsGrounded;
            if(!_isHooked) GroundCheck();

            // landing
            if (IsGrounded && !wasGrounded)
            {
                // Fall damage
                float fallSpeed = -Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y);
                float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) /
                                       (MaxSpeedForFallDamage - MinSpeedForFallDamage);
                if (RecievesFallDamage && fallSpeedRatio > 0f)
                {
                    float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                    m_Health.TakeDamage(dmgFromFall, null);

                    // fall damage SFX
                    AudioSource.PlayOneShot(FallDamageSfx);
                }
                else
                {
                    // land SFX
                    AudioSource.PlayOneShot(LandSfx);
                }
            }

            // crouching
            if (m_InputHandler.GetCrouchInputDown() && !_isHooked)
            {
                SetCrouchingState(!IsCrouching, false);
            }

            UpdateCharacterHeight(false);

            HandleCharacterMovement();


            CheckHook();
            HandleHookAnimation();
            
        }

        void OnDie()
        {
            IsDead = true;

            // Tell the weapons manager to switch to a non-existing weapon in order to lower the weapon
            m_WeaponsManager.SwitchToWeaponIndex(-1, true);

            EventManager.Broadcast(Events.PlayerDeathEvent);
        }

        void GroundCheck()
        {
            // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
            float chosenGroundCheckDistance =
                IsGrounded ? (m_Controller.skinWidth + GroundCheckDistance) : k_GroundCheckDistanceInAir;

            // reset values before the ground check
            IsGrounded = false;
            m_GroundNormal = Vector3.up;

            // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
            if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
            {
                // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
                if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height),
                    m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, GroundCheckLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    // storing the upward direction for the surface found
                    m_GroundNormal = hit.normal;

                    // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                    // and if the slope angle is lower than the character controller's limit
                    if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                        IsNormalUnderSlopeLimit(m_GroundNormal))
                    {
                        IsGrounded = true;

                        // handle snapping to the ground
                        if (hit.distance > m_Controller.skinWidth)
                        {
                            m_Controller.Move(Vector3.down * hit.distance);
                        }
                    }
                }
            }
        }

        void HandleCharacterMovement()
        {
            // horizontal character rotation
            {
                // rotate the transform with the input speed around its local Y axis
                transform.Rotate(
                    new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * RotationSpeed * RotationMultiplier),
                        0f), Space.Self);
            }

            // vertical camera rotation
            {
                // add vertical inputs to the camera's vertical angle
                m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * RotationSpeed * RotationMultiplier;

                // limit the camera's vertical angle to min/max
                m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

                // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
                PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }

            // character movement handling
            bool isSprinting = m_InputHandler.GetSprintInputHeld();
            {
                if (isSprinting)
                {
                    isSprinting = SetCrouchingState(false, false);
                }

                float speedModifier = isSprinting ? SprintSpeedModifier : 1f;

                // converts move input to a worldspace vector based on our character's transform orientation
                Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

                
                
                if (_isHooked)
                {
                    if (UpdateHookEnd())
                    {
                        Vector3 direction = (_hookEnd - PlayerCamera.transform.position).normalized;


                        float distanceTraveled = Vector3.Distance(_hookStartPos, transform.position);
                        _totalDistance = Vector3.Distance(_hookStartPos, _hookEnd);
                        
                        float t = Mathf.Clamp01(distanceTraveled / _totalDistance);
                        if (t < 0.001f)
                        {
                            t = Time.deltaTime;
                        }
                        float velocityMultiplier = hookVelocityCurve.Evaluate(t);
                        CharacterVelocity = direction * (_hookSpeed * velocityMultiplier);
                        IsGrounded = false;

                        if (IsHookJumpEnabled && _isHookingEnemy && _distanceToHookEnd <= _hookJumpDistanceThreshold && 
                            m_InputHandler.GetJumpInputDown())
                        {
                            StartCoroutine(HookJumpKick());
                        }
                    }
                }
                
                
                
                // handle grounded movement
                else if (IsGrounded)
                {
                    // calculate the desired velocity from inputs, max speed, and current slope
                    Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround * speedModifier;
                    // reduce speed if crouching by crouch speed ratio
                    if (IsCrouching)
                        targetVelocity *= MaxSpeedCrouchedRatio;
                    targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) *
                                     targetVelocity.magnitude;

                    // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                    CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                        MovementSharpnessOnGround * Time.deltaTime);

                    // jumping
                    if (IsGrounded && m_InputHandler.GetJumpInputDown())
                    {
                        StartJump(Vector3.up);
                    }

                    // footsteps sound
                    float chosenFootstepSfxFrequency =
                        (isSprinting ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);
                    if (m_FootstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                    {
                        m_FootstepDistanceCounter = 0f;
                        AudioSource.PlayOneShot(FootstepSfx);
                    }

                    // keep track of distance traveled for footsteps sound
                    m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
                }
                // handle air movement
                else
                {
                    // add air acceleration
                    CharacterVelocity += worldspaceMoveInput * AccelerationSpeedInAir * Time.deltaTime;

                    // limit air speed to a maximum, but only horizontally
                    float verticalVelocity = CharacterVelocity.y;
                    Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);
                    CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                    // apply the gravity to the velocity
                    CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
                }
            }

            // apply the final calculated velocity value as a character movement
            Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
            Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
            m_Controller.Move(CharacterVelocity * Time.deltaTime);

            // detect obstructions to adjust velocity accordingly
            m_LatestImpactSpeed = Vector3.zero;
            if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius,
                CharacterVelocity.normalized, out RaycastHit hit, CharacterVelocity.magnitude * Time.deltaTime, -1,
                QueryTriggerInteraction.Ignore))
            {
                // We remember the last impact speed because the fall damage logic might need it
                m_LatestImpactSpeed = CharacterVelocity;

                CharacterVelocity = Vector3.ProjectOnPlane(CharacterVelocity, hit.normal);
            }
        }

        // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
        bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
        }

        void StartJump(Vector3 jumpDirection)
        {
            // force the crouch state to false
            if (SetCrouchingState(false, false))
            {
                // start by canceling out the vertical component of our velocity
                CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);

                // then, add the jumpSpeed value upwards
                CharacterVelocity += jumpDirection * _jumpForce;

                // play sound
                AudioSource.PlayOneShot(JumpSfx);

                // remember last time we jumped because we need to prevent snapping to ground for a short time
                m_LastTimeJumped = Time.time;
                HasJumpedThisFrame = true;

                // Force grounding to false
                IsGrounded = false;
                m_GroundNormal = Vector3.up;
            }
        }

        // Gets the center point of the bottom hemisphere of the character controller capsule    
        Vector3 GetCapsuleBottomHemisphere()
        {
            return transform.position + (transform.up * m_Controller.radius);
        }

        // Gets the center point of the top hemisphere of the character controller capsule    
        Vector3 GetCapsuleTopHemisphere(float atHeight)
        {
            return transform.position + (transform.up * (atHeight - m_Controller.radius));
        }

        // Gets a reoriented direction that is tangent to a given slope
        public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        void UpdateCharacterHeight(bool force)
        {
            // Update height instantly
            if (force)
            {
                m_Controller.height = m_TargetCharacterHeight;
                m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * CameraHeightRatio;
                m_Actor.AimPoint.transform.localPosition = m_Controller.center;
            }
            // Update smooth height
            else if (m_Controller.height != m_TargetCharacterHeight)
            {
                // resize the capsule and adjust camera position
                m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight,
                    CrouchingSharpness * Time.deltaTime);
                m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition,
                    Vector3.up * m_TargetCharacterHeight * CameraHeightRatio, CrouchingSharpness * Time.deltaTime);
                m_Actor.AimPoint.transform.localPosition = m_Controller.center;
            }
        }

        // returns false if there was an obstruction
        bool SetCrouchingState(bool crouched, bool ignoreObstructions)
        {
            // set appropriate heights
            if (crouched)
            {
                m_TargetCharacterHeight = CapsuleHeightCrouching;
            }
            else
            {
                // Detect obstructions
                if (!ignoreObstructions)
                {
                    Collider[] standingOverlaps = Physics.OverlapCapsule(
                        GetCapsuleBottomHemisphere(),
                        GetCapsuleTopHemisphere(CapsuleHeightStanding),
                        m_Controller.radius,
                        -1,
                        QueryTriggerInteraction.Ignore);
                    foreach (Collider c in standingOverlaps)
                    {
                        if (c != m_Controller)
                        {
                            return false;
                        }
                    }
                }

                m_TargetCharacterHeight = CapsuleHeightStanding;
            }

            if (OnStanceChanged != null)
            {
                OnStanceChanged.Invoke(crouched);
            }

            IsCrouching = crouched;
            return true;
        }

        void HandleHookInput()
        {
            if (!IsHookEnabled) return;
            
            if(_isHooked && m_InputHandler.GetUnhookButtonDown()) Unhook();
            if (m_InputHandler.GetHookButtonDown())
            {
                if(_isHooked) Unhook();
                TryHook();
            }
        }
        void TryHook()
        {
            Vector3 origin = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.forward;
            if (!Physics.Raycast(origin, direction, out var hitInfo)) return;
            if (hitInfo.collider.GetComponentInParent<MeshCombiner>() is not null)
            {
                HookWall(hitInfo);
            }
            else if (hitInfo.collider.GetComponentInParent<EnemyController>())
            {
                HookEnemy(hitInfo);
            }
        }
        
        
        void HookWall(RaycastHit hitInfo)
        {
            if (Mathf.Abs(hitInfo.normal.y) > .5f) return;
            Hook(hitInfo, false);
        }

        void HookEnemy(RaycastHit hitInfo)
        {
            Hook(hitInfo, true);
        }

        void Hook(RaycastHit hitInfo, bool isEnemy)
        {
            _isHooked = true;
            _isOnHookAnim = true;
            _hookAnimT = 0f;
            _lineRenderer.enabled = true;
            _lineRenderer.colorGradient = _defaultColor;
            SetCrouchingState(false, false);
            _hookEnd = hitInfo.point;
            _isHookingEnemy = isEnemy;
            _hookStartPos = transform.position;
            if (isEnemy)
            {
                m_Health.Invincible = true;
                _enemyHooked = hitInfo.collider.GetComponentInParent<EnemyController>().transform;
                _hookEnd = hitInfo.collider.transform.position;
                _hookOffset = _hookEnd - _enemyHooked.position;
            }
            _totalDistance = Vector3.Distance(_hookStartPos, _hookEnd);
            
            _hookAudioSource.PlayOneShot(_hookThrowSound);
        }
        
        void Unhook()
        {
            _isHooked = false;
            _isOnHookAnim = false;
            _hookAnimT = 0f;
            _lineRenderer.enabled = false;
            _enemyHooked = null;
            if (_isHookingEnemy) m_Health.Invincible = false;
            _isHookingEnemy = false;
        }

        void HandleHookAnimation()
        {
            if (!_isHooked) return;
            Vector3 hookEnd = _hookEnd;

            if (_isOnHookAnim)
            {
                _hookAnimT += Time.deltaTime * _hookAnimSpeed;
                hookEnd = Vector3.Lerp(_hookAimPoint.position, _hookEnd, _hookAnimT);
                if (_hookAnimT >= 1f)
                {
                    _hookAnimT = 0f;
                    hookEnd = _hookEnd;
                    _isOnHookAnim = false;
                    _hookAudioSource.PlayOneShot(_hookLandSound);
                }
            }
            
            _lineRenderer.SetPositions(new[]{_hookAimPoint.position, hookEnd});

            if (IsHookJumpEnabled && _isHookingEnemy && _distanceToHookEnd <= _hookJumpDistanceThreshold)
            {
                _lineRenderer.colorGradient = _jumpThresholdColor;
            }
        }

        void CheckHook()
        {
            if (!_isHooked) return;
            _distanceToHookEnd = Vector3.Distance(transform.position, _hookEnd);
            if(_distanceToHookEnd > _hookDistanceThreshold) return;
            
            Unhook();
        }

        bool UpdateHookEnd()
        {   
            if (_isHookingEnemy)
            {
                if (_enemyHooked is null)
                {
                    Unhook();
                    return false;
                }
                _hookEnd = _enemyHooked.position + _hookOffset;
                return true;
            }
            else
            {
                return true;
            }
        }

        IEnumerator HookJumpKick()
        {
            //pegamos un ostion al enemigo
            Damageable enemyDmg = _enemyHooked.GetComponentInChildren<Damageable>();
            enemyDmg.InflictDamage(_hookJumpDamage, false, gameObject);
                            
            //calculamos la direccion del salto
                            
            _jumpForce = JumpForce * _hookJumpForceMultiplier;
                            
            //calculamos direccion del enemigo al jugador
            Vector3 enemyToPlayer = transform.position - _enemyHooked.position;
            enemyToPlayer.y = 0f; //para quedarnos con la proyeccion horizontal

            //segun el multiplier el salto es mas horizontal hacia atras o mas vertical
            Vector3 jumpDirection = Vector3.up * (1 - _horizontalJumpMultiplier) + enemyToPlayer.normalized * _horizontalJumpMultiplier;
                            
            //desenganchamos y saltamos 
            Unhook();
            StartJump(jumpDirection.normalized);
            _jumpForce = JumpForce;
                            
            _camShake.Shake(_camShakeDuration);
            
            StartCoroutine(SlowTime());
            
            _hookAudioSource.PlayOneShot(_kickSound);

            
            //animamos la patada
            _legAnimator.SetBool("Kick", true);
            yield return null;
            _legAnimator.SetBool("Kick", false);
        }

        IEnumerator SlowTime()
        {
            float defaultTimeScale = Time.timeScale;
            Time.timeScale = _timeSlowMultiplier;
            yield return new WaitForSecondsRealtime(_timeSlowDuration);
            if(Math.Abs(Time.timeScale - _timeSlowMultiplier) < float.Epsilon) //un == de floats para saber si algo externo ha cambiado el timescale
                Time.timeScale = defaultTimeScale;
        }

        void OnShoot()
        {
            if (!_canPropulse) return;
            
            if (!Physics.Raycast(_hookAimPoint.position, PlayerCamera.transform.forward, out var hitInfo,_maxPropulsionDistance)) return;
            if(hitInfo.distance > _maxPropulsionDistance) return;
            if (hitInfo.collider.GetComponentInParent<EnemyController>() is not null) return;
            if (hitInfo.collider.GetComponent<PlayerCharacterController>() is not null) return;
            
            StartCoroutine(AddPropulsion(-PlayerCamera.transform.forward));
            
        }

        IEnumerator AddPropulsion(Vector3 direction)
        {
            if (SetCrouchingState(false, false))
            {
                CharacterVelocity += direction * _propulsionForce;

                m_LastTimeJumped = Time.time;
                HasJumpedThisFrame = true;

                IsGrounded = false;
                m_GroundNormal = Vector3.up;

                _canPropulse = false;
                yield return new WaitForSeconds(_propulsionCooldown);
                _canPropulse = true;
            }
        }

    }
}