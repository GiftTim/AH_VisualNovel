#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define ACTK_DEBUG_ENABLED
#endif

//#define ENABLE_DEBUG_UI

namespace CodeStage.AntiCheat.Detectors
{
	using Common;

	using System;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// Allows detecting Cheat Engine's speed hack (and maybe some other speed hack tools) usage.
	/// </summary>
	/// <remarks>
	/// Add it to any GameObject as usual or through the `GameObject > Create Other > Code Stage > Anti-Cheat Toolkit`
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction to cheating.
	///
	/// Avoid using detectors from code at the Awake phase.
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// // Basic usage - start detection with callback
	/// void Start()
	/// {
	///     SpeedHackDetector.StartDetection(OnSpeedHackDetected);
	/// }
	/// 
	/// private void OnSpeedHackDetected()
	/// {
	///     Debug.Log("Speed hack detected!");
	///     
	///     // Get detailed detection info
	///     Debug.Log($"Detection details: {SpeedHackDetector.Instance.LastDetectionInfo}");
	///     
	///     // Handle the detection - ban player, show warning, etc.
	/// }
	/// ]]> 
	/// </code>
	/// </example>
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// // Advanced usage with custom settings
	/// void Start()
	/// {
	///     // Start with custom interval, max false positives, and cooldown
	///     SpeedHackDetector.StartDetection(OnSpeedHackDetected, 0.5f, 5, 30);
	/// }
	/// 
	/// private void OnSpeedHackDetected()
	/// {
	///     Debug.Log("Speed hack detected!");
	///     // Your response logic here
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// // Using SpeedHackProofTime for reliable timers
	/// void Update()
	/// {
	///     // Use SpeedHackProofTime instead of Time.time for reliable timing
	///     float deltaTime = SpeedHackProofTime.deltaTime;
	///     float time = SpeedHackProofTime.time;
	///     
	///     // Your game logic using reliable timers
	///     transform.Translate(Vector3.forward * deltaTime * speed);
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// // Safely changing timeScale without triggering false positives
	/// void PauseGame()
	/// {
	///     // Use SetTimeScale instead of Time.timeScale = 0
	///     SpeedHackDetector.SetTimeScale(0f);
	///     Debug.Log("Game paused safely");
	/// }
	/// 
	/// void ResumeGame()
	/// {
	///     SpeedHackDetector.SetTimeScale(1f);
	/// }
	/// 
	/// // For third-party assets that need to change timeScale
	/// void AllowThirdPartyTimeScale()
	/// {
	///     // Allow any timeScale changes for 5 seconds
	///     SpeedHackDetector.AllowAnyTimeScaleFor(5f);
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	[AddComponentMenu(MenuPath + ComponentName)]
	[DisallowMultipleComponent]
	[HelpURL(ACTk.DocsRootUrl + "manual/detectors.html#speed-hack-detector")]
	public class SpeedHackDetector : ACTkDetectorBase<SpeedHackDetector>
	{
		public const string ComponentName = "Speed Hack Detector";
		internal const string LogPrefix = ACTk.LogPrefix + ComponentName + ": ";

		/// <summary>
		/// Holds detailed information about latest triggered detection.
		/// </summary>
		/// <remarks>
		/// Provides information about which detection source triggered the speed hack detection
		/// (environment ticks, realtime ticks, DSP, or timeScale changes).
		/// </remarks>
		public SpeedHackDetectionInfo LastDetectionInfo => LastDetectionInfoAbstract as SpeedHackDetectionInfo;

		#region public fields
		/// <summary>
		/// Time (in seconds) between detector checks.
		/// </summary>
		[Tooltip("Time (in seconds) between detector checks.")]
		public float interval = 1f;

		/// <summary>
		/// Allowed speed multiplier threshold. Do not set to too low values (e.g. 0 or 0.00*) since there are timer fluctuations on different hardware.
		/// </summary>
		[Tooltip("Allowed speed multiplier threshold. Do not set to too low values (e.g. 0 or 0.00*) since there are timer fluctuations on different hardware.")]
		[Range(0.05f, 5f)]
		public float threshold = 0.2f;

