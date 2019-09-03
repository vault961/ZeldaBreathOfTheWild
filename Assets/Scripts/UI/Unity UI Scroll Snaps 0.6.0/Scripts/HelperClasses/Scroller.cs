//Contributors:
//BeksOmega

namespace UnityEngine.UI.ScrollSnaps
{
    /// <summary>
    /// This class encapsulates scrolling. You can use scrollers to collect
    /// the data you need to create a scrolling animation, for example, in
    /// responce to a fling gesture. Scrollers track
    /// scroll offsets for you over time, but they don't automatically apply those
    /// positions to your view. It's your responsibility to get and apply new
    /// coordinates at a rate that will make the scrolling animation look smooth.
    /// </summary>
    public class Scroller
    {
        public enum Mode
        {
            Scrolling,
            Flinging
        }

        private Interpolator m_Interpolator = new ViscousFluidInterpolator();

        private Mode m_Mode;

        private int m_StartX;
        private int m_StartY;
        private int m_FinalX;
        private int m_FinalY;

        private int m_MinX;
        private int m_MaxX;
        private int m_MinY;
        private int m_MaxY;

        private int m_CurrX;
        private int m_CurrY;
        private float m_StartTime;
        private int m_Duration;
        private float m_DurationReciprocal;
        private float m_DeltaX;
        private float m_DeltaY;
        private bool m_Finished = true;

        private float m_Velocity;
        private float m_CurrVelocity;
        private int m_Distance;

        private static readonly int DEFAULT_DURATION = 250;
        private static readonly int DEFAULT_MIN_DURATION = 250;
        private static readonly int DEFAULT_MAX_DURATION = 2000;
        private int m_MinDuration;
        private int m_MaxDuration;

        private const float SECONDS_TO_MILLIS = 1000f;

        private static float DECELERATION_RATE = Mathf.Log(0.78f) / Mathf.Log(0.9f);
        private static readonly float INFLEXION = 0.35f; // Tension lines cross at (INFLEXION, 1)
        private static readonly float START_TENSION = 0.5f;
        private static readonly float END_TENSION = 1.0f;
        private static readonly float P1 = START_TENSION * INFLEXION;
        private static readonly float P2 = 1.0f - END_TENSION * (1.0f - INFLEXION);

        private static readonly int NB_SAMPLES = 100;
        private static readonly float[] SPLINE_POSITION = new float[NB_SAMPLES + 1];
        private static readonly float[] SPLINE_TIME = new float[NB_SAMPLES + 1];

        private static readonly float DEFAULT_FLING_FRICTION = .25f;
        // A context-specific coefficient adjusted to physical values.
        private float mPhysicalCoeff;
        private float m_FlingFriction;
        private float mDeceleration;
        private float mPpi;

        /// <summary>
        /// Create a scroller with default friction, min/max durations, and interpolator.
        /// </summary>
        public Scroller()
        {
            float x_min = 0.0f;
            float y_min = 0.0f;
            for (int i = 0; i < NB_SAMPLES; i++)
            {
                float alpha = (float)i / NB_SAMPLES;
                float x_max = 1.0f;
                float x, tx, coef;
                while (true)
                {
                    x = x_min + (x_max - x_min) / 2.0f;
                    coef = 3.0f * x * (1.0f - x);
                    tx = coef * ((1.0f - x) * P1 + x * P2) + x * x * x;
                    if (Mathf.Abs(tx - alpha) < 1E-5) break;
                    if (tx > alpha) x_max = x;
                    else x_min = x;
                }
                SPLINE_POSITION[i] = coef * ((1.0f - x) * START_TENSION + x) + x * x * x;
                float y_max = 1.0f;
                float y, dy;
                while (true)
                {
                    y = y_min + (y_max - y_min) / 2.0f;
                    coef = 3.0f * y * (1.0f - y);
                    dy = coef * ((1.0f - y) * START_TENSION + y) + y * y * y;
                    if (Mathf.Abs(dy - alpha) < 1E-5) break;
                    if (dy > alpha) y_max = y;
                    else y_min = y;
                }
                SPLINE_TIME[i] = coef * ((1.0f - y) * P1 + y * P2) + y * y * y;
            }
            SPLINE_POSITION[NB_SAMPLES] = SPLINE_TIME[NB_SAMPLES] = 1.0f;

            mPpi = GetDPI();
            mDeceleration = ComputeDeceleration(m_FlingFriction);

            mPhysicalCoeff = ComputeDeceleration(0.84f);
        }

