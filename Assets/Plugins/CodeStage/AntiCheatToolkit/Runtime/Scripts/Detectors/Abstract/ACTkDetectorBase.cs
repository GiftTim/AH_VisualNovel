#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	using Common;

	using System;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Base class for all detectors.
	/// </summary>
	[AddComponentMenu("")]
	public abstract class ACTkDetectorBase<T> : KeepAliveBehaviour<T> where T: ACTkDetectorBase<T>
	{
		private protected const string MenuPath = "Code Stage/Anti-Cheat Toolkit/";

		/// <summary>
		/// Allows to start detector automatically.
		/// Otherwise, you'll need to call StartDetection() method to start it.
		/// </summary>
		/// <remarks>
		/// Useful in conjunction with proper Detection Event configuration in the inspector.
		/// Allows to use detector without writing any code except the actual reaction on cheating.
		/// </remarks>
		[Tooltip("Automatically start detector. Detection Event will be called on detection.")]
		public bool autoStart = true;

		/// <summary>
		/// Detector component will be automatically disposed after firing callback if enabled.
		/// Otherwise, it will just stop internal processes.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit": it will be automatically
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		[Tooltip("Automatically dispose Detector after firing callback.")]
		public bool autoDispose = true;

		/// <summary>
		/// Subscribe to this event to get notified when cheat will be detected.
		/// </summary>
		public event Action CheatDetected;
		
		/// <summary>
		/// Indicates if cheat was detected by this detector.
		/// </summary>
		public bool IsCheatDetected { get; private protected set; }

		[SerializeField]
		private protected UnityEvent detectionEvent;

		[SerializeField]
		private protected bool detectionEventHasListener;

		/// <summary>
		/// Allows to check if detector is started (stays true even when it's paused).
		/// </summary>
		public bool IsStarted { get; private protected set; }

		/// <summary>
		/// Allows to check if detection is currently running and not paused.
		/// </summary>
		public bool IsRunning { get; private protected set; }

		/// <summary>
		/// Holds detailed information about the latest detection, if any.
		/// </summary>
		/// <remarks>
		/// This property is reset when detection starts and is set when a cheat is detected.
		/// The information remains available even after the detector stops, allowing you to inspect
		/// what exactly triggered the detection (e.g., which timer source, which module, etc.).
		/// </remarks>
		protected ICheatDetectionInfo LastDetectionInfoAbstract { get; private protected set; }

		#region unity messages
#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private protected override void Start()
		{
			base.Start();

			if (autoStart && !IsStarted)
			{
				StartDetectionAutomatically();
			}
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnEnable()
		{
			ResumeDetector();
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnDisable()
		{
			PauseDetector();
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnApplicationQuit()
		{
			DisposeInternal();
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private protected override void OnDestroy()
		{
			StopDetectionInternal();
			base.OnDestroy();
		}
		#endregion

		internal virtual void OnCheatingDetected(ICheatDetectionInfo detectionInfo)
		{
			IsCheatDetected = true;
			LastDetectionInfoAbstract = detectionInfo;
			InvokeCheatingDetectedEvent();

			if (detectionEventHasListener)
				detectionEvent.Invoke();

			if (autoDispose)
				DisposeInternal();
			else
				StopDetectionInternal();
		}
		
		private protected virtual void InvokeCheatingDetectedEvent()
		{
			CheatDetected?.Invoke();
		}

		private protected virtual bool DetectorHasListeners()
		{
			return IsUserListeningToCheatDetectedEvent() || detectionEventHasListener;
		}

		private protected virtual void StopDetectionInternal()
		{
			CheatDetected = null;
			IsStarted = false;
			IsRunning = false;
		}

		private protected virtual void PauseDetector()
		{
			if (!IsStarted)
				return;

			IsRunning = false;
		}

		private protected virtual bool ResumeDetector()
		{
			if (!IsStarted || !DetectorHasListeners())
				return false;

			IsRunning = true;
			return true;
		}

		private protected virtual bool IsUserListeningToCheatDetectedEvent()
		{
			return CheatDetected != null;
		}
		
		private protected abstract void StartDetectionAutomatically();
	}
}