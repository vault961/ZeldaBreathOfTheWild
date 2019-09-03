//Dependencies:
// - OmniDirectionalScrollSnap: Source > Scripts > Components

//Contributors:
//BeksOmega

using UnityEditor;
using UnityEditor.AnimatedValues;

namespace UnityEngine.UI.ScrollSnaps
{
    [CustomEditor(typeof(OmniDirectionalScrollSnap))]
    public class OmniDirectionalScrollSnapEditor : Editor
    {
        private AnimBool
            showFriction,
            showTension,
            showScrollInfo,
            showCalculateError,
            showSnapError,
            showDrawGizmos;

        private bool
            showFilters,
            showCalculateFilter,
            showSnapFilter,
            showEvents;

        private SerializedProperty
            content,
            movementType,
            useVelocity,
            friction,
            interpolator,
            tension,
            scrollSensativity,
            scrollWheelDirection,
            scrollDelay,
            minDuration,
            maxDuration,
            scrollDuration,
            addToCalculateFilter,
            calculateFilterMode,
            calculateFilter,
            addToSnapFilter,
            snapFilterMode,
            snapFilter,
            viewPort,
            horizontalScrollBar,
            verticalScrollBar,
            onValueChanged,
            startMovement,
            closestSnapChanged,
            snappedToItem,
            targetItemSelected,
            drawGizmos;

        OmniDirectionalScrollSnap scrollSnap;