        /// <summary>
        /// Create a scroller with the specified friction, default min/max durations, and default interpolator.
        /// </summary>
        /// <param name="friction">Controls how quickly or slowly fling animations come to a stop.</param>
        public Scroller(float friction) : this(friction, DEFAULT_MIN_DURATION, DEFAULT_MAX_DURATION, null) { }

        /// <summary>
        /// Create a scroller with the specified interpolator, default friction, and default min/max durations.
        /// If the interpolator is null the default interpolator will be used.
        /// </summary>
        /// <param name="interpolator">Controls how the scroller animates scrolling.</param>
        public Scroller(Interpolator interpolator) : this(DEFAULT_FLING_FRICTION, DEFAULT_MIN_DURATION, DEFAULT_MAX_DURATION, interpolator) { }

        /// <summary>
        /// Create a scroller with the specified min/max durations, default friction, and default interpolator.
        /// </summary>
        /// <param name="minDuration">The minimum amount of time in milliseconds any animation will take.</param>
        /// <param name="maxDuration">The maximum amount of time any animation will take.</param>
        public Scroller(int minDuration, int maxDuration) : this(DEFAULT_FLING_FRICTION, minDuration, maxDuration, null) { }

        /// <summary>
        /// Create a scroller with the specified friction, specified interpolator, and default min/max durations.
        /// If the interpolator is null the default interpolator will be used.
        /// </summary>
        /// <param name="friction">Controls how quickly or slowly fling animations come to a stop.</param>
        /// <param name="interpolator">Controls how the scroller animates scrolling.</param>
        public Scroller(float friction, Interpolator interpolator) : this(DEFAULT_FLING_FRICTION, DEFAULT_MIN_DURATION, DEFAULT_MAX_DURATION, interpolator) { }

        /// <summary>
        /// Create a scroller with the specified friction, specified min/max durations, and default interpolator.
        /// </summary>
        /// <param name="friction">Controls how quickly or slowly fling animations come to a stop.</param>
        /// <param name="minDuration">The minimum amount of time in milliseconds any animation will take.</param>
        /// <param name="maxDuration">The maximum amount of time in milliseconds any animation will take.</param>
        public Scroller(float friction, int minDuration, int maxDuration) : this(friction, minDuration, maxDuration, null) { }

        /// <summary>
        /// Create a scroller with the specified min/max durations and specified interpolator.
        /// If the interpolator is null the default interpolator will be used.
        /// </summary>
        /// <param name="minDuration">The minimum amount of time in milliseconds any animation will take.</param>
        /// <param name="maxDuration">The maximum amount of time in milliseconds any animation will take.</param>
        /// <param name="interpolator">Controls how the scroller animates scrolling.</param>
        public Scroller(int minDuration, int maxDuration, Interpolator interpolator) : this(DEFAULT_FLING_FRICTION, minDuration, maxDuration, interpolator) { }

        /// <summary>
        /// Create a scroller with the specified friction, min/max durations, and interpolator.
        /// If the interpolator is null the default interpolator will be used.
        /// </summary>
        /// <param name="friction">Controls how quickly or slowly fling animations come to a stop.</param>
        /// <param name="minDuration">The minimum amount of time in milliseconds any animation will take.</param>
        /// <param name="maxDuration">The maximum amount of time in milliseconds any animation will take.</param>
        /// <param name="interpolator">Controls how the scroller animates scrolling.</param>
        public Scroller(float friction, int minDuration, int maxDuration, Interpolator interpolator) : this()
        {
            m_FlingFriction = friction;
            m_MinDuration = minDuration;
            m_MaxDuration = maxDuration;
            if (interpolator != null)
            {
                m_Interpolator = interpolator;
            }
        }

