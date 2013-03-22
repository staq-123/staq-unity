using UnityEngine;
using System.Collections;

public class Staq : MonoBehaviour
{
	/// <summary>
	/// The current instance of the client.
	/// </summary>
	static Staq defaultInstance;
	
	public string GameId = "AppId";
	public bool Connected = false;
	public bool EnforceIapOnError = false;
	
	StaqRest staqRestClient = null;
	
	public void Awake ()
	{
		//make sure we only have one object with this Staq script at any time
		if (defaultInstance != null)
		{
			Destroy(gameObject);
			return;
		}
		
		/*if (PublicKey.Equals("") || PrivateKey.Equals(""))
		{
			Debug.LogWarning("Staq Error: Public key and/or private key not set.");
			Destroy(gameObject);
			return;
		}*/
		
		defaultInstance = this;
		DontDestroyOnLoad(this);
		
		//gameObject.AddComponent<StaqSpecialEvents>();
		//gameObject.AddComponent<StaqGui>();
	}
	
	void Start () {
		// TODO: Application.RegisterLogCallback(StaqDebug.HandleLog);
		
		Connected = StaqUtilities.IsConnected();
		
		/*if (DebugMode)
		{
			if (InternetConnectivity)
				Debug.Log("Staq Wrapper initialized, waiting for events..");
			else
				Debug.Log("Staq Wrapper detects no internet connection..");
		}*/
		
		// Add system specs to the submit queue
		/*if (true)
		{
			var systemspecs = StaqDeviceInfo.GetGenericInfo("");
			
			foreach (Dictionary<string, object> spec in systemspecs)
			{
				StaqQueue.AddItem(spec, StaqSubmit.CategoryType.StaqLog, false);
			}
		}*/
	
		staqRestClient = new StaqRest(GameId, StaqDeviceInfo.CreateUid(), this);
		StartCoroutine(staqRestClient.StartPosting());
		staqRestClient.AppendSessionStart();
		
		//Start the submit queue for sending messages to the server
		/*if (!CustomUserID && StaqGenericInfo.UserID != string.Empty)
			RunCoroutine(StaqQueue.SubmitQueue());
		
		//If we're playing the unity demo "AngryBots", then add the Staq_AngryBots component
		if (Application.loadedLevelName == "AngryBots")
		{
			gameObject.AddComponent("Staq_AngryBots");
		}*/
	}
	
    void OnApplicationQuit()
    {
        // app quits
		staqRestClient.AppendSessionEnd();
    }
	
	public static void OverrideUserId(string userId)
	{
		if (!string.IsNullOrEmpty(userId))
		{
			StaqDeviceInfo.OverridUserIdInternal(userId);
			
			//RunCoroutine(StaqQueue.SubmitQueue());
		}
	}
	
	public static void Iap(string platform, string receipt, string itemId, double price)
	{
		defaultInstance.staqRestClient.AppendIap(platform, receipt, itemId, price);
	}
	
	/*public static void SessionStart()
	{
		defaultInstance.staqRestClient.AppendSessionStart();
	}
	
	public static void SessionEnd()
	{
		defaultInstance.staqRestClient.AppendSessionEnd();
	}*/
}
