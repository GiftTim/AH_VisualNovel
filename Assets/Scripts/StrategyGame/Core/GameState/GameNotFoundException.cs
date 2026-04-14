using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장 파일이 존재하지 않을 때 SaveSystem.Load()에서 던지는 예외.
/// GameManager.LoadData()에서 catch해 기본 게임 상태로 초기화하는 데 사용한다.
/// </summary>
public class GameNotFoundException : Exception {}

/// <summary>
/// 저장 파일이 존재하지만 JSON 형식이 올바르지 않을 때 던지는 예외.
/// SaveSystem.Load() → JsonUtility.FromJson 실패 시 이 예외로 감싸서 던진다.
/// GameManager.LoadData()에서 catch해 기본 게임 상태로 초기화한다.
/// </summary>
public class BadFormatGameException : Exception
{
    public BadFormatGameException(string message) : base(message) { }
}