        /// <summary>
        /// Returns the scroller's interpolator. The interpolator modifies how scrolling is animated.
        /// </summary>
        public Interpolator interpolator
        {
            get
            {
                return m_Interpolator;
            }
        }

        /// <summary>
        /// Returns the scroller's friction. The friction controls how quickly or slowly fling animations come to a stop.
        /// </summary>
        public float friction
        {
            get
            {
                return m_FlingFriction;
            }
        }

        /// <summary>
        /// Returns the minimum duration, in miliseconds, for all animations, scroll and fling.
        /// </summary>
        public int minDuration
        {
            get
            {
                return m_MinDuration;
            }
        }

        /// <summary>
        /// Returns the maximum duration, in miliseconds, for all animations, scroll and fling.
        /// </summary>
        public int maxDuraiton
        {
            get
            {
                return m_MaxDuration;
            }
        }

        /// <summary>
        /// Returns the current position in the scroll. Before calling this it is best to call ComputeScrollOffset() if you are looking for up-to-date data.
        /// </summary>
        public Vector2 currentPosition
        {
            get
            {
                return new Vector2(m_CurrX, m_CurrY);
            }
        }

        /// <summary>
        /// Returns the start position of the scroll.
        /// </summary>
        public Vector2 startPosition
        {
            get
            {
                return new Vector2(m_StartX, m_StartY);
            }
        }

        /// <summary>
        /// Returns the final position of the scroll.
        /// </summary>
        public Vector2 finalPosition
        {
            get
            {
                return new Vector2(m_FinalX, m_FinalY);
            }
        }

        /// <summary>
        /// Returns the total duration of the latest animation.
        /// </summary>
        public int durationOfLatestAnimation
        {
            get
            {
                return m_Duration;
            }
        }

        /// <summary>
        /// Returns the amount of time since the start of the latest animation.
        /// </summary>
        public int timePassedSinceStartOfAnimation
        {
            get
            {
                return (int)(Time.time * SECONDS_TO_MILLIS - m_StartTime);
            }
        }

        /// <summary>
        /// Returns the current velocity of the scroll.
        /// </summary>
        public float currentVelocity
        {
            get
            {
                return m_Mode == Mode.Flinging ? m_CurrVelocity : m_Velocity - mDeceleration * timePassedSinceStartOfAnimation / 2000.0f;
            }
        }

        /// <summary>
        /// Returns if the latest animation is finished.
        /// </summary>
        public bool isFinished
        {
            get
            {
                return m_Finished;
            }
        }

        /// <summary>
        /// Returns if true if the scroller is currently animating a fling.
        /// </summary>
        public bool isFlinging
        {
            get
            {
                return m_Mode == Mode.Flinging && !isFinished;
            }
        }

        /// <summary>
        /// Returns true if the scroller is currently animating a scroll.
        /// </summary>
        public bool isScrolling
        {
            get
            {
                return m_Mode == Mode.Scrolling && !isFinished;
            }
        }

        /// <summary> 
        /// Set the final position on the X axis, of the current animation.
        /// </summary>
        /// <param name="newX">The new position on the X axis.</param>
        public void SetFinalX(int newX)
        {
            m_FinalX = newX;
            m_DeltaX = m_FinalX - m_StartX;
            m_Finished = false;
        }

        /// <summary>
        /// Set the final position on the Y axis, of the current animation.
        /// </summary>
        /// <param name="newY">The new position on the Y axis.</param>
        public void SetFinalY(int newY)
        {
            m_FinalY = newY;
            m_DeltaY = m_FinalY - m_StartY;
            m_Finished = false;
        }

