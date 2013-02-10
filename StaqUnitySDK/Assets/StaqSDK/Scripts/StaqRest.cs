using UnityEngine;
using System;
using System.Collections;

public class StaqRest {
	public StaqRest(string gameId, MonoBehaviour behaviour)
	{
		this.gameId = gameId;
		this.behaviour = behaviour;
	}
	
	string gameId;
	string uid;
	string authToken;
	MonoBehaviour behaviour;
	
	static float postIntervalSeconds = .5F;
	
	public IEnumerator StartPosting()
	{
		while (true) // TODO: check whether we should keep posting events
		{
			// if offline log to storage
			
			// if not authenticated do auth
			if (uid == null || authToken == null)
				yield return behaviour.StartCoroutine(Auth("testapp1", "teeeeeest!!!")); // TODO: pass the right appId and UDID
			
			Debug.Log("Posting data...");
			
			// submit data
			//yield return behaviour.StartCoroutine(StartDataSubmit());
			
			// wait timer
			yield return new WaitForSeconds(postIntervalSeconds);
		}
	}
	
	IEnumerator StartDataSubmit()
	{
		Debug.Log("Begin submit.");
		
		// TODO: submit the queue to the server
		
		Debug.Log("Done submit.");
		yield return null;
	}
	
	IEnumerator Auth(string appId, string udid)
	{
		var www = new WWW(FormatStaqUrl(appId) + "auth/device/" + udid);
		yield return www;
	
		var parsedResponse = StaqLitJson.JsonMapper.ToObject(www.text);
		
		uid = (string)parsedResponse["uid"];
		authToken = (string)parsedResponse["authtoken"];
		Debug.Log("Count: " + parsedResponse.Count);
	}
	
	static string FormatStaqUrl(string appId)
	{
		return FormatStaqUrl(appId, null);
	}
	
	static string FormatStaqUrl(string appId, string userId)
	{
		string staqBaseUrl = "http://staqapi.cloudapp.net/v1/";
		
		if (userId != null)
			return string.Format("{0}apps/{1}/users/{2}/", staqBaseUrl, appId, userId);
		
		return string.Format("{0}apps/{1}/", staqBaseUrl, appId);
	}
}


