using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Net;
using System.Security.Cryptography;

#if !UNITY_WEBPLAYER
using System.Net.NetworkInformation;
#endif

public static class StaqDeviceInfo {
	
	static string userId;
	
	/// <summary>
	/// The current player uid
	/// </summary>
	public static string UserId
	{
		get {
			/*if (userId == null && !Staq.CUSTOMUSERID)
			{
				if (PlayerPrefs.HasKey("staq_uid"))
				{
					userId = PlayerPrefs.GetString("staq_uid");
				}
				else
				{
					userId = CreateUid();
					PlayerPrefs.SetString("staq_uid", userId);
					PlayerPrefs.Save();
				}
			}*/
			return userId;
		}
	}
	
	public static void OverridUserIdInternal(string userId)
	{
		StaqDeviceInfo.userId = userId;
	}
	
	/// <summary>
	/// Creates a universally unique ID to represent the user.
	/// </summary>
	/// <returns>
	/// The generated UUID <see cref="System.String"/>
	/// </returns>
	public static string CreateUid()
	{
		#if UNITY_ANDROID
		
		return SystemInfo.deviceUniqueIdentifier;
		
		#elif UNITY_WEBPLAYER
		
		return SystemInfo.deviceUniqueIdentifier;
		
		#else
		
		var nics = NetworkInterface.GetAllNetworkInterfaces();
		string mac = "";

        foreach (var adapter in nics)
        {
        	var address = adapter.GetPhysicalAddress();
			if (address.ToString() != "" && mac == "")
			{
				byte[] bytes = address.GetAddressBytes();
				mac = CreateSha1Hash(bytes);
			}
		}
		return mac;
		
		#endif
	}
	
	static string CreateSha1Hash(byte[] input)
	{
		var sha1 = new SHA1CryptoServiceProvider();
		byte[] hash = sha1.ComputeHash(input);
		
		return Convert.ToBase64String(hash);
	}
}