        /// <summary>
        /// Set the final position of the current animation.
        /// </summary>
        /// <param name="newPos">The new final position.</param>
        public void SetFinalPosition(Vector2 newPos)
        {
            m_FinalX = (int)newPos.x;
            m_FinalY = (int)newPos.y;
            m_DeltaX = m_FinalX - m_StartX;
            m_DeltaY = m_FinalY - m_StartY;
            m_Finished = false;
        }

        /// <summary>
        /// Shifts the start and end positions of the animation by the offset, does not affect the duration or speed.
        /// </summary>
        public void ShiftAnimation(Vector2 offset)
        {
            m_StartX = m_StartX + (int)offset.x;
            m_StartY = m_StartY + (int)offset.y;
            m_FinalX = m_FinalX + (int)offset.x;
            m_FinalY = m_FinalY + (int)offset.y;
        }

        /// <summary>
        /// Extend the scroll animation. This allows the running animation to scroll further and longer in combination with SetFinalPosition().
        /// </summary>
        /// <param name="extendMS">Additional time to add to the animation in milliseconds.</param>
        public void ExtendDuration(int extendMS)
        {
            int passed = timePassedSinceStartOfAnimation;
            m_Duration = Mathf.Clamp(passed + extendMS, m_MinDuration, m_MaxDuration);
            m_DurationReciprocal = 1.0f / m_Duration;
            m_Finished = false;
        }

        /// <summary>
        /// Call this when you want to know the new location.
        /// </summary>
        /// <returns>Returns true if the animation is not done yet.</returns>
        public bool ComputeScrollOffset()
        {
            if (m_Finished)
            {
                return false;
            }
            int timePassed = timePassedSinceStartOfAnimation;

            if (timePassed < m_Duration)
            {
                switch (m_Mode)
                {
                    case Mode.Scrolling:
                        float x = m_Interpolator.GetInterpolation(timePassed * m_DurationReciprocal);
                        m_CurrX = (int)(m_StartX + Mathf.Round(x * m_DeltaX));
                        m_CurrY = (int)(m_StartY + Mathf.Round(x * m_DeltaY));
                        break;
                    case Mode.Flinging:
                        float t = (float)timePassed / m_Duration;
                        int index = (int)(NB_SAMPLES * t);
                        float distanceCoef = 1f;
                        float velocityCoef = 0f;
                        if (index < NB_SAMPLES)
                        {
                            float t_inf = (float)index / NB_SAMPLES;
                            float t_sup = (float)(index + 1) / NB_SAMPLES;
                            float d_inf = SPLINE_POSITION[index];
                            float d_sup = SPLINE_POSITION[index + 1];
                            velocityCoef = (d_sup - d_inf) / (t_sup - t_inf);
                            distanceCoef = d_inf + (t - t_inf) * velocityCoef;
                        }
                        m_CurrVelocity = velocityCoef * m_Distance / m_Duration * 1000.0f;

                        m_CurrX = (int)(m_StartX + Mathf.Round(distanceCoef * (m_FinalX - m_StartX)));
                        // Pin to mMinX <= mCurrX <= mMaxX
                        m_CurrX = Mathf.Min(m_CurrX, m_MaxX);
                        m_CurrX = Mathf.Max(m_CurrX, m_MinX);

                        m_CurrY = (int)(m_StartY + Mathf.Round(distanceCoef * (m_FinalY - m_StartY)));
                        // Pin to mMinY <= mCurrY <= mMaxY
                        m_CurrY = Mathf.Min(m_CurrY, m_MaxY);
                        m_CurrY = Mathf.Max(m_CurrY, m_MinY);
                        if (m_CurrX == m_FinalX && m_CurrY == m_FinalY)
                        {
                            m_Finished = true;
                        }
                        break;
                }
            }
            else
            {
                m_CurrX = m_FinalX;
                m_CurrY = m_FinalY;
                m_Finished = true;
            }
            return true;
        }