        private void OnEnable()
        {
            scrollSnap = (OmniDirectionalScrollSnap)target;

            content = serializedObject.FindProperty("m_Content");
            movementType = serializedObject.FindProperty("m_MovementType");
            useVelocity = serializedObject.FindProperty("m_UseVelocity");
            friction = serializedObject.FindProperty("m_Friction");
            interpolator = serializedObject.FindProperty("m_InterpolatorType");
            tension = serializedObject.FindProperty("m_Tension");
            scrollSensativity = serializedObject.FindProperty("m_ScrollSensitivity");
            scrollWheelDirection = serializedObject.FindProperty("m_ScrollWheelDirection");
            scrollDelay = serializedObject.FindProperty("m_ScrollDelay");
            minDuration = serializedObject.FindProperty("m_MinDurationMillis");
            maxDuration = serializedObject.FindProperty("m_MaxDurationMillis");
            scrollDuration = serializedObject.FindProperty("m_ScrollDurationMillis");
            addToCalculateFilter = serializedObject.FindProperty("m_AddInactiveChildrenToCalculatingFilter");
            calculateFilterMode = serializedObject.FindProperty("m_FilterModeForCalculatingSize");
            calculateFilter = serializedObject.FindProperty("m_CalculatingFilter");
            addToSnapFilter = serializedObject.FindProperty("m_AddInactiveChildrenToSnapPositionsFilter");
            snapFilterMode = serializedObject.FindProperty("m_FilterModeForSnapPositions");
            snapFilter = serializedObject.FindProperty("m_SnapPositionsFilter");
            viewPort = serializedObject.FindProperty("m_Viewport");
            horizontalScrollBar = serializedObject.FindProperty("m_HorizontalScrollbar");
            verticalScrollBar = serializedObject.FindProperty("m_VerticalScrollbar");
            onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            startMovement = serializedObject.FindProperty("m_StartMovementEvent");
            closestSnapChanged = serializedObject.FindProperty("m_ClosestSnapPositionChanged");
            snappedToItem = serializedObject.FindProperty("m_SnappedToItem");
            targetItemSelected = serializedObject.FindProperty("m_TargetItemSelected");
            drawGizmos = serializedObject.FindProperty("m_DrawGizmos");

            showFriction = new AnimBool(useVelocity.boolValue);
            showFriction.valueChanged.AddListener(Repaint);
            showTension = new AnimBool(interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Anticipate || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.AnticipateOvershoot || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Overshoot);
            showTension.valueChanged.AddListener(Repaint);
            showScrollInfo = new AnimBool(scrollSensativity.floatValue > 0);
            showScrollInfo.valueChanged.AddListener(Repaint);
            showCalculateError = new AnimBool(calculateFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
            showCalculateError.valueChanged.AddListener(Repaint);
            showSnapError = new AnimBool(snapFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
            showSnapError.valueChanged.AddListener(Repaint);
            showDrawGizmos = new AnimBool(scrollSnap.content != null);
            showDrawGizmos.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(scrollSnap), typeof(OmniDirectionalScrollSnap), false);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(content);
            EditorGUILayout.PropertyField(movementType, new GUIContent("Movement Type", "Clamped mode keeps the content within the bounds of the Scroll Snap. Elastic bounces the content when it gets to the edge of the Scroll Snap."));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useVelocity);
            showFriction.target = useVelocity.boolValue;
            if (EditorGUILayout.BeginFadeGroup(showFriction.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(friction);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(interpolator, new GUIContent("Interpolator", "Changes how the scroll snap animates when scrolling. This is used when the scroll snap is not moving based on velocity or manual movement, such as when a button is pressed."));
            showTension.target = (interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Anticipate || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.AnticipateOvershoot || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Overshoot);
            if (EditorGUILayout.BeginFadeGroup(showTension.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(tension, new GUIContent("Tension", "Modifies the interpolator"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(scrollSensativity, new GUIContent("Scroll Sensativity", "How sensative the scroll snap is to touch pad and scroll wheel events"));
            showScrollInfo.target = scrollSensativity.floatValue > 0;
            if (EditorGUILayout.BeginFadeGroup(showScrollInfo.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(scrollDelay, new GUIContent("Scroll Delay", "The time in seconds between the last touch pad/scroll wheel event and when the scroll snap starts snapping"));
                EditorGUILayout.PropertyField(scrollWheelDirection, new GUIContent("Scroll Direction", "What direction scrollwheel/touchpad events will move the content."));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(minDuration, new GUIContent("Min Duration", "The minimum duration in milliseconds for any snapping or scrolling animations"));
            EditorGUILayout.PropertyField(maxDuration, new GUIContent("Max Duration", "The maximum duration in milliseconds for any snapping or scrolling animations"));
            EditorGUILayout.PropertyField(scrollDuration, new GUIContent("Scroll Duration", "The default duration in milliseconds for any scrolling animations"));

            EditorGUILayout.Space();

            showFilters = EditorGUILayout.Foldout(showFilters, "Item Filters", true, EditorStyles.foldout);
            if (showFilters)
            {
                EditorGUI.indentLevel++;
                showCalculateFilter = EditorGUILayout.Foldout(showCalculateFilter, new GUIContent("Calclulate Filter", "Used to filter out any RectTransforms you don't want used in the Content's size calculation and you don't want to be able to snap to."), true, EditorStyles.foldout);
                if (showCalculateFilter)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(calculateFilterMode, new GUIContent());
                    EditorGUI.indentLevel--;
                    addToCalculateFilter.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Add Inactive Children", "Adds inactive/disabled children to the filter."), addToCalculateFilter.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();

                    showCalculateError.target = (calculateFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
                    if (EditorGUILayout.BeginFadeGroup(showCalculateError.faded))
                        EditorGUILayout.HelpBox("An empty whitelist will render the Scroll Snap unable to calculate its size correctly.", MessageType.Error);
                    EditorGUILayout.EndFadeGroup();

                    for (int i = 0; i < calculateFilter.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(calculateFilter.GetArrayElementAtIndex(i), new GUIContent());
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            calculateFilter.DeleteArrayElementAtIndex(i);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 14);
                    if (GUILayout.Button("Add Child"))
                    {
                        calculateFilter.InsertArrayElementAtIndex(calculateFilter.arraySize);
                    }
                    GUILayout.EndHorizontal();
                }
                showSnapFilter = EditorGUILayout.Foldout(showSnapFilter, new GUIContent("Available Snaps Filter", "Used to filter out any RectTransforms you don't want to be able to snap to. If a RectTransform is filtered out in the Calculate Size Filter you cannot snap to it here even if you whitelist it."), true);
                if (showSnapFilter)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(snapFilterMode, new GUIContent());
                    EditorGUI.indentLevel--;
                    addToSnapFilter.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Add Inactive Children", "Adds inactive/disabled children to the filter."), addToSnapFilter.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();

                    showSnapError.target = (snapFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
                    if (EditorGUILayout.BeginFadeGroup(showSnapError.faded))
                        EditorGUILayout.HelpBox("An empty whitelist will render the Scroll Snap unable to snap to items.", MessageType.Error);
                    EditorGUILayout.EndFadeGroup();

                    for (int i = 0; i < snapFilter.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(snapFilter.GetArrayElementAtIndex(i), new GUIContent());
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            snapFilter.DeleteArrayElementAtIndex(i);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 14);
                    if (GUILayout.Button("Add Child"))
                    {
                        snapFilter.InsertArrayElementAtIndex(snapFilter.arraySize);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(viewPort);
            EditorGUILayout.PropertyField(horizontalScrollBar);
            EditorGUILayout.PropertyField(verticalScrollBar);

            EditorGUILayout.Space();

            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            if (showEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(onValueChanged);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(startMovement);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(closestSnapChanged);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(snappedToItem);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(targetItemSelected);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showDrawGizmos.target = scrollSnap.content != null;
            if (EditorGUILayout.BeginFadeGroup(showDrawGizmos.faded))
                EditorGUILayout.PropertyField(drawGizmos);
            EditorGUILayout.EndFadeGroup();

            if (GUILayout.Button("Update"))
            {
                scrollSnap.UpdateLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