		/// <summary>
		/// Maximum false positives count allowed before registering speed hack.
		/// </summary>
		[Tooltip("Maximum false positives count allowed before registering speed hack.")]
		public byte maxFalsePositives = 3;

		/// <summary>
		/// Amount of sequential successful checks before clearing internal false positives counter.<br/>
		/// Set 0 to disable Cool Down feature.
		/// </summary>
		[Tooltip("Amount of sequential successful checks before clearing internal false positives counter.\nSet 0 to disable Cool Down feature.")]
		public int coolDown = 30;

		/// <summary>
		/// Time jump threshold in seconds. Detects suspicious backward or forward time jumps exceeding this value.
		/// </summary>
		[field: SerializeField, Tooltip("Time jump threshold in seconds. Detects suspicious backward or forward time jumps exceeding this value.")]
		public int TimeJumpThreshold { get; set; } = 5;
		#endregion

		/// <summary>
		/// Controls whether to use DSP Timer to catch speed hacks in sandboxed environments (like WebGL, VMs, etc.).
		/// </summary>
		/// <remarks>
		/// Uses AudioSettings.dspTime under the hood, which can catch some extra speed hacks in sandboxed environments
		/// but can potentially cause false positives on some hardware due to way too high sensitivity.
		/// <strong>⚠️ Warning:</strong> Use at your peril!
		/// </remarks>
		[field: SerializeField, Tooltip("Uses AudioSettings.dspTime under the hood, which can catch some extra speed hacks " +
									   "in sandboxed environments but can potentially cause false positives on some hardware. " +
									   "Use at your peril!")]
		public bool UseDsp { get; set; }

		/// <summary>
		/// Controls whether to watch Time.timeScale for unauthorized changes.
		/// </summary>
		/// <remarks>
		/// When enabled, the detector will monitor for unauthorized changes to Time.timeScale.
		/// Use SetTimeScale() method to safely change timeScale without triggering false positives.
		/// <strong>⚠️ Warning:</strong> May cause false positives if you change timeScale
		/// without using the provided API.
		/// </remarks>
		[field: SerializeField, Tooltip("Watches Time.timeScale for unauthorized changes. Use SetTimeScale() method to safely change timeScale " +
									   "without triggering false positives. May cause false positives if you change timeScale directly.")]
		public bool WatchTimeScale { get; set; } = true;

		#region private fields
		private byte currentFalsePositives;
		private int currentCooldownShots;
		private long previousReliableTicks;

		private long previousEnvironmentTicks;
		private long previousRealtimeTicks;
		private long previousDspTicks;

		private bool resetTicksOnNextInterval;

		// timeScale watching fields
		private float lastTimeScale = 1f;
		private bool timeScaleInitialized;
		private double allowAnyTimeScaleUntil = -1f;
		private bool cheatedReliable;

#if ENABLE_DEBUG_UI
		private const int DebugWindowId = 1000;
		private Rect debugWindowRect = new Rect(10, 10, 500, 400);
#endif
		#endregion

		#region public static methods
		/// <summary>
		/// Creates new instance of the detector at scene if it doesn't exists. Make sure to call NOT from Awake phase.
		/// </summary>
		/// <returns>New or existing instance of the detector.</returns>
		public static SpeedHackDetector AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Starts speed hack detection for detector you have in scene.
		/// </summary>
		/// <remarks>
		/// Make sure you have properly configured detector in scene with autoStart disabled before using this method.
		/// </remarks>
		public static SpeedHackDetector StartDetection()
		{
			if (Instance != null)
				return Instance.StartDetectionInternal(null, Instance.interval, Instance.maxFalsePositives, Instance.coolDown);

			Debug.LogError(LogPrefix + "can't be started since it doesn't exists in scene or not yet initialized!");
			return null;
		}