        /// <summary>
        /// Start scrolling by providing a start position and an end position, the duration will be the default value of 250 milliseconds.
        /// </summary>
        public void StartScroll(Vector2 startPos, Vector2 endPos)
        {
            StartScroll((int)startPos.x, (int)startPos.y, (int)endPos.x - (int)startPos.x, (int)endPos.y - (int)startPos.y, DEFAULT_DURATION);
        }

        /// <summary>
        /// Start scrolling by providing a start position, an end position, and the duration of the scroll in milliseconds.
        /// </summary>
        /// <param name="duration">The duration of the scroll in milliseconds.</param>
        public void StartScroll(Vector2 startPos, Vector2 endPos, int duration)
        {
            StartScroll((int)startPos.x, (int)startPos.y, (int)endPos.x - (int)startPos.x, (int)endPos.y - (int)startPos.y, duration);
        }

        /// <summary>
        /// Start scrolling by providing a start position, the movement delta, and the duration of the scroll in milliseconds.
        /// </summary>
        /// <param name="startX">Start X position.</param>
        /// <param name="startY">Start Y position.</param>
        /// <param name="dx">Movement delta on the x axis.</param>
        /// <param name="dy">Movement delta on the y axis.</param>
        /// <param name="duration">The duration of the scroll in milliseconds.</param>
        public void StartScroll(int startX, int startY, int dx, int dy, int duration)
        {
            m_Mode = Mode.Scrolling;
            m_Finished = false;
            m_Duration = Mathf.Clamp(duration, m_MinDuration, m_MaxDuration);
            m_StartTime = Time.time * SECONDS_TO_MILLIS;
            m_StartX = startX;
            m_StartY = startY;
            m_FinalX = startX + dx;
            m_FinalY = startY + dy;
            m_DeltaX = dx;
            m_DeltaY = dy;
            m_DurationReciprocal = 1.0f / (float)m_Duration;
        }

