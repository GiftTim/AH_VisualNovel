# Changelog

Changelog format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## Types of changes

- **Added** for new features.
- **Changed** for changes in existing functionality.
- **Deprecated** for soon-to-be removed features.
- **Removed** for now removed features.
- **Fixed** for any bug fixes.
- **Security** in case of vulnerabilities.

💡 _Always remove previous plugin version before updating_

## [6.8.0] - 2026-01-09

### Added

- TimeCheatingDetector: Add `GetOnlineTimeAwaitable` and `ForceCheckAwaitable` APIs for Unity 2023.1+ with better async performance
- SpeedHack Detector now detects suspicious forward / backward time jumps to catch system clock tampering

### Changed

- WallHack Detector:
  - Change default container spawn position to (-10000, -10000, -10000)
  - Reduce chance of false positives from the Character Controller module
- Improve SpeedHack Detector accuracy when running in WebGL builds
- Reduce memory allocations in ObscuredGuid and hash calculations for smoother performance

### Fixed

- Fix Obscured Types prefab overrides corruption in collaborative workflows (thx JYC)

## [6.7.0] - 2025-11-13

### Added

- Add detection metadata system: all detectors now expose `LastDetectionInfo` property with detailed information about what triggered the detection

### Changed

- `SpeedHackDetector.SetTimeScale()` now returns `void` instead of `bool`

## [6.6.0] - 2025-10-09

### Added

- Allow showing unity-managed prefs at the Prefs Editor

### Changed

- Improve Prefs Editor UX with multiple small QoL updates

### Fixed

- Fix Prefs Editor could prevent editing values with UX issues (thx Thiago)
- Fix new empty prefs couldn't be added in Prefs Editor

## [6.5.0] - 2025-10-06

### Added

