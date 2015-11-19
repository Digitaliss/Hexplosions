using UnityEngine;
using System.Collections;

public class Ally : MonoBehaviour {

	public HexGrid hexGrid;
	public int xCoord;
	public int yCoord;
	public bool edge;
	public int nextXCoord;
	public int nextYCoord;

	private int chosen; // functional
	public bool isSaved;
	public bool hasMoved;
	public bool printIt;

	public void initAlly(int x, int y)
	{
		hexGrid = GameObject.Find ("Grid").GetComponent<HexGrid> ();
		xCoord = x;
		yCoord = y;
		isEdge ();
		getNextXY ();

		isSaved = false;
		hasMoved = false;

	}

	void Update(){
		if (printIt)
			Debug.Log ("Ally: x:" + xCoord + ", y:"+ yCoord+ " Adj: x:" + nextXCoord + ", y:" + nextYCoord + edge); 
	}

	public void isEdge(){
		if (yCoord==0 || yCoord==hexGrid.gridHeightInHexes-1)
			edge=true;
		else
			edge=false;
		}
	public void getNextXY()
	{
		if (!edge)// if this ally is not at the edge of the map
		{
			chosen = Random.Range (1, 4); // Three options (the min is included, the max is not included)
			if (yCoord%2==0) // if this ally is on an even row
			{
				if (chosen==1)
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord-1;
				}
				else if (chosen==2)
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord;
				}
				else
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord+1;
				}
			}
			else // if this ally is on an odd row
			{
				if (chosen==1)
				{
					nextXCoord= xCoord;
					nextYCoord= yCoord-1;
				}
				else if (chosen==2)
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord;
				}
				else
				{
					nextXCoord= xCoord;
					nextYCoord= yCoord+1;
				}
			}
		}

		else // if the ally is at the edge of the map
		{
			chosen = Random.Range (1, 3); // Two options (the min is included, the max is not included)
			if (yCoord==hexGrid.gridHeightInHexes-1)// if the ally is in the last row
			{
				if(yCoord%2!=0)// if the ally is in an odd row
				{
					if (chosen==1)
					{
						nextXCoord= xCoord;
						nextYCoord= yCoord-1;
					}
					else
					{
						nextXCoord= xCoord-1;
						nextYCoord= yCoord;
					}
				}
				else // ally is in an even row (this would only apply if the height changes and may well be useless....
				{
					if (chosen==1)
					{
						nextXCoord= xCoord-1;
						nextYCoord= yCoord-1;
					}
					else
					{
						nextXCoord= xCoord-1;
						nextYCoord= yCoord;
					}
				}
			}
			else // the ally is in the first row
			{
				if (chosen==1)
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord;
				}
				else
				{
					nextXCoord= xCoord-1;
					nextYCoord= yCoord+1;
				}
			}
		}
	}


}
