using UnityEngine;
using System.Collections;

public static class GAMEPLAY 
{
	public static bool GameStarted;
	public static bool CountDownInitiated;
	public static bool InPause;
	
	public static int Score;
	public static int Cubes;
	public static int Lines;
	public static int Turns;
	
	public static float TimeBetweenTurns;

	public static CountDownScript CountDownScript_Intro;

	private static MovePanelScript MovePanelScript_Level;
	private static GameObject PauseButton;


	public static void Initialize() 
	{
		MovePanelScript_Level = GameObject.Find("P_LEVEL").GetComponent<MovePanelScript>();
		
		CountDownScript_Intro = GameObject.Find("P_CD").GetComponent<CountDownScript>();

		if (CONSTANTS.VERSION == EVersion.STEAM)
			PauseButton = (GameObject)GameObject.Find("P_GP_B_Pause");
		else
			PauseButton = (GameObject)GameObject.Find("P_GPL_B_Pause");

	}
	
	
	public static void Update()
	{
		if (!GameStarted)
			return;
		
		if (MovePanelScript_Level.PanelInMotion())
			return;
		
		if (!CountDownInitiated)
		{
			CountDownInitiated = true;
			CountDownScript_Intro.LaunchCountDown();
		}
		
		if (CountDownScript_Intro.CountDownOccuring)
			return;
		else
			PauseButton.GetComponent<BoxCollider>().enabled = true;
		
		if (!InPause)
			UpdateGamePlay();
	}


