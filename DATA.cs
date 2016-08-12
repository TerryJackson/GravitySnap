using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class DATA 
{
	public static List<string> LevelNameList;
	public static List<bool> SaveGameList;
	public static List<int> CurrentHighScoreIndexList;

	private static string persistentDataPath =  Application.persistentDataPath + "/";


	public static void Initialize() 
	{
		LevelNameList = new List<string>();
		SaveGameList = new List<bool>();
		CurrentHighScoreIndexList = new List<int>();

		if (FirstTimeLoading())
		{
			LoadDefaultLevels();
		}
		else
		{
			DefineDataLists();
		}
	}
	

	// Test if this is the first time the game has been loaded on the device.
	// This is true if and only if the file "LevelOrderList" exits.
	// Even if the player deletes all the levels in the paid verision this file still exists.
	public static bool FirstTimeLoading()
	{
		return !File.Exists(persistentDataPath + "LevelOrderList.xml");
	}


	public static void LoadDefaultLevels()
	{
		GameObject.Find("GameManager").GetComponent<DefaultLevelsScript>().LoadDefaultLevels();
	}


	// Should only be called once from the Initialization method.
	private static void DefineDataLists()
	{
		if (NumberLevelsSaved() == 0)
		{
			Debug.Log("There are no levels");
			return; 
		}
		
		List<int> levelOrderList = GetLevelOrderList();
		SLevelData levelData;
		
		for (int i = 0; i < levelOrderList.Count; i++)
		{
			levelData = GetLevel(levelOrderList[i]);

			LevelNameList.Add(levelData.LevelName);
			SaveGameList.Add(levelData.SaveGameData != null);
			CurrentHighScoreIndexList.Add(levelData.HighScoreData.CurrentHighScoreIndex);
		}
	}

	
	//****************************
	// Methods for level files
	//****************************

	// Create a new level with a new ID.
	public static int SaveLevel(SLevelData ld)
	{
		int levelID = GetNewID();
		
		if (levelID == -1)
		{
			Debug.Log("Can not save a new level since the maximum number of levels have been saved.");
			return -1;
		}
		
		string fileName = "Level_" + levelID.ToString();
		string path = persistentDataPath + fileName + ".xml";
		
		SerializationHelper<SLevelData>.SaveToXML(path, ld);	
		AddLevelIDToLevelOrderList(levelID);

		// Modify the lists
		LevelNameList.Add(ld.LevelName);
		SaveGameList.Add(ld.SaveGameData != null);
		CurrentHighScoreIndexList.Add(ld.HighScoreData.CurrentHighScoreIndex);

		return levelID;
	}


	// Create a new level at a particular index.
	public static int SaveLevel(SLevelData ld, int index)
	{
		int levelID = GetNewID();
		
		if (levelID == -1)
		{
			Debug.Log("Can not save a new level since the maximum number of levels have been saved.");
			return -1;
		}
		
		string fileName = "Level_" + levelID.ToString();
		string path = persistentDataPath + fileName + ".xml";
		
		SerializationHelper<SLevelData>.SaveToXML(path, ld);	
		AddLevelIDToLevelOrderList(levelID, index);

		// Modify the lists
		LevelNameList.Insert(index, ld.LevelName);
		SaveGameList.Insert(index, ld.SaveGameData != null);
		CurrentHighScoreIndexList.Insert(index, ld.HighScoreData.CurrentHighScoreIndex);
		
		return levelID;
	}


	// Update a currently existing level
	public static void UpdateSavedLevel(int levelID, SLevelData levelData)
	{
		if (!LevelIDExists(levelID))
		{
			Debug.Log("The levelID is not valid");
			return;
		}
		
		string fileName = "Level_" + levelID.ToString();
		string path = persistentDataPath + fileName + ".xml";
		
		SerializationHelper<SLevelData>.SaveToXML(path, levelData);

		// Modify the lists
		int index = GetLevelIDIndex(levelID);
		LevelNameList[index] = levelData.LevelName;	// This line should not be necessary.
		SaveGameList[index] = (levelData.SaveGameData != null);
		CurrentHighScoreIndexList[index] = levelData.HighScoreData.CurrentHighScoreIndex;
	}


	// Modify the status of a level's save game status to not have a saved game.
	public static void RemoveSavedGame(int levelID)
	{
		if (!LevelIDExists(levelID))
		{
			Debug.Log("The levelID is not valid");
			return;
		}

		SLevelData levelData = GetLevel(levelID);	// Get the current level information.

		levelData.HighScoreData.CurrentHighScoreIndex = -1;
		levelData.SaveGameData = null;

		UpdateSavedLevel(levelID, levelData);

		// Modify the lists
		int index = GetLevelIDIndex(levelID);
		SaveGameList[index] = false;
		CurrentHighScoreIndexList[index] = -1;
	}


	// Given a high score table (oldHighScoreData), and a new score, modify the high score table (if necessary) based on that new score.
	public static SHighScoreData UpdateHighScoreData(SHighScoreData oldHighScoreData, int score, int cubes, int lines, int turns)
	{
		SHighScoreData newHighScoreData = new SHighScoreData();

		if (oldHighScoreData.CurrentHighScoreIndex == -1)
		{
			// Update high score data in the case that the current game was a new game or the
			// case that the current game was from a saved game whose score then did not qualify as a high score.
			
			newHighScoreData.CurrentHighScoreIndex = -1;
			
			// See if the score in the saved game data qualifies to be entered in the high score table.
			for (int i = 0; i < 5; i++)
			{
				if  (score > oldHighScoreData.ScoreArray[i])
				{
					newHighScoreData.CurrentHighScoreIndex = i;
					break;
				}
			}

			// The value of newHighScoreData.CurrentHighScoreIndex is now determined.

			if (newHighScoreData.CurrentHighScoreIndex != -1)
			{
				// Get a deep copy of the four arrays that make up the old high score data.
				newHighScoreData.ScoreArray = (int[])oldHighScoreData.ScoreArray.Clone();
				newHighScoreData.CubesArray = (int[])oldHighScoreData.CubesArray.Clone();
				newHighScoreData.LinesArray = (int[])oldHighScoreData.LinesArray.Clone();
				newHighScoreData.TurnsArray = (int[])oldHighScoreData.TurnsArray.Clone();
				
				// The high score data corresponding to the indicies 0,...,newHighScoreIndex - 1 do not need to be changed.
				
				// Update the new high score 
				newHighScoreData.ScoreArray[newHighScoreData.CurrentHighScoreIndex] = score;
				newHighScoreData.CubesArray[newHighScoreData.CurrentHighScoreIndex] = cubes;
				newHighScoreData.LinesArray[newHighScoreData.CurrentHighScoreIndex] = lines;
				newHighScoreData.TurnsArray[newHighScoreData.CurrentHighScoreIndex] = turns;
				
				// Update the other values by shifting the old values down by one.
				for (int i = newHighScoreData.CurrentHighScoreIndex + 1; i < 5; i++)
				{
					newHighScoreData.ScoreArray[i] = oldHighScoreData.ScoreArray[i - 1];
					newHighScoreData.CubesArray[i] = oldHighScoreData.CubesArray[i - 1];
					newHighScoreData.LinesArray[i] = oldHighScoreData.LinesArray[i - 1];
					newHighScoreData.TurnsArray[i] = oldHighScoreData.TurnsArray[i - 1];
				}
			}
			else
			{
				// In this case newHighScoreData.CurrentHighScoreIndex == -1, which means the current score still does not qualify 
				// to be put in the high score table. So the high score data does not to be changed.

				newHighScoreData = oldHighScoreData;
			}
		}
		else
		{
			// Update high score data in the case that the current game was a saved game whose high score 
			// is located at position oldHighScoreData.CurrentHighScoreIndex in the high score table.
			
			newHighScoreData.CurrentHighScoreIndex = oldHighScoreData.CurrentHighScoreIndex;
			
			for (int i = 0; i < oldHighScoreData.CurrentHighScoreIndex; i++)
			{
				if (score > oldHighScoreData.ScoreArray[i])
				{
					newHighScoreData.CurrentHighScoreIndex = i;
					break;
				}
			}

			// The value of newHighScoreData.CurrentHighScoreIndex is now determined.
			
			// Get a deep copy of the four arrays that make up the high score data.
			newHighScoreData.ScoreArray = (int[])oldHighScoreData.ScoreArray.Clone();
			newHighScoreData.CubesArray = (int[])oldHighScoreData.CubesArray.Clone();
			newHighScoreData.LinesArray = (int[])oldHighScoreData.LinesArray.Clone();
			newHighScoreData.TurnsArray = (int[])oldHighScoreData.TurnsArray.Clone();
			
			// The high score data corresponding to the indicies 0,...,newHighScoreData.CurrentHighScoreIndex - 1 and to
			// oldHighScoreData.CurrentHighScoreIndex + 1,...4  do not need to be changed.
			
			// Update the new high score.
			newHighScoreData.ScoreArray[newHighScoreData.CurrentHighScoreIndex] = score;
			newHighScoreData.CubesArray[newHighScoreData.CurrentHighScoreIndex] = cubes;
			newHighScoreData.LinesArray[newHighScoreData.CurrentHighScoreIndex] = lines;
			newHighScoreData.TurnsArray[newHighScoreData.CurrentHighScoreIndex] = turns;
			
			// Update the other values.
			for (int i = newHighScoreData.CurrentHighScoreIndex + 1; i <= oldHighScoreData.CurrentHighScoreIndex; i++)
			{
				newHighScoreData.ScoreArray[i] = oldHighScoreData.ScoreArray[i - 1];
				newHighScoreData.CubesArray[i] = oldHighScoreData.CubesArray[i - 1];
				newHighScoreData.LinesArray[i] = oldHighScoreData.LinesArray[i - 1];
				newHighScoreData.TurnsArray[i] = oldHighScoreData.TurnsArray[i - 1];
			}
		}

		return newHighScoreData;
	}


	// Method called from the button "Save and Quit" on the Pause Menu.
	public static void UpdateGameSave(int levelID, SSaveGameData saveGameData)
	{
		if (saveGameData == null)
			return;

		SLevelData levelData = GetLevel(levelID);	// Get the current level information.

		levelData.HighScoreData = UpdateHighScoreData(levelData.HighScoreData, 
		                                              saveGameData.Score, 
		                                              saveGameData.Cubes, 
		                                              saveGameData.Lines, 
		                                              saveGameData.Turns);

		levelData.SaveGameData = saveGameData;
	
		UpdateSavedLevel(levelID, levelData);
	}


	// Method to be called when a game ends. Returns the updated high score table.
	public static void EndGameSave(int levelID, int score, int cubes, int lines, int turns)
	{
		SLevelData levelData = GetLevel(levelID);	// Get the current level information.

		levelData.HighScoreData = UpdateHighScoreData(levelData.HighScoreData, 
		                                              score, 
		                                              cubes, 
		                                              lines, 
		                                              turns);

		// Set the highScoreData for the High Score screen of the Game Over panel before we set levelData.HighScoreData.CurrentHighScoreIndex = -1
		PANEL_PUGO.SetHighScoreData(levelData.HighScoreData, levelData.LevelName);

		levelData.HighScoreData.CurrentHighScoreIndex = -1;		// There is no longer a current high score index.
		levelData.SaveGameData = null;							// There is no longer a saved game present.

		UpdateSavedLevel(levelID, levelData);
	}


	public static SLevelData GetLevel(int levelID)
	{
		string fileName = "Level_" + levelID.ToString();
		string path = persistentDataPath + fileName + ".xml";
		
		if (!File.Exists(path))
		{
			Debug.Log("No level exits with that ID");
			return null;
		}
		
		return SerializationHelper<SLevelData>.LoadFromXML(path);
	}
	
	
	public static void DeleteLevel(int levelID)
	{
		string fileName = "Level_" + levelID.ToString();
		string path = persistentDataPath + fileName + ".xml";
		
		if (!File.Exists(path))
		{
			Debug.Log("Can not delete level file since it does not exist");
			return;
		}
		
		File.Delete(path);	

		int index = GetLevelIDIndex(levelID);
		LevelNameList.RemoveAt(index);
		SaveGameList.RemoveAt(index);
		CurrentHighScoreIndexList.RemoveAt(index);
		DeleteLevelIDFromLevelOrderList(levelID);
	}


	public static int DuplicateLevel(int levelID)
	{
		SLevelData levelData = GetLevel(levelID);

		return SaveLevel(levelData);
	}


	public static bool SaveGamePresent(int levelID)
	{
		if (!LevelIDExists(levelID))
		{
			Debug.Log("The levelID is not valid");
			return false;
		}

		int index = GetLevelIDIndex(levelID);

		return SaveGameList[index];
	}




	//*************************************
	// Methods for the LevelOrderList
	//*************************************
	
	public static void SetLevelOrderList(List<int> levelOrderList)
	{
		if (levelOrderList == null)
		{
			Debug.Log("The levelOrderList paramater is null, we do not want a null LevelOrderList");
			return;
		}
		
		string path = persistentDataPath + "LevelOrderList.xml";
		
		if (File.Exists(path))
			File.Delete(path);	
		
		SLevelOrder lo = new SLevelOrder();
		lo.LevelOrderList = levelOrderList;
		
		SerializationHelper<SLevelOrder>.SaveToXML(path, lo);	
	}
	
	
	public static List<int> GetLevelOrderList()
	{
		string path = persistentDataPath + "LevelOrderList.xml";
		
		if (!File.Exists(path))
		{
			SetLevelOrderList(new List<int>());	
		}
		
		SLevelOrder lo = SerializationHelper<SLevelOrder>.LoadFromXML(path); 	
		
		if (lo.LevelOrderList == null)
		{
			SetLevelOrderList(new List<int>());	
		}
		
		return lo.LevelOrderList;
	}
	
	
	public static bool LevelIDExists(int levelID)
	{
		List<int> levelOrderList = GetLevelOrderList();
		
		for (int i = 0; i < levelOrderList.Count; i++)
		{
			if (levelOrderList[i] == levelID)
				return true;
		}
		
		return false;
	}
	
	
	public static void AddLevelIDToLevelOrderList(int levelID)
	{
		List<int> levelOrderList = GetLevelOrderList();
		levelOrderList.Add (levelID);
		SetLevelOrderList(levelOrderList);
	}


	// Add the levelID to the LevelOrderList at a particular index
	public static void AddLevelIDToLevelOrderList(int levelID, int index)
	{
		List<int> levelOrderList = GetLevelOrderList();
		levelOrderList.Insert(index, levelID);
		SetLevelOrderList(levelOrderList);
	}


	
	public static void DeleteLevelIDFromLevelOrderList(int levelID)
	{
		int index = GetLevelIDIndex(levelID);
		
		if (index == -1)	
		{
			Debug.Log("The levelID does not currently exist, so it can not be deleted");
			return;
		}
		
		List<int> levelOrderList = GetLevelOrderList();
		levelOrderList.RemoveAt(index);
		SetLevelOrderList(levelOrderList);
	}
	
	
	public static int GetNewID()
	{
		for (int id = 0; id < CONSTANTS.MAX_LEVELS; id++)
		{
			if (!LevelIDExists(id))
				return id;
		}
		
		return -1;
	}
	
	
	public static int GetLevelIDIndex(int levelID)
	{
		if (!LevelIDExists(levelID))	
		{
			Debug.Log("This levelID does not currently exist");
			return -1;
		}	
		
		List<int> levelOrderList = GetLevelOrderList();
		
		for (int i = 0; i < levelOrderList.Count; i++)
		{
			if (levelOrderList[i] == levelID)
				return i;
		}
		
		return -1;
	}
	
	
	public static void SwapIDUp(int levelID)
	{
		int index = GetLevelIDIndex(levelID);
		
		if (index == -1)	
		{
			Debug.Log("The levelID does not currently exist, so can not swap it up");
			return;
		}
		
		if (index == 0)
		{
			Debug.Log("The levelID is at the top of the list, so can not swap it up");
			return;	
		}
		
		List<int> levelOrderList = GetLevelOrderList();
		
		levelOrderList[index] = levelOrderList[index - 1];
		levelOrderList[index - 1] = levelID;
		
		SetLevelOrderList(levelOrderList);

		// Update the LevelNameList
		string levelName = LevelNameList[index];
		string levelNamePrevious = LevelNameList[index - 1]; 
		LevelNameList[index] = levelNamePrevious;
		LevelNameList[index - 1] = levelName;

		// Update the SaveGameList
		bool saveGame = SaveGameList[index];
		bool saveGamePrevious = SaveGameList[index - 1]; 
		SaveGameList[index] = saveGamePrevious;
		SaveGameList[index - 1] = saveGame;

		// Update the HighScoreIndexList
		int highScoreIndex = CurrentHighScoreIndexList[index];
		int highScoreIndexPrevious = CurrentHighScoreIndexList[index - 1];
		CurrentHighScoreIndexList[index] = highScoreIndexPrevious;
		CurrentHighScoreIndexList[index - 1] = highScoreIndex;
	}

	
	public static void SwapIDDown(int levelID)
	{
		int index = GetLevelIDIndex(levelID);
		
		if (index == -1)	
		{
			Debug.Log("The levelID does not currently exist, so can not swap it down");
			return;
		}
		
		List<int> levelOrderList = GetLevelOrderList();
		
		if (index == levelOrderList.Count - 1)
		{
			Debug.Log("The levelID is at the bottom of the list, so can not swap it down");
			return;	
		}
		
		levelOrderList[index] = levelOrderList[index + 1];
		levelOrderList[index + 1] = levelID;
		
		SetLevelOrderList(levelOrderList);

		// Update the LevelNameList
		string levelName = LevelNameList[index];
		string levelNameNext = LevelNameList[index + 1]; 
		LevelNameList[index] = levelNameNext;
		LevelNameList[index + 1] = levelName;

		// Update the SaveGameList
		bool saveGame = SaveGameList[index];
		bool saveGameNext = SaveGameList[index + 1]; 
		SaveGameList[index] = saveGameNext;
		SaveGameList[index + 1] = saveGame;

		// Update the HighScoreIndexList
		int highScoreIndex = CurrentHighScoreIndexList[index];
		int highScoreIndexNext = CurrentHighScoreIndexList[index + 1];
		CurrentHighScoreIndexList[index] = highScoreIndexNext;
		CurrentHighScoreIndexList[index + 1] = highScoreIndex;
	}
	
	
	public static int NumberLevelsSaved()
	{
		return GetLevelOrderList().Count;	
	}


	public static string GetLevelName(int levelID)
	{
		int index = GetLevelIDIndex(levelID);

		return LevelNameList[index];
	}


	public static bool GetSaveGame(int levelID)
	{
		int index = GetLevelIDIndex(levelID);

		return SaveGameList[index];
	}


	//****************
	// Debug methods
	//****************

	
	public static void DebugLevelOrderList()
	{
		List<int> levelOrderList = GetLevelOrderList();
		
		for (int i = 0; i < levelOrderList.Count; i++)
		{
			Debug.Log(i.ToString() + " " + levelOrderList[i]);	
		}
	}
	
	
	public static void DebugLevelNameList()
	{
		for (int i = 0; i < LevelNameList.Count; i++)
		{
			Debug.Log(i.ToString() + " " + LevelNameList[i]);
		}
	}


	public static void DebugSaveGameList()
	{
		for (int i = 0; i < SaveGameList.Count; i++)
		{
			Debug.Log(i.ToString() + " " + SaveGameList[i]);
		}
	}
	

	
}
