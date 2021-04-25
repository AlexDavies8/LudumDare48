using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardHandler : MonoBehaviour
{
    const string _privateCode = @"dmsGV7a2Ck-awI4BsBq9nA-HwBSSDkFkWu2M2NlBVUcg";
    const string _publicCode = @"60858bfc8f40bb12282be45b";
	const string _webURL = "http://dreamlo.com/lb/";

	public void SetScore(string name, int score)
    {
		if (name.Trim().Length != 0)
			StartCoroutine(UploadNewHighscore(name.ToUpper(), score));
    }

	IEnumerator UploadNewHighscore(string username, int score)
	{
		UnityWebRequest uwr = UnityWebRequest.Get($"{_webURL}{_privateCode}/add/{UnityWebRequest.EscapeURL(username)}/{score}");
		Debug.Log($"{_webURL}{_privateCode}/add/{UnityWebRequest.EscapeURL(username)}/{score}");
		yield return uwr.SendWebRequest();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.LogWarning("Error sending highscore");
		}
		else
		{
			Debug.Log("Score sent successfully");
			Debug.Log(uwr.downloadHandler.text);
		}
	}

	[ContextMenu("Download Highscores")]
	public void DownloadHighscores(Action<LeaderboardEntry[]> callback)
	{
		StartCoroutine(DownloadHighscoresFromDatabase(callback));
	}

	IEnumerator DownloadHighscoresFromDatabase(Action<LeaderboardEntry[]> callback)
	{
		UnityWebRequest uwr = UnityWebRequest.Get($"{_webURL}{_publicCode}/quote/");

		yield return uwr.SendWebRequest();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.LogWarning("Error getting highscores");
		}
		else
		{
			Debug.Log("Scores retrieved successfully");
			FormatHighscores(uwr.downloadHandler.text, callback);
		}
	}

	void FormatHighscores(string textStream, Action<LeaderboardEntry[]> callback)
	{
		Debug.Log(textStream);
		string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		LeaderboardEntry[] highscoresList = new LeaderboardEntry[entries.Length];

		for (int i = 0; i < entries.Length; i++)
		{
			string[] entryInfo = entries[i].Split(new char[] { ',' });
			string username = entryInfo[0].Trim('"');
			int score = int.Parse(entryInfo[1].Trim('"'));
			string date = entryInfo[4].Trim('"');
			highscoresList[i] = new LeaderboardEntry() { name=username, score=score, date=date};
		}

		callback?.Invoke(highscoresList);
	}

	public struct LeaderboardEntry
	{
		public string name;
		public int score;
		public string date;
	}

}
