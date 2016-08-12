using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TILES
{
	public static ESquareStatus[,] StatusGrid;
	public static Color[] TileColors;

	public static List<GameObject> LevelTilesList;	
	public static List<CubeSelectScript> CubeSelectScriptList;


	public static void Initialize()
	{
		LevelTilesList = new List<GameObject>();
		CubeSelectScriptList = new List<CubeSelectScript>();

		GameObject tilePrefab =  GameObject.Find("GameManager").GetComponent<GameManagerScript>().TilePrefab;

		int maxNumberRows = (int)(CONSTANTS.MAX_LEVEL_HEIGHT / CONSTANTS.MIN_SQUARE_PIXEL_SIZE);
		int maxNumberColumns = (int)(CONSTANTS.MAX_LEVEL_WIDTH / CONSTANTS.MIN_SQUARE_PIXEL_SIZE);

		// Instantiate all tiles at the start of the game
		for (int i = 0; i < maxNumberRows * maxNumberColumns; i++)
		{
			GameObject newTile = (GameObject)GameObject.Instantiate(tilePrefab);
			newTile.renderer.enabled = false;
			newTile.transform.parent = TRANSFORM.TilesTransform;
			LevelTilesList.Add(newTile);
			CubeSelectScriptList.Add (newTile.GetComponent<CubeSelectScript>());
		}

		ResetStatusGrid();
	}


	// Defines TileColor list based on the current StatusGrid
	public static void SetOpenClosedTileColors()
	{
		TileColors = new Color[NumberRows() * NumberCols()];

		for (int row = 0; row < NumberRows(); row++)
			for (int col = 0; col < NumberCols(); col++)
			{
				int index = col + row * NumberCols();

				if (StatusGrid[row, col] == ESquareStatus.Open)
					TileColors[index] = COLOR.OPEN_COLOR;
				else
					TileColors[index] = COLOR.CLOSED_COLOR;
			}
	}


	public static void ResetStatusGrid()
	{
		StatusGrid = new ESquareStatus[CONSTANTS.DEFAULT_NUMBER_ROWS, CONSTANTS.DEFAULT_NUMBER_COLS];
		
		// Set all squares to have an open status
		for (int row = 0; row < CONSTANTS.DEFAULT_NUMBER_ROWS; row++)
			for (int col = 0; col < CONSTANTS.DEFAULT_NUMBER_COLS; col++)
			{
				StatusGrid[row, col] = ESquareStatus.Open;
			}
	}



	public static void SetAllTilesJustChangedStatusToFalse()
	{
		for (int i = 0; i < NumberRows() * NumberCols(); i++)
		{
			CubeSelectScriptList[i].JustChangedStatus = false;
		}
	}




	public static void SetTiles(ESquareStatus[,] statusGrid, Color[] tileColors, int pixelSize, bool useOffset)
	{
		StatusGrid = statusGrid;
		TileColors = tileColors;

		if (NumberRows() * NumberCols() != TileColors.Length)
		{
			Debug.Log("The tileColors array does not have the right number of elements");
			return;
		}

		for (int row = 0; row < NumberRows(); row++)
		for (int col = 0; col < NumberCols(); col++)
		{
			int index = NumberCols() * row + col;	
			LevelTilesList[index].renderer.enabled = true;
			LevelTilesList[index].name = "Tile_" + row.ToString() + "_" + col.ToString();
			LevelTilesList[index].GetComponent<CubeSelectScript>().SetRowCol(row, col);
			LevelTilesList[index].transform.localScale = Vector3.one * pixelSize;
			LevelTilesList[index].transform.position = HELPER.ConvertToWorldCoordinates(row, 
		                                                                                col, 
		                                                                                NumberRows(), 
		                                                                                NumberCols(), 
		                                                                                pixelSize, 
		                                                                                ELayer.TileLayer,
		                                                                                useOffset)
													   + TRANSFORM.TilesTransform.position;
		   
			LevelTilesList[index].GetComponent<SetColorScript>().SetColor(TileColors[index]);
		}

		for (int i = NumberRows() * NumberCols(); i <  LevelTilesList.Count; i++)
		{
			LevelTilesList[i].renderer.enabled = false;
			LevelTilesList[i].name = "NotUsed";
		}
	}


	// We use this overload when the TileColors are just the open and closed colors.
	public static void SetTiles(ESquareStatus[,] statusGrid, int pixelSize, bool useOffset)
	{
		StatusGrid = statusGrid;
		SetOpenClosedTileColors();
		SetTiles(statusGrid, TileColors, pixelSize, useOffset);
	}


	public static void SetTiles(int pixelSize)
	{
		SetOpenClosedTileColors();
		SetTiles(StatusGrid, TileColors, pixelSize, true);
	}


	public static int NumberRows()
	{
		return StatusGrid.GetLength(0);
	}
	
	
	public static int NumberCols()
	{
		return StatusGrid.GetLength(1);
	}


	public static void SetTileColor(int row, int col, Color color)
	{
		int index = col + row * NumberCols();

		TileColors[index] = color;
		LevelTilesList[index].GetComponent<SetColorScript>().SetColor(TileColors[index]);
	}


	public static Color GetTileColor(int row, int col)
	{
		int index = col + row * NumberCols();

		return TileColors[index];
	}


	public static bool TileOccupied(int row, int col)
	{
		int index = col + row * NumberCols();

		return TileColors[index] != COLOR.OPEN_COLOR && TileColors[index] != COLOR.CLOSED_COLOR;
	}

	
	public static void WriteBlockToTiles(Block block)
	{
		for (int row = 0; row < 4; row++)
			for (int col = 0; col < 4; col++)
			{
				if (block.Layout[row, col] == 1)
				{
					SetTileColor(block.Position.Row + row, block.Position.Col + col, block.Color);
				}
			}
	}


	// Method used when the game ends.
	public static void PopOffAllColoredTiles()
	{
		List<Square> positionsList = new List<Square>();
		List<Color> colorsList = new List<Color>();
		List<EDirection> directionsList = new List<EDirection>();


		for (int row = 0; row < NumberRows(); row++)
		for (int col = 0; col < NumberCols(); col++)
		{
			Color tileColor = GetTileColor(row, col);

			if (tileColor != COLOR.OPEN_COLOR && tileColor != COLOR.CLOSED_COLOR)
			{
				positionsList.Add (new Square(row, col));
				colorsList.Add(tileColor);
				directionsList.Add((EDirection)Random.Range(0, 4));		// The direction of the pop is random.
				SetTileColor(row, col, COLOR.OPEN_COLOR);			    // Reset the tile to being empty.
			}
		}

		POP.ActivatePop(positionsList, colorsList, directionsList);
	}


	public static void AddTopRow()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows() + 1, NumberCols()];
		
		for (int row = 0; row < NumberRows() + 1; row++)
			for (int col = 0; col < NumberCols(); col++)
			{
				if (row == 0)
					newStatusGrid[row, col] = ESquareStatus.Open;
				else
					newStatusGrid[row, col] = StatusGrid[row - 1, col];   
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void RemoveTopRow()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows() - 1, NumberCols()];
		
		for (int row = 0; row < NumberRows() - 1; row++)
			for (int col = 0; col < NumberCols(); col++)
			{
				newStatusGrid[row, col] = StatusGrid[row + 1, col];
			}
			
		StatusGrid = newStatusGrid;
	}
	
	
	public static void AddBottomRow()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows() + 1, NumberCols()];
		
		for (int row = 0; row < NumberRows() + 1; row++)
			for (int col = 0; col < NumberCols(); col++)
			{
				if (row == NumberRows())
					newStatusGrid[row, col] = ESquareStatus.Open;
				else
					newStatusGrid[row, col] = StatusGrid[row, col];  
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void RemoveBottomRow()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows() - 1, NumberCols()];
		
		for (int row = 0; row < NumberRows() - 1; row++)
			for (int col = 0; col < NumberCols(); col++)
			{
				newStatusGrid[row, col] = StatusGrid[row, col];
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void AddRightCol()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows(), NumberCols() + 1];
		
		for (int row = 0; row < NumberRows(); row++)
			for (int col = 0; col < NumberCols() + 1; col++)
			{
				if (col == NumberCols())
					newStatusGrid[row, col] = ESquareStatus.Open;   
				else
					newStatusGrid[row, col] = StatusGrid[row, col];    
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void RemoveRightCol()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows(), NumberCols() - 1];
		
		for (int row = 0; row < NumberRows(); row++)
			for (int col = 0; col < NumberCols() - 1; col++)
			{
				newStatusGrid[row, col] = StatusGrid[row, col];
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void AddLeftCol()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows(), NumberCols() + 1];
		
		for (int row = 0; row < NumberRows(); row++)
			for (int col = 0; col < NumberCols() + 1; col++)
			{
				if (col == 0)
					newStatusGrid[row, col] = ESquareStatus.Open;   
				else
					newStatusGrid[row, col] = StatusGrid[row, col - 1];  
			}
		
		StatusGrid = newStatusGrid;
	}
	
	
	public static void RemoveLeftCol()
	{
		ESquareStatus[,] newStatusGrid = new ESquareStatus[NumberRows(), NumberCols() - 1];
		
		for (int row = 0; row < NumberRows(); row++)
			for (int col = 0; col < NumberCols() - 1; col++)
			{
				newStatusGrid[row, col] = StatusGrid[row, col + 1];
			}
		
		StatusGrid = newStatusGrid;
	}





}
