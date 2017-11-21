using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public bool isStart;
public bool isQuit;

void OnMouseUp(){
	if (isStart)
	{
		Application.LoadLevel(1);
	}

	if (isQuit)

	{

		Application.Quit();

	}


}