        /// <summary>
        /// Start scrolling based on a velocity. The distance traveled will depend on the velocity.
        /// </summary>
        /// <param name="startPos">The initial velocity in units per second.</param>
        public void Fling(Vector2 startPos, Vector2 velocity)
        {
            Fling((int)startPos.x, (int)startPos.y, (int)velocity.x, (int)velocity.y, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
        }

        /// <summary>
        /// Start scrolling based on a velocity. The distance traveled will depend on the velocity.
        /// </summary>
        /// <param name="velocity">The initial velocity in units per second.</param>
        /// <param name="minPos">The scroller will not scroll past this point.</param>
        /// <param name="maxPos">The scroller will not scroll past this point.</param>
        public void Fling(Vector2 startPos, Vector2 velocity, Vector2 minPos, Vector2 maxPos)
        {
            Fling((int)startPos.x, (int)startPos.y, (int)velocity.x, (int)velocity.y, (int)minPos.x, (int)minPos.y, (int)maxPos.x, (int)maxPos.y);
        }

        /// <summary>
        /// Start scrolling based on velocity. THe distance traveled will depend on the velocity.
        /// </summary>
        /// <param name="startX">Start X position.</param>
        /// <param name="startY">Start Y position.</param>
        /// <param name="velocityX">The initial velocity on the x axis in units per second.</param>
        /// <param name="velocityY">The initial velocity on the y axis in units per second.</param>
        /// <param name="minX">The scroller will not scroll past this point.</param>
        /// <param name="minY">The scroller will not scroll past this point.</param>
        /// <param name="maxX">The scroller will not scroll past this point.</param>
        /// <param name="maxY">The scroller will not scroll past this point.</param>
        public void Fling(int startX, int startY, int velocityX, int velocityY,
            int minX, int minY, int maxX, int maxY)
        {
            m_Mode = Mode.Flinging;
            m_Finished = false;
            float velocity = Hypot(velocityX, velocityY);

            m_Velocity = velocity;
            m_Duration = Mathf.Clamp(GetSplineFlingDuration(velocity), m_MinDuration, m_MaxDuration);
            m_StartTime = Time.time * SECONDS_TO_MILLIS;
            m_StartX = startX;
            m_StartY = startY;
            float coeffX = velocity == 0 ? 1.0f : velocityX / velocity;
            float coeffY = velocity == 0 ? 1.0f : velocityY / velocity;
            float totalDistance = GetSplineFlingDistance(velocity);
            m_Distance = (int)(totalDistance * Mathf.Sign(velocity));

            m_MinX = minX;
            m_MaxX = maxX;
            m_MinY = minY;
            m_MaxY = maxY;
            m_FinalX = startX + (int)Mathf.Round(totalDistance * coeffX);
            // Pin to mMinX <= mFinalX <= mMaxX
            m_FinalX = Mathf.Min(m_FinalX, m_MaxX);
            m_FinalX = Mathf.Max(m_FinalX, m_MinX);

            m_FinalY = startY + (int)Mathf.Round(totalDistance * coeffY);
            // Pin to mMinY <= mFinalY <= mMaxY
            m_FinalY = Mathf.Min(m_FinalY, m_MaxY);
            m_FinalY = Mathf.Max(m_FinalY, m_MinY);
        }

        private float ComputeDeceleration(float friction)
        {
            return 9.80665f               //gravity earth g (m/s^2)
                          * 39.37f               // inch/meter
                          * mPpi                 // pixels per inch
                          * friction;
        }

        private float GetSplineDeceleration(float velocity)
        {
            return Mathf.Log(INFLEXION * Mathf.Abs(velocity) / (m_FlingFriction * mPhysicalCoeff));
        }

        private int GetSplineFlingDuration(float velocity)
        {
            float l = GetSplineDeceleration(velocity);
            float decelMinusOne = DECELERATION_RATE - 1f;
            return (int)(1000.0 * Mathf.Exp(l / decelMinusOne));
        }

        private float GetSplineFlingDistance(float velocity)
        {
            float l = GetSplineDeceleration(velocity);
            float decelMinusOne = DECELERATION_RATE - 1f;
            return m_FlingFriction * mPhysicalCoeff * Mathf.Exp(DECELERATION_RATE / decelMinusOne * l);
        }

        /// <summary>
        /// Stops the animation. Contrary to AbortAnimation() force finishing the animation does *not* move the scroller to the final position.
        /// </summary>
        public void ForceFinish()
        {
            m_Finished = true;
        }

        /// <summary>
        /// Stops the animation. Contrary to ForceFinish() aborting the animation moves the scroller to the final position.
        /// </summary>
        public void AbortAnimation()
        {
            m_CurrX = m_FinalX;
            m_CurrY = m_FinalY;
            m_Finished = true;
        }

        float GetDPI()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

                return (metrics.Get<float>("density") * 160);
            }
            else
            {
                return Screen.dpi;
            }
        }

        private float Hypot(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }


        #region Interpolators

        public class ViscousFluidInterpolator : Interpolator
        {
            /** Controls the viscous fluid effect (how much of it). */
            private readonly float VISCOUS_FLUID_SCALE = 8.0f;
            private readonly float VISCOUS_FLUID_NORMALIZE;
            private readonly float VISCOUS_FLUID_OFFSET;

            public ViscousFluidInterpolator()
            {
                // must be set to 1.0 (used in viscousFluid())
                VISCOUS_FLUID_NORMALIZE = 1.0f / viscousFluid(1.0f);
                // account for very small floating-point error
                VISCOUS_FLUID_OFFSET = 1.0f - VISCOUS_FLUID_NORMALIZE * viscousFluid(1.0f);
            }

            private float viscousFluid(float x)
            {
                x *= VISCOUS_FLUID_SCALE;
                if (x < 1.0f)
                {
                    x -= (1.0f - Mathf.Exp(-x));
                }
                else
                {
                    float start = 0.36787944117f;   // 1/e == exp(-1)
                    x = 1.0f - Mathf.Exp(1.0f - x);
                    x = start + x * (1.0f - start);
                }
                return x;
            }

