#if AR_FOUNDATION_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    /// <summary>
    /// Spawns an object at an <see cref="IARInteractor"/>'s raycast hit position when a trigger is activated.
    /// </summary>
    public class ARInteractorSpawnTrigger : MonoBehaviour
    {
        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public enum SpawnTriggerType
        {
            /// <summary>
            /// Spawn an object when the interactor activates its select input
            /// but no selection actually occurs.
            /// </summary>
            SelectAttempt,

            /// <summary>
            /// Spawn an object when an input is performed.
            /// </summary>
            InputAction,
        }

        [SerializeField]
        [Tooltip("The AR ray interactor that determines where to spawn the object.")]
        XRRayInteractor m_ARInteractor;

        /// <summary>
        /// The AR ray interactor that determines where to spawn the object.
        /// </summary>
        public XRRayInteractor arInteractor
        {
            get => m_ARInteractor;
            set => m_ARInteractor = value;
        }

        [SerializeField]
        [Tooltip("The behavior to use to spawn objects.")]
        ObjectSpawner m_ObjectSpawner;

        /// <summary>
        /// The behavior to use to spawn objects.
        /// </summary>
        public ObjectSpawner objectSpawner
        {
            get => m_ObjectSpawner;
            set => m_ObjectSpawner = value;
        }

        [SerializeField]
        [Tooltip("Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.")]
        bool m_RequireHorizontalUpSurface;

        /// <summary>
        /// Whether to require that the <see cref="IARInteractor"/> hits an <see cref="ARPlane"/> with an alignment of
        /// <see cref="PlaneAlignment.HorizontalUp"/> in order to spawn anything.
        /// </summary>
        public bool requireHorizontalUpSurface
        {
            get => m_RequireHorizontalUpSurface;
            set => m_RequireHorizontalUpSurface = value;
        }

        [SerializeField]
        [Tooltip("The type of trigger to use to spawn an object, either when the Interactor's select action occurs or " +
            "when a button input is performed.")]
        SpawnTriggerType m_SpawnTriggerType;

        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public SpawnTriggerType spawnTriggerType
        {
            get => m_SpawnTriggerType;
            set => m_SpawnTriggerType = value;
        }

        [SerializeField]
        XRInputButtonReader m_SpawnObjectInput = new XRInputButtonReader("Spawn Object");

        /// <summary>
        /// The input used to trigger spawn, if <see cref="spawnTriggerType"/> is set to <see cref="SpawnTriggerType.InputAction"/>.
        /// </summary>
        public XRInputButtonReader spawnObjectInput
        {
            get => m_SpawnObjectInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_SpawnObjectInput, value, this);
        }

        [SerializeField]
        [Tooltip("When enabled, spawn will not be triggered if an object is currently selected.")]
        bool m_BlockSpawnWhenInteractorHasSelection = true;
        
        /*
         ---------------- CUSTOM PROPERTIES ------------------
         */
        
        [SerializeField]
        [Tooltip("How much to push the object along the world Z axis.")]
        float m_ZOffset = -2f;
        
        [SerializeField]
        [Tooltip("Reference to the ARRaycastManager, used for checking the offset point.")]
        ARRaycastManager m_RaycastManager;
        
        // cache list to avoid alloc every frame
        static readonly List<ARRaycastHit> s_OffsetHits = new List<ARRaycastHit>();
        public List<Vector2> ContentOffsets { get; set; }= new List<Vector2>();
        
        public event Action<int> placementFailed;
        
        /*
         ---------------- END CUSTOM PROPERTIES ------------------
         */

        /// <summary>
        /// When enabled, spawn will not be triggered if an object is currently selected.
        /// </summary>
        public bool blockSpawnWhenInteractorHasSelection
        {
            get => m_BlockSpawnWhenInteractorHasSelection;
            set => m_BlockSpawnWhenInteractorHasSelection = value;
        }

        bool m_AttemptSpawn;
        bool m_EverHadSelection;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
            m_SpawnObjectInput.EnableDirectActionIfModeUsed();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
            m_SpawnObjectInput.DisableDirectActionIfModeUsed();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Start()
        {
            if (m_ObjectSpawner == null)
#if UNITY_2023_1_OR_NEWER
                m_ObjectSpawner = FindAnyObjectByType<ObjectSpawner>();
#else
                m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();
#endif

            if (m_ARInteractor == null)
            {
                Debug.LogError("Missing AR Interactor reference, disabling component.", this);
                enabled = false;
            }
        }

        private bool spawned = false;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Update()
        {
            // Wait a frame after the Spawn Object input is triggered to actually cast against AR planes and spawn
            // in order to ensure the touchscreen gestures have finished processing to allow the ray pose driver
            // to update the pose based on the touch position of the gestures.
            if (!spawned)
            {
                m_AttemptSpawn = false;

                // ignore UI taps
                /*if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
                    return;*/

                // first do the normal AR‐plane hit test
                if (m_ARInteractor.TryGetCurrentARRaycastHit(out var arHit)
                    && arHit.trackable is ARPlane arPlane
                    && (!m_RequireHorizontalUpSurface || arPlane.alignment == PlaneAlignment.HorizontalUp))
                {
                    Vector3 basePos = arHit.pose.position;

                    bool canPlace = true;
                    float neededMeters = 0f;
                    
                    // Old placement system
                    // now for each offset in your list
                    foreach (var offset in ContentOffsets)
                    {
                        // flip the sign of X and Z
                        var flipped = -offset;

                        // build the world‐space spawn position
                        var spawnPos = basePos + new Vector3(flipped.x, 0f, flipped.y);

                        // raycast downward a tiny bit to make sure the scan covered it
                        var rayOrig = spawnPos + Vector3.up * 0.1f;
                        var ray    = new Ray(rayOrig, Vector3.down);
                        s_OffsetHits.Clear();

                        if (!m_RaycastManager.Raycast(ray, s_OffsetHits, TrackableType.PlaneWithinPolygon))
                        {
                            canPlace = false;
                            neededMeters += Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
                        }
                    }

                    if (!canPlace)
                    {
                        int scanSize = Mathf.CeilToInt(neededMeters);
                        Debug.Log($"Failed to place object, recommended area size is {scanSize}x{scanSize}m");
                        //placementFailed?.Invoke(scanSize);
                        return;
                    }
                    
                    // If can place
                    spawned = true;
                    m_ObjectSpawner.TrySpawnObject(arHit, arPlane);
                }

                return;
            }

            var selectState = m_ARInteractor.logicalSelectState;

            if (m_BlockSpawnWhenInteractorHasSelection)
            {
                if (selectState.wasPerformedThisFrame)
                    m_EverHadSelection = m_ARInteractor.hasSelection;
                else if (selectState.active)
                    m_EverHadSelection |= m_ARInteractor.hasSelection;
            }

            m_AttemptSpawn = false;
            switch (m_SpawnTriggerType)
            {
                case SpawnTriggerType.SelectAttempt:
                    if (selectState.wasCompletedThisFrame)
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;

                case SpawnTriggerType.InputAction:
                    if (m_SpawnObjectInput.ReadWasPerformedThisFrame())
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;
            }
        }
    }
}
#endif
