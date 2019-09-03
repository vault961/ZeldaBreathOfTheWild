//Dependencies:
// - Scroller: Source > Scripts > HelperClasses
// - DirectionalScrollSnapEditor: Source > Editor (optional)

//Contributors:
//BeksOmega

using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI.ScrollSnaps
{
    [AddComponentMenu("UI/Scroll Snaps/Directional Scroll Snap")]
    [ExecuteInEditMode]
    public class DirectionalScrollSnap : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, ICanvasElement, IScrollHandler, ILayoutGroup
    {

        #region Variables
        public enum MovementDirection
        {
            Horizontal,
            Vertical
        }

        public enum MovementType
        {
            Clamped,
            Elastic
        }

        public enum SnapType
        {
            SnapToNearest,
            SnapToLastPassed,
            SnapToNext
        }

        private enum Direction
        {
            TowardsStart,
            TowardsEnd
        }

        public enum InterpolatorType
        {
            AccelerateDecelerate,
            Accelerate,
            Anticipate,
            AnticipateOvershoot,
            Decelerate,
            Linear,
            Overshoot,
            ViscousFluid,
        }

        public enum FilterMode
        {
            BlackList,
            WhiteList
        }

        public enum StartMovementEventType
        {
            OnBeginDrag,
            ScrollBar,
            OnScroll,
            ButtonPress,
            Programmatic
        }
        
        public enum LockMode
        {
            Before,
            After,
        }

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2> { }
        [Serializable]
        public class StartMovementEvent : UnityEvent<StartMovementEventType> { }
        [Serializable]
        public class IntEvent : UnityEvent<int> { }

        [SerializeField]
        private RectTransform m_Content;
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        [SerializeField]
        private MovementDirection m_MovementDirection;
        public MovementDirection movementDirection
        {
            get
            {
                return m_MovementDirection;
            }
            set
            {
                if (contentIsHorizonalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Horizontal;
                }
                else if (contentIsVerticalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Vertical;
                }
                else
                {
                    m_MovementDirection = value;
                }
            }
        }

        [SerializeField]
        private bool m_LockOtherDirection = true;
        public bool lockNonScrollingDirection { get { return m_LockOtherDirection; } set { m_LockOtherDirection = value; } }

        [SerializeField]
        private MovementType m_MovementType;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private SnapType m_SnapType = SnapType.SnapToNearest;
        public SnapType snapType { get { return m_SnapType; } set { m_SnapType = value; } }

        [SerializeField]
        private bool m_UseVelocity = true;
        public bool useVelocity { get { return m_UseVelocity; } set { m_UseVelocity = value; } }

        [SerializeField]
        private float m_Friction = .25f;
        public float friction { get { return m_Friction; } }

        [SerializeField]
        private InterpolatorType m_InterpolatorType = InterpolatorType.ViscousFluid;
        public InterpolatorType interpolator { get { return m_InterpolatorType; } }

        [SerializeField]
        private float m_Tension = 2f;
        public float tension { get { return m_Tension; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private float m_ScrollDelay = .02f;
        public float scrollDelay
        {
            get
            {
                return m_ScrollDelay;
            }
            set
            {
                m_ScrollDelay = Mathf.Max(value, 0);
            }
        }

        [SerializeField]
        private int m_MinDurationMillis = 200;
        public int minDuration {  get { return m_MinDurationMillis; } }

        [SerializeField]
        private int m_MaxDurationMillis = 2000;
        public int maxDuration { get { return m_MaxDurationMillis; } }

        [SerializeField]
        private int m_ScrollDurationMillis = 250;
        public int scrollDurationMillis
        {
            get
            {
                return m_ScrollDurationMillis;
            }
            set
            {
                m_ScrollDurationMillis = Mathf.Max(value, 1);
            }
        }

        [SerializeField]
        private bool m_AddInactiveChildrenToCalculatingFilter;
        public bool addInactiveChildrenToCalculatingFilter{ get { return m_AddInactiveChildrenToCalculatingFilter; } }

        [SerializeField]
        private FilterMode m_FilterModeForCalculatingSize;
        public FilterMode calculateSizeFilterMode { get { return m_FilterModeForCalculatingSize; }}

        [SerializeField]
        private List<RectTransform> m_CalculatingFilter = new List<RectTransform>();
        public List<RectTransform> calculatingFilter { get { return m_CalculatingFilter; }}

        [SerializeField]
        private bool m_AddInactiveChildrenToSnapPositionsFilter;
        public bool addInactiveChildrenToSnapPositionsFilter { get { return m_AddInactiveChildrenToSnapPositionsFilter; }}

        [SerializeField]
        private FilterMode m_FilterModeForSnapPositions;
        public FilterMode snapPositionsFilterMode { get { return m_FilterModeForSnapPositions; } }

        [SerializeField]
        private List<RectTransform> m_SnapPositionsFilter = new List<RectTransform>();
        public List<RectTransform> snapPositionsFilter { get { return m_SnapPositionsFilter; }}

        [SerializeField]
        private RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        private ScrollBarEventsListener m_HorizontalScrollbarEventsListener;
        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                {
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                    if (m_HorizontalScrollbarEventsListener)
                    {
                        m_HorizontalScrollbarEventsListener.onPointerDown -= ScrollBarPointerDown;
                        m_HorizontalScrollbarEventsListener.onPointerUp -= ScrollBarPointerUp;
                        DestroyImmediate(m_HorizontalScrollbarEventsListener);
                    }
                }

                if (!m_Loop)
                {
                    m_HorizontalScrollbar = value;
                    if (m_HorizontalScrollbar)
                    {
                        m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                        m_HorizontalScrollbarEventsListener = m_HorizontalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                        m_HorizontalScrollbarEventsListener.onPointerDown += ScrollBarPointerDown;
                        m_HorizontalScrollbarEventsListener.onPointerUp += ScrollBarPointerUp;
                    }
                }
            }
        }

        private ScrollBarEventsListener m_VerticalScrollBarEventsListener;
        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                    if (m_VerticalScrollBarEventsListener)
                    {
                        m_VerticalScrollBarEventsListener.onPointerDown -= ScrollBarPointerDown;
                        m_VerticalScrollBarEventsListener.onPointerUp -= ScrollBarPointerUp;
                        DestroyImmediate(m_VerticalScrollBarEventsListener);
                    }
                }

                if (!m_Loop)
                {
                    m_VerticalScrollbar = value;
                    if (m_VerticalScrollbar)
                    {
                        m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                        m_VerticalScrollBarEventsListener = m_VerticalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                        m_VerticalScrollBarEventsListener.onPointerDown += ScrollBarPointerDown;
                        m_VerticalScrollBarEventsListener.onPointerUp += ScrollBarPointerUp;
                    }
                }
            }
        }

        [SerializeField]
        private Button m_BackButton;
        public Button backButton
        {
            get
            {
                return m_BackButton;
            }
            set
            {
                if (m_BackButton)
                {
                    m_BackButton.onClick.RemoveListener(OnBack);
                }
                m_BackButton = value;
                if (m_BackButton)
                {
                    m_BackButton.onClick.AddListener(OnBack);
                }
            }
        }

        [SerializeField]
        private Button m_ForwardButton;
        public Button forwardButton
        {
            get
            {
                return m_ForwardButton;
            }
            set
            {
                if (m_ForwardButton)
                {
                    m_ForwardButton.onClick.RemoveListener(OnForward);
                }
                m_ForwardButton = value;
                if (m_ForwardButton)
                {
                    m_ForwardButton.onClick.AddListener(OnForward);
                }
            }
        }
        
        [SerializeField]
        private bool m_Loop = false;
        public bool loop { get { return loop; } set { m_Loop = value; } }
        
        [SerializeField]
        private int m_EndSpacing;
        public int endSpacing { get { return m_EndSpacing; } }

        [SerializeField]
        private bool m_DrawGizmos = false;

        [SerializeField]
        private Vector2Event m_OnValueChanged = new Vector2Event();
        public Vector2Event onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        private StartMovementEvent m_StartMovementEvent = new StartMovementEvent();
        public StartMovementEvent startMovementEvent { get { return m_StartMovementEvent; } set { m_StartMovementEvent = value; } }

        [SerializeField]
        private IntEvent m_ClosestSnapPositionChanged = new IntEvent();
        public IntEvent closestSnapPositionChanged { get { return m_ClosestSnapPositionChanged; } set { m_ClosestSnapPositionChanged = value; } }

        [SerializeField]
        private IntEvent m_SnappedToItem = new IntEvent();
        public IntEvent snappedToItem { get { return m_SnappedToItem; } set { m_SnappedToItem = value; } }

        [SerializeField]
        private IntEvent m_TargetItemSelected = new IntEvent();
        public IntEvent targetItemSelected { get { return m_TargetItemSelected; } set { m_TargetItemSelected = value; } }


        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
                {
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                }
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
                {
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
                }
                return 1 - (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } }

        private int m_ClosestSnapPositionIndex;
        public int closestSnapPositionIndex { get { return m_ClosestSnapPositionIndex; } }  

        public RectTransform closestItem
        {
            get
            {
                RectTransform child;
                GetChildAtSnapIndex(m_ClosestSnapPositionIndex, out child);
                return child;
            }
        }

        private List<RectTransform> m_ChildrenForSizeFromStartToEnd = new List<RectTransform>();
        public List<RectTransform> calculateChildren { get { return m_ChildrenForSizeFromStartToEnd; } }

        private List<RectTransform> m_ChildrenForSnappingFromStartToEnd = new List<RectTransform>();
        public List<RectTransform> snapChildren { get { return m_ChildrenForSnappingFromStartToEnd; } }

        private Scroller m_Scroller;
        public Scroller scroller
        {
            get
            {
                if (m_Scroller == null)
                {
                    m_Scroller = new Scroller(m_MinDurationMillis, m_MaxDurationMillis, GetInterpolator());
                }
                return m_Scroller;
            }
            set
            {
                if (m_Scroller != null)
                {
                    m_Scroller.AbortAnimation();
                }
                m_Scroller = value;
            }
        }


        private string filterWhitelistException = "The {0} is set to whitelist and is either empty or contains an empty object. You probably need to assign a child to the {0} or set the {0} to blacklist.";
        private string availableChildrenListEmptyException = "The Content has no children available for {0}. This is probably because they are all blacklisted. You should check what children you have blacklisted in your item filters and if you have Add Inactive Children checked.";
        private string childOutsideValidRegionWarning = "Child: {0} is outside the valid bounds of the content. If this was unintentional move it inside region indicated by the green arrow gizmos. If you see no green arrows turn on Draw Gizmos.";

        private DrivenRectTransformTracker m_Tracker;

        private List<RectTransform> m_AvailableForCalculating = new List<RectTransform>();
        private List<RectTransform> m_AvailableForSnappingTo = new List<RectTransform>();

        private List<Vector2> m_SnapPositions = new List<Vector2>();
        
        private List<float> m_CalculateDistances = new List<float>();
        private List<float> m_SnapDistances = new List<float>();

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        private Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        private RectTransform m_PrevClosestItem;
        private bool m_PrevScrolling;
        private float m_PrevTension;
        private float m_PrevFriction;
        private int m_PrevMinDuration;
        private int m_PrevMaxDuration;

        [NonSerialized]
        private bool m_HasRebuiltLayout = false;
        private bool m_TrackerSetup = false;

        private float m_TotalScrollableLength;
        private Vector2 m_MinPos;
        private Vector2 m_MaxPos;
        private int m_ExtraLoopSpace;

        private bool m_WaitingForEndScrolling;
        private float m_TimeOfLastScroll;

        private Camera m_LastPressedCamera;

        [NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        private RectTransform m_ViewRect;
        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                {
                    m_ViewRect = m_Viewport;
                }
                if (m_ViewRect == null)
                {
                    m_ViewRect = (RectTransform)transform;
                }
                return m_ViewRect;
            }
        }

        private bool m_LayoutGroupWasEnabled;
        private LayoutGroup m_LayoutGroup;
        private bool contentIsLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                m_LayoutGroup = m_Content.GetComponent<LayoutGroup>();
                return m_LayoutGroup;
            }
        }
        
        private bool contentIsHorizonalLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                HorizontalLayoutGroup horizLayoutGroup = m_Content.GetComponent<HorizontalLayoutGroup>();
                return horizLayoutGroup && horizLayoutGroup.enabled;
            }
        }
        
        private bool contentIsVerticalLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                VerticalLayoutGroup vertLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
                return vertLayoutGroup && vertLayoutGroup.enabled;
            }
        }

        private RectTransform m_StartChild;
        private RectTransform firstCalculateChild
        {
            get
            {
                if (m_ChildrenForSizeFromStartToEnd == null)
                {
                    GetChildrenFromStartToEnd();
                }
                if (m_StartChild == null)
                {
                    m_StartChild = m_ChildrenForSizeFromStartToEnd[0];
                }
                return m_StartChild;
            }
        }

        private RectTransform m_EndChild;
        private RectTransform lastCalculateChild
        {
            get
            {
                if (m_ChildrenForSizeFromStartToEnd == null)
                {
                    GetChildrenFromStartToEnd();
                }
                if (m_EndChild == null)
                {
                    m_EndChild = m_ChildrenForSizeFromStartToEnd[m_ChildrenForSizeFromStartToEnd.Count - 1];
                }
                return m_EndChild;
            }
        }

        private int viewRectSize
        {
            get
            {

                Vector3[] viewRectCorners = new Vector3[4];
                viewRect.GetWorldCorners(viewRectCorners);
                return (int)DistanceOnAxis(viewRectCorners[1], viewRectCorners[3], axis);
            }
        }

        private Vector3 contentTopLeft
        {
            get
            {
                Vector3[] contentCorners = new Vector3[4];
                m_Content.GetWorldCorners(contentCorners);
                return contentCorners[1];
            }
        }

        private int axis
        {
            get
            {
                return (int)movementDirection;
            }
        }

        private int inverseAxis
        {
            get
            {
                return 1 - axis;
            }
        }

        private int movementDirectionMult
        {
            get
            {
                return (m_MovementDirection == MovementDirection.Vertical) ? -1 : 1;
            }
        }
        #endregion

        #region Temp
        
        #endregion

        #region SetupScrollSnap
        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar && !m_Loop)
            {
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_HorizontalScrollbarEventsListener = m_HorizontalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_HorizontalScrollbarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_HorizontalScrollbarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
            }
            if (m_VerticalScrollbar && !m_Loop)
            {
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_VerticalScrollBarEventsListener = m_VerticalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_VerticalScrollBarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_VerticalScrollBarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
            }

            if (m_BackButton)
            {
                m_BackButton.onClick.AddListener(OnBack);
            }
            if (m_ForwardButton)
            {
                m_ForwardButton.onClick.AddListener(OnForward);
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

            RebuildLayoutGroups();
            if (contentIsLayoutGroup && Application.isPlaying)
            {
                m_LayoutGroupWasEnabled = m_LayoutGroup.enabled;
                m_LayoutGroup.enabled = false;
            }

            m_Scroller = new Scroller(m_Friction, m_MinDurationMillis, m_MaxDurationMillis, GetInterpolator());

            UpdatePrevData();
            UpdateLayout();
            Loop(Direction.TowardsStart);
            Loop(Direction.TowardsEnd);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                if (m_HorizontalScrollbarEventsListener != null)
                {
                    m_HorizontalScrollbarEventsListener.onPointerDown -= ScrollBarPointerDown;
                    m_HorizontalScrollbarEventsListener.onPointerUp -= ScrollBarPointerUp;
                    DestroyImmediate(m_HorizontalScrollbarEventsListener);
                }
            }
            if (m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                if (m_VerticalScrollBarEventsListener != null)
                {
                    m_VerticalScrollBarEventsListener.onPointerDown -= ScrollBarPointerDown;
                    m_VerticalScrollBarEventsListener.onPointerUp -= ScrollBarPointerUp;
                    DestroyImmediate(m_VerticalScrollBarEventsListener);
                }
            }
            if (m_BackButton)
            {
                m_BackButton.onClick.RemoveListener(OnBack);
            }
            if (m_ForwardButton)
            {
                m_ForwardButton.onClick.RemoveListener(OnForward);
            }

            if (contentIsLayoutGroup && m_LayoutGroupWasEnabled)
            {
                m_LayoutGroup.enabled = true;
            }

            m_HasRebuiltLayout = false;
            m_TrackerSetup = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }
        
        /// <summary>
        /// Updates the Scroll Snap. Use only if you need updated info about the Scroll Snap before it has updated itself (e.g. first frame).
        /// If you need to modify the Scroll Snap (e.g. adding items, manipulating spacing, changing an item's snappability, removing items) please use the provided functions.
        /// </summary>
        public void UpdateLayout()
        {
            if (m_Content == null || m_Content.childCount == 0)
            {
                return;
            }
            
            //OnValidate();
            EnsureLayoutHasRebuilt();
            GetValidChildren();
            SetupDrivenTransforms();
            m_TrackerSetup = true;
            GetChildrenFromStartToEnd();
            GetCalculateDistances();
            
            SetReferencePos(firstCalculateChild);

            ResizeContent();
            GetSnapPositions();
            GetSnapDistances();

            ResetContentPos();
        }

        private void SetupDrivenTransforms()
        {
            Vector2 anchorPos = new Vector2(0, 1);

            m_Tracker.Clear();

            m_Tracker.Add(this, m_Content, DrivenTransformProperties.Anchors);

            m_Content.anchorMax = anchorPos;
            m_Content.anchorMin = anchorPos;

            //So that we can calculate everything correctly
            foreach (RectTransform transform in m_AvailableForCalculating)
            {
                m_Tracker.Add(this, transform, DrivenTransformProperties.Anchors);

                transform.anchorMax = anchorPos;
                transform.anchorMin = anchorPos;
            }
        }

        private void GetValidChildren()
        {
            m_AvailableForCalculating = new List<RectTransform>();
            m_AvailableForSnappingTo = new List<RectTransform>();

            Func<RectTransform, bool> childIsAvailableForCalculating;
            Func<RectTransform, bool> childIsAvailableForSnappingTo;

            if (m_FilterModeForCalculatingSize == FilterMode.WhiteList)
            {
                if (m_CalculatingFilter.Count < 1 || m_CalculatingFilter.Contains(null))
                {
                    throw (new UnassignedReferenceException(string.Format(filterWhitelistException, "Calculate Size Filter")));
                }
                childIsAvailableForCalculating = (RectTransform child) => m_CalculatingFilter.Contains(child) || (m_AddInactiveChildrenToCalculatingFilter && !child.gameObject.activeInHierarchy);
            }
            else
            {
                childIsAvailableForCalculating = (RectTransform child) => !m_CalculatingFilter.Contains(child) && (!m_AddInactiveChildrenToCalculatingFilter || child.gameObject.activeInHierarchy);
            }


            if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
            {
                if (m_SnapPositionsFilter.Count < 1 || m_SnapPositionsFilter.Contains(null))
                {
                    throw (new UnassignedReferenceException(string.Format(filterWhitelistException, "Available Snaps Filter")));
                }
                childIsAvailableForSnappingTo = (RectTransform child) => m_SnapPositionsFilter.Contains(child) || (m_AddInactiveChildrenToSnapPositionsFilter && !child.gameObject.activeInHierarchy);
            }
            else
            {
                childIsAvailableForSnappingTo = (RectTransform child) => !m_SnapPositionsFilter.Contains(child) && (!m_AddInactiveChildrenToSnapPositionsFilter || child.gameObject.activeInHierarchy);
            }

            foreach (RectTransform child in m_Content)
            {

                if (childIsAvailableForCalculating(child))
                {
                    if (child.position.x < contentTopLeft.x || child.position.y > contentTopLeft.y)
                    {
                        Debug.LogWarningFormat(this, childOutsideValidRegionWarning, child.name);
                    }
                    m_AvailableForCalculating.Add(child);
                    if (childIsAvailableForSnappingTo(child))
                    {
                        m_AvailableForSnappingTo.Add(child);
                    }
                }
            }

            if (m_AvailableForCalculating.Count < 1)
            {
                throw (new MissingReferenceException(string.Format(availableChildrenListEmptyException, "calculating")));
            }

            if (m_AvailableForSnappingTo.Count < 1)
            {
                throw (new MissingReferenceException(string.Format(availableChildrenListEmptyException, "snapping to")));
            }
        }

        private void GetChildrenFromStartToEnd()
        {
            m_StartChild = null;
            m_EndChild = null;
            m_ChildrenForSizeFromStartToEnd = new List<RectTransform>();
            foreach (RectTransform child in m_Content)
            {
                if (m_AvailableForCalculating.Contains(child))
                {
                    int insert = m_ChildrenForSizeFromStartToEnd.Count;
                    for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
                    {
                        if (TransformAIsNearerStart(child, m_ChildrenForSizeFromStartToEnd[i]))
                        {
                            insert = i;
                            break;
                        }
                    }
                    m_ChildrenForSizeFromStartToEnd.Insert(insert, child);
                }
            }
        } 

        private void GetCalculateDistances()
        {
            if (m_CalculateDistances.Count == 0 || m_CalculateDistances == null)
            {
                m_CalculateDistances = new List<float>();
                for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count - 1; i++)
                {
                    m_CalculateDistances.Add(DistanceOnAxis(m_ChildrenForSizeFromStartToEnd[i].anchoredPosition, m_ChildrenForSizeFromStartToEnd[i + 1].anchoredPosition, axis));
                }

                m_CalculateDistances.Add(GetCalculateDistance(m_ChildrenForSizeFromStartToEnd.Count - 1, m_EndSpacing, m_ChildrenForSizeFromStartToEnd.Count));
            }
        }

        private void ResizeContent()
        {
            m_ExtraLoopSpace = m_Loop ? (int)(Mathf.Max(m_CalculateDistances.ToArray()) / 2) : 0;
            float halfViewRect = (viewRectSize / 2);
            int paddingStart = (int)(halfViewRect - Mathf.Abs(firstCalculateChild.anchoredPosition[axis])) + m_ExtraLoopSpace;
            int paddingEnd = (int)(halfViewRect - (m_Content.sizeDelta[axis] - Mathf.Abs(lastCalculateChild.anchoredPosition[axis]))) + m_ExtraLoopSpace;

            if (!Application.isPlaying &&  contentIsLayoutGroup)
            {
                if (m_MovementDirection == MovementDirection.Horizontal)
                {
                    m_LayoutGroup.padding.left = m_LayoutGroup.padding.left + paddingStart;
                    m_LayoutGroup.padding.right = m_LayoutGroup.padding.right + paddingEnd;
                    m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x + paddingStart + paddingEnd, m_Content.sizeDelta.y);
                }
                else
                {
                    m_LayoutGroup.padding.top = m_LayoutGroup.padding.top + paddingStart;
                    m_LayoutGroup.padding.bottom = m_LayoutGroup.padding.bottom + paddingEnd;
                    m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, m_Content.sizeDelta.y + paddingStart + paddingEnd);
                }
                RebuildLayoutGroups();
            }
            else
            {
                foreach (RectTransform child in m_ChildrenForSizeFromStartToEnd)
                {
                    if (m_MovementDirection == MovementDirection.Horizontal)
                    {
                        child.anchoredPosition = new Vector2(child.anchoredPosition.x + paddingStart, child.anchoredPosition.y);
                    }
                    else
                    {
                        child.anchoredPosition = new Vector2(child.anchoredPosition.x, child.anchoredPosition.y - paddingStart);
                    }
                }
                float totalSize = Mathf.Abs(lastCalculateChild.anchoredPosition[axis]) + halfViewRect + m_ExtraLoopSpace;
                m_Content.sizeDelta = (movementDirection == MovementDirection.Horizontal) ? new Vector2(totalSize, m_Content.sizeDelta.y) : new Vector2(m_Content.sizeDelta.x, totalSize);
            }
            SetNormalizedPosition(1, 0);
            SetNormalizedPosition(0, 1);
            m_MinPos = m_Content.anchoredPosition;
            SetNormalizedPosition(0, 0);
            SetNormalizedPosition(1, 1);
            m_MaxPos = m_Content.anchoredPosition;
        }

        private void GetSnapPositions()
        {
            m_ChildrenForSnappingFromStartToEnd = new List<RectTransform>();
            m_SnapPositions = new List<Vector2>();
            m_TotalScrollableLength = m_Content.sizeDelta[axis] - viewRectSize;

            for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
            {
                RectTransform child = m_ChildrenForSizeFromStartToEnd[i];

                if (m_AvailableForSnappingTo.Contains(child))
                {
                    m_ChildrenForSnappingFromStartToEnd.Add(child);
                    float normalizedPosition;
                    GetNormalizedPositionOfChild(child, out normalizedPosition);
                    SetNormalizedPosition(normalizedPosition, axis);
                    m_SnapPositions.Add(RoundVector2ToInts(m_Content.anchoredPosition));
                }
            }
        }

        private void GetSnapDistances()
        {
            if (m_SnapDistances.Count == 0 || m_SnapDistances == null)
            {
                m_SnapDistances = new List<float>();
                float currentDistance = 0;
                int index = 0;
                GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[0], out index);

                for (int i = 0; i < m_CalculateDistances.Count; i++)
                {
                    currentDistance += m_CalculateDistances[LoopIndex(index, m_CalculateDistances.Count)];

                    if (m_ChildrenForSnappingFromStartToEnd.Contains(m_ChildrenForSizeFromStartToEnd[LoopIndex(index + 1, m_ChildrenForSizeFromStartToEnd.Count)]))
                    {
                        m_SnapDistances.Add(currentDistance);
                        currentDistance = 0;
                    }

                    index++;
                }
            }
        }
        #endregion

        #region Scrolling
        private void LateUpdate()
        {
            if (!m_Content)
            {
                return;
            }

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            bool loop = LoopBasedOnVelocity();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);


            if (m_WaitingForEndScrolling && Time.time - m_TimeOfLastScroll > m_ScrollDelay)
            {
                m_WaitingForEndScrolling = false;
                SelectSnapPos();
            }

            if (m_Scroller.ComputeScrollOffset())
            {
                m_Content.anchoredPosition = m_Scroller.currentPosition;
            }

            if (!loop)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            m_ClosestSnapPositionIndex = m_SnapPositions.IndexOf(FindClosestSnapPositionToPosition(m_Content.anchoredPosition));

            if (m_Content.anchoredPosition != m_PrevPosition)
            {
                m_OnValueChanged.Invoke(new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition));
            }
            if (closestItem != m_PrevClosestItem)
            {
                m_ClosestSnapPositionChanged.Invoke(m_ClosestSnapPositionIndex);
            }
            if (m_Scroller.isFinished && m_PrevScrolling)
            {
                m_SnappedToItem.Invoke(m_ClosestSnapPositionIndex);
                m_Velocity = Vector2.zero;
            }


            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
            }
            UpdatePrevData();
        }

        private void ScrollBarPointerDown(PointerEventData ped)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.ScrollBar);
        }

        private void ScrollBarPointerUp(PointerEventData ped)
        {
            m_LastPressedCamera = ped.pressEventCamera;
            SelectSnapPos();
        }
        
        public void OnBack()
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.ButtonPress);
            scroller.ForceFinish();
            Vector2 targetPosition = m_SnapPositions[closestSnapPositionIndex];
            targetPosition[axis] = targetPosition[axis] + (movementDirectionMult * m_SnapDistances[LoopIndex(closestSnapPositionIndex - 1, m_SnapDistances.Count)]);
            scroller.StartScroll(m_Content.anchoredPosition, targetPosition, m_ScrollDurationMillis);
            m_TargetItemSelected.Invoke(closestSnapPositionIndex - 1);
        }

        public void OnForward()
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.ButtonPress);
            scroller.ForceFinish();
            Vector2 targetPosition = m_SnapPositions[closestSnapPositionIndex];
            targetPosition[axis] = targetPosition[axis] - (movementDirectionMult * m_SnapDistances[closestSnapPositionIndex]);
            scroller.StartScroll(m_Content.anchoredPosition, targetPosition, m_ScrollDurationMillis);
            m_TargetItemSelected.Invoke(closestSnapPositionIndex + 1);
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
            {
                return;
            }

            if (!m_WaitingForEndScrolling)
            {
                m_LastPressedCamera = data.pressEventCamera;
                m_StartMovementEvent.Invoke(StartMovementEventType.OnScroll);
                m_WaitingForEndScrolling = true;
                m_Scroller.ForceFinish();
            }
            m_TimeOfLastScroll = Time.time;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (m_MovementDirection == MovementDirection.Vertical)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    delta.y = delta.x;
                }
                delta.x = 0;
            }
            else
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    delta.x = delta.y;
                }
                delta.y = 0;
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
            {
                position += CalculateOffset(position - m_Content.anchoredPosition);
            }

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnBeginDrag(PointerEventData ped)
        {
            if (!IsActive())
            {
                return;
            }

            m_StartMovementEvent.Invoke(StartMovementEventType.OnBeginDrag);

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, ped.position, ped.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;

            m_Scroller.ForceFinish();
            m_Velocity = Vector2.zero;
        }
        
        public virtual void OnDrag(PointerEventData ped)
        {
            if (!IsActive())
            {
                return;
            }

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, ped.position, ped.pressEventCamera, out localCursor))
            {
                return;
            }

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 localtemp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, Input.mousePosition, Camera.main, out localtemp);
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;

            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                {
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                }
                if (offset.y != 0)
                {
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
                }
            }

            SetContentAnchoredPosition(position);
        }

        public virtual void OnEndDrag(PointerEventData ped)
        {
            m_LastPressedCamera = ped.pressEventCamera;
            SelectSnapPos();
        }

        private void SelectSnapPos()
        {
            Vector2 offset = CalculateOffset(Vector2.zero);
            Vector2 snapPos;

            if (!m_UseVelocity)
            {
                Vector2 finalPos = m_Content.anchoredPosition;
                if (m_SnapType == SnapType.SnapToNearest)
                {
                    snapPos = FindClosestSnapPositionToPosition(m_Content.anchoredPosition, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                else if (m_SnapType == SnapType.SnapToLastPassed)
                {
                    snapPos = FindLastSnapPositionBeforePosition(m_Content.anchoredPosition, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                else
                {
                    snapPos = FindNextSnapAfterPosition(m_Content.anchoredPosition, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                finalPos.x = Mathf.Clamp(finalPos.x, m_MinPos.x, m_MaxPos.x);
                finalPos.y = Mathf.Clamp(finalPos.y, m_MinPos.y, m_MaxPos.y);
                m_Scroller.StartScroll(m_Content.anchoredPosition, finalPos, m_ScrollDurationMillis);
            }
            else
            {
                Vector2 min = new Vector2(m_MinPos.x - Mathf.Abs(offset.x), m_MinPos.y - Mathf.Abs(offset.y));
                Vector2 max = new Vector2(m_MaxPos.x + Mathf.Abs(offset.x), m_MaxPos.y + Mathf.Abs(offset.y));

                if (m_Loop)
                {
                    m_Scroller.Fling(m_Content.anchoredPosition, m_Velocity);
                }
                else
                {
                    m_Scroller.Fling(m_Content.anchoredPosition, m_Velocity, min, max);
                }
                
                Vector2 finalPos = m_Scroller.finalPosition;
                if (m_SnapType == SnapType.SnapToNearest)
                {
                    snapPos = FindClosestSnapPositionToPosition(finalPos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                else if (m_SnapType == SnapType.SnapToLastPassed)
                {
                    snapPos = FindLastSnapPositionBeforePosition(finalPos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                else
                {
                    snapPos = FindNextSnapAfterPosition(finalPos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop);
                    finalPos[axis] = snapPos[axis];
                }
                finalPos[inverseAxis] = Mathf.Clamp(finalPos[inverseAxis], m_MinPos[inverseAxis], m_MaxPos[inverseAxis]);
                m_Scroller.SetFinalPosition(finalPos);
            }
            m_TargetItemSelected.Invoke(GetSnapIndexOfSnapPosition(snapPos, GetDirectionFromVelocity(m_Velocity, axis)));
        }
        #endregion

        #region Looping
        private bool LoopBasedOnVelocity()
        {
            if (m_Velocity[axis] == 0)
            {
                return false;
            }

            return Loop(GetDirectionFromVelocity(m_Velocity, axis));
        }

        private bool Loop(Direction direction)
        {
            if (!m_Loop || !Application.isPlaying)
            {
                return false;
            }

            bool looped = false;
            float distance = (m_TotalScrollableLength / 2f);
            Vector2 totalOffset = Vector2.zero;
            Vector2 contentStartSize = m_Content.sizeDelta;

            if (direction == Direction.TowardsStart)
            {
                m_ChildrenForSizeFromStartToEnd.Reverse();
            }

            for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
            {
                RectTransform child = m_ChildrenForSizeFromStartToEnd[i];
                Vector3 childLocation = viewRect.InverseTransformPoint(child.position);
                
                bool loopAtEnd = (direction == Direction.TowardsStart) && LoopAtEnd(childLocation, distance);
                bool loopAtStart = (direction == Direction.TowardsEnd) && LoopAtStart(childLocation, distance);

                if (loopAtEnd || loopAtStart)
                {
                    looped = true;

                    if (m_ChildrenForSizeFromStartToEnd.Count > 1)
                    {
                        SetReferencePos(m_ChildrenForSizeFromStartToEnd[LoopIndex(i + 1, m_ChildrenForSizeFromStartToEnd.Count)]);
                    }

                    if (loopAtEnd)
                    {
                        if (m_ChildrenForSizeFromStartToEnd.Count == 1)
                        {
                            Vector2 newContentPos = m_Content.anchoredPosition;
                            totalOffset[axis] = -m_CalculateDistances[0] * movementDirectionMult;
                            newContentPos[axis] = m_Content.anchoredPosition[axis] - (m_CalculateDistances[0] * movementDirectionMult);
                            m_Content.anchoredPosition = newContentPos;
                        }
                        else
                        {
                            Vector3 newChildPos = child.anchoredPosition;
                            newChildPos[axis] = movementDirectionMult * ((viewRectSize / 2) + m_ExtraLoopSpace);
                            child.anchoredPosition = newChildPos;

                            float movementAmout = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            for (int j = 0; j < m_ChildrenForSizeFromStartToEnd.Count; j++)
                            {
                                if (i == j)
                                {
                                    continue;
                                }

                                RectTransform siblingBeingMoved = m_ChildrenForSizeFromStartToEnd[j];
                                Vector2 newSiblingPos = siblingBeingMoved.anchoredPosition;
                                newSiblingPos[axis] = newSiblingPos[axis] + (movementAmout * movementDirectionMult);
                                siblingBeingMoved.anchoredPosition = newSiblingPos;
                            }

                            float cacheCalculateDistance = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            m_CalculateDistances.RemoveAt(m_CalculateDistances.Count - 1);
                            m_CalculateDistances.Insert(0, cacheCalculateDistance);

                            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
                            {
                                float cacheSnapDistance = m_SnapDistances[m_SnapDistances.Count - 1];
                                m_SnapDistances.RemoveAt(m_SnapDistances.Count - 1);
                                m_SnapDistances.Insert(0, cacheSnapDistance);
                            }
                        }
                    }
                    else if (loopAtStart)
                    {
                        if (m_ChildrenForSizeFromStartToEnd.Count == 1)
                        {
                            Vector2 newContentPos = m_Content.anchoredPosition;
                            totalOffset[axis] = m_CalculateDistances[0] * movementDirectionMult;
                            newContentPos[axis] = m_Content.anchoredPosition[axis] + (m_CalculateDistances[0] * movementDirectionMult);
                            m_Content.anchoredPosition = newContentPos;
                        }
                        else
                        {
                            float movementAmount = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            totalOffset[axis] = totalOffset[axis] + (movementAmount * movementDirectionMult);

                            Vector3 newChildPos = child.anchoredPosition;
                            newChildPos[axis] = m_ChildrenForSizeFromStartToEnd[LoopIndex(i - 1, m_ChildrenForSizeFromStartToEnd.Count)].anchoredPosition[axis] + (movementAmount * movementDirectionMult);
                            child.anchoredPosition = newChildPos;

                            float cacheCalculateDistance = m_CalculateDistances[0];
                            m_CalculateDistances.RemoveAt(0);
                            m_CalculateDistances.Insert(m_CalculateDistances.Count, cacheCalculateDistance);

                            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
                            {
                                float cacheSnapDistance = m_SnapDistances[0];
                                m_SnapDistances.RemoveAt(0);
                                m_SnapDistances.Insert(m_SnapDistances.Count, cacheSnapDistance);
                            }
                        }
                    }

                    if (m_ChildrenForSizeFromStartToEnd.Count > 1)
                    {
                        totalOffset += ResetContentPos();
                    }
                }
            }

            if (looped)
            {
                UpdateLayout();

                Vector2 localCursor;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, Input.mousePosition, m_LastPressedCamera, out localCursor);
                m_PointerStartLocalCursor[axis] = localCursor[axis];
                m_ContentStartPosition[axis] = m_Content.anchoredPosition[axis];


                if (!m_Scroller.isFinished)
                {
                    Vector2 sizeShift = ((contentStartSize - m_Content.sizeDelta) / 2);
                    if ((m_MovementDirection == MovementDirection.Horizontal && direction == Direction.TowardsEnd) || (m_MovementDirection == MovementDirection.Vertical && direction == Direction.TowardsStart))
                    {
                        sizeShift = -sizeShift;
                    }
                    m_Scroller.ShiftAnimation(totalOffset - sizeShift);
                }

            }

            if (direction == Direction.TowardsStart)
            {
                m_ChildrenForSizeFromStartToEnd.Reverse();
            }

            return looped;
        }

        private bool LoopAtEnd(Vector3 childLocation, float distance)
        {
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                return childLocation[axis] >= distance;
            }
            else
            {
                return childLocation[axis] <= -distance;
            }
        }

        private bool LoopAtStart(Vector3 childLocation, float distance)
        {
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                return childLocation[axis] <= -distance;
            }
            else
            {
                return childLocation[axis] >= distance;
            }
        }

        private void ShiftChildrenForEndSpacing(int calculateIndex, float spacing) //does not manipulate calculateDistances list
        {
            if (calculateIndex + 1 < m_ChildrenForSizeFromStartToEnd.Count)
            {
                RectTransform child = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex, m_ChildrenForSizeFromStartToEnd.Count)];
                RectTransform childAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];

                float movementAmount = (child.anchoredPosition[axis] + (movementDirectionMult * GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1))) - childAfterChild.anchoredPosition[axis];
                for (int i = calculateIndex + 1; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
                {
                    Vector2 newSiblingPos = m_ChildrenForSizeFromStartToEnd[i].anchoredPosition;
                    newSiblingPos[axis] = newSiblingPos[axis] + movementAmount;
                    m_ChildrenForSizeFromStartToEnd[i].anchoredPosition = newSiblingPos;
                }
            }
        }

        private void SetParentToContent(RectTransform child)
        {
            child.SetParent(m_Content, false);
            m_Tracker.Add(this, child, DrivenTransformProperties.Anchors);
            child.anchorMax = new Vector2(0, 1);
            child.anchorMin = new Vector2(0, 1);
        }

        private void SetParentToNewParent(RectTransform child, RectTransform newParent)
        {
            if (newParent == null)
            {
                child.SetParent(GetCanvasTransform(child.parent));
            }
            else
            {
                child.SetParent(newParent);
            }
        }

        private RectTransform GetTrackingChild(int calculateIndex, LockMode lockMode)
        {
            if (lockMode == LockMode.Before)
            {
                return m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            }
            else
            {
                return m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];
            }
        }

        private List<float> CombineDistance(int middleIndex, List<float> list)
        {
            float combinedDistance = list[LoopIndex(middleIndex - 1, list.Count)] + list[LoopIndex(middleIndex, list.Count)];
            list[LoopIndex(middleIndex - 1, list.Count)] = combinedDistance;
            list.RemoveAt(middleIndex);
            return list;
        }

        private void RemoveViaCombine(RectTransform child)
        {
            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);
            CombineDistance(calculateIndexOfChild, m_CalculateDistances);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndexOfChild = 0;
                GetSnapIndexOfChild(child, out snapIndexOfChild);
                CombineDistance(snapIndexOfChild, m_SnapDistances);
            }
        }

        private void RemoveViaShift(RectTransform child, float calculateSpacing, LockMode lockMode)
        {
            calculateSpacing = Mathf.Max(0, calculateSpacing);

            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);

            int snapIndexOfChild = 0;
            GetSnapIndexOfChild(child, out snapIndexOfChild);

            SetReferencePos(GetTrackingChild(calculateIndexOfChild, lockMode));

            RectTransform calculateBeforeChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndexOfChild - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            RectTransform calculateAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndexOfChild + 1, m_ChildrenForSizeFromStartToEnd.Count)];

            m_CalculateDistances[LoopIndex(calculateIndexOfChild - 1, m_ChildrenForSizeFromStartToEnd.Count)] = GetCalculateDistance(calculateIndexOfChild - 1, calculateSpacing, calculateIndexOfChild + 1);
            m_CalculateDistances.RemoveAt(calculateIndexOfChild);

            m_ChildrenForSizeFromStartToEnd.Remove(child);
            ShiftChildrenForEndSpacing(calculateIndexOfChild - 1, calculateSpacing);

            m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild + 1);
            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                m_SnapDistances.RemoveAt(snapIndexOfChild);
            }
        }

        private void InsertChild(RectTransform child, int calculateIndex, float nonAxisPosition, float startSpacing, float endSpacing, RectTransform trackingChild, bool snappable)
        {
            SetReferencePos(trackingChild);

            startSpacing = Mathf.Max(0, startSpacing);
            endSpacing = Mathf.Max(0, endSpacing);
            RectTransform calculateBeforeChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            RectTransform calculateAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];

            Vector2 newChildPos = Vector2.zero;
            newChildPos[inverseAxis] = nonAxisPosition;
            if (calculateIndex > 0)
            {
                newChildPos[axis] = calculateBeforeChild.anchoredPosition[axis] + (movementDirectionMult * GetCalculateDistance(calculateIndex - 1, startSpacing, calculateIndex));
            }
            child.anchoredPosition = newChildPos;

            ShiftChildrenForEndSpacing(calculateIndex, endSpacing);

            int snapIndexOfChild = 0;
            GetSnapIndexOfChild(child, out snapIndexOfChild);
            m_ChildrenForSnappingFromStartToEnd.Insert(snapIndexOfChild, child);

            m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, startSpacing, calculateIndex);
            m_CalculateDistances.Insert(calculateIndex, GetCalculateDistance(calculateIndex, endSpacing, calculateIndex + 1));

            if (snappable)
            {
                m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild);
                m_SnapDistances.Insert(snapIndexOfChild, GetSnapDistance(snapIndexOfChild, snapIndexOfChild + 1));
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
            }
            else
            {
                m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild) + GetSnapDistance(snapIndexOfChild, snapIndexOfChild + 1);
                if (m_FilterModeForSnapPositions == FilterMode.BlackList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
            }

            if (m_FilterModeForCalculatingSize == FilterMode.WhiteList)
            {
                m_CalculatingFilter.Add(child);
            }

            ResetContentPos();

            UpdateLayout();

            if (!m_Scroller.isFinished)
            {
                scroller.ForceFinish();
                SelectSnapPos();
            }
        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Gets the normalized position of the scroll snap when it is snapped to the child. 
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetNormalizedPositionOfChild(RectTransform child, out float normalizedPosition) //bool = is child of content
        {
            Vector2 startPos = Vector2.zero;
            if (m_MovementDirection == MovementDirection.Vertical)
            {
                startPos = new Vector2(firstCalculateChild.anchoredPosition.x + m_ExtraLoopSpace, firstCalculateChild.anchoredPosition.y + m_ExtraLoopSpace);
            }
            else
            {
                startPos = new Vector2(firstCalculateChild.anchoredPosition.x - m_ExtraLoopSpace, firstCalculateChild.anchoredPosition.y - m_ExtraLoopSpace);
            }
            normalizedPosition = DistanceOnAxis(child.anchoredPosition, startPos/*firstCalculateChild.anchoredPosition*/, axis) / m_TotalScrollableLength;
            return child.parent == m_Content;
        }

        /// <summary>
        /// Gets the position of the content in the content's local coordinates when it is snapped to the child. 
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetPositionOfChild(RectTransform child, out Vector2 position) //bool = is child of content
        {
            Vector2 anchoredPos = m_Content.anchoredPosition;
            float normalizedPos;
            GetNormalizedPositionOfChild(child, out normalizedPos);
            SetNormalizedPosition(normalizedPos, axis);
            position = m_Content.anchoredPosition;
            m_Content.anchoredPosition = anchoredPos;
            return child.parent == m_Content;
        }

        /// <summary>
        /// Gets the index of the child, based on the snappable items. 
        /// </summary>
        /// <returns>Returns true if the the supplied RectTransform is a valid snap position.</returns>
        public bool GetSnapIndexOfChild(RectTransform child, out int index) //bool = is it a snap position
        {
            index = 0;
            foreach (RectTransform rect in m_ChildrenForSnappingFromStartToEnd)
            {
                if(TransformAIsNearerStart(rect, child))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return m_ChildrenForSnappingFromStartToEnd.Contains(child);
        }

        /// <summary>
        /// Gets the index of the child, based on the calculable items.
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a calculable item.</returns>
        public bool GetCalculateIndexOfChild(RectTransform child, out int index)
        {
            index = 0;
            foreach (RectTransform rect in m_ChildrenForSizeFromStartToEnd)
            {
                if (TransformAIsNearerStart(rect, child))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return (m_ChildrenForSizeFromStartToEnd.Contains(child));
        }

        /// <summary>
        /// Gets the normalized position of the content when snapped to the supplied snap position. 
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetNormalizedPositionOfSnapPosition(int snapPositionIndex, out float normalizedPosition)
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                RectTransform child;
                GetChildAtSnapIndex(snapPositionIndex, out child);
                GetNormalizedPositionOfChild(child, out normalizedPosition);
                return true;
            }
            normalizedPosition = 0f;
            return false;
        }

        /// <summary>
        /// Gets the position of the content in the content's local coordinates when snapped to the snap position at the specified coordinates. 
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetSnapPositionAtIndex(int snapPositionIndex, out Vector2 location) //bool = is it a snap position
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                location = m_SnapPositions[snapPositionIndex];
                return true;
            }
            location = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Gets the item at the supplied snap index.
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetChildAtSnapIndex(int snapPositionIndex, out RectTransform child)
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                child = m_ChildrenForSnappingFromStartToEnd[snapPositionIndex];
                return true;
            }
            child = null;
            return false;
        }

        /// <summary>
        /// Gets the item at the supplied calculate index.
        /// </summary>
        /// <param name="calculateIndex"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public bool GetChildAtCalculateIndex(int calculateIndex, out RectTransform child)
        {
            if(calculateIndex >= 0 && calculateIndex < m_ChildrenForSizeFromStartToEnd.Count)
            {
                child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
                return true;
            }
            child = null;
            return false;
        }

        /// <summary>
        /// Insert a new child into the Scroll Snap.
        /// </summary>
        /// <param name="child">The RectTransform to be inserted.</param>
        /// <param name="calculateIndex">What index you want the child to be inserted at, based on all the calculable items.</param>
        /// <param name="nonAxisPosition">The position of the child on the non-scrolling axis relative to the top left of the content.</param>
        /// <param name="startSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the child being added.</param>
        /// <param name="endSpacing">The spacing between the start (top/left) edge of the next calculable child, and the end (bottom/right) edge of the child being added.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        /// <param name="snappable">If the new child should be snappable.</param>
        public void InsertChild(RectTransform child, int calculateIndex, float nonAxisPosition, float startSpacing, float endSpacing, LockMode lockMode, bool snappable) //nonAxisPosition is relative to the top left of the content
        {
            if (child == null)
            {
                return;
            }

            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count);
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                nonAxisPosition = Mathf.Min(0, nonAxisPosition);
            }
            else
            {
                nonAxisPosition = Mathf.Max(0, nonAxisPosition);
            }

            SetParentToContent(child);
            m_ChildrenForSizeFromStartToEnd.Insert(calculateIndex, child);

            InsertChild(child, calculateIndex, nonAxisPosition, startSpacing, endSpacing, GetTrackingChild(calculateIndex, lockMode), snappable);
        }

        /// <summary>
        /// Insert a new child into the Scroll Snap.
        /// </summary>
        /// <param name="child">The RectTransform to be inserted.</param>
        /// <param name="worldPos">The position, in world coordinates, the new child will be placed at.</param>
        /// <param name="startSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the child being added.</param>
        /// <param name="endSpacing">The spacing between the start (top/left) edge of the next calculable child, and the end (bottom/right) edge of the child being added.</param>
        /// <param name="snappable">If the new child should be snappable</param>
        public void InsertChild(RectTransform child, Vector3 worldPos, float startSpacing, float endSpacing, bool snappable)
        {
            if (child == null)
            {
                return;
            }

            Matrix4x4 matrix = Matrix4x4.TRS(m_ContentWorldCorners[1], m_Content.rotation, m_Content.localScale);
            Vector2 posRelativeToContentTopLeft = matrix.inverse.MultiplyPoint3x4(worldPos);

            posRelativeToContentTopLeft.x = Mathf.Max(0, posRelativeToContentTopLeft.x);
            posRelativeToContentTopLeft.y = Mathf.Min(0, posRelativeToContentTopLeft.y);

            SetParentToContent(child);
            child.anchoredPosition = posRelativeToContentTopLeft;

            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);
            m_ChildrenForSizeFromStartToEnd.Insert(calculateIndexOfChild, child);

            InsertChild(child, calculateIndexOfChild, posRelativeToContentTopLeft[inverseAxis], startSpacing, endSpacing, child, snappable);
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The distances between items will remain the same and the child will be deleted.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        public void RemoveChild(RectTransform child)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaCombine(child);

            m_SnapPositionsFilter.Remove(child);
            m_CalculatingFilter.Remove(child);

            Destroy(child.gameObject);

            UpdateLayout();
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The child will be deleted.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="calculateSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void RemoveChild(RectTransform child, float calculateSpacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaShift(child, calculateSpacing, lockMode);

            m_SnapPositionsFilter.Remove(child);
            m_CalculatingFilter.Remove(child);

            Destroy(child.gameObject);

            ResetContentPos();

            UpdateLayout();
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The distances between items will remain the same and the child will be reparented to the newParent.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="newParent">The RectTransform the child will be parented to, if the newParent is null the new child will be reparented to the parent Canvas.</param>
        public void RemoveChild(RectTransform child, RectTransform newParent) //when newParent null sets to parent parent canvas
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaCombine(child);

            SetParentToNewParent(child, newParent);

            UpdateLayout();
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The child will be reparented to the newParent.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="newParent">The RectTransform the child will be parented to, if the newParent is null the new child will be reparented to the parent Canvas.</param>
        /// <param name="calculateSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void RemoveChild(RectTransform child, RectTransform newParent, float calculateSpacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaShift(child, calculateSpacing, lockMode);

            m_SnapPositionsFilter.Remove(child);
            m_CalculatingFilter.Remove(child);

            SetParentToNewParent(child, newParent);

            ResetContentPos();

            UpdateLayout();
        }

        /// <summary>
        /// Tells the Scroll Snap whether it should snap to this child. If the child is not calculable it cannot be snapped to.
        /// Used for "Locking" or "Unlocking" Items that are already included in the Scroll Snap. (e.g. in a level/map menu)
        /// If you would like to add/remove a child use the Insert/Remove child functions.
        /// </summary>
        /// <param name="child">The child you would like to change the snappability of.</param>
        /// <param name="snappable">Whether the child should be snapped to or not.</param>
        public void SetChildSnappability(RectTransform child, bool snappable)
        {
            if (m_ChildrenForSnappingFromStartToEnd.Contains(child) == snappable || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);

            int snapIndex = 0;
            GetSnapIndexOfChild(child, out snapIndex);

            if (snappable)
            {
                m_ChildrenForSnappingFromStartToEnd.Insert(snapIndex, child);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
                m_SnapDistances.Insert(snapIndex, GetSnapDistance(snapIndex, snapIndex + 1));
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                else
                {
                    m_SnapPositionsFilter.Remove(child);
                }
            }
            else
            {
                CombineDistance(snapIndex, m_SnapDistances);
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Remove(child);
                }
                else
                {
                    m_SnapPositionsFilter.Add(child);
                }
            }

            UpdateLayout();
        }

        /// <summary>
        /// Used only in the case of adding a decorative object at runtime (an item that never has been and never will be calculable).
        /// If you would like to remove a calculable item use the RemoveChild functions.
        /// </summary>
        /// <param name="transform">The RectTransform you would like to set uncalculable.</param>
        public void SetRectTransformUncalculable(RectTransform transform)
        {
            if (m_FilterModeForCalculatingSize == FilterMode.BlackList)
            {
                m_CalculatingFilter.Add(transform);
            }
        }

        /// <summary>
        /// Sets the spacing between the child and the next calculable item.
        /// </summary>
        /// <param name="child">The child you would like to change the spacing of.</param>
        /// <param name="spacing">The spacing between the end (bottom/right) edge of the child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemEndSpacing(RectTransform child, float spacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            spacing = Mathf.Max(spacing, 0);
            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex, spacing);
            m_CalculateDistances[calculateIndex] = GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[snapIndex] = GetSnapDistance(snapIndex, snapIndex + 1);
            }

            ResetContentPos();
            UpdateLayout();
        }

        /// <summary>
        /// Sets the spacing between the child at the calculateIndex and the next calculable item.
        /// </summary>
        /// <param name="calculateIndex">The index of the child you would like to modify, based on the calculable items.</param>
        /// <param name="spacing">The spacing between the end (bottom/right) edge of the child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemEndSpacing(int calculateIndex, float spacing, LockMode lockMode)
        {
            spacing = Mathf.Max(spacing, 0);
            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count - 1);
            RectTransform child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex, spacing);
            m_CalculateDistances[calculateIndex] = GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[snapIndex] = GetSnapDistance(snapIndex, snapIndex + 1);
            }

            ResetContentPos();
            UpdateLayout();
        }

        /// <summary>
        /// Sets the spacing between the child and the previous calculable item.
        /// </summary>
        /// <param name="child">The child you would like to change the spacing of.</param>
        /// <param name="spacing">The spacing between the start (top/left) edge of the child, and the end (bottom/right) edge of the previous calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemStartSpacing(RectTransform child, float spacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            spacing = Mathf.Max(spacing, 0);
            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex - 1, spacing);
            m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, spacing, calculateIndex);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
            }

            ResetContentPos();
            UpdateLayout();
        }

        /// <summary>
        /// Sets the spacing between the child and the previous calculable item.
        /// </summary>
        /// <param name="calculateIndex">The index of the child you would like to modify, based on the calculable items.</param>
        /// <param name="spacing">The spacing between the start (top/left) edge of the child, and the end (bottom/right) edge of the previous calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemStartSpacing(int calculateIndex, float spacing, LockMode lockMode)
        {
            spacing = Mathf.Max(spacing, 0);
            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count - 1);
            RectTransform child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex - 1, spacing);
            m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, spacing, calculateIndex);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
            }

            ResetContentPos();
            UpdateLayout();
        }
        
        /// <summary>
        /// Scrolls to the supplied index in the supplied duration of time.
        /// </summary>
        /// <param name="index">The index of the snap position you want to scroll to.</param>
        /// <param name="durationMillis">The duration of the scroll in milliseconds.</param>
        public void ScrollToSnapPosition(int index, int durationMillis) //zero based index
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = m_SnapPositions[index][axis];
            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, durationMillis);
            m_TargetItemSelected.Invoke(index);
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the end position in the supplied duration of time.
        /// </summary>
        /// <param name="endPos">The reference end position of the content, in the content's local coordinates.</param>
        /// <param name="durationMillis">The duration of the scroll in milliseconds.</param>
        public void ScrollToNearestSnapPosition(Vector2 endPos, int durationMillis)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = FindClosestSnapPositionToPosition(endPos)[axis];
            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, durationMillis);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(targetPosition));
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the normalized position in the supplied duration of time.
        /// </summary>
        /// <param name="normalizedPos">The reference end position of the content, normalized.</param>
        /// <param name="durationMillis">The duration of the scroll in milliseconds.</param>
        public void ScrollToNearestSnapPosition(float normalizedPos, int durationMillis)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 anchoredPos = m_Content.anchoredPosition;
            SetNormalizedPosition(normalizedPos, axis);
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = FindClosestSnapPositionToPosition(m_Content.anchoredPosition)[axis];
            m_Scroller.StartScroll(anchoredPos, targetPosition, durationMillis);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(targetPosition));
        }

        /// <summary>
        /// Flings to the supplied index at the supplied velocity.
        /// </summary>
        /// <param name="index">The index of the snap position you want to scroll to.</param>
        /// <param name="velocity">The velocity the scroll snap will move at in units per second, in the content's local space.</param>
        public void FlingToSnapPosition(int index, float velocity)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 velocityV2 = Vector2.zero;
            velocityV2[axis] = velocity;
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = m_SnapPositions[index][axis];

            Vector2 offset = CalculateOffset(Vector2.zero);
            Vector2 min = new Vector2(m_MinPos.x - offset.x, m_MinPos.y - offset.y);
            Vector2 max = new Vector2(m_MaxPos.x - offset.x, m_MaxPos.y - offset.y);
            m_Scroller.Fling(m_Content.anchoredPosition, velocityV2, min, max);
            m_Scroller.SetFinalPosition(targetPosition);
            m_TargetItemSelected.Invoke(index);
        }

        /// <summary>
        /// Flings to the nearest snap position to the end position at the supplied velocity.
        /// </summary>
        /// <param name="endPos">The reference end position of the content, in the content's local coordinates.</param>
        /// <param name="velocity">The velocity the scroll snap will move at in units per second, in the content's local space.</param>
        public void FlingToNearestSnapPosition(Vector2 endPos, float velocity)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 velocityV2 = Vector2.zero;
            velocityV2[axis] = velocity;
            Vector2 finalPosition = m_Content.anchoredPosition;
            Vector2 snapPos = FindClosestSnapPositionToPosition(endPos);
            finalPosition[axis] = snapPos[axis];

            Vector2 offset = CalculateOffset(Vector2.zero);
            Vector2 min = new Vector2(m_MinPos.x - offset.x, m_MinPos.y - offset.y);
            Vector2 max = new Vector2(m_MaxPos.x - offset.x, m_MaxPos.y - offset.y);
            m_Scroller.Fling(m_Content.anchoredPosition, velocityV2, min, max);
            m_Scroller.SetFinalPosition(finalPosition);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(snapPos));
        }

        /// <summary>
        /// Flings to the nearest snap position to the normalized position at the supplied velocity.
        /// </summary>
        /// <param name="normalizedPos">The reference end position of the content, normalized.</param>
        /// <param name="velocity">The velocity the scroll snap will move at in units per second, in the content's local space.</param>
        public void FlingToNearestSnapPosition(float normalizedPos, float velocity)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 velocityV2 = Vector2.zero;
            velocityV2[axis] = velocity;
            Vector2 anchoredPos = m_Content.anchoredPosition;
            SetNormalizedPosition(normalizedPos, axis);
            Vector2 finalPos = m_Content.anchoredPosition;
            Vector2 snapPos = FindClosestSnapPositionToPosition(finalPos);
            finalPos[axis] = snapPos[axis];

            Vector2 offset = CalculateOffset(Vector2.zero);
            Vector2 min = new Vector2(m_MinPos.x - offset.x, m_MinPos.y - offset.y);
            Vector2 max = new Vector2(m_MaxPos.x - offset.x, m_MaxPos.y - offset.y);
            m_Scroller.Fling(m_Content.anchoredPosition, velocityV2, min, max);
            m_Scroller.SetFinalPosition(finalPos);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(snapPos));
        }

        /// <summary>
        /// Flings to the nearest snap position to where the scroll snap would land after flinging at the supplied velocity.
        /// </summary>
        /// <param name="velocity">The velocity the scroll snap will move at in units per second, in the content's local space.</param>
        public void FlingToNearestSnapPosition(float velocity)
        {

            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();

            Vector2 velocityV2 = Vector2.zero;
            velocityV2[axis] = velocity;

            Vector2 offset = CalculateOffset(Vector2.zero);
            Vector2 min = new Vector2(m_MinPos.x - offset.x, m_MinPos.y - offset.y);
            Vector2 max = new Vector2(m_MaxPos.x - offset.x, m_MaxPos.y - offset.y);
            m_Scroller.Fling(m_Content.anchoredPosition, velocityV2, min, max);

            Vector2 finalPos = m_Scroller.finalPosition;
            Vector2 snapPos = FindClosestSnapPositionToPosition(finalPos);
            finalPos[axis] = snapPos[axis];
            m_Scroller.SetFinalPosition(finalPos);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(snapPos));
        }

        #endregion

        #region Calculations
        private Vector2 FindClosestSnapPositionToPosition(Vector2 contentEndPositon, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();

            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;
            
            float distance = DistanceOnAxis(contentEndPositon, snapPos, axis);

            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                if (direction == Direction.TowardsEnd)
                {
                    snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                    if (DistanceOnAxis(contentEndPositon, snapPos, axis) > distance)
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        distance = DistanceOnAxis(contentEndPositon, snapPos, axis);
                        distanceIndex++;
                    }
                }
                else
                {
                    snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                    if (DistanceOnAxis(contentEndPositon, snapPos, axis) > distance)
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        distance = DistanceOnAxis(contentEndPositon, snapPos, axis);
                        distanceIndex--;
                    }
                }
            }
            return snapPos;
        }

        private Vector2 FindClosestSnapPositionToPosition(Vector2 contentEndPosition)
        {
            Vector3 closest = Vector3.zero;
            float distance = Mathf.Infinity;

            foreach (Vector2 snapPosition in m_SnapPositions)
            {
                if (DistanceOnAxis(contentEndPosition, snapPosition, axis) < distance)
                {
                    distance = DistanceOnAxis(contentEndPosition, snapPosition, axis);
                    closest = snapPosition;
                }
            }
            return closest;
        }

        private Vector2 FindLastSnapPositionBeforePosition(Vector2 contentEndPosition, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();

            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;

            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                if (direction == Direction.TowardsEnd)
                {
                    if (ContentPosAIsNearerEnd(snapPos, contentEndPosition))
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex - 1, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex++;
                    }
                }
                else
                {
                    if (ContentPosAIsNearerStart(snapPos, contentEndPosition))
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex + 1, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex--;
                    }
                }
            }

            return snapPos;
        }

        private Vector2 FindNextSnapAfterPosition(Vector2 contentEndPosition, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();
            
            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;

            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                //if we're moving towards the end we want to go through the items from start -> end (& beyond for looping)
                if (direction == Direction.TowardsEnd)
                {
                    if(ContentPosAIsNearerEnd(snapPos, contentEndPosition)) //we have found the one that is beyond the end point
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex++;
                    }
                }
                //if we're moving towards the start we want to go through the items from end -> start (& beyond for looping)
                else
                {
                    if (ContentPosAIsNearerStart(snapPos, contentEndPosition)) //we have found the one that is beyond the end point
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex--;
                    }
                }
            }
            return snapPos;
        }

        private int GetSnapIndexOfSnapPosition(Vector2 snapPosition, Direction direction)
        {
            Vector2 pos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int index = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;
            while (true)
            {
                if (direction == Direction.TowardsEnd)
                {
                    if (ContentPosAIsNearerStart(pos, snapPosition))
                    {
                        pos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(index, m_SnapDistances.Count)];
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (ContentPosAIsNearerEnd(pos, snapPosition))
                    {
                        pos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(index, m_SnapDistances.Count)];
                        index--;
                    }
                    else
                    {
                        index++;
                        break;
                    }
                }
            }

            return LoopIndex(index, m_SnapDistances.Count);
        }

        private Direction GetDirectionFromVelocity(Vector2 velocity, int axis)
        {
            if (axis == 0)
            {
                if (Mathf.Sign(velocity[axis]) > 0)
                {
                    return Direction.TowardsStart;
                }
                else
                {
                    return Direction.TowardsEnd;
                }
            }
            else
            {
                if (Mathf.Sign(velocity[axis]) > 0)
                {
                    return Direction.TowardsEnd;
                }
                else
                {
                    return Direction.TowardsStart;
                }
            }
        }

        private float DistanceOnAxis(Vector2 posOne, Vector2 posTwo, int axis)
        {
            return Mathf.Abs(posOne[axis] - posTwo[axis]);
        }

        private bool TransformAIsNearerStart(RectTransform transformA, RectTransform transformB)
        {
            if (transformA == null)
            {
                return false;
            }

            if (transformB == null)
            {
                return true;
            }

            if (movementDirection == MovementDirection.Horizontal)
            {
                return transformA.anchoredPosition[axis] < transformB.anchoredPosition[axis];
            }
            else
            {
                return transformA.anchoredPosition[axis] > transformB.anchoredPosition[axis];
            }
        }

        private bool ContentPosAIsNearerStart(Vector2 posA, Vector2 posB)
        {
            if (movementDirection == MovementDirection.Horizontal)
            {
                return posA[axis] > posB[axis];
            }
            else
            {
                return posA[axis] < posB[axis];
            }
        }

        private bool ContentPosAIsNearerEnd(Vector2 posA, Vector2 posB)
        {
            if (movementDirection == MovementDirection.Horizontal)
            {
                return posA[axis] < posB[axis];
            }
            else
            {
                return posA[axis] > posB[axis];
            }
        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        private Vector2 RoundVector2ToInts(Vector2 vector)
        {
            return new Vector2((int)vector.x, (int)vector.y);
        }

        private Interpolator GetInterpolator()
        {
            Interpolator interpolator = new Scroller.ViscousFluidInterpolator();
            switch (m_InterpolatorType)
            {
                case InterpolatorType.Accelerate:
                    interpolator = new Scroller.AccelerateInterpolator();
                    break;
                case InterpolatorType.AccelerateDecelerate:
                    interpolator = new Scroller.AccelerateDecelerateInterpolator();
                    break;
                case InterpolatorType.Anticipate:
                    interpolator = new Scroller.AnticipateInterpolator(m_Tension);
                    break;
                case InterpolatorType.AnticipateOvershoot:
                    interpolator = new Scroller.AnticipateOvershootInterpolator(m_Tension);
                    break;
                case InterpolatorType.Decelerate:
                    interpolator = new Scroller.DecelerateInterpolator();
                    break;
                case InterpolatorType.Linear:
                    interpolator = new Scroller.LinearInterpolator();
                    break;
                case InterpolatorType.Overshoot:
                    interpolator = new Scroller.OvershootInterpolator(m_Tension);
                    break;
            }

            return interpolator;
        }
        
        /// <summary>
        /// The calculate distances must be calculated and both children must be added before calling this.
        /// </summary>
        private float GetSnapDistance(int startSnapIndex, int endSnapIndex)
        {
            int startCalculateIndex = 0;
            GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[LoopIndex(startSnapIndex, m_ChildrenForSnappingFromStartToEnd.Count)], out startCalculateIndex);

            int endCalculateIndex = 0;
            GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[LoopIndex(endSnapIndex, m_ChildrenForSnappingFromStartToEnd.Count)], out endCalculateIndex);

            float distanceToSnap = 0;
            while (true)
            {
                distanceToSnap += m_CalculateDistances[LoopIndex(startCalculateIndex, m_CalculateDistances.Count)];

                if (LoopIndex(startCalculateIndex + 1, m_CalculateDistances.Count) == endCalculateIndex)
                {
                    break;
                }

                startCalculateIndex++;
            }
            return distanceToSnap;
        }

        /// <summary>
        /// Make sure that both of the children have been added to m_ChildrenForSizeFromStartToEnd before calling.
        /// </summary>
        private float GetCalculateDistance(int startCalculateIndex, float spacing, int endCalculateIndex)
        {
            Vector3[] childOneCorners = new Vector3[4];
            m_ChildrenForSizeFromStartToEnd[LoopIndex(startCalculateIndex, m_ChildrenForSizeFromStartToEnd.Count)].GetLocalCorners(childOneCorners);

            Vector3[] childTwoCorners = new Vector3[4];
            m_ChildrenForSizeFromStartToEnd[LoopIndex(endCalculateIndex, m_ChildrenForSizeFromStartToEnd.Count)].GetLocalCorners(childTwoCorners);

            return DistanceOnAxis(childOneCorners[3], Vector2.zero, axis) + spacing + DistanceOnAxis(childTwoCorners[1], Vector2.zero, axis);
        }
        
        private int LoopIndex(int index, int count)
        {
            if (count == 0)
            {
                return 0;
            }

            if (index >= count)
            {
                return index % count;
            }

            if (index < 0)
            {
                int test = (index % count == 0) ? 0 : count + (index % count);
                return test;
            }

            return index;
        }
        #endregion

        #region Control
        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null && m_AvailableForSnappingTo.Count > 0;
        }

        public virtual void SetLayoutHorizontal()
        {
            if (Application.isPlaying)
            {
                m_Tracker.Clear();
                UpdateLayout();
            }
        }

        public virtual void SetLayoutVertical()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        private void RebuildLayoutGroups()
        {
            if (contentIsLayoutGroup && m_LayoutGroup.enabled)
            {
                m_LayoutGroup.CalculateLayoutInputHorizontal();
                m_LayoutGroup.CalculateLayoutInputVertical();
                m_LayoutGroup.SetLayoutHorizontal();
                m_LayoutGroup.SetLayoutVertical();
            }
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private void EnsureTrackerSetup()
        {
            if (!m_TrackerSetup)
            {
                UpdateLayout();
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        private void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            EnsureTrackerSetup();
            UpdateBounds();
            // How much the content is larger than the view.
            float scrollableLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            //the amount of content below the left corner of the viewbounds
            float amountBelowLeftCorner = (axis == 0) ? value * scrollableLength : (1 - value) * scrollableLength;
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - amountBelowLeftCorner;
            // The new content localPosition, in the space of the view.
            float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                UpdateBounds();
            }
        }

        private void UpdateBounds()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
            {
                return;
            }

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        private readonly Vector3[] m_ContentWorldCorners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = viewRect.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_ContentWorldCorners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_ContentWorldCorners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (movementDirection == MovementDirection.Horizontal || (movementDirection == MovementDirection.Vertical && !m_LockOtherDirection))
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)
                {
                    offset.x = m_ViewBounds.min.x - min.x;
                }
                else if (max.x < m_ViewBounds.max.x)
                {
                    offset.x = m_ViewBounds.max.x - max.x;
                }
            }

            if (movementDirection == MovementDirection.Vertical || (movementDirection == MovementDirection.Horizontal && !m_LockOtherDirection))
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < m_ViewBounds.max.y)
                {
                    offset.y = m_ViewBounds.max.y - max.y;
                }
                else if (min.y > m_ViewBounds.min.y)
                {
                    offset.y = m_ViewBounds.min.y - min.y;
                }
            }

            return offset;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                {
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                }
                else
                {
                    m_HorizontalScrollbar.size = 1;
                }

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                {
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                }
                else
                {
                    m_VerticalScrollbar.size = 1;
                }

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (m_LockOtherDirection)
            {
                if (movementDirection == MovementDirection.Vertical)
                {
                    position.x = m_Content.anchoredPosition.x;
                }
                if (movementDirection == MovementDirection.Horizontal)
                {
                    position.y = m_Content.anchoredPosition.y;
                }
            }

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        private Vector2 m_ReferencePos;
        private RectTransform m_TrackingChild;
        private void SetReferencePos(RectTransform trackingChild)
        {
            m_TrackingChild = trackingChild;
            m_ReferencePos = viewRect.InverseTransformPoint(trackingChild.position);
        }

        private Vector2 ResetContentPos()
        {
            Vector2 newPos = viewRect.InverseTransformPoint(m_TrackingChild.position);
            Vector2 offset = m_ReferencePos - newPos;
            m_Content.anchoredPosition = m_Content.anchoredPosition + offset;
            return offset;
        }

        private void UpdatePrevData()
        {
            if (m_Content == null)
            {
                m_PrevPosition = Vector2.zero;
            }
            else
            {
                m_PrevPosition = m_Content.anchoredPosition;
            }

            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
            m_PrevClosestItem = closestItem;
            m_PrevScrolling = !m_Scroller.isFinished;
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
            {
                return;
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private Transform GetCanvasTransform(Transform transform)
        {
            if (transform.GetComponent<Canvas>() != null)
            {
                return transform;
            }
            else
            {
                if (transform.parent != null)
                {
                    return GetCanvasTransform(transform.parent);
                }
                else
                {
                    return transform;
                }
            }
        }
        #endregion

        private void OnDrawGizmos()
        {

            if (m_DrawGizmos)
            {
                Vector3[] corners = new Vector3[4];
                m_Content.GetWorldCorners(corners);

                Vector3 topLeft = corners[1];
                Vector3 topRight = corners[2];
                Vector3 bottomRight = corners[3];
                Vector3 bottomLeft = corners[0];

                Gizmos.color = Color.white;
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);

                Vector3 topDirection = topRight - topLeft;
                Vector3 leftDirection = bottomLeft - topLeft;
                Vector3 topEndPoint = topLeft + topDirection.normalized * GetGizmoSize(topLeft);
                Vector3 leftEndPoint = topLeft + leftDirection.normalized * GetGizmoSize(topLeft);
                Vector3 perpendicularDirection = Vector3.Cross(topDirection, leftDirection);

                Vector3 arrowDirectionOne = Quaternion.AngleAxis(-135, perpendicularDirection) * leftDirection;
                Vector3 arrowDirectionTwo = Quaternion.AngleAxis(135, perpendicularDirection) * leftDirection;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(topLeft, topEndPoint);
                Gizmos.DrawLine(topLeft, leftEndPoint);
                Gizmos.DrawRay(leftEndPoint, arrowDirectionOne.normalized * GetGizmoSize(leftEndPoint) * .25f);
                Gizmos.DrawRay(leftEndPoint, arrowDirectionTwo.normalized * GetGizmoSize(leftEndPoint) * .25f);
                Gizmos.DrawRay(topEndPoint, -(arrowDirectionOne.normalized * GetGizmoSize(leftEndPoint) * .25f));
                Gizmos.DrawRay(topEndPoint, arrowDirectionTwo.normalized * GetGizmoSize(leftEndPoint) * .25f);

                Vector3[] childCorners = new Vector3[4];
                foreach (RectTransform child in m_Content)
                {
                    child.GetWorldCorners(childCorners);
                    if (child.position.x < topLeft.x || child.position.y > topLeft.y)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(childCorners[1], childCorners[2]);
                        Gizmos.DrawLine(childCorners[2], childCorners[3]);
                        Gizmos.DrawLine(childCorners[3], childCorners[0]);
                        Gizmos.DrawLine(childCorners[0], childCorners[1]);
                    }
                    else if (m_AvailableForSnappingTo.Contains(child))
                    {
                        Gizmos.color = Color.cyan;

                        if (m_MovementDirection == MovementDirection.Horizontal)
                        {
                            Gizmos.DrawRay(child.position, leftDirection.normalized * GetGizmoSize(child.position) * .5f);
                            Gizmos.DrawRay(child.position, -(leftDirection.normalized * GetGizmoSize(child.position) * .5f));
                        }
                        else
                        {
                            Gizmos.DrawRay(child.position, topDirection.normalized * GetGizmoSize(child.position) * .5f);
                            Gizmos.DrawRay(child.position, -(topDirection.normalized * GetGizmoSize(child.position) * .5f));
                        }
                    }
                }

                if (m_Loop && m_Content.childCount > 0 && !Application.isPlaying)
                {
                    Vector3[] firstChildCorners = new Vector3[4];
                    float distance = GetCalculateDistance(m_ChildrenForSizeFromStartToEnd.Count - 1, m_EndSpacing, 0);
                    firstCalculateChild.GetLocalCorners(firstChildCorners);

                    Vector2 matrixPos = Vector2.zero;
                    matrixPos[axis] = lastCalculateChild.position[axis] + distance;
                    matrixPos[inverseAxis] = firstCalculateChild.position[inverseAxis];

                    Matrix4x4 matrix = Matrix4x4.TRS(matrixPos, firstCalculateChild.rotation, firstCalculateChild.localScale);

                    Vector3[] gizmoCorners = new Vector3[4];
                    gizmoCorners[0] = matrix.MultiplyPoint3x4(firstChildCorners[0]);
                    gizmoCorners[1] = matrix.MultiplyPoint3x4(firstChildCorners[1]);
                    gizmoCorners[2] = matrix.MultiplyPoint3x4(firstChildCorners[2]);
                    gizmoCorners[3] = matrix.MultiplyPoint3x4(firstChildCorners[3]);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(gizmoCorners[1], gizmoCorners[2]);
                    Gizmos.DrawLine(gizmoCorners[2], gizmoCorners[3]);
                    Gizmos.DrawLine(gizmoCorners[3], gizmoCorners[0]);
                    Gizmos.DrawLine(gizmoCorners[0], gizmoCorners[1]);
                }
            }
        }

        private float GetGizmoSize(Vector3 position)
        {
            Camera current = Camera.current;
            position = Gizmos.matrix.MultiplyPoint(position);

            if (current)
            {
                Transform transform = current.transform;
                Vector3 position2 = transform.position;
                float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
                Vector3 a = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
                Vector3 b = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
                float magnitude = (a - b).magnitude;
                return 80f / Mathf.Max(magnitude, 0.0001f);
            }

            return 20f;
        }

        private void Validate()
        {

            m_Friction = Mathf.Max(m_Friction, .001f);
            m_Tension = Mathf.Max(m_Tension, 0);

            m_MinDurationMillis = Mathf.Max(m_MinDurationMillis, 1);
            m_MaxDurationMillis = Math.Max(m_MaxDurationMillis, Mathf.Max(m_MinDurationMillis, 1));
            m_ScrollDurationMillis = Mathf.Max(m_ScrollDurationMillis, 1);

            m_ScrollSensitivity = Mathf.Max(m_ScrollSensitivity, 0);
            m_ScrollDelay = Mathf.Max(m_ScrollDelay, 0);

            m_EndSpacing = Mathf.Max(m_EndSpacing, 0);

            if (m_ChildrenForSizeFromStartToEnd.Count == 1)
            {
                m_EndSpacing = Mathf.Max(m_EndSpacing, viewRectSize);
            }


            if (m_Scroller != null && (m_Tension != m_PrevTension || m_Friction != m_PrevFriction || m_MinDurationMillis != m_PrevMinDuration || m_MaxDurationMillis != m_PrevMaxDuration))
            {
                m_PrevTension = m_Tension;
                m_PrevFriction = m_Friction;
                m_PrevMinDuration = m_MinDurationMillis;
                m_PrevMaxDuration = m_MaxDurationMillis;
                m_Scroller = new Scroller(m_Friction, m_MinDurationMillis, m_MaxDurationMillis, GetInterpolator());
            }

            if (contentIsHorizonalLayoutGroup)
            {
                m_MovementDirection = MovementDirection.Horizontal;
            }

            if (contentIsVerticalLayoutGroup)
            {
                m_MovementDirection = MovementDirection.Vertical;
            }

            SetDirtyCaching();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Validate();
        }

        [MenuItem("GameObject/UI/ScrollSnaps/DirectionalScrollSnap", false, 10)]
        private static void CreateDirectionalScrollSnap(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null || !canvas.gameObject.activeInHierarchy)
                {
                    parent = new GameObject("Canvas");
                    parent.layer = LayerMask.NameToLayer("UI");
                    canvas = parent.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    parent.AddComponent<CanvasScaler>();
                    parent.AddComponent<GraphicRaycaster>();
                    Undo.RegisterCreatedObjectUndo(parent, "Create " + parent.name);

                    EventSystem evsy = FindObjectOfType<EventSystem>();
                    if (evsy == null || !evsy.gameObject.activeInHierarchy)
                    {
                        GameObject eventSystem = new GameObject("EventSystem");
                        eventSystem.AddComponent<EventSystem>();
                        eventSystem.AddComponent<StandaloneInputModule>();

                        Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
                    }
                }
                else
                {
                    parent = canvas.gameObject;
                }
            }

            int numChildren = 2;

            GameObject GO = new GameObject("Directional Scroll Snap");
            RectTransform rectTransform = GO.AddComponent<RectTransform>();
            DirectionalScrollSnap scrollSnap = GO.AddComponent<DirectionalScrollSnap>();
            Image image = GO.AddComponent<Image>();

            GameObject content = new GameObject("Content");
            RectTransform contentRectTransform = content.AddComponent<RectTransform>();
            Image contentImage = content.AddComponent<Image>();


            GO.transform.SetParent(parent.transform, false);
            rectTransform.sizeDelta = new Vector2(200, 200);
            scrollSnap.content = contentRectTransform;
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.color = Color.red;

            content.transform.SetParent(GO.transform, false);
            contentRectTransform.anchorMin = new Vector2(0, 1);
            contentRectTransform.anchorMax = new Vector2(0, 1);
            contentRectTransform.sizeDelta = new Vector2(200 + (150 * (numChildren - 1)), 200);
            contentRectTransform.anchoredPosition = new Vector2(100, -100);
            contentImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            contentImage.type = Image.Type.Sliced;
            contentImage.color = new Color(0, 0, 1, .5f);

            for (int i = 0; i < numChildren; i++)
            {
                GameObject child = new GameObject("Child Item");
                RectTransform childRectTransform = child.AddComponent<RectTransform>();
                child.AddComponent<Image>();

                child.transform.SetParent(content.transform, false);
                childRectTransform.anchorMin = new Vector2(0, 1);
                childRectTransform.anchorMax = new Vector2(0, 1);
                childRectTransform.sizeDelta = new Vector2(100, 100);
                childRectTransform.anchoredPosition = new Vector2(100 + (150 * i), -100);
            }

            GameObjectUtility.SetParentAndAlign(GO, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(GO, "Create " + GO.name);
            Selection.activeObject = GO;


            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
            {
                return;
            }

            // Create world space Plane from canvas position.
            RectTransform canvasRTransform = parent.GetComponent<RectTransform>();
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * rectTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * rectTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + rectTransform.sizeDelta.x * rectTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + rectTransform.sizeDelta.y * rectTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - rectTransform.sizeDelta.x * rectTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - rectTransform.sizeDelta.y * rectTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            rectTransform.anchoredPosition = position;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }
#endif

        protected class ScrollBarEventsListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            public Action<PointerEventData> onPointerDown;
            public Action<PointerEventData> onPointerUp;

            public virtual void OnPointerDown(PointerEventData ped)
            {
                onPointerDown(ped);
            }

            public virtual void OnPointerUp(PointerEventData ped)
            {
                onPointerUp(ped);
            }
        }
    }
}
