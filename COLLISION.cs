using UnityEngine;
using System.Collections;


// This class handles all collision detection.
// The two types of collision are with geometry and with gravity.
public static class COLLISION 
{
	// Checks to see if the game square is on the game grid.
    public static bool CheckValidSquare(Square square)
    {
        return (square.Row >= 0) && (square.Row < LEVEL.NumberRows)
               && (square.Col >= 0) && (square.Col < LEVEL.NumberCols);
    }


    // Check to see if a game grid square is valid and playable (i.e. open or a square of the generator).
    private static bool CheckPlayableSquare(Square square)
    {
        if (!CheckValidSquare(square))
            return false;
        else
		{
			return ((LEVEL.StatusGrid[square.Row, square.Col] == ESquareStatus.Open) && !TILES.TileOccupied(square.Row, square.Col))||
				   (GENERATOR.CurrentGenerator.SquareOnGenerator(square) && GENERATOR.GeneratorState == EGeneratorState.Placed);
		}
    }
	
	
	// This method checks if a game piece at a given position colides with any of the closed squares of the level.
	// A return value of true means there is a collision, and false means no collision.
	public static bool CheckCurrentLevelCollision(int[,] gamePieceLayout, Square gamePiecePosition)
	{
		Square testSquare = new Square();
		
		for (int row = 0; row < 4; row++)
			for (int col = 0; col < 4; col++)
			{
				if (gamePieceLayout[row, col] == 1)
				{
					testSquare.Row = gamePiecePosition.Row + row;
					testSquare.Col = gamePiecePosition.Col + col;
					
					if (!CheckPlayableSquare(testSquare))
						return true;
				}
			}
		
		return false;
	}
	
	
	// This method checks if a game piece at a given position when moved in a given 
	// direction will collide with any of the closed squares of the level or go outside the level bounds.
	// A return value of true means there is a collision, and false means no collision.
	public static bool CheckImminentLevelCollisionByTranslation(Block block, EDirection direction)
	{
		Square translatedPosition = block.Position.ReturnAdjacentSquare(direction); 
		return CheckCurrentLevelCollision(block.Layout, translatedPosition);
	}
	
	
	// This method checks if a game piece at a given position when rotated in a given 
	// direction will collide with any of the closed squares of the level or go outside the level bounds.
	// A return value of true means there is a collision, and false means no collision.
	public static bool CheckImminentLevelCollisionByRotation(Block block, ERotation rotation)
	{
		block.Rotate(rotation);
		return CheckCurrentLevelCollision(block.Layout, block.Position);
	}
	
	
	// This method checks if a block at a given position colides with the current gravity wall.
	// A return value of true means there is a collision, and false means no collision.
	private static bool CheckCurrentCollisionDeadZone(Block block)
	{
		Zone zone = BLOCK.CurrentBlock.DetermineCurrentDeadZone(GENERATOR.CurrentGenerator.GravityDirection);  
		return zone.BlockIntersectsZone(block); 
	}
		
	
	// This method checks if a game piece at a given position when tranlated in a given direction will collide with the gravity wall.
	// A return value of true means there is a collision, and false means no collision.
	private static bool CheckImminentDeadZoneCollisionByTranslation(Block block, EDirection direction)
	{
		// If you try to move the opposite direction of gravity you will be coliding with gravity.
		if (HELPER.OppositeDirection(direction) == GENERATOR.CurrentGenerator.GravityDirection)
			return true;	
		else
			return false;	
	}
	
	
	// This method checks if a game piece at a given position when rotated in a given direction will collide with the gravity wall.
	// A return value of true means there is a collision, and false means no collision.
	private static bool CheckImminentDeadZoneCollisionByRotation(Block block, ERotation rotation)
	{
		block.Rotate(rotation);
		return CheckCurrentCollisionDeadZone(block);
	}
	
	
	// This method checks if a game piece at a given position when moved in a given direction will have any collisions of any type.
	// A return value of true means there is a collision, and false means no collision.
	public static bool CheckImminentCollisionByTranslation(Block block, EDirection direction)
	{
		return CheckImminentLevelCollisionByTranslation(block, direction) ||
			   CheckImminentDeadZoneCollisionByTranslation(block, direction);
	}
	
	
	// This method checks if a game piece at a given position when rotated in a given direction will have any collisions of any type.
	// A return value of true means there is a collision, and false means no collision.
	public static bool CheckImminentCollisionByRotation(Block block, ERotation rotation)
	{
		return CheckImminentLevelCollisionByRotation(block, rotation) ||
			   CheckImminentDeadZoneCollisionByRotation(block, rotation);
			
	}
	
	
	
}



