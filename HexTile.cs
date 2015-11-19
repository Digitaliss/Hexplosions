using UnityEngine;
using System.Collections;

public class HexTile : MonoBehaviour {

	public HexGrid hexGrid;
	private bool isSelect; // is this tile Selected
	private bool isNewHazard; // is this tile a newly spawned Hazard 
	private bool isArmedHazard; // this is an armed Hazard
	private bool isKIAHazard; // this is a hazard that killed an ally
	private bool isExploded; // has the explosion effect played
	private bool isZone; // is this tile a Zone of Attraction
	private int hasXAllies; // this tile has X many Allies on it
	private int xCoord;
	private int yCoord;


	public HexTile(){
		hexGrid = GameObject.Find ("Grid").GetComponent<HexGrid> ();
		isSelect = false;
		isNewHazard = false;
		isArmedHazard = false;
		isZone = false;
		hasXAllies = 0;	
		xCoord = 0;
		yCoord = 0;
	}

	public bool getIsSelect()
	{
		return isSelect;
	}

	public bool getIsNewHazard()
	{
		return isNewHazard;
	}

	public bool getIsKIAHazard()
	{
		return isKIAHazard;
	}

	public bool getIsArmedHazard()
	{
		return isArmedHazard;
	}

	public bool getIsExploded(){
		return isExploded;
	}

	public bool getIsZone()
	{ 
		return isZone;
	}

	public int getHasXAllies()
	{
		if (hasXAllies < 0)
			return 0;
		else
			return hasXAllies;
	}

	public int getXCoord()
	{
		return xCoord;
	}

	public int getYCoord()
	{
		return yCoord;
	}

	public void setIsSelect(bool state)
	{
		isSelect=state;
	}
	
	public void setIsNewHazard(bool state)
	{
		isNewHazard=state;
	}

	public void setIsArmedHazard(bool state)
	{
		isArmedHazard=state;
	}

	public void setIsExploded(bool state)
	{
		isExploded = state;
	}

	public void setIsKIAHazard(bool state)
	{
		isKIAHazard=state;
	}

	public void setIsZone(bool state)
	{
		isZone=state;
	}
	
	public void setHasXAllies(int numA)
	{
		if (hasXAllies >= 0)
			hasXAllies = numA;
		else
			Debug.Log ("Less than 0 allies...");
	}

	public void addAllies()
	{
		if (hasXAllies >= 0)
			hasXAllies ++;
		else
			Debug.Log ("Less than 0 allies...");
	}

	public void removeAllies()
	{
		if (hasXAllies >= 0)
			hasXAllies --;
		else
			Debug.Log ("Less than 0 allies...");
	}

	public void setXCoord(int x)
	{
		xCoord=x;
	}
	
	public void setYCoord(int y)
	{
		yCoord=y;
	}

}