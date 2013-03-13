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
	
	public static bool IsResponseValid(WWW www)
	{
		if (www.error != null)
			return false;
		
		var status = www.responseHeaders["STATUS"];
		var tokens = status.Split(' ');
		int statusCode;
		
		if (tokens.Length > 1 && int.TryParse(tokens[1], out statusCode))
			if (statusCode >= 200 && statusCode < 300)
				return true;
		
		return false;
	}
	
	public static IEnumerator Sleep(long milliseconds)
	{
		yield return new WaitForSeconds(milliseconds / 1000);
	}
	
	public static string FormatStaqUrl(string appId)
	{
		return FormatStaqUrl(appId, null);
	}
	
	public static string FormatStaqUrl(string appId, string userId)
	{
		string staqBaseUrl = "http://staqapi.cloudapp.net/v1/";
		
		if (userId != null)
			return string.Format("{0}apps/{1}/users/{2}/", staqBaseUrl, appId, userId);
		
		return string.Format("{0}apps/{1}/", staqBaseUrl, appId);
	}
	
	/// <summary>
	/// Checks whether staq.io is reachable
	/// </summary>
	public static bool IsConnected()
	{
		#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
		
		try
		{
			System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient("staq.io", 80);
			clnt.Close();
			return true;
		}
		catch(System.Exception)
		{
			return false;
		}
		
		#else
		
		if  (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork || 
			(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && ALLOWROAMING))
		{
			return true;
		}
		else
		{
			return false;
		}
		
		#endif
	}
}
