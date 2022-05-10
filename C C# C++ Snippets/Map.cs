/*
 * Map.cs
 * Author(s): Albert Njubi
 * Date Created: 10/4/17
 */
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

/// <summary>
/// This class writes and saves all map text to the path specified
/// </summary>
public class Map : MonoBehaviour {

	#region private functions
	/// <summary>
	/// Calls the method to write all of the text
	/// </summary>
	void Start()
	{
		WriteAllText ();
	}

	/// <summary>
	/// This method writes all of the 2D Array matrix to file
    /// and saves it to a path in documents.
	/// </summary>
	void WriteAllText() 
	{

		// To write array to file
		string str = "";

		//2D Array matrix
		int[,] mazeArray = new int [127, 127]; 
		for (int i = 0; i < mazeArray.GetLength (0); i++) 
		{
			
			for (int j = 0; j < mazeArray.GetLength (1); j++) 
			{
				mazeArray [i, j] = 0 + 1;

				str = str + (i.ToString() + " " + j.ToString() + " " + System.Environment.NewLine + "\n");
				Debug.Log(str);
			}
		}
        #endregion

        string path = "C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat";

		//Write all text into file, but remember: path to file must be
		System.IO.File.WriteAllText("C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat" , str);

		//Read and print all text from file into the debugger
		string readText = File.ReadAllText("C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat");
	}
}