            public float GetInterpolation(float input)
            {
                float interpolated = VISCOUS_FLUID_NORMALIZE * viscousFluid(input);
                if (interpolated > 0)
                {
                    return interpolated + VISCOUS_FLUID_OFFSET;
                }
                return interpolated;
            }
        }

        public class AccelerateDecelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return (float)(Mathf.Cos((input + 1) * Mathf.PI) / 2.0f) + 0.5f;
            }
        }

        public class AccelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return input * input;
            }
        }

        public class AnticipateInterpolator : Interpolator
        {
            private float mTension = 2f;

            public AnticipateInterpolator() { }

            public AnticipateInterpolator(float tension)
            {
                mTension = tension;
            }

            public float GetInterpolation(float input)
            {
                // a(t) = t * t * ((tension + 1) * t - tension)
                return input * input * ((mTension + 1) * input - mTension);
            }
        }

        public class AnticipateOvershootInterpolator : Interpolator
        {
            private float mTension = 2f * 1.5f;

            public AnticipateOvershootInterpolator() { }

            public AnticipateOvershootInterpolator(float tension)
            {
                mTension = tension * 1.5f;
            }

            private static float a(float t, float s)
            {
                return t * t * ((s + 1) * t - s);
            }
            private static float o(float t, float s)
            {
                return t * t * ((s + 1) * t + s);
            }

            public float GetInterpolation(float input)
            {
                // a(t, s) = t * t * ((s + 1) * t - s)
                // o(t, s) = t * t * ((s + 1) * t + s)
                // f(t) = 0.5 * a(t * 2, tension * extraTension), when t < 0.5
                // f(t) = 0.5 * (o(t * 2 - 2, tension * extraTension) + 2), when t <= 1.0
                if (input < 0.5f) return 0.5f * a(input * 2.0f, mTension);
                else return 0.5f * (o(input * 2.0f - 2.0f, mTension) + 2.0f);
            }
        }

        public class BounceInterpolator : Interpolator
        {
            private static float mBouncyness = 8f;

            public BounceInterpolator() { }

            public BounceInterpolator(float _bouncyness)
            {
                mBouncyness = _bouncyness;
            }

            private static float bounce(float t)
            {
                return t * t * mBouncyness;
            }

            public float GetInterpolation(float input)
            {
                // _b(t) = t * t * 8
                // bs(t) = _b(t) for t < 0.3535
                // bs(t) = _b(t - 0.54719) + 0.7 for t < 0.7408
                // bs(t) = _b(t - 0.8526) + 0.9 for t < 0.9644
                // bs(t) = _b(t - 1.0435) + 0.95 for t <= 1.0
                // b(t) = bs(t * 1.1226)
                input *= 1.1226f;
                if (input < 0.3535f) return bounce(input);
                else if (input < 0.7408f) return bounce(input - 0.54719f) + 0.7f;
                else if (input < 0.9644f) return bounce(input - 0.8526f) + 0.9f;
                else return bounce(input - 1.0435f) + 0.95f;
            }
        }

        public class DecelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return (1.0f - (1.0f - input) * (1.0f - input));
            }
        }

        public class LinearInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return input;
            }
        }

        public class OvershootInterpolator : Interpolator
        {
            private float mTension = 2f;

            public OvershootInterpolator() { }

            public OvershootInterpolator(float tension)
            {
                mTension = tension;
            }

            public float GetInterpolation(float input)
            {
                // _o(t) = t * t * ((tension + 1) * t + tension)
                // o(t) = _o(t - 1) + 1
                input -= 1.0f;
                return input * input * ((mTension + 1) * input + mTension) + 1.0f;
            }
        }


        #endregion
    }


    public interface Interpolator
    {
        float GetInterpolation(float input);
    }
}