- Add DOTS ECS support to the ObscuredFloat and ObscuredGuid
- Add simple DOTS Hybrid setup example under `CodeStage\AntiCheatToolkit\Examples\DOTS ECS Examples`
- Add [User Manual](https://docs.codestage.net/actk/manual/index.html) to the online docs
- Add property drawers for `ObscuredUShort`, `ObscuredByte`, and `ObscuredSByte`

### Changed

- Increment minimum supported Unity version to 2021.3.0f1
- Auto-ignore "ToolchainAutomaticallyInstallPackage" in PrefsEditor
- Improve manual adding Wallhack Detector shader UX with Highlight
- Make overriding functionality from external assemblies harder

### Fixed

- Fix automatic Wallhack Detector shader removal could produce errors
- Fix broken \ outdated urls in documentation
- Fix incorrect urls in ACTk Setting

## [6.4.0] - 2025-09-05

### Added
- ObscuredString: Add GC-free `IsNullOrEmpty()` and `IsNullOrWhiteSpace()` methods

### Changed
- Improve error handling in Obscured Types Validator
- Improve Prefs Editor stability
- Improve Unity 6.2 compatibility

## [6.3.0] - 2025-08-14

### Added
- Add read-only `Hash` property to all Obscured types
- Add `HashUtils.CalculateHash(string)` GC-free overload for hashing plain strings without allocations 

## [6.2.0] - 2025-07-18

### Added
- Add TimeScale watching to the SpeedHackDetector
  - New `WatchTimeScale` property (enabled by default) to detect unauthorized Time.timeScale modifications
  - New `SpeedHackDetector.SetTimeScale()` API for safe timeScale changes
  - New `SpeedHackDetector.AllowAnyTimeScaleFor()` API for temporary third-party asset control
  - New `SpeedHackDetector.AllowAnyTimeScale()` / `StopAllowingAnyTimeScale()` APIs for indefinite control
  - New `SpeedHackProofTime.timeScale` property as drop-in replacement for Time.timeScale

## [6.1.0] - 2025-06-23

### Added
- Add ObscuredDateTimeOffset type
- Add ObscuredGuid type

## [6.0.0] - 2025-04-09

### Changed
- Change versioning to be year-agnostic (following Unity decision)

### Fixed
- Fix false positives after changing serialized obscured variables in ScriptableObject at PlayMode
- Fix GC allocations from ObscuredFloat in newer Unity versions

## [2024.3.5] - 2025-03-04

### Fixed
- Fix obscured types migration and fixing when used in nested types (thx sol3breaker)

## [2024.3.4] - 2025-02-28

### Fixed
- Fix a false positives from inconsistent hashing (thx Dmitry Statsenko & Icaro)

## [2024.3.3] - 2024-12-26

### Fixed
- Fix a false positive while editing ObscuredString in Play Mode (thx Thiago)

## [2024.3.2] - 2024-12-19

### Fixed
- Fix a regression with error from empty ObscuredStrings editing in Inspector

## [2024.3.1] - 2024-12-18

### Changed
- Improve error handling in serialized data parser

### Fixed
- Fix possible exceptions while parsing serialized data
- Fix invalid variables Inspector highlight inside nested items
- Fix possible edge case obscured false positives 

## [2024.3.0] - 2024-12-10

### Added
- Add obscured types validation / migration in Build scenes

### Changed
- Improve URP / HDRP compatibility
- Update icons
- Update changelog format and [release as html](https://docs.codestage.net/actk/changelog/)

### Security
- Fix SpeedHackDetector vulnerability

### Fixed
- Fix deprecation warnings when migrating from PlayerPrefs to ObscuredPrefs

## [2024.2.1] - 2024-11-18

### Fixed
- Fix default ObscuredBool was rendered as True in Inspector (thx sol3breaker)

## [2024.2.0] - 2024-11-17

### Added
- Add Project View context menus to validate or migrate specified assets

### Changed
- Improve RAM usage while validating or migrating assets
- Improve asset validation and migration API for more flexibility

### Fixed
- Fix possible exceptions while iterating scripting objects
- Fix non-initialized obscured variables were marked as invalid

## [2024.1.0] - 2024-11-04

### Added
- Add Honeypot option to the ObscuredCheatingDetector

### Changed
- Improve Obscured types cheating resistance
- Improve ObscuredTypesNewtonsoftConverter performance
- Improve ObscuredVector2Int API compatibility
- Improve ObscuredVector3Int API compatibility

### Security
- Fix few reported vulnerabilities

### Deprecated
- Deprecate ACTK_OBSCURED_AUTO_MIGRATION flag with auto-migration from legacy versions

### Fixed
- Fix AndroidScreenRecordingBlocker example
- Fix rare ObscuredBigInteger data corruption
- Fix ObscuredUInt inspector couldn't be set to values more than 2147483647

## [2024.0.0] - 2024-07-07

### Added
- Add prevent screen recording feature for Android platform

### Changed
- Improve Unity 6 compatibility
- Increase minimum Android supported version to Android 5.0 (API SDK 21)

### Fixed
- Fix few compilation warnings

## [2023.2.6] - 2024-01-14

### Fixed
- Fix Obscured Types json serialization could produce exception in obfuscated build (thx Thiago)

## [2023.2.5] - 2024-01-04

### Fixed
- Fix ObscuredPrefs.HasKey() could return wrong value when migrating from v1 format (thx Avocco)

## [2023.2.4] - 2023-12-19

### Changed
- Make sure domain reload support is editor-only

### Fixed
- Fix CodeHashGenerator warnings in Editor

## [2023.2.3] - 2023-09-12

### Fixed
- Fix harmless errors in console while using Prefs Editor (thx Rono)
- Fix rare RuntimeInitializeOnLoadMethodAttribute errors (thx Silent)

## [2023.2.2] - 2023-07-08

### Changed
- Improve disabled domain reload compatibility (thx KonstantGames)

## [2023.2.1] - 2023-06-16

### Fixed
- Fix ObscuredFilePrefs didn't allow saving after removing a key (thx Tyle)

## [2023.2.0] - 2023-05-31

### Changed
- Improve ObscuredDateTime compatibility
- ObscuredDateTime.GetDecrypted() now returns DateTime instead of binary long value
- Improve AppInstallationSource accuracy for PackageInstaller source
- Make ObscuredBigInteger serialize into JSON as human-readable string instead of b64 bytes

### Fixed
- Fix wrong Culture could be used while deserializing obscured types from JSON (thx spikyworm5)

## [2023.1.0] - 2023-05-20

### Added
- Add ObscuredDateTime (thx spikyworm5)

### Changed
- Include ObscuredDecimal into the obscured types validation

### Fixed
- Fix ObscuredDecimal might not parse properly from the Inspector
- Fix ObscuredString equality check against regular string (thx haeggongs)

## [2023.0.1] - 2023-05-11

### Added
- Add switch for the ACTK_NEWTONSOFT_JSON conditional in ACTk Settings

### Fixed
- Fix CodeHashGeneratorPostprocessor.HashesGenerated event didn't invoke on post build step (thx mhosoya)

## [2023.0.0] - 2023-05-09

### Added
- Add AppInstallationSourceValidator to easily figure out Android app installation source
- Add ObscuredCheatingDetector.LastDetectionInfo property with detection context
- Add built-in Newtonsoft Json Converter for Obscured Types
- Add CodeHashGenerator.GenerateAsync() API
- Add CodeHashGeneratorPostprocessor APIs:
	- CalculateBuildReportHashesAsync() method
	- CalculateExternalBuildHashesAsync() method
- Add HashGeneratorResult.PrintToConsole() API for debugging purposes
- Add state corruption checks when API accessed too early (before Awake)
- Add Windows build hashing progress bar in Editor
- Add Normalize() method and normalized property to ObscuredVector2, ObscuredVector3, ObscuredQuaternion
- Add buildPath argument to CalculateExternalBuildHashes so you could calculate hashes for any build path from CLI
- Add migration notes to the User Manual to help you migrate from v2021 to v2023

### Changed
- Update minimum Unity version to 2019.4
- Improve Obscured Types equality checks
- Improve how ObscuredFile handles custom path in some rare cases
- Significantly improve CodeHashGenerator performance:
	- Utilize all available cores in Editor's CodeHashGeneratorPostprocessor
	- Utilize specified threads count in Runtime CodeHashGenerator
	- Make Summary Hash generation magnitudes faster
- Change CodeHashGeneratorPostprocessor API:
	- Refactor Instance.callbackOrder to static CallbackOrder
	- Refactor Instance.HashesGenerated to static HashesGenerated
	- Refactor HashesGenerated delegate `BuildHashes[]` hashedBuilds argument to `IReadOnlyList<BuildHashes> hashedBuilds`
- Refactor BuildHashes.FileHashes property type from Array to IReadOnlyList
- Refactor HashGeneratorResult.FileHashes property type from Array to IReadOnlyList
- Improve CodeHashGeneratorPostprocessor progress reporting in Editor
- Improve CodeHashGenerator filtering to include all .dex and .so files on Android
- Prepare CodeHashGenerator filtering to include content files so whole build could be covered in future
- Introduce various minor improvements

### Removed
- Remove static CodeHashGeneratorPostprocessor.Instance property

### Fixed
- Fix InjectionDetector build processor could keep the service temp file if build fails
- Fix ObscuredBigInteger.Equals(ObscuredBigInteger) check didn't work properly
- Fix ObscuredBigInteger.GetHashCode() did return value affected by random crypto key
- Fix ObscuredFile could have inconsistent path delimiters in the FilePath
- Fix some critical errors didn't print to console
- Fix regression where ACTK_PREVENT_READ_PHONE_STATE didn't remove permissions caused by SystemInfo.deviceUniqueIdentifier

## [2021.6.4] - 2023-03-09
_I know it's 2023 already, fine? xD_

### Changed
- Improve Unity 2023 compatibility

### Fixed
- Fix inspector fields regression introduced at v2021.2.1 for Unity versions below 2022.2 (thx mrm83)
- Fix possible SpeedHackDetector false positives regression introduced at v2021.3.0, now DSP module is optional and off by default with proper warning about its sensitivity (thx mrm83, Kazeon, gpedrani and others 🙏)
- Fix some buttons didn't open Project Settings in Unity 2019+

## [2021.6.3] - 2022-12-19

### Added
- Add few more operators to the ObscuredBigInteger to better match BigInteger API.

### Changed
- Change CodeHashGenerator Editor warning to error to make it more visible and reduce possible confusion

## [2021.6.2] - 2022-11-12

### Changed
- Make ObscuredCheatingDetector to print logs when ACTK_DETECTION_BACKLOGS is enabled
- Improve Obscured Types serialization Validation logs to include exact path and location

## [2021.6.1] - 2022-11-10

### Fixed
- Fix rare SpeedHackDetector false positive

## [2021.6.0] - 2022-11-09

### Added
- Add new WallHackDetector compatibility check and safety warning (thx naezith)
- Add serialization corruption detection for Obscured Types

### Changed
- Improve ObscuredVector2Int and ObscuredVector3Int vector components access performance

### Fixed
- Reduce rare SpeedHackDetector false positive possibility
- Fix few rare ObscuredCheatingDetector false positives (thx thiagolr)

## [2021.5.1] - 2022-09-10

### Changed
- Improve ObscuredFilePrefsAutoSaver behavior in Editor (thx YeahBoi)

### Fixed
- Fix ObscuredBigInteger corruption (thx jaeyoung)
- Fix ambiguous APIs at the ObscuredBigInteger

## [2021.5.0] - 2022-07-31

### Added
- Add IDisposable implementation to the SHA1Wrapper class
- Add DurationSeconds property to the CodeHashGenerator results

### Changed
- Improve CodeHashGenerator accuracy in Editor for IL2CPP platforms
- Deprecate few obsolete CodeHashGenerator APIs
- Improve Unity 2023 compatibility

### Fixed
- Fix WebGL compilation regression

## [2021.4.2] - 2022-07-25

### Fixed
- ObscuredCheatingDetector: fix possible rare false positive (thx tbiz5270)

## [2021.4.1] - 2022-07-21

### Fixed
- SpeedHackDetector: fix possible rare false positive in Editor

## [2021.4.0] - 2022-07-16

### Added
- Add LastOnlineTimeResult instance property to the TimeCheatingDetector
- Add automatic ProGuard configuration to prevent errors due to minification
- Add new menu item to configure proguard-user.txt on demand

### Changed
- Make ProGuard configuration more granular to obfuscate more of the native code
- Expose internal TimeCheatingDetector.IsReadyForForceCheck() API

### Fixed
- Fix possible TimeCheatingDetector error due to certificate validation (thx murat303)

## [2021.3.0] - 2022-07-10

### Changed
- Improve Speed Hack Detector sensitivity in sandboxed environments
- Improve detectors' keepAlive logic when using additive scenes
- Improve WebGL file system compatibility at Obscured File and Obscured File Prefs

### Fixed
- Fix possible undesired detector self-destroy on additive scene load

## [2021.2.1] - 2022-07-04

### Changed
- Change some property drawers to use Delayed fields to reduce CPU overhead while editing obscured fields in inspector

## [2021.2.0] - 2022-06-29

### Added
- Add ObscuredBigInteger type
- Add BigInteger type support to the ObscuredPrefs / ObscuredFilePrefs
- Add TriggerDetection() utility method to all detectors
- Add 'Trigger detection' context menu item to all detectors components

## [2021.1.1] - 2022-05-04

### Added
- Add TimeCheatingDetector.GetOnlineTimeTask() overloads with CancellationToken argument

## [2021.1.0] - 2022-04-11

### Added
- Add ObscuredFilePrefs Auto Save on mobile platforms (enabled by default)
	- Automatically saves unsaved changes on app loose focus / pause
- Add API to disable ObscuredFilePrefs Auto Save (disables Auto Save on both mobile and non-mobile platforms)
- Introduce IObscuredFileSettings to improve API usage experience

### Changed
- Add locks to the ObscuredFilePrefs sync operations to improve stability when accessing it from different threads
- Move ObscuredFilePrefs Save-On-Quit code to the Auto Save feature entity so it's disableable now

### Fixed
- Prevent ObscuredFilePrefs Save-On-Quit while not initialized
- Fix ObscuredFilePrefs behavior with disabled Reload Domain
- Fix compilation error at Unity 2018 Android
- Fix compilation warnings for WebGL platform

## [2021.0.10] - 2022-03-09

### Fixed
- Fix ObscuredString name in Inspector might render incorrect in arrays (thx Sungmin An)

## [2021.0.9] - 2022-03-06

### Changed
- CodeHashGenerator's Summary Hash is no longer printed for AAB builds
- Skip Android Patch Packages hashing by CodeHashGenerator

### Fixed
- Fix obsolete API usage leading to compilation error in Unity 2022.1

## [2021.0.8] - 2022-02-08

### Changed
- Minor Prefs Editor UI improvements

### Fixed
- Fix Prefs Editor window didn't update properly under specific conditions (thx Todd Gillissie)

## [2021.0.7] - 2021-11-18

### Fixed
- Fix iOS Conditional compilation constants settings could not apply in some Unity versions (thx Hesham)
- Fix empty ObscuredString fields automatic migration (thx thiagolr)

## [2021.0.6] - 2021-11-18

### Changed
- Warn when trying to use ObscuredFile with StreamingAssets on Android and WebGL (thx Harama)

### Fixed
- Fix automatic ObscuredString migration didn't happen properly in some cases (thx thiagolr)
- Fix exception in ObscuredFilePrefs on iOS could happen under rare conditions
- Fix ObscuredString example log

## [2021.0.5] - 2021-10-26

### Changed
- Improve ObscuredPrefs and ObscuredFilePrefs compatibility with Obscured types

### Fixed
- Fix TimeUtils could be disposed unexpectedly (thx Hesham)
- Fix TimeUtils might not reinitialize properly in rare case

## [2021.0.4] - 2021-10-02

### Fixed
- Fix BehaviorDesigner integration package compilation errors (thx Levent)

## [2021.0.3] - 2021-09-27

### Changed
- Improve TimeCheatingDetector performance a bit

### Fixed
- Fix missing script at the example scene
- Fix CodeHashGeneratorListener example compilation errors

## [2021.0.2] - 2021-09-17

### Fixed
- Fix empty string was read as null from ObscuredPrefs and ObscuredFilePrefs (thx C0dingschmuser)

## [2021.0.1] - 2021-09-10

### Changed
- Improve exceptions logging a bit

### Fixed
- Fix compilation exception for iOS platform (thx Vladnee)

## [2021.0.0] - 2021-09-06

### Added
- Add new ObscuredFile and ObscuredFilePrefs tools to the ACTk 🧰
	- Encrypted and plain modes
	- All modes have data consistency validation
	- All modes have lock to device feature
	- ObscuredFilePrefs has simple and easy to use PlayerPrefs-like APIs
	- Async compatible
	- Supports UWP starting from Unity 2019.1
	- BehaviorDesigner and PlayMaker Actions
- Add generic APIs to ObscuredPrefs
- Add new types support to ObscuredPrefs:
	- rest of simple C# types (SByte, Byte, Int16, UInt16, Char)
	- System.DateTime
	- Color (it's possible to save HDR colors now)
	- Matrix4x4, RangeInt, Ray, Ray2D, RectInt, Vector2Int, Vector3Int, Vector4
- Add ObscuredQuaternion property drawer (now it's editable from inspector)
- Add automatic link.xml generation option to complement fix for WallHack Detector false positives due to stripping
- Add additional information to the important error logs
- Make ThreadSafeRandom utility public
- Add Copy Player Prefs path context menu item to the Prefs Editor tab
- Add ObscuredPrefs Vector2Int and Vector3Int support to BehaviorDesigner integration
- Add new support contact, let's chat at [Discord](https://discord.gg/Ppsb89naWf)!

### Changed
- Swap Changelog to md version to better match Unity packages format ([Keep a Changelog](https://keepachangelog.com/en/1.0.0/))
- Rename following ObscuredPrefs API in order to better suite coding style:
	- OnAlterationDetected -> NotGenuineDataDetected
	- OnPossibleForeignSavesDetected -> DataFromAnotherDeviceDetected
	- lockToDevice -> DeviceLockLevel
- Move ObscuredPrefs.DeviceLockLevel enum out from the ObscuredPrefs type
- Introduce DeviceLockTamperingSensitivity instead of readForeignSaves and emergencyMode settings for additional clarity
- Decimal values processing at ObscuredPrefs are much faster now with much lesser GC-allocations footprint
- Improve exceptions handling across whole codebase
- Improve incorrect type usage handling at ObscuredPrefs (thx David E)
- Improve Settings UI a bit
- Improve detectors startup a bit
- Improve Prefs Editor error handling
- Minor code refactoring and cleanup
- Update some API docs

### Deprecated
- Deprecate non-generic ObscuredPrefs APIs (to be removed in future versions)

### Removed
- Remove .NET 3.5 scripting runtime version support

### Fixed
- Fix possible data corruption at all Obscured types in super rare scenarios (only one rare case for ObscuredBool was found)
- Fix possible false positives from WallHackDetector on Unity 2019.3 or newer when IL2CPP "Strip Engine Code" setting is used (thx Hesham)
- Fix compilation warning on UWP platform
- Fix redundant injection detector support were added into IL2CPP builds in some conditions
- Fix exceptions in Unity 2021.2 and newer while browsing ACTk settings
- Fix code hash pre-generation was run redundantly when building with Create Visual Studio Solution option enabled
- Fix Behavior Tree at BehaviorDesigner's integration ObscuredPrefsExample scene
- Fix other minor stuff here and there

## [2.3.4] and older

See older versions changelog in legacy text format [here](https://codestage.net/uas_files/actk/changelog-legacy.txt)