		/// <summary>
		/// Starts speed hack detection with specified callback.
		/// </summary>
		/// <remarks>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// </remarks>
		/// <param name="callback">Method to call after detection.</param>
		public static SpeedHackDetector StartDetection(Action callback)
		{
			return StartDetection(callback, GetOrCreateInstance.interval);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval.<br/>
		/// </summary>
		/// <remarks>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// </remarks>
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides <see cref="interval"/> property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval)
		{
			return StartDetection(callback, interval, GetOrCreateInstance.maxFalsePositives);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval and maxFalsePositives.<br/>
		/// </summary>
		/// <remarks>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// </remarks>
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides <see cref="interval"/> property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides <see cref="maxFalsePositives"/> property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval, byte maxFalsePositives)
		{
			return StartDetection(callback, interval, maxFalsePositives, GetOrCreateInstance.coolDown);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval, maxFalsePositives and coolDown.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides <see cref="interval"/> property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides <see cref="maxFalsePositives"/> property.</param>
		/// <param name="coolDown">Amount of sequential successful checks before resetting false positives counter. Overrides <see cref="coolDown"/> property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval, byte maxFalsePositives, int coolDown)
		{
			return GetOrCreateInstance.StartDetectionInternal(callback, interval, maxFalsePositives, coolDown);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null)
				Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component.
		/// </summary>
		/// <remarks>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		/// </remarks>
		public static void Dispose()
		{
			if (Instance != null)
				Instance.DisposeInternal();
		}

		/// <summary>
		/// Safely changes Time.timeScale without triggering false positives in timeScale detection.
		/// </summary>
		/// <remarks>
		/// Use this method instead of directly setting Time.timeScale when timeScale detection is enabled.
		/// </remarks>
		/// <param name="newTimeScale">New timeScale value to set.</param>
		public static void SetTimeScale(float newTimeScale)
		{
			if (Instance == null || !Instance.IsRunning)
			{
				Time.timeScale = newTimeScale;
				return;
			}

			Instance.SetTimeScaleInternal(newTimeScale);
		}

		/// <summary>
		/// Temporarily allows any timeScale changes for the specified duration.
		/// </summary>
		/// <remarks>
		/// Useful when third-party assets need to change timeScale directly.
		/// </remarks>
		/// <param name="durationSeconds">Duration in real-time seconds to allow any timeScale changes.</param>
		public static void AllowAnyTimeScaleFor(float durationSeconds)
		{
			if (Instance == null || !Instance.IsRunning)
				return;

			Instance.AllowAnyTimeScaleForInternal(durationSeconds);
		}

		/// <summary>
		/// Immediately allows any timeScale changes until StopAllowingAnyTimeScale() is called.
		/// </summary>
		/// <remarks>
		/// Useful when third-party assets need to change timeScale directly for an indefinite period.
		/// </remarks>
		public static void AllowAnyTimeScale()
		{
			if (Instance == null || !Instance.IsRunning)
				return;

			Instance.AllowAnyTimeScaleForInternal(float.MaxValue);
		}

		/// <summary>
		/// Stops allowing any timeScale changes if AllowAnyTimeScale() was called.
		/// </summary>
		public static void StopAllowingAnyTimeScale()
		{
			if (Instance == null || !Instance.IsRunning)
				return;

			Instance.StopAllowingAnyTimeScaleInternal();
		}
		#endregion

#if UNITY_EDITOR
		// making sure it will reset statics even if domain reload is disabled
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void SubsystemRegistration()
		{
			Instance = null;
		}
#endif

		/// <summary>
		/// Manually triggers cheating detection and invokes assigned events.
		/// </summary>
		[ContextMenu("Trigger detection")]
		public void TriggerDetection()
		{
			if (!IsRunning)
			{
				Debug.LogWarning(LogPrefix + "Detector is not running, can't trigger detection.");
				return;
			}

			var detectionInfo = new SpeedHackDetectionInfo(false, false, false, false, false);
			OnCheatingDetected(detectionInfo);
		}

		private SpeedHackDetector() // prevents direct instantiation
		{
#if UNITY_EDITOR
			// prevents in-editor false positives due to PlayMode pause
			UnityEditor.EditorApplication.pauseStateChanged += state =>
			{
				OnApplicationPause(state == UnityEditor.PauseState.Paused);
			};
#endif
		}

#if ENABLE_DEBUG_UI
		private void OnGUI()
		{
			debugWindowRect = GUI.Window(DebugWindowId, debugWindowRect, DrawDebugWindow, "Speed Hack Detector Debug");
		}

		private void DrawDebugWindow(int windowId)
		{
			GUILayout.BeginVertical("Box");
			GUILayout.Label("TimeUtils.GetReliableTicks(): " + TimeUtils.GetReliableTicks() / TimeUtils.TicksPerSecond);
			GUILayout.Label("TimeUtils.GetEnvironmentTicks(): " + TimeUtils.GetEnvironmentTicks() / TimeUtils.TicksPerSecond);
			GUILayout.Label("TimeUtils.GetRealtimeTicks(): " + TimeUtils.GetRealtimeTicks() / TimeUtils.TicksPerSecond);
			GUILayout.Label("TimeUtils.GetDspTicks(): " + TimeUtils.GetDspTicks() / TimeUtils.TicksPerSecond);
			GUILayout.Label("TimeScale: " + Time.timeScale);
			GUILayout.Label("Time.time: " + Time.time.ToString("F0"));
			GUILayout.Label("Time.unscaledTime: " + Time.unscaledTime.ToString("F0"));
			GUILayout.Label("Time.fixedTime: " + Time.fixedTime.ToString("F0"));
			GUILayout.Label("Time.timeSinceLevelLoad: " + Time.timeSinceLevelLoad.ToString("F0"));
			GUILayout.Label("Time.realtimeSinceStartup: " + Time.realtimeSinceStartup.ToString("F0"));
			GUILayout.Label("Time.captureFramerate: " + Time.captureFramerate);
			GUILayout.EndVertical();

			GUI.DragWindow();
		}
#endif
		#region unity messages

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnApplicationPause(bool pause)
		{
			if (!pause && IsStarted)
			{
				ResetLastTicks();
				resetTicksOnNextInterval = true;
			}
		}


#if UNITY_WEBGL
#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus && IsStarted)
			{
				ResetLastTicks();
				resetTicksOnNextInterval = true;
			}
		}
#endif

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void Update()
		{
			if (!IsRunning)
				return;

#if UNITY_WEBGL
			if (!Application.isFocused)
				return;
#endif

			var reliableTicks = TimeUtils.GetReliableTicks();
			var intervalTicks = (long)(interval * TimeUtils.TicksPerSecond);
			var reliableDelta = reliableTicks - previousReliableTicks;

			if (reliableDelta < 0)
			{
				var backwardJumpSeconds = Math.Abs(reliableDelta) / (double)TimeUtils.TicksPerSecond;
				if ((backwardJumpSeconds > TimeJumpThreshold) && !cheatedReliable)
				{
#if ACTK_DETECTION_BACKLOGS
					Debug.LogWarning(LogPrefix + "Suspicious backward jump detected:\n" +
					                 $"reliableDelta: {reliableDelta} ({backwardJumpSeconds:F2}s)\n" +
					                 $"Jump threshold: {TimeJumpThreshold}s");
#endif
					cheatedReliable = true;
				}

				reliableDelta = intervalTicks;
				resetTicksOnNextInterval = true;
			}
			else
			{
				var expectedDelta = intervalTicks;
				var forwardJumpTicks = reliableDelta - expectedDelta;

				if (forwardJumpTicks > 0)
				{
					var forwardJumpSeconds = forwardJumpTicks / (double)TimeUtils.TicksPerSecond;
					if (forwardJumpSeconds > TimeJumpThreshold)
					{
						if (!cheatedReliable)
						{
#if ACTK_DETECTION_BACKLOGS
							Debug.LogWarning(LogPrefix + "Suspicious forward jump detected:\n" +
											$"reliableDelta: {reliableDelta} ({forwardJumpSeconds:F2}s)\n" +
											$"Jump threshold: {TimeJumpThreshold}s");
#endif
							cheatedReliable = true;
						}

						reliableDelta = intervalTicks;
						resetTicksOnNextInterval = true;
					}
				}
			}

			// return if configured interval is not passed yet
			if (reliableDelta < intervalTicks)
				return;
			
			if (resetTicksOnNextInterval)
			{
				ResetLastTicks();
				resetTicksOnNextInterval = false;
				previousReliableTicks = reliableTicks;
				return;
			}

			var cheatedEnvironment = IsTicksCheated(TimeUtils.GetEnvironmentTicks(), ref previousEnvironmentTicks, reliableDelta);
			var cheatedRealtime = IsTicksCheated(TimeUtils.GetRealtimeTicks(), ref previousRealtimeTicks, reliableDelta);
			var cheatedDsp = false;
			var cheatedTimeScale = false;

#if UNITY_AUDIO_MODULE
			if (SystemInfo.supportsAudio && UseDsp)
			{
				var dspTicks = TimeUtils.GetDspTicks();
				if (dspTicks != 0 && !AudioListener.pause)
					cheatedDsp = IsTicksCheated(dspTicks, ref previousDspTicks, reliableDelta);
				else
					previousDspTicks = 0;
			}
			else
			{
				previousDspTicks = 0;
			}
#endif
			// Check for unauthorized timeScale changes
			if (WatchTimeScale)
			{
				cheatedTimeScale = IsTimeScaleCheated();
			}

			if (cheatedEnvironment || cheatedRealtime || cheatedDsp || cheatedTimeScale || cheatedReliable)
			{
#if ACTK_DETECTION_BACKLOGS
				Debug.LogWarning(LogPrefix + "Detection backlog:\n" +
								 $"reliableTicks: {reliableTicks}\n" +
								 $"cheatedEnvironment: {cheatedEnvironment}\n" +
								 $"cheatedRealtime: {cheatedRealtime}\n" +
								 $"cheatedDsp: {cheatedDsp}\n" +
								 $"cheatedTimeScale: {cheatedTimeScale}\n" +
								 $"cheatedReliable: {cheatedReliable}");
#endif
				currentFalsePositives++;
				if (currentFalsePositives > maxFalsePositives)
				{
#if ACTK_DEBUG_ENABLED
					Debug.LogWarning(LogPrefix + "final detection!", this);
#endif
					var detectionInfo = new SpeedHackDetectionInfo(cheatedEnvironment, cheatedRealtime, cheatedDsp, cheatedTimeScale, cheatedReliable);
					OnCheatingDetected(detectionInfo);
				}
				else
				{
#if ACTK_DEBUG_ENABLED
					Debug.LogWarning(LogPrefix + "detection! Allowed false positives left: " + (maxFalsePositives - currentFalsePositives), this);
#endif
					currentCooldownShots = 0;
					cheatedReliable = false;
					ResetLastTicks();
				}
			}
			else if (currentFalsePositives > 0 && coolDown > 0)
			{
#if ACTK_DEBUG_ENABLED
				Debug.Log(LogPrefix + "success shot! Shots till cool down: " + (coolDown - currentCooldownShots), this);
#endif
				currentCooldownShots++;
				if (currentCooldownShots >= coolDown)
				{
#if ACTK_DEBUG_ENABLED
					Debug.Log(LogPrefix + "cool down!", this);
#endif
					currentFalsePositives = 0;
				}
			}

			previousReliableTicks = reliableTicks;
		}

