using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using StaqLitJson;
using System.IO;

public class StaqRest {
	public StaqRest(string gameId, string udid, MonoBehaviour behaviour)
	{
		this.udid = udid;
		this.gameId = gameId;
		this.behaviour = behaviour;
	}
	
	string udid;
	string gameId;
	string uid;
	string authToken;
	MonoBehaviour behaviour;
	Queue<EventModel> eventQueue = new Queue<EventModel>();
	
	static float postIntervalSeconds = .5F;
	const int MAX_FILE_SIZE = 2 * 1024 * 1024;
	
	string currentSession = Guid.NewGuid().ToString();
	DateTime currentSessionStart = DateTime.MinValue;
	
	public IEnumerator StartPosting()
	{
		while (true) // TODO: check whether we should keep posting events
		{
			if (StaqUtilities.IsConnected())
			{
				// if not authenticated do auth
				if (uid == null || authToken == null)
					yield return behaviour.StartCoroutine(Auth(udid)); // TODO: pass the right appId and UDID
				
				StaqUtilities.Log("Posting data...");
				
				// submit data
				yield return behaviour.StartCoroutine(PostEvents());
			}
			else
			{
				var events = eventQueue.ToArray();
				eventQueue.Clear();
				
				// if offline log to storage
				var sessionData = GetRequestContents(events);
				
				SaveToDisk(sessionData);
			}
			
			// wait timer
			yield return new WaitForSeconds(postIntervalSeconds);
		}
	}
	
	IEnumerator PostEvents()
	{
		StaqUtilities.Log("Begin submit.");
		
		// read old events from file
		var oldEvents = ReadFromDisk().ToArray();
		
		var writeQueue = new Queue<string>(oldEvents);
		
		// get new events from the queue
		var newEvents = GetRequestContents(eventQueue.ToArray());
		
		// clear queue
		eventQueue.Clear();
		
		foreach (var ev in newEvents)
			writeQueue.Enqueue(ev);
		
		string current;
		while ((current = writeQueue.Dequeue ()) != null)
		{
			var www = PostWWW(StaqUtilities.FormatStaqUrl(gameId, uid), current, authToken);
			yield return www;
			
			if (!StaqUtilities.IsResponseValid(www))
			{
				writeQueue.Enqueue(current);
				break;
			}
		}
		
		var notPosted = writeQueue.ToArray();
		if (notPosted.Length > 0)
		{
			StaqUtilities.Log("Failed posting " + notPosted.Length + " sessions, writing to disk.");
			SaveToDisk (notPosted);
		}
		
		StaqUtilities.Log("Done submit.");
		yield return null;
	}
	
	IEnumerable<string> GetRequestContents(EventModel[] events)
	{
		var bySession = eventQueue.GroupBy(ev => ev.sid).ToArray();
		var request = bySession.Select(session => new
		{
			sid = session.Key,
			events = session.Select(ev => new
			{
				ev.@event,
				ev.timestamp,
				ev.value,
				ev.metadata
			}).ToArray()
		});
		
		foreach (var item in request)
		{
			yield return JsonMapper.ToJson(item)
			.Replace("\n", "")
			.Replace("\r", "");
		}
	}
	
	IEnumerator Auth(string udid)
	{
		var uri = StaqUtilities.FormatStaqUrl(gameId) + "auth/device"; 
		
		var content = JsonMapper.ToJson(new {	
			deviceInfo = new { os = "iOS" },
			deviceId = udid
		});
		
		var www = PostWWW(uri, content, null);
		
		yield return www;
	
		var parsedResponse = StaqLitJson.JsonMapper.ToObject(www.text);
		
		uid = (string)parsedResponse["uid"];
		authToken = (string)parsedResponse["authtoken"];
		currentSession = (string)parsedResponse["sid"];
		StaqUtilities.Log("Auth: " + uid);
	}
	
	public void AppendEvent(string @event, object value, object metadata)
	{
		if (currentSession == null)
			return;
		
		var currentEvent = new EventModel
		{
			sid = currentSession,
			@event = @event,
			timestamp = DateTime.UtcNow,
			value = value,
			metadata = metadata
		};
		
		eventQueue.Enqueue(currentEvent);
	}
	
	public void AppendSessionStart()
	{
		StaqUtilities.Log("Session start.");
		
		currentSession = Guid.NewGuid().ToString();
		currentSessionStart = DateTime.UtcNow;
		
		AppendEvent("SESSION_START", null, null);
	}
	
	public void AppendSessionEnd()
	{
		StaqUtilities.Log("Session end.");
		if (currentSession == null)
			return;
		
		var sessionLength = (currentSessionStart - DateTime.UtcNow).TotalSeconds;
		
		AppendEvent("SESSION_END", sessionLength, null);
		
		currentSession = null;
		currentSessionStart = DateTime.MinValue;
	}
	
	public void AppendIap(string platform, string receipt, string itemId, double price)
	{
		StaqUtilities.Log("IAP.");
		if (currentSession == null)
			return;
		
		AppendEvent("IAP", itemId, new
		{
			price_usd = price,
			platform = platform,
			receipt = receipt
		});
	}
	
	class EventModel
	{
		public string sid { get; set; }
		public string @event { get; set; }
		public DateTime timestamp { get; set; }
		public object value { get; set; }
		public object metadata { get; set; }
	}
	
	public bool VerifyReceipt(string receipt)
	{
		var www = new WWW("http://api.com");
  
        /*  client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Auth-Token", token);

            var url = string.Format("http://api.staq.io/v1/apps/" + app + "/users/{0}/iap/apple", uid);
            var content = new
            {
                sid,
                receipt,
                priceUsd = price,
                status
            };

            var body = JsonConvert.SerializeObject(content, jsonSettings);
            var resp = UploadString(client, url, body);
            return (bool)((dynamic)JObject.Parse(resp)).isValid;
		*/
		while (!www.isDone)
		{
			behaviour.StartCoroutine(StaqUtilities.Sleep(200));
			Debug.Log("Not too often!");
		}
		
		return true;
	}
	
	static IEnumerable<string> ReadFromDisk()
	{
		var filename = "StaqEvents.log";
		var filepath = Application.persistentDataPath + "/" + filename;
		var exists = File.Exists(filepath);
		
		if (!exists)
			yield break;
		
		foreach (var line in File.ReadAllLines(filepath))
			yield return line;
		
		File.Delete(filepath);
	}
	
	static void SaveToDisk(IEnumerable<string> sessions)
	{
		using (var writer = GetArchiveWriter())
		{
			if (writer != null)
			{
				foreach (var line in sessions)
					writer.WriteLine(line);
			}
		}
	}
	
	static StreamWriter GetArchiveWriter()
	{
		var filename = "StaqEvents.log";
		var filepath = Application.persistentDataPath + "/" + filename;
		var exists = File.Exists(filepath);
		
		if (exists && new FileInfo(filepath).Length > MAX_FILE_SIZE)
			return null;
		
		if (exists)
		{
			return File.AppendText(filepath);
		}
		else
		{
			return File.CreateText(filepath);
		}
	}
	
	static WWW PostWWW(string uri, string body, string token)
	{
		var headers = new Hashtable();
		headers.Add("Content-Type", "application/json");
		if (token != null)
			headers.Add("Auth-Token", token);
		
		var content = System.Text.Encoding.UTF8.GetBytes(body);
		
		return new WWW(uri, content, headers); 
	}
	
}


