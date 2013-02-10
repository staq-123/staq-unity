using UnityEngine;
using System.Collections;

public class Staq : MonoBehaviour
{
	// Update is called once per frame
	void Update () {
	
	}
	
	/// <summary>
	/// The current instance of the client.
	/// </summary>
	static Staq defaultInstance;
	
	public string GameId = "game id";
	public bool Connected = false;
	
	StaqRest staqRestClient = null;
	
	/// <summary>
	/// Setup this component
	/// </summary>
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
	
	/// <summary>
	/// Initialization
	/// </summary>
	void Start () {
		// TODO: Application.RegisterLogCallback(StaqDebug.HandleLog);
		
		Connected = IsConnected();
		
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
	
		staqRestClient = new StaqRest(GameId, this);
		StartCoroutine(staqRestClient.StartPosting());
		
		//Start the submit queue for sending messages to the server
		/*if (!CustomUserID && StaqGenericInfo.UserID != string.Empty)
			RunCoroutine(StaqQueue.SubmitQueue());
		
		//If we're playing the unity demo "AngryBots", then add the Staq_AngryBots component
		if (Application.loadedLevelName == "AngryBots")
		{
			gameObject.AddComponent("Staq_AngryBots");
		}*/
	}
	
	public static void OverrideUserId(string userId)
	{
		if (!string.IsNullOrEmpty(userId))
		{
			StaqDeviceInfo.OverridUserIdInternal(userId);
			
			//RunCoroutine(StaqQueue.SubmitQueue());
		}
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
