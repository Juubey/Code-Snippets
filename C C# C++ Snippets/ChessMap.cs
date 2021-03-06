/*
 * ChessMap.cs
 * Author(s): Albert Njubi
 * Date Created: 10/3/17
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class creates and writes the initial map data needed for the MazeCreation.cs.
/// Creates the size of the map and places enemies in random coordinates.
/// </summary>
public class ChessMap : MonoBehaviour
{
    #region public variables
    public int[][] map;
    public static string dataPath;
    #endregion

    #region private variables
    // To write array to file
    string maze = "";
    char[][] mazeArray;
    char[][] chessArray11x11;
    char[][] chessArray5x5;
    char[][] chessArray21x21;
    char[][] enemyArray;
    List<int[]> enemyCoordinates = new List<int[]>();
    int enemySpawnParam;
    #endregion

    // Use this for initialization
    void Start()
    {
        #region Map Gen
        /* Uncomment which CreateChess map we want. This will be done via UI button when integrated into the build.
         * Essentially this sets up the boundaries of the map and also sets enemySpawnParam to the appropriate int.
         * This allows us to soft code the create enemy string method using variables rather than do the math on the fly. */
        CreateChessMap5x5();
        //CreateChessMap11x11();
        //CreateChessMap21x21();


        /* This copies the array boundaries formed above and... actually I think that's all this does so we can get rid
         * of this and consolidate somehow.*/
        CreateEnemyMap();


        /* This is where we spawn in enemies. We set the param for enemy spawn boundries in the relevant CreateChessMap...() method
         * and we use it here to set the parameters for possible enemy spawns. NOTE: the 5x5 gets super crowded as is. Also, we need
         * to spawn the player somehow. Probs just a array[64][64] = '0' (or whatever the player is represented as) in the right place */
        MonsterGen();
        #endregion

        #region Deprecated
        /*Map Gen
        *CreateChessString5x5();
        *CreateChessString11x11();
        *CreateChessString21x21();
        */
        #endregion
        /* The above three methods in the Deprecated region are so because whichever ChessMap method we choose prints out an array into 
         * the variable mazeArray, which gets printed out in this method. */
        CreateEnemyString();


        #region Data Path
        /*Write all text into file, but remember: path to file must be. */
        System.IO.File.WriteAllText(Application.dataPath + "/Map.dat.txt", maze + System.Environment.NewLine);
        Debug.Log(Application.dataPath);
        #endregion
    }



    #region public methods
    /// <summary>
    /// This method creates a 5x5 map with 2 enemies spawned.
    /// </summary>
    public void CreateChessMap5x5()
    {
        chessArray5x5 = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            chessArray5x5[i] = new char[127];
        }


        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                chessArray5x5[i][j] = '.';


                if (i <= 61 || i >= 67 || j <= 61 || j >= 67)
                {
                    chessArray5x5[i][j] = 'A';
                }
            }


        }
        mazeArray = chessArray5x5;
        enemySpawnParam = 2;
    }


    /// <summary>
    /// This method creates a 11x11 map with 5 enemies spawned.
    /// </summary>
    public void CreateChessMap11x11()
    {
        chessArray11x11 = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            chessArray11x11[i] = new char[127];
        }


        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                chessArray11x11[i][j] = '.';


                if (i <= 58 || i >= 70 || j <= 58 || j >= 70)
                {
                    chessArray11x11[i][j] = 'A';
                }
            }


        }
        mazeArray = chessArray11x11;
        enemySpawnParam = 5;
    }


    /// <summary>
    /// This method creates a 21x21 map with 10 enemies spawned.
    /// </summary>
    public void CreateChessMap21x21()
    {
        chessArray21x21 = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            chessArray21x21[i] = new char[127];
        }


        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                chessArray21x21[i][j] = '.';


                if (i <= 53 || i >= 75 || j <= 53 || j >= 75)
                {
                    chessArray21x21[i][j] = 'A';
                }
            }


        }
        mazeArray = chessArray21x21;
        enemySpawnParam = 10;
    }


    /// <summary>
    /// This method creates a map of possible enemy spawns.
    /// </summary>
    public void CreateEnemyMap()
    {
        enemyArray = new char[127][];
        for (int i = 0; i < 127; i++)
        {
            enemyArray[i] = new char[127];
        }


        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                enemyArray[i][j] = mazeArray[i][j];


                if (i <= 53 || i >= 75 || j <= 53 || j >= 75)
                {
                    enemyArray[i][j] = mazeArray[i][j];
                }
            }


        }
    }

    ///<summary>
    /// Formats the 11x11 array in a readable format
    /// </summary>
    public void CreateChessString11x11()
    {
        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                maze += chessArray11x11[i][j];


            }
            maze = maze + '\n';
        }
        maze = maze + '\n';
    }

    ///<summary>
    /// Formats the 5x5 array in a readable format
    /// </summary>
    public void CreateChessString5x5()
    {
        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                maze += chessArray5x5[i][j];


            }
            maze = maze + '\n';
        }
        maze = maze + '\n';
    }

    ///<summary>
    /// Formats the 21x21 array in a readable format
    /// </summary>
    public void CreateChessString21x21()
    {
        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                maze += chessArray21x21[i][j];


            }
            maze = maze + '\n';
        }
        maze = maze + '\n';
    }

    ///<summary>
    /// Formats the enemy array in a readable format
    /// </summary>
    public void CreateEnemyString()
    {


        for (int i = 0; i < 127; i++)
        {
            for (int j = 0; j < 127; j++)
            {
                maze += enemyArray[i][j];
            }
            maze = maze + '\n';
        }
        maze = maze + '\n';
    }

    #endregion

    #region private methods
    ///<summary>
    /// Generates monsters randomly onto an array
    /// </summary>
    void MonsterGen()
    {
        int count = 0;
        int[] currentEnemyCoordinate = new int[2];


        while (true)
        {
            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    if ((i <= 64 + enemySpawnParam && i >= 64 - enemySpawnParam) && j <= 64 + enemySpawnParam && j >= 64 - enemySpawnParam)
                    {
                        bool checkDistance = true;

                        float enemySpawn = UnityEngine.Random.value;


                        if (enemySpawn >= .005)
                        {
                            continue;
                        }

                        foreach (int[] current in enemyCoordinates)
                        {
                            int distanceBetween = distanceTo(i, j, current[0], current[1]);


                            if (distanceBetween < 2)
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


                        if (count == 6)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }

    ///<summary>
    /// Checks the distance of coordinates to one another.
    /// </summary>
    int distanceTo(int y1, int x1, int y2, int x2)
    {
        int distanceX = Mathf.Abs(x1 - x2);
        int distanceY = Mathf.Abs(y1 - y2);


        if (distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);
        return 14 * distanceX + 10 * (distanceY - distanceX);


    }
    #endregion
}



