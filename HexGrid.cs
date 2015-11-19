using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HexGrid : MonoBehaviour {

///////////////////FIELDS///////////////////////////////////////////////////////////////////////////////////

	// following public variable is used to store the hex model prefab;
	// instantiate it by dragging the prefab on this variable using unity editor
	public GameObject hex; // regular hex
	public GameObject zone; // zone hex
	public GameObject newHazard; // spawned hazard 
	public GameObject armedHazard; // armed hazard
	public GameObject kIAHazard; // hazard that killed an ally
	public GameObject ally; // an ally
	public GameObject player; // player's selection
	public HexTile hexTile; // an oject of the HexTile class
	public GameObject allyExplosion; //////////////////////// MAYBE PROBLEM

	// audio
	public AudioClip selection;
	public AudioClip selMove;
	public AudioClip explosion;
	public AudioClip saveAlly;

	// Text elements
	public Text restartText;
	public Text creditText;
	public Text gameOverText;
	public Text numLivesText;
	public Text scoreText;
	//private Text alliesText;

	// These can be changed in the inspector
	public int gridWidthInHexes = 10; // keep same as gridHeightInHexes
	public int gridHeightInHexes = 10; // keep same as gridWidthInHexes
	public int numZones = 10; // number of zones (zones spawn as one per hex)
	public int numHazards = 5; // number of hazards per turn (hazards spawn on different hexes, but can pile up)
	public int numAllies = 3; // number of allies per 4 turns (spawning limited to one ally per hex)
	public int numLives = 3;
	public int win = 20; // winning condition

	//Controlling var
	private int playerY = 0; // Player's location
	private bool ready = false;
	private int numDead = 0;
	private bool gameOver;
	private bool restart;
	private int health;
	private int score;

	private int roundCounter;

	//Hexagon tile width and height in game world
	private float hexWidth;
	private float hexHeight;

	// Arrays & Lists
	private HexTile[,]hexArray; // array to hold hextiles
	private List<GameObject> allyList= new List<GameObject>();

///////////////////METHODS///////////////////////////////////////////////////////////////////////////////////

	//Method to initialise hexagon width and height

	void setSizes()
	{
		//renderer component attached to the Hex prefab is used to get the current width and height
		hexWidth = hex.renderer.bounds.size.x+0.2f;// float is to create a space between hexes
		hexHeight = hex.renderer.bounds.size.z+0.2f; 

		//creating an array to hold the tiles of the grid
		hexArray= new HexTile[gridWidthInHexes,gridHeightInHexes];
		
		for (float y = 0; y < gridHeightInHexes; y++)
			for (float x = 0; x < gridWidthInHexes; x++)
				hexArray[(int)x,(int)y]=new HexTile();
	}

	void setZones()
	{
		int[] xZones = new int[numZones];
		int[] yZones = new int[numZones];
		for (int i=0; i<numZones; i++)
		{
			xZones [i] = Random.Range (1, gridWidthInHexes);
			yZones [i] = Random.Range (1, gridHeightInHexes);
			for (int j=0; j<i; j++)
			{
				while(xZones[i]==xZones[j])
					xZones [i] = Random.Range (1, gridWidthInHexes);
				while(yZones[i]==yZones[j])
					yZones [i] = Random.Range (1, gridHeightInHexes);
			}
		}

		for (int i=0; i<numZones; i++) 
		{
			int xCoord= xZones[i];
			int yCoord= yZones[i];
			hexArray[xCoord,yCoord].setIsZone(true);
		}
	}

	void setHazards()
	{
		int[] xHazards = new int[numHazards];
		int[] yHazards = new int[numHazards];
		for (int i=0; i<numHazards; i++)
		{
			xHazards [i] = Random.Range (0, gridWidthInHexes);
			yHazards [i] = Random.Range (0, gridHeightInHexes);
			for (int j=0; j<i; j++)
			{
				while(xHazards[i]==xHazards[j])
					xHazards [i] = Random.Range (0, gridWidthInHexes);
				while(yHazards[i]==yHazards[j])
					yHazards [i] = Random.Range (0, gridHeightInHexes);
			}
		}
		
		for (int i=0; i<numHazards; i++) 
		{
			int xCoord= xHazards[i];
			int yCoord= yHazards[i];
			hexArray[xCoord,yCoord].setIsNewHazard(true);
		}
	}

	void setAllies()
	{
		int[] yAllies = new int[numAllies];
		for (int i=0; i<numAllies; i++)
		{
			yAllies [i] = Random.Range (0, gridHeightInHexes);
			for (int j=0; j<i; j++)
			{
				while(yAllies[i]==yAllies[j])
					yAllies [i] = Random.Range (0, gridHeightInHexes);
			}
		}
		
		for (int i=0; i<numAllies; i++) 
		{
			int yCoord= yAllies[i];
			hexArray[gridWidthInHexes-1,yCoord].addAllies();
			Debug.Log ("Allies:" + hexArray[gridWidthInHexes-1,yCoord].getHasXAllies());
		}
	}

	void setPlayer()
	{
		for (int y = 0; y < gridHeightInHexes; y++) 
		{
			for (int x = 0; x < gridWidthInHexes; x++) 
			{
				hexArray[x,y].setIsSelect(false);
			}
		}
		for (int i=0; i<gridWidthInHexes; i++) 
			hexArray[i,playerY].setIsSelect(true);
	}

	void updateHazards()
	{
		for (int y = 0; y < gridHeightInHexes; y++) 
		{
			for (int x = 0; x < gridWidthInHexes; x++) 
			{

				if (hexArray[x,y].getIsKIAHazard())
					hexArray[x,y].setIsKIAHazard(false);

				if (hexArray[x,y].getIsExploded())// is it an exploded hazard
				    hexArray[x,y].setIsExploded(false);

				if (hexArray[x,y].getIsArmedHazard())
				{
					if (hexArray[x,y].getHasXAllies()>0)
					{
						numDead+=hexArray[x,y].getHasXAllies();
						hexArray[x,y].setHasXAllies(0);
						hexArray[x,y].setIsKIAHazard(true);
						hexArray[x,y].setIsExploded(true);
						audio.PlayOneShot(explosion, 1f);
						Debug.Log(numDead + " dead ally. " + hexArray[x,y].getHasXAllies());

					}

					hexArray[x,y].setIsArmedHazard(false);
				}

				if (hexArray[x,y].getIsNewHazard())
				{
					hexArray[x,y].setIsNewHazard(false);
					hexArray[x,y].setIsArmedHazard(true);
				}
					
			}
		}
		setHazards ();
	}

	void updateAllies()
	{
				Debug.Log ("Updating Allies");
				for (int y = 0; y < gridHeightInHexes; y++) {
						for (int x = 0; x < gridWidthInHexes; x++) {
								if (hexArray [x, y].getIsSelect () == true) { // is the hex selected?
										Debug.Log ("Hex x:" + hexArray [x, y].getXCoord () + " y:" + hexArray [x, y].getYCoord () + "is selected");
										if (hexArray [x, y].getXCoord () == 0) {// check for the allies that are about to be saved
												if (hexArray [x, y].getHasXAllies () > 0) {
														for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																if (allyList [i] != null) {
																		bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																		bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																		int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																		int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																		if (!saved && !moved && xAlly == (x) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																				allyList [i].GetComponent<Ally> ().isSaved = true; // set it to they are saved!
																				allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved (just in case)
																				hexArray [x, y].removeAllies ();
																				score++;
																				audio.PlayOneShot(saveAlly, 0.5f);
																				Debug.Log ("score:" + score);
																		}
																}
														}
												}
										}

										if (hexArray [x, y].getYCoord () != 0 && hexArray [x, y].getYCoord () != gridHeightInHexes - 1) {// if the hex isn't on an edge
												if (hexArray [x, y].getYCoord () % 2 == 0) { // if even and not edge
														if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// if even and not edge, but not on the last column
																Debug.Log ("1:" + hexArray [x, y - 1].getHasXAllies () + " 2:" + hexArray [x + 1, y].getHasXAllies () + " 3:" + hexArray [x, y + 1].getHasXAllies ());
																// First of three: does this hex have allies on it?
																if (hexArray [x, y - 1].getHasXAllies () > 0) {
																		Debug.Log ("Hex x:" + hexArray [x, y - 1].getXCoord () + " y:" + hexArray [x, y - 1].getYCoord () + "is being pulled");
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						Debug.Log ("moved:" + moved + "saved:" + saved + "x:" + xAlly + "y:" + yAlly);
																						if (!saved && !moved && xAlly == x && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								Debug.Log ("Qualified");
																								int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										Debug.Log ("Allies arrived:" + hexArray [x, y].getHasXAllies ());
																										hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
																										Debug.Log ("Allies left:" + hexArray [x, y - 1].getHasXAllies ());
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn

																						}
																				}
																		}
																}

																// Second of three: does this hex have allies on it?
																if (hexArray [x + 1, y].getHasXAllies () > 0) {
																		Debug.Log ("Hex x:" + hexArray [x + 1, y].getXCoord () + " y:" + hexArray [x + 1, y].getYCoord () + "is being pulled");
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x + 1) && yAlly == y) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
																// Third of three: does this hex have allies on it?
																if (hexArray [x, y + 1].getHasXAllies () > 0) {
																		Debug.Log ("Hex x:" + hexArray [x, y + 1].getXCoord () + " y:" + hexArray [x, y + 1].getYCoord () + "is being pulled");
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == x && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
														} else { // if even and not edge, but last column
																// One of two: does this hex have allies on it?
																if (hexArray [x, y - 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
								
																// Two of two: does this hex have allies on it?
																if (hexArray [x, y + 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
														}
												} else { // if odd and not edge
														if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// if odd and not edge, but not on the last column
																// First of three: does this hex have allies on it?
																if (hexArray [x + 1, y - 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x + 1) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x + 1, y - 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x + 1, y - 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
								
																// Second of three: does this hex have allies on it?
																if (hexArray [x + 1, y].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x + 1) && yAlly == y) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
																// Third of three: does this hex have allies on it?
																if (hexArray [x + 1, y + 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x + 1) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x + 1, y + 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x + 1, y + 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
														} else { // if odd and not edge, but last column
																//DOES NOTHING! Just here for me to organise thoughts
														}
												}	
										} else { // if the hex IS on an edge
												Debug.Log ("Tile x:" + hexArray [x, y].getXCoord () + " y:" + hexArray [x, y].getYCoord () + "selected and is on the edge");
												if (hexArray [x, y].getYCoord () == gridHeightInHexes - 1) {// if the hex is on the last row
														if (hexArray [x, y].getYCoord () % 2 == 0) { // if on last row and even (this would only apply if the height changes and may well be useless....
																if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// on the last row, but not on the last column
																		// One of two: does this hex have allies on it?
																		if (hexArray [x, y - 1].getHasXAllies () > 0) {
																				for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																						if (allyList [i] != null) {
																								bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																								bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																								int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																								int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																								if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																										int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
																										for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																												hexArray [x, y].addAllies (); //add allies to the selected hex
																												hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
																										}
																										allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																								}
																						}
																				}
																		}

																		// Two of two: does this hex have allies on it?
																		if (hexArray [x + 1, y].getHasXAllies () > 0) {
																				for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																						if (allyList [i] != null) {
																								bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																								bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																								int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																								int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																								if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																										int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
																										for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																												hexArray [x, y].addAllies (); //add allies to the selected hex
																												hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
																										}
																										allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																								}
																						}
																				}
																		}
																} else { // even, on the last row, last col
																		// One option: does this hex have allies on it?
																		if (hexArray [x, y - 1].getHasXAllies () > 0) {
																				for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																						if (allyList [i] != null) {
																								bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																								bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																								int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																								int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																								if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																										int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
																										for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																												hexArray [x, y].addAllies (); //add allies to the selected hex
																												hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
																										}
																										allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																								}
																						}
																				}
																		}
																}
														} else { // if on last row and odd
																if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// odd on the last row, but not on the last column

																		// One of two: does this hex have allies on it?
																		if (hexArray [x + 1, y - 1].getHasXAllies () > 0) {
																				for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																						if (allyList [i] != null) {
																								bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																								bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																								int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																								int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																								if (!saved && !moved && xAlly == (x + 1) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																										int movingAllies = hexArray [x + 1, y - 1].getHasXAllies (); //how many allies on it?
																										for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																												hexArray [x, y].addAllies (); //add allies to the selected hex
																												hexArray [x + 1, y - 1].removeAllies (); // remove allies from the checked hex
																										}
																										allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																								}
																						}
																				}
																		}
									
																		// Two of two: does this hex have allies on it?
																		if (hexArray [x + 1, y].getHasXAllies () > 0) {
																				for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																						if (allyList [i] != null) {
																								bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																								bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																								int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																								int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																								if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																										int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
																										for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																												hexArray [x, y].addAllies (); //add allies to the selected hex
																												hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
																										}
																										allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																								}
																						}
																				}
																		}
																} else { // odd on the last row, last column 
																		// DOES NOTHING!!! (Seriously, this is just so that I know I thought about it)
																}
														}	
												} else if (hexArray [x, y].getYCoord () == 0) {// if the hex is on the first row
														if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// on the first row, but not on the last column//////////////////////////////////
																// One does this hex have allies on it?
																if (hexArray [x + 1, y].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
								
																// Two of two: does this hex have allies on it?
																if (hexArray [x, y + 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
														} else { // on the first row, last column 
																//only one option
																if (hexArray [x, y + 1].getHasXAllies () > 0) {
																		for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
																				if (allyList [i] != null) {
																						bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
																						bool saved = allyList [i].GetComponent<Ally> ().isSaved;
																						int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
																						int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
																						if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
																								int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
																								for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
																										hexArray [x, y].addAllies (); //add allies to the selected hex
																										hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
																								}
																								allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
																						}
																				}
																		}
																}
														}
												}
										}

								}
						}
				}

		for (int y = 0; y < gridHeightInHexes; y++) {
			for (int x = 0; x < gridWidthInHexes; x++) {
				if (hexArray [x, y].getIsZone () == true) { // is the hex a zone?
					Debug.Log ("Hex x:" + hexArray [x, y].getXCoord () + " y:" + hexArray [x, y].getYCoord () + "is a zone");
										
					if (hexArray [x, y].getYCoord () != 0 && hexArray [x, y].getYCoord () != gridHeightInHexes - 1) {// if the hex isn't on an edge
						if (hexArray [x, y].getYCoord () % 2 == 0) { // if even and not edge
							if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// if even and not edge, but not on the last column
								Debug.Log ("1:" + hexArray [x, y - 1].getHasXAllies () + " 2:" + hexArray [x + 1, y].getHasXAllies () + " 3:" + hexArray [x, y + 1].getHasXAllies ());
								// First of three: does this hex have allies on it?
								if (hexArray [x, y - 1].getHasXAllies () > 0) {
									Debug.Log ("Hex x:" + hexArray [x, y - 1].getXCoord () + " y:" + hexArray [x, y - 1].getYCoord () + "is being pulled");
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											Debug.Log ("moved:" + moved + "saved:" + saved + "x:" + xAlly + "y:" + yAlly);
											if (!saved && !moved && xAlly == x && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												Debug.Log ("Qualified");
												int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													Debug.Log ("Allies arrived:" + hexArray [x, y].getHasXAllies ());
													hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
													Debug.Log ("Allies left:" + hexArray [x, y - 1].getHasXAllies ());
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												
											}
										}
									}
								}
								
								// Second of three: does this hex have allies on it?
								if (hexArray [x + 1, y].getHasXAllies () > 0) {
									Debug.Log ("Hex x:" + hexArray [x + 1, y].getXCoord () + " y:" + hexArray [x + 1, y].getYCoord () + "is being pulled");
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x + 1) && yAlly == y) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
								// Third of three: does this hex have allies on it?
								if (hexArray [x, y + 1].getHasXAllies () > 0) {
									Debug.Log ("Hex x:" + hexArray [x, y + 1].getXCoord () + " y:" + hexArray [x, y + 1].getYCoord () + "is being pulled");
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == x && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
							} else { // if even and not edge, but last column
								// One of two: does this hex have allies on it?
								if (hexArray [x, y - 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
								
								// Two of two: does this hex have allies on it?
								if (hexArray [x, y + 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
							}
						} else { // if odd and not edge
							if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// if odd and not edge, but not on the last column
								// First of three: does this hex have allies on it?
								if (hexArray [x + 1, y - 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x + 1) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x + 1, y - 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x + 1, y - 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
								
								// Second of three: does this hex have allies on it?
								if (hexArray [x + 1, y].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x + 1) && yAlly == y) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
								// Third of three: does this hex have allies on it?
								if (hexArray [x + 1, y + 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x + 1) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x + 1, y + 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x + 1, y + 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
							} else { // if odd and not edge, but last column
								//DOES NOTHING! Just here for me to organise thoughts
							}
						}	
					} else { // if the hex IS on an edge
						Debug.Log ("Tile x:" + hexArray [x, y].getXCoord () + " y:" + hexArray [x, y].getYCoord () + "selected and is on the edge");
						if (hexArray [x, y].getYCoord () == gridHeightInHexes - 1) {// if the hex is on the last row
							if (hexArray [x, y].getYCoord () % 2 == 0) { // if on last row and even (this would only apply if the height changes and may well be useless....
								if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// on the last row, but not on the last column
									// One of two: does this hex have allies on it?
									if (hexArray [x, y - 1].getHasXAllies () > 0) {
										for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
											if (allyList [i] != null) {
												bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
												bool saved = allyList [i].GetComponent<Ally> ().isSaved;
												int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
												int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
												if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
													int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
													for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
														hexArray [x, y].addAllies (); //add allies to the selected hex
														hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
													}
													allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												}
											}
										}
									}
									
									// Two of two: does this hex have allies on it?
									if (hexArray [x + 1, y].getHasXAllies () > 0) {
										for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
											if (allyList [i] != null) {
												bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
												bool saved = allyList [i].GetComponent<Ally> ().isSaved;
												int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
												int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
												if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
													int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
													for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
														hexArray [x, y].addAllies (); //add allies to the selected hex
														hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
													}
													allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												}
											}
										}
									}
								} else { // even, on the last row, last col
									// One option: does this hex have allies on it?
									if (hexArray [x, y - 1].getHasXAllies () > 0) {
										for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
											if (allyList [i] != null) {
												bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
												bool saved = allyList [i].GetComponent<Ally> ().isSaved;
												int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
												int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
												if (!saved && !moved && xAlly == (x) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
													int movingAllies = hexArray [x, y - 1].getHasXAllies (); //how many allies on it?
													for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
														hexArray [x, y].addAllies (); //add allies to the selected hex
														hexArray [x, y - 1].removeAllies (); // remove allies from the checked hex
													}
													allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												}
											}
										}
									}
								}
							} else { // if on last row and odd
								if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// odd on the last row, but not on the last column
									
									// One of two: does this hex have allies on it?
									if (hexArray [x + 1, y - 1].getHasXAllies () > 0) {
										for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
											if (allyList [i] != null) {
												bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
												bool saved = allyList [i].GetComponent<Ally> ().isSaved;
												int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
												int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
												if (!saved && !moved && xAlly == (x + 1) && yAlly == (y - 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
													int movingAllies = hexArray [x + 1, y - 1].getHasXAllies (); //how many allies on it?
													for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
														hexArray [x, y].addAllies (); //add allies to the selected hex
														hexArray [x + 1, y - 1].removeAllies (); // remove allies from the checked hex
													}
													allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												}
											}
										}
									}
									
									// Two of two: does this hex have allies on it?
									if (hexArray [x + 1, y].getHasXAllies () > 0) {
										for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
											if (allyList [i] != null) {
												bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
												bool saved = allyList [i].GetComponent<Ally> ().isSaved;
												int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
												int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
												if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
													int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
													for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
														hexArray [x, y].addAllies (); //add allies to the selected hex
														hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
													}
													allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
												}
											}
										}
									}
								} else { // odd on the last row, last column 
									// DOES NOTHING!!! (Seriously, this is just so that I know I thought about it)
								}
							}	
						} else if (hexArray [x, y].getYCoord () == 0) {// if the hex is on the first row
							if (hexArray [x, y].getXCoord () != gridWidthInHexes - 1) {// on the first row, but not on the last column//////////////////////////////////
								// One does this hex have allies on it?
								if (hexArray [x + 1, y].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x + 1) && yAlly == (y)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x + 1, y].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x + 1, y].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
								
								// Two of two: does this hex have allies on it?
								if (hexArray [x, y + 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
							} else { // on the first row, last column 
								//only one option
								if (hexArray [x, y + 1].getHasXAllies () > 0) {
									for (int i = 0; i < allyList.Count; i++) { // now check the AllyList
										if (allyList [i] != null) {
											bool moved = allyList [i].GetComponent<Ally> ().hasMoved;
											bool saved = allyList [i].GetComponent<Ally> ().isSaved;
											int xAlly = allyList [i].GetComponent<Ally> ().xCoord;
											int yAlly = allyList [i].GetComponent<Ally> ().yCoord;
											if (!saved && !moved && xAlly == (x) && yAlly == (y + 1)) {// have the allies been saved or have they moved? if not, find the ally in question from the array
												int movingAllies = hexArray [x, y + 1].getHasXAllies (); //how many allies on it?
												for (int j=0; j<movingAllies; j++) { // for every ally on the checked hex,
													hexArray [x, y].addAllies (); //add allies to the selected hex
													hexArray [x, y + 1].removeAllies (); // remove allies from the checked hex
												}
												allyList [i].GetComponent<Ally> ().hasMoved = true; // set it to moved so that it doesn't move again this turn
											}
										}
									}
								}
							}
						}
					}
					
				}
			}
		}
		for (int y = 0; y < gridHeightInHexes; y++) {
			for (int x = 0; x < gridWidthInHexes; x++) {
				if (hexArray[x,y].getHasXAllies()>0)// does the hex still have allies on it?
				{	
					if (hexArray[x,y].getIsZone()==true)// if the hex is a zone
					{ 
						for(int i = 0; i < allyList.Count; i++)
						{
							if(allyList[i]!=null)
							{
								bool moved= allyList[i].GetComponent<Ally>().hasMoved;
								bool saved= allyList[i].GetComponent<Ally>().isSaved;
								int xAlly= allyList[i].GetComponent<Ally>().xCoord;
								int yAlly= allyList[i].GetComponent<Ally>().yCoord;
								if(!saved && !moved && xAlly ==x && yAlly ==y)// have the allies been saved or have they moved? if not, find the ally in question from the array
									allyList[i].GetComponent<Ally>().hasMoved=true; // set it to moved so that it stays in the zone
							}
						}
					}

					else // if it isn't a zone
					{
						for(int i = 0; i < allyList.Count; i++)
						{
							if(allyList[i]!=null)
							{
								bool moved= allyList[i].GetComponent<Ally>().hasMoved;
								bool saved= allyList[i].GetComponent<Ally>().isSaved;
								int xAlly= allyList[i].GetComponent<Ally>().xCoord;
								int yAlly= allyList[i].GetComponent<Ally>().yCoord;
								if(!saved && !moved && xAlly ==x && yAlly ==y)// have the allies been saved or have they moved? if not, find the ally in question from the array
								{
									int nextX = allyList[i].GetComponent<Ally>().nextXCoord;// set the ally's x coord to the next x coord
									int nextY = allyList[i].GetComponent<Ally>().nextYCoord;
									hexArray[x,y].removeAllies();//remove an ally from that hex
									if(nextX > -1) // check if the ally is about to exit the map
										hexArray[nextX, nextY].addAllies();// add an ally to hex where the ally is moving
									else
									{
										allyList[i].GetComponent<Ally>().isSaved=true;
										score++;
										audio.PlayOneShot(saveAlly, 0.5f);
										Debug.Log("score:" + score);
									}
									allyList[i].GetComponent<Ally>().hasMoved=true; // set it to moved

								}
							}
						}

					}	
					
				}	
			}
		}
		for (int i = 0; i < allyList.Count; i++) 
		{
			if(allyList[i]!=null)
				allyList [i].GetComponent<Ally> ().hasMoved = false;
		}
		Debug.Log ("Done updating Allies");
	}
	//Method to calculate the position of the first hexagon tile
	//The center of the hex grid is (0,0,0)
	Vector3 calcInitPos()
	{
		Vector3 initPos;
		//the initial position will be in the left upper corner
		initPos = new Vector3(-hexWidth * gridWidthInHexes / 2f + hexWidth / 2, 0,
		                      gridHeightInHexes / 2f * hexHeight - hexHeight / 2);
		return initPos;
	}
	
	//method used to convert hex grid coordinates to game world coordinates
	public Vector3 calcWorldCoord(Vector2 gridPos)
	{
		//Position of the first hex tile
		Vector3 initPos = calcInitPos();
		//Every second row is offset by half of the tile width
		float offset = 0;
		if (gridPos.y % 2 != 0)
			offset = hexWidth / 2;
		
		float x =  initPos.x + offset + gridPos.x * hexWidth;
		//Every new line is offset in z direction by 3/4 of the hexagon height
		float z = initPos.z - gridPos.y * hexHeight * 0.75f;
		return new Vector3(x, 0, z);
	}

	void createGrid()
	{					
		//Game object which is the parent of all the hex tiles
		GameObject hexGridGO = new GameObject("HexGrid");
		hexGridGO.tag = "Finish";
		for (float y = 0; y < gridHeightInHexes; y++)
		{
			for (float x = 0; x < gridWidthInHexes; x++)
			{
				hexTile = hexArray[(int)x,(int)y];
				hexTile.setXCoord((int)x);
				hexTile.setYCoord((int)y);

				bool isZone = hexTile.getIsZone();
				bool isNewHazard = hexTile.getIsNewHazard();
				bool isArmedHazard = hexTile.getIsArmedHazard();
				bool isKIAHazard = hexTile.getIsKIAHazard();
				bool isExploded = hexTile.getIsExploded();
				int hasXAllies = hexTile.getHasXAllies();
				bool isSelect = hexTile.getIsSelect();
				if(isZone)
				{
					GameObject zZone= (GameObject)Instantiate(zone);
					//Current position in grid
					Vector2 gridPos = new Vector2(x, y);
					zZone.transform.position = calcWorldCoord(gridPos)+new Vector3(0,0.1f,0);
					zZone.transform.parent = hexGridGO.transform;
				}

				if(isNewHazard)
				{
					GameObject hHazard= (GameObject)Instantiate(newHazard);
					//Current position in grid
					Vector2 gridPos = new Vector2(x, y);
					hHazard.transform.position = calcWorldCoord(gridPos)+new Vector3(0,0.18f,0);
					hHazard.transform.parent = hexGridGO.transform;
				}

				if(isArmedHazard)
				{
					GameObject hHazard= (GameObject)Instantiate(armedHazard);
					//Current position in grid
					Vector2 gridPos = new Vector2(x, y);
					hHazard.transform.position = calcWorldCoord(gridPos)+new Vector3(0,0.15f,0);
					hHazard.transform.parent = hexGridGO.transform;
				}

				if(isKIAHazard)
				{
					GameObject hHazard= (GameObject)Instantiate(kIAHazard);
					//Current position in grid
					Vector2 gridPos = new Vector2(x, y);
					hHazard.transform.position = calcWorldCoord(gridPos)+new Vector3(0,0.19f,0);
					hHazard.transform.parent = hexGridGO.transform;
					if (isExploded){
						Instantiate(allyExplosion, hHazard.transform.position, transform.rotation); /////////Maybe Problem
						hexTile.setIsExploded(false);

					}

				}

				if(hasXAllies>0)
				{

					for(int i=0; i<hasXAllies;i++)
					{
						GameObject aAlly= (GameObject)Instantiate(ally);
						aAlly.GetComponent<Ally>().initAlly((int)x, (int) y);
					//Current position in grid
						Vector2 gridPos = new Vector2(x, y);
						aAlly.transform.position = calcWorldCoord(gridPos)+new Vector3(0,0.15f,0);
					//alliesText.transform.position= calcWorldCoord(gridPos)+new Vector3(0,0.15f,0.2f);
					//alliesText.text= hexArray[(int)x,(int)y].getHasXAllies().ToString();
						aAlly.transform.parent = hexGridGO.transform;
						if(aAlly!= null)
							allyList.Add(aAlly);
					}		
				}

				if(isSelect)
				{
					GameObject pPlayer= (GameObject)Instantiate(player);
					//Current position in grid
					Vector2 gridPos = new Vector2(x, y);
					pPlayer.transform.position = calcWorldCoord(gridPos)+new Vector3(0,-0.1f,0);
					pPlayer.transform.parent = hexGridGO.transform;

				}

				//normal hexes:
				GameObject hexa= (GameObject)Instantiate(hex);
				//Current position in grid
				Vector2 gGridPos = new Vector2(x, y);
				hexa.transform.position = calcWorldCoord(gGridPos);
				hexa.transform.parent = hexGridGO.transform;

			}
		}
	}


	//void playerCall(){


	//}

