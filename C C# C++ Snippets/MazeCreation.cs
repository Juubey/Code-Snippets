using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeCreation : MonoBehaviour {

    string maze = "";
    public int MapWidth { get; set; }
    public int MapHeight { get; set; }
    public int PercentAreWalls { get; set; }
    public int[,] Map;
    public static string dataPath;

    //public ScriptName cellScript;
    //CellMapCreation cellScript;

    char[][] mazeArray;
    char[][] enemyArray;
    char[][] textureArray;
    int[][] mat;

    List<int[]> enemyCoordinates = new List<int[]>();






    // Use this for initialization
    void Start()
    {



    }

    public void createMazeFile()
    {
        //cellScript = gameObject.GetComponent<CellMapCreation>();
        mat = new int[127][];
        #region ArrayDeclarations
        mazeArray = new char[127][]; //declared jagged2D array
        for (int i = 0; i < 127; i++)
        {
            mazeArray[i] = new char[127];

        }

        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                mazeArray[i][j] = 'A';
            }
        }

        enemyArray = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            enemyArray[i] = new char[127];

        }

        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                enemyArray[i][j] = '.';
            }
        }

        textureArray = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            textureArray[i] = new char[127];

        }

        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                textureArray[i][j] = '0';
            }
        }
        #endregion

        /**---Maze Gen---**/
        recursiveFuction(10, 10);
        //createFilledCircle (30,30);

        /**---Item Gen---**/
        monsterGen();
        createString();

        /**---Cellular Generation---**/
        //cellScript.MakeCaverns();
        //cellScript.createString();
        MakeCaverns();
        createString();

        /**---Data Path---**/
        //Write all text into file, but remember: path to file must be
        System.IO.File.WriteAllText(Application.dataPath + "/Map.dat.txt", maze + System.Environment.NewLine);
    }


    ///<summary>
    /// A recursive fuction that checks the currect positon of x and y
    /// Picks a random direction and decides if its an empty space
    /// 
    /// </summary>
    void recursiveFuction(int x, int y)
    {
        if (y <= 0 || y >= 126 || x <= 0 || x >= 126)
        {
            return;
        }
        if (mazeArray[y][x] == '.')
        {
            return;
        }

        if (mazeArray[y + 1][x + 1] == '.' && mazeArray[y + 1][x] == '.' && mazeArray[y][x + 1] == '.')
        {
            return;
        }
        if (mazeArray[y + 1][x - 1] == '.' && mazeArray[y + 1][x] == '.' && mazeArray[y][x - 1] == '.')
        {
            return;
        }
        if (mazeArray[y - 1][x - 1] == '.' && mazeArray[y - 1][x] == '.' && mazeArray[y][x + 1] == '.')
        {
            return;
        }
        if (mazeArray[y - 1][x - 1] == '.' && mazeArray[y][x - 1] == '.' && mazeArray[y - 1][x] == '.')
        {
            return;
        }

        mazeArray[y][x] = '.';

        float directionPicker;
        directionPicker = UnityEngine.Random.value;

        if (directionPicker == 0)
        {
            directionPicker += .1f;
        }

        for (int i = 0; i < 4; i++)
        {
            if (directionPicker <= .25)
            {
                recursiveFuction(x, y - 1);
                directionPicker += .25f;
            }
            else if (directionPicker <= .5)
            {
                recursiveFuction(x, y + 1);
                directionPicker += .25f;
            }
            else if (directionPicker <= .75)
            {
                recursiveFuction(x - 1, y);
                directionPicker += .25f;
            }
            else
            {
                recursiveFuction(x + 1, y);
                directionPicker -= .75f;
            }
        }
        return;
    }

    ///<summary>
    /// Creates randomly placed circles on a maze
    /// 
    /// </summary>
    void createFilledCircle(int x, int y)
    {
        for (int a = 0; a < 3; a++)
        {

            if (y <= 20 || y >= 126 || x <= 20 || x >= 126)
            {
                return;
            }

            int start_X;
            int start_Y;
            int r;

            float randomRadius = UnityEngine.Random.Range(5.0f, 20.0f);
            float randomY = UnityEngine.Random.Range(10.0f, 100.0f);
            float randomX = UnityEngine.Random.Range(10.0f, 100.0f);

            start_X = (int)randomX;
            start_Y = (int)randomY;
            r = (int)randomRadius;


            for (int i = start_X - r; i < start_X + r; i++)
            {
                for (int j = start_Y - r; j < start_Y + r; j++)
                {
                    if ((i - start_X) * (i - start_X) + (j - start_Y) * (j - start_Y) <= r * r)
                    {
                        mazeArray[i][j] = '.';
                    }
                }
            }
        }
        return;
    }

    ///<summary>
    /// Generates areas of Items in a spiral
    /// 
    /// </summary>
    void monsterGen()
    {
        int count = 0;
        bool firstPass = false;
        int[] currentEnemyCoordinate = new int[2];
        while (true)
        {

            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    bool checkDistance = true;
                    if (mazeArray[i][j] == 'A')
                        continue;

                    float enemySpawn = UnityEngine.Random.value;

                    if (enemySpawn >= .005)
                    {
                        continue;
                    }
                    /*if(firstPass == false)
                    {
                        mazeArray[i][j] = '1';
                        currentEnemyCoordinate[0] = i;
                        currentEnemyCoordinate[1] = j;
                        enemyCoordinates.Add(currentEnemyCoordinate);
                        firstPass = true;
                        continue;
                    }*/

                    foreach (int[] current in enemyCoordinates)
                    {
                        int distanceBetween = distanceTo(i, j, current[0], current[1]);

                        if (distanceBetween < 50)
                        {
                            checkDistance = false;
                            break;
                        }
                    }
                    if (checkDistance == false)
                    {
                        Debug.Log("Distance triggered");
                        continue;
                    }
                    enemyArray[i][j] = '1';
                    currentEnemyCoordinate[0] = i;
                    currentEnemyCoordinate[1] = j;
                    enemyCoordinates.Add(currentEnemyCoordinate);
                    count++;
                    if (count == 30)
                    {
                        return;
                    }
                }
            }
        }
    }

    int distanceTo(int y1, int x1, int y2, int x2)
    {
        int distanceX = Mathf.Abs(x1 - x2);
        int distanceY = Mathf.Abs(y1 - y2);

        if (distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);
        return 14 * distanceX + 10 * (distanceY - distanceX);

    }



    ///<summary>
    /// Formats the array in a readable format
    /// 
    /// </summary>
    /**public void createString()
	{
		for(int i = 0; i < 127; i++)
		{
			for (int j = 0; j < 127; j++) 
			{
				maze += mazeArray [i] [j];

			}
			maze = maze + '\n';
		}
       
    }**/

    public void createString()
    {
        mazeArray[64][64] = '.';
        enemyArray[64][64] = '0';
        for (int l = 0; l < 3; l++)
        {
            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    switch (l)
                    {
                        case 2:
                            maze += textureArray[i][j];
                            break;
                        case 1:
                            maze += enemyArray[i][j];
                            break;
                        case 0:
                            maze += mazeArray[i][j];
                            break;
                    }
                }
                maze = maze + '\n';
            }
            maze = maze + '\n';
        }
    }

    ///<summary>
    /// Makes a Cellular Cavern Map
    /// 
    /// </summary>
    public void MakeCaverns()
    {

        MapWidth = 127;
        MapHeight = 127;
        PercentAreWalls = 40;

        // New, empty map
        Map = new int[MapWidth, MapHeight];

        int mapMiddle = 0; // Temp variable
        for (int column = 0, row = 0; row < MapHeight; row++)
        {
            for (column = 0; column < MapWidth; column++)
            {
                // If coordinants lie on the the edge of the map (creates a border)
                if (column == '.')
                {
                    Map[column, row] = 'A';
                }
                else if (row == '.')
                {
                    Map[column, row] = 'A';
                }
                else if (column == MapWidth - 1)
                {
                    Map[column, row] = 'A';
                }
                else if (row == MapHeight - 1)
                {
                    Map[column, row] = 'A';
                }
                // Else, fill with a wall a random percent of the time
                else
                {
                    mapMiddle = (MapHeight / 2);

                    if (row == mapMiddle)
                    {
                        Map[column, row] = '.';
                    }
                    else
                    {
                        Map[column, row] = RandomPercent(PercentAreWalls);
                    }
                }
            }
        }

        // By initilizing column in the outter loop, its only created ONCE
        for (int column = 0, row = 0; row <= MapHeight - 1; row++)
        {
            for (column = 0; column <= MapWidth - 1; column++)
            {
                Map[column, row] = PlaceWallLogic(column, row);
            }
        }
    }

    public int PlaceWallLogic(int x, int y)
    {
        int numWalls = GetAdjacentWalls(x, y, 1, 1);


        if (Map[x, y] == 'A')
        {
            if (numWalls >= 4)
            {
                return 1;
            }
            if (numWalls < 2)
            {
                return 0;
            }

        }
        else
        {
            if (numWalls >= 5)
            {
                return 1;
            }
        }
        return 0;
    }
    public int GetAdjacentWalls(int x, int y, int scopeX, int scopeY)
    {
        int startX = x - scopeX;
        int startY = y - scopeY;
        int endX = x + scopeX;
        int endY = y + scopeY;

        int iX = startX;
        int iY = startY;

        int wallCounter = 0;

        for (iY = startY; iY <= endY; iY++)
        {
            for (iX = startX; iX <= endX; iX++)
            {
                if (!(iX == x && iY == y))
                {
                    if (IsWall(iX, iY))
                    {
                        wallCounter += 1;
                    }
                }
            }
        }
        return wallCounter;
    }

    bool IsWall(int x, int y)
    {
        // Consider out-of-bound a wall
        if (IsOutOfBounds(x, y))
        {
            return true;
        }

        if (Map[x, y] == 'A')
        {
            return true;
        }

        if (Map[x, y] == '.')
        {
            return false;
        }
        return false;
    }

    bool IsOutOfBounds(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return true;
        }
        else if (x > MapWidth - 1 || y > MapHeight - 1)
        {
            return true;
        }
        return false;
    }

    int RandomPercent(int percent)
    {
        if (percent >= UnityEngine.Random.Range(1, 101))
        {
            return 1;
        }
        return 0;
    }


    // Update is called once per frame
    void Update()
    {

    }
}

