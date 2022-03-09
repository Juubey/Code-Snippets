using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class Map : MonoBehaviour {

	//public GameObject mazeCells;

	void Start()
	{
		Debug.Log("Runs");
		WriteAllText ();


	}

	void WriteAllText() 
	{
		//Opens the text Document
		//System.IO.File.Open(@"C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat");

		// To write array to file
		string str = "";

		//int [][] mazeArray = new int[127][]; jagged array

		int [,] mazeArray = new int [127, 127]; //2D Array matrix
		for (int i = 0; i < mazeArray.GetLength (0); i++) 
		{
			
			for (int j = 0; j < mazeArray.GetLength (1); j++) 
			{
				mazeArray [i, j] = 0 + 1;

				str = str + (i.ToString() + " " + j.ToString() + " " + System.Environment.NewLine + "\n");
				Debug.Log(str);
			}
		}

		Debug.Log(mazeArray);

		/*for(int i = 0; i <= 127; i++) 
		{

			for(int j = 0; j <= 127; j++) 
			{

				str = str + (i.ToString() + " " + j.ToString() + " " + System.Environment.NewLine + "\n");
				Debug.Log(str);
			}
		}*/

		string path = "C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat";
		Debug.Log (path);

		//Write all text into file, but remember: path to file must be
		System.IO.File.WriteAllText("C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat" , str);
		//File.ReadAllText(

		//Read and print all text from file into the debugger
		string readText = File.ReadAllText("C:/Users/" + System.Environment.UserName + "/Documents/PerfectMazeGenerator/Assets/Scripts/Map.dat");
		Debug.Log(readText);
	}
}