	public static void StartNewGame()
	{
		SLevelData levelData = DATA.GetLevel(PANEL_LD.CurrentlySelectedLevelID());
		
		GameStarted = true;
		CountDownInitiated = false;
		InPause = false;
		
		Score = 0;
		Cubes = 0;
		Lines = 0;
		Turns = 0;
		
		SCORE.UpdateScoreLabel();
		SCORE.UpdateCubesLabel();
		SCORE.UpdateLinesLabel();
		SCORE.UpdateTurnsLabel();
		
		TimeBetweenTurns = CONSTANTS.START_TIME_BETWEEN_TURNS;
		
		LEVEL.Initialize(levelData);

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE || CONSTANTS.VERSION == EVersion.IPAD_PAID)
			TILES.SetTiles(LEVEL.StatusGrid, LEVEL.PixelSize, false);
		else if (CONSTANTS.VERSION == EVersion.STEAM)
			TILES.SetTiles(LEVEL.StatusGrid, LEVEL.PixelSize, true);

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE || CONSTANTS.VERSION == EVersion.IPAD_PAID)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_EMPTY_CENTER);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_GPL);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_GPR);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_LEVEL);
		}
		else if (CONSTANTS.VERSION == EVersion.STEAM)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_EMPTY_CENTER);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_GP);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_LEVEL);
		}

		BLOCK.InitializeNewGame();
		GENERATOR.InitializeNewGame();
		SNAP.InitializeNewGame();
		GRAVITY.InitializeNewGame();
		GRAVITYLINE.InitializeNewGame();
		RANDOM.InitializeNewGame();
		HOLD.InitializeNewGame();
		
		NEXT.SetNextTiles(RANDOM.NextBlock, RANDOM.NextGeneratorIndex);
		
		PauseButton.GetComponent<BoxCollider>().enabled = false;   
		
		AUDIO.SetPMVolumeFromLBVolume();

		// Since a new game is being started we want to remove information from a previously saved game.
		DATA.RemoveSavedGame(PANEL_LD.CurrentlySelectedLevelID());
	}


	public static void StartSavedGame()
	{
		SLevelData levelData = DATA.GetLevel(PANEL_LD.CurrentlySelectedLevelID());
		
		GameStarted = true;
		CountDownInitiated = false;
		InPause = false;
		
		Score = levelData.SaveGameData.Score;
		Cubes = levelData.SaveGameData.Cubes;
		Lines = levelData.SaveGameData.Lines;
		Turns = levelData.SaveGameData.Turns;
		
		SCORE.UpdateScoreLabel();
		SCORE.UpdateCubesLabel();
		SCORE.UpdateLinesLabel();
		SCORE.UpdateTurnsLabel();
		
		TimeBetweenTurns = levelData.SaveGameData.TimeBewteenTurns;
		
		LEVEL.Initialize(levelData);

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE || CONSTANTS.VERSION == EVersion.IPAD_PAID)
			TILES.SetTiles(LEVEL.StatusGrid, levelData.SaveGameData.TileColors, LEVEL.PixelSize, false);
		else if (CONSTANTS.VERSION == EVersion.STEAM)
			TILES.SetTiles(LEVEL.StatusGrid, levelData.SaveGameData.TileColors, LEVEL.PixelSize, true);

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE || CONSTANTS.VERSION == EVersion.IPAD_PAID)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_EMPTY_CENTER);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_GPL);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_GPR);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_LEVEL);
		}
		else if (CONSTANTS.VERSION == EVersion.STEAM)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_EMPTY_CENTER);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_GP);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_LEVEL);
		}

		BLOCK.InitializeNewGame();
		GENERATOR.InitializeNewGame();
		SNAP.InitializeNewGame();
		GRAVITY.InitializeNewGame();
		GRAVITYLINE.InitializeNewGame();
		RANDOM.InitializeSaveGame(levelData.SaveGameData);
		HOLD.InitializeSaveGame(levelData.SaveGameData);
		
		NEXT.SetNextTiles(RANDOM.NextBlock, RANDOM.NextGeneratorIndex);
		
		PauseButton.GetComponent<BoxCollider>().enabled = false;
		
		AUDIO.SetPMVolumeFromLBVolume();
	}
	
	
	public static void EndGameFromPauseMenu()
	{
		GameStarted = false;
		
		GENERATOR.HideGenerator();
		BLOCK.HideBlock();
		GRAVITYLINE.HideGravityLine();

		PANEL_MANAGER.HideCurrentPopUpPanel();

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LBFREE);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_EMPTY_RIGHT);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
		else if (CONSTANTS.VERSION == EVersion.IPAD_PAID)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LB);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_EMPTY_RIGHT);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
		else if (CONSTANTS.VERSION == EVersion.STEAM)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LB);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
	}


	public static void EndGameFromGameOverMenu()
	{
		GameStarted = false;

		PANEL_MANAGER.HideCurrentPopUpPanel();

		if (CONSTANTS.VERSION == EVersion.IPAD_FREE)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LBFREE);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_EMPTY_RIGHT);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
		else if (CONSTANTS.VERSION == EVersion.IPAD_PAID)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LB);
			PANEL_MANAGER.ChangeRightPanel(PANEL_MANAGER.P_EMPTY_RIGHT);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
		else if (CONSTANTS.VERSION == EVersion.STEAM)
		{
			PANEL_MANAGER.ChangeCenterPanel(PANEL_MANAGER.P_LD);
			PANEL_MANAGER.ChangeLeftPanel(PANEL_MANAGER.P_LB);
			PANEL_MANAGER.ChangeTopPanel(PANEL_MANAGER.P_EMPTY_TOP);
		}
	}

	
	public static void UpdateGamePlay()
	{
		if (!GRAVITY.CurrentlyProcessingGravity && 
		    GENERATOR.GeneratorState == EGeneratorState.OffScreen && 
		    BLOCK.BlockState == EBlockState.NotInPlay)
		{
			StartNewTurn();
		}
		
		BLOCK.Update();
		GENERATOR.Update();
		SNAP.Update();
		GRAVITY.Update();
		GRAVITYLINE.Update();
		HOLD.Update();
	}
	
	
	public static void StartNewTurn()
	{
		RANDOM.NewRandomGeneratorAndBlock();
		GENERATOR.InitializeGeneratorEntrance();
		GRAVITYLINE.ActivateNewGravityLine(BLOCK.CurrentBlock.Color);
		
		if (HOLD.BlockHeldLastTurn)
			HOLD.BlockHeldLastTurn = false;
		else
			HOLD.BlockMayBeHeld = true;

		UpdateTimeBetweenTurns();
	}


	public static void UpdateTimeBetweenTurns()
	{
		// Every cube popped decreases the time between turns by 0.001 seconds, to a minimum of 0.1 seconds.
		TimeBetweenTurns = Mathf.Max(0.1f, CONSTANTS.START_TIME_BETWEEN_TURNS - 0.001f * Cubes);
	}

	
	public static bool TestIfGameIsOver(Block block)
	{
		return GENERATOR.CurrentGenerator.BlockOnGenerator(block) && GENERATOR.GeneratorState == EGeneratorState.Placed;
	}


	public static void GameOver()
	{
		AUDIO.Play_GameOverSound();
		TILES.PopOffAllColoredTiles();
		BLOCK.PopCurrentBlock();
		GENERATOR.PopCurrentGenerator();
		GRAVITYLINE.GravityLineActive = false;

		PANEL_MANAGER.ShowPopUpPanel(PANEL_MANAGER.P_PUGO);

		GameStarted = false;

		PANEL_PUHS.PopUpFromLevelPreview = false;

		// Update the level's high score table with the new score and set the high score index on the game over pop up.
		DATA.EndGameSave(PANEL_LD.CurrentlySelectedLevelID(), Score, Cubes, Lines, Turns);
	}





}
