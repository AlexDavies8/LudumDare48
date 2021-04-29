using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardHandler : MonoBehaviour
{
	/*
	const string _appPassword = @"WYbsgbsJszOwtHDfS$rn";
	const string _dbPassword = @"v^E4aQEg-Hyk<(s4";
	*/

	const string _highscoreURL = "https://ld48.000webhostapp.com/highscore.php";

	/* old leaderboard
	const string _privateCode = @"dmsGV7a2Ck-awI4BsBq9nA-HwBSSDkFkWu2M2NlBVUcg";
    const string _publicCode = @"60858bfc8f40bb12282be45b";
	const string _webURL = "http://dreamlo.com/lb/";
	*/

	public void SetScore(string name, int score)
    {
		if (name.Trim().Length != 0)
			StartCoroutine(PostScore(name.ToUpper(), score));
	}

	public void DownloadHighscores(Action<LeaderboardEntry[]> callback)
	{
		StartCoroutine(GetLeaderboard(callback));
	}

	IEnumerator GetLeaderboard(Action<LeaderboardEntry[]> callback)
	{
		WWWForm form = new WWWForm();
		form.AddField("retrieve_leaderboard", "true");

		UnityWebRequest uwr = UnityWebRequest.Post(_highscoreURL, form);

		yield return uwr.SendWebRequest();

		if (uwr.result != UnityWebRequest.Result.ConnectionError)
		{
			Debug.Log("Leaderboard Downloaded");

			List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

			string contents = uwr.downloadHandler.text;

			Debug.Log(contents);

			using (StringReader reader = new StringReader(contents))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					LeaderboardEntry entry = new LeaderboardEntry();
					entry.name = line;
					try
					{
						entry.score = int.Parse(reader.ReadLine());
					}
					catch (Exception e)
					{
						Debug.Log("Invalid score: " + e);
						continue;
					}

					entries.Add(entry);
				}
			}
			callback?.Invoke(entries.ToArray());
		}
		else
			Debug.Log("Error Downloading Leaderboard");
	}

	IEnumerator PostScore(string name, int score)
	{
		WWWForm form = new WWWForm();
		form.AddField("post_leaderboard", "true");
		form.AddField("name", name);
		form.AddField("score", score);

		UnityWebRequest uwr = UnityWebRequest.Post(_highscoreURL, form);

		yield return uwr.SendWebRequest();

		if (uwr.result != UnityWebRequest.Result.ConnectionError)
		{
			Debug.Log("Score Posted!");
		}
		else
			Debug.Log("Error Posting Score");
	}

	public struct LeaderboardEntry
	{
		public string name;
		public int score;
		public string date;
	}

}