///////////////////IN GAME///////////////////////////////////////////////////////////////////////////////////

	void Awake(){
		setSizes();
		setZones ();
		setHazards ();
		setAllies ();
		setPlayer ();
		createGrid ();
		ready = true;
	}

	void Start()
	{
		gameOver = false;
		restart = false;
		roundCounter = 0;
		score = 0;
		restartText.text = "";
		creditText.text = "";
		gameOverText.text = "";
		numLivesText.text = "Lives: " + numLives.ToString ();
		//alliesText.text = "";
		scoreText.text = "Allies saved: " + score;
	}

	void Update()
	{

	}

	void FixedUpdate()
	{
		if (ready && Input.GetKeyUp (KeyCode.Space)) 
		{
			audio.PlayOneShot(selection, 0.5f);
			ready = false;
			roundCounter++;
			//playerCall();
			updateAllies();
			updateHazards();
			health = numLives - numDead;
			if (numDead >= numLives || score >= win)
			{
					gameOver=true;
					if(score >= win)
						gameOverText.text = "YOU WIN!!!";
					else
						gameOverText.text = "GAME OVER!";
					restartText.text= "Press 'R' for Restart";
					creditText.text= "Music is Ouroboros by Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 3.0, http://creativecommons.org/licenses/by/3.0/";
					restart= true;
					
			}
			numLivesText.text = "Lives: " + health.ToString ();
			scoreText.text = "Score: " + score.ToString();
			if (roundCounter==6)//spawn allies every 6th round
			{
				roundCounter=0;
				setAllies ();
			}
			Destroy(GameObject.FindWithTag("Finish"));
			createGrid ();
			if (gameOver == false)
				ready = true;
		}

		if (restart)
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Application.LoadLevel(Application.loadedLevel);
			}
		}

		if (ready && Input.GetKeyUp (KeyCode.UpArrow)) 
		{
			audio.PlayOneShot(selMove, 0.2f);
			if (playerY>0)
				playerY--;
			setPlayer ();
			Destroy(GameObject.FindWithTag("Finish"));
			createGrid ();
		}

		else if (ready && Input.GetKeyUp (KeyCode.DownArrow)) 
		{
			audio.PlayOneShot(selMove, 0.2f);
			if (playerY<gridHeightInHexes-1)
				playerY++;
			setPlayer ();
			Destroy(GameObject.FindWithTag("Finish"));
			createGrid ();
			
		}

	}


}