		#endregion

		private SpeedHackDetector StartDetectionInternal(Action callback, float checkInterval, byte falsePositives, int shotsTillCooldown)
		{
			if (IsRunning)
			{
				Debug.LogWarning(LogPrefix + "already running!", this);
				return this;
			}

			if (!enabled)
			{
				Debug.LogWarning($"{LogPrefix}disabled but {nameof(StartDetection)} still called from somewhere (see stack trace for this message)!", this);
				return this;
			}

			if (callback != null && DetectorHasListeners())
				Debug.LogWarning($"{LogPrefix}has properly configured Detection Event in the inspector or {nameof(CheatDetected)} event subscriber, but still get started with Action callback." +
								 $"Action will be called at the same time with Detection Event or {nameof(CheatDetected)} on detection." +
								 "Are you sure you wish to do this?", this);

			if (callback == null && !DetectorHasListeners())
				Debug.LogWarning($"{LogPrefix}was started without Detection Event, Callback or {nameof(CheatDetected)} event subscription." +
								 $"Cheat will not be detected until you subscribe to {nameof(CheatDetected)} event.", this);

			if (callback != null)
				CheatDetected += callback;
			
			interval = checkInterval;
			maxFalsePositives = falsePositives;
			coolDown = shotsTillCooldown;

			LastDetectionInfoAbstract = null;
			ResetLastTicks();
			currentFalsePositives = 0;
			currentCooldownShots = 0;

			// Initialize timeScale tracking
			if (WatchTimeScale)
			{
				lastTimeScale = Time.timeScale;
				timeScaleInitialized = true;
			}

			IsStarted = true;
			IsRunning = true;

			return this;
		}

