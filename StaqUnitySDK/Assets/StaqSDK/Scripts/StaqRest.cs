using UnityEngine;
using System.Collections;

public class StaqRest {
	public StaqRest(string gameId, MonoBehaviour behaviour)
	{
		this.gameId = gameId;
		this.behaviour = behaviour;
	}
	
	string gameId;
	MonoBehaviour behaviour;
	
	static float postIntervalSeconds = .5F;
	
	public IEnumerator StartPosting()
	{
		while (true) // TODO: check whether we should keep posting events
		{
			// check if we are already submitting data
			// while (submittind) yeld new WaitForSeconds(.5);
			
			Debug.Log("Posting data...");
			
			// submit data
			yield return behaviour.StartCoroutine(StartDataSubmit());
			
			// wait timer
			yield return new WaitForSeconds(postIntervalSeconds);
		}
	}
	
	IEnumerator StartDataSubmit()
	{
		Debug.Log("Begin submit.");
		
		yield return behaviour.StartCoroutine(Auth("", null));
		
		Debug.Log("Done submit.");
		yield return null;
	}
	
	IEnumerator Auth(string udid, string[] token)
	{
		var www = new WWW("http://staqapi.cloudapp.net/");
		
		yield return www;
		
		var content = www.text;
	}
}
