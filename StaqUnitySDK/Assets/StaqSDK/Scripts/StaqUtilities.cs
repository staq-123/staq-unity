using UnityEngine;
using System.Collections;

public static class StaqUtilities
{
	public static void Log(string message)
	{
		Debug.Log(string.Format("//staq: {0}", message));
	}
	
	public static void LogError(string message)
	{
		Debug.LogError(string.Format("//staq: {0}", message));
	}
}