		private protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, interval, maxFalsePositives, coolDown);
		}

		private protected override void StopDetectionInternal()
		{
			// Reset timeScale watching state
			if (WatchTimeScale)
			{
				timeScaleInitialized = false;
				allowAnyTimeScaleUntil = -1f;
			}
			
			base.StopDetectionInternal();
		}
		
		private bool IsTicksCheated(long ticks, ref long previousTicks, long reliableDelta)
		{
			var delta = ticks - previousTicks;
			var multiplier = Math.Abs(1 - (double)delta / reliableDelta);

			var cheated = multiplier > threshold;
			if (cheated)
			{
#if ACTK_DETECTION_BACKLOGS
				Debug.LogWarning(LogPrefix + "Detection backlog:\n" +
								 $"reliableDelta: {reliableDelta}\n" +
								 $"delta: {delta}\n" +
								 $"multiplier > threshold: {multiplier} > {threshold}\n" +
								 $"ticks: {ticks}");
#endif
			}

			previousTicks = ticks;
			return cheated;
		}

		private void SetTimeScaleInternal(float newTimeScale)
		{
			if (!WatchTimeScale)
			{
				Time.timeScale = newTimeScale;
				return;
			}

			// Always apply the change immediately (like vanilla Time.timeScale)
			Time.timeScale = newTimeScale;
			lastTimeScale = newTimeScale;
			timeScaleInitialized = true;
		}



		private bool IsTimeScaleCheated()
		{
			if (!WatchTimeScale)
				return false;

			var currentTimeScale = Time.timeScale;
			var currentTime = TimeUtils.GetReliableSeconds();

			// Initialize on first check
			if (!timeScaleInitialized)
			{
				lastTimeScale = currentTimeScale;
				timeScaleInitialized = true;
				return false;
			}

			// Check if we're in a temporary "allow any timeScale" period
			var isAllowedPeriod = allowAnyTimeScaleUntil > 0 && currentTime < allowAnyTimeScaleUntil;
			
			// Check if timeScale has changed
			var difference = Mathf.Abs(currentTimeScale - lastTimeScale);
			if (difference > 0.001f)
			{
				if (!isAllowedPeriod)
				{
#if DEBUG
					Debug.LogWarning($"{LogPrefix}Unauthorized timeScale change detected: {lastTimeScale} -> {currentTimeScale} (difference: {difference})\n" +
					                 $"This would trigger a false positive. Use {nameof(SpeedHackDetector)}.{nameof(SetTimeScale)}() to change timeScale safely, " +
					                 $"or {nameof(SpeedHackDetector)}.{nameof(AllowAnyTimeScaleFor)}() for third-party assets.");
#endif
					return true;
				}
				
				// Update tracking for allowed changes (only during allowed periods)
				lastTimeScale = currentTimeScale;
			}
			else if (isAllowedPeriod)
			{
				// Even if no change detected, update tracking during allowed periods to prevent false positives
				lastTimeScale = currentTimeScale;
			}

			return false;
		}

		private void AllowAnyTimeScaleForInternal(float durationSeconds)
		{
			var currentTime = TimeUtils.GetReliableSeconds();
			allowAnyTimeScaleUntil = currentTime + durationSeconds;
#if DEBUG
			Debug.Log($"{LogPrefix}Allowing any timeScale changes for {durationSeconds} seconds (currentTime: {currentTime}, until: {allowAnyTimeScaleUntil})");
#endif
		}

		private void StopAllowingAnyTimeScaleInternal()
		{
			allowAnyTimeScaleUntil = -1f;
#if DEBUG
			Debug.Log($"{LogPrefix}Stopped allowing any timeScale changes");
#endif
		}

		private void ResetLastTicks()
		{
			previousReliableTicks = TimeUtils.GetReliableTicks();
			previousEnvironmentTicks = TimeUtils.GetEnvironmentTicks();
			previousRealtimeTicks = TimeUtils.GetRealtimeTicks();
			previousDspTicks = TimeUtils.GetDspTicks();
		}
	}
}