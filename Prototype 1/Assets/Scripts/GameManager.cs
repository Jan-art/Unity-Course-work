using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager instance;

  

  [System.Serializable]public class Player
  {
     public enum PlayerType //In future versions of the game, can add NPC players that are controlled by AI. 
     {
         HUMAN
     }

     public PlayerType playerType;
     public Tile[,] myGrid = new Tile[10,10];
     public bool[,] revealGrid = new bool[10,10];
     public PhysicalGameBoard pgb;
     //public LayerMask layerToPlaceOn;

        [Space]
        public GameObject cameraPos;
        public GameObject placePanel;
        public GameObject shootPanel;
        [Space]
        public GameObject WinPanel;
     //SHOW & HIDE SHIPS

     public Player()
     {
         for (int x = 0; x < 10; x++)
         {
             for (int y = 0; y < 10; y++)
             {
                 OccupationType t = OccupationType.EMPTY;
                 myGrid[x, y] = new Tile(t, null);
                 revealGrid[x, y] = false;
             }
         }     
     }
     
     public List<GameObject> placedShipList = new List<GameObject>();
  }

  int activePlayer; //Track current Turn
  public Player[] players = new Player[2];


    //STATE MACHINE
    public enum GameStates
    {
        P1_PLACE_SHIPS,
        P2_PLACE_SHIPS,
        KILL,
        IDLE
    }

    public GameStates gameState;
    public GameObject WarCamPos;
    bool CamMoved;
    public GameObject placingCanvas;

    //MISSILE
    public GameObject missilePrefab;
    float altitude = 3f;
    float Timer;

    //ADD SPEED float

    bool isShooting;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        

        //HIDE PANELS
        HideAllPanels();

        //

        players[0].WinPanel.SetActive(false);
        players[1].WinPanel.SetActive(false);

        //ACTIVATE PLACE PANEL P1

        players[activePlayer].placePanel.SetActive(true);
        gameState = GameStates.IDLE;

        //MOVE CAMERA



    }

 

  void AddShipToList(GameObject placedShip)
  {
      players[activePlayer].placedShipList.Add(placedShip);
  }

  public void UpdateGrid(Transform shipTransform, ShipBehaviour ship, GameObject placedShip)
  {
      foreach(Transform child in shipTransform)
      {
          TileInfo tInfo = child.GetComponent<GhostBehaviour>().GetTileInfo();
          players[activePlayer].myGrid[tInfo.xPos, tInfo.zPos] = new Tile(ship.type, ship);
      }

      AddShipToList(placedShip);
      //DebugGrid();
  }

    public bool CheckIfOccupied(int xPos, int zPos)
    {
        return players[activePlayer].myGrid[xPos, zPos].IsOccupied();
    }

  public void DebugGrid()
  {
      string s = "";
      //Separator
      int sep = 0; 
      for (int x = 0; x < 10; x++)
      {
          s += "|";
          for (int y = 0; y < 10; y++)
          {
              string t = "0"; //Occupation Type
              if(players[activePlayer].myGrid[x, y].type == OccupationType.CARRIER)
              {
                  t = "C";
              }
               if(players[activePlayer].myGrid[x, y].type == OccupationType.BATTLESHIP)
              {
                  t = "B";
              }
               if(players[activePlayer].myGrid[x, y].type == OccupationType.SUBMARINE)
              {
                  t = "S";
              }
               if(players[activePlayer].myGrid[x, y].type == OccupationType.CRUISER)
              {
                  t = "U";
              }
               if(players[activePlayer].myGrid[x, y].type == OccupationType.CORVETTE)
              {
                  t = "R";
              }

              s += t;
              sep = y % 10;
              if(sep == 9)
              {
                  s += "|";
              }

          }

          s += "\n";
          
      }
      print(s);
  }
  
    public void RemoveAllShipsFromList()
    {
        foreach (GameObject ship in players[activePlayer].placedShipList)
        {
            Destroy(ship);
        }
        players[activePlayer].placedShipList.Clear();

        InitialiseGrid();
    }

    void InitialiseGrid()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                OccupationType t = OccupationType.EMPTY;
                players[activePlayer].myGrid[x, y] = new Tile(t, null);
                players[activePlayer].revealGrid[x, y] = false;
            }
        }
    }

    
    ///[GAME BATTLE SCRIPT]

    
    void Update()
    {
        switch (gameState)
        {
            case GameStates.P1_PLACE_SHIPS:
                {
                    //DEACTIVATE PANEL
                    players[activePlayer].placePanel.SetActive(false);
                    PlaceSystem.instance.SetPlayerField(players[activePlayer].pgb, players[activePlayer].playerType.ToString());
                    StartCoroutine(MoveCamera(players[activePlayer].cameraPos));
                    gameState = GameStates.IDLE;
                }
                break;
            case GameStates.IDLE: //WAIT-TIME
                {
                    
                }
                break;
            case GameStates.P2_PLACE_SHIPS:
                {
                    //DEACTIVATE PANEL
                    players[activePlayer].placePanel.SetActive(false);
                    PlaceSystem.instance.SetPlayerField(players[activePlayer].pgb, players[activePlayer].playerType.ToString());
                    gameState = GameStates.IDLE;
                }
                break;
            case GameStates.KILL:
                {
                    //WARFARE

                    
                }
                break;
        }
    }

     void HideAllPanels()
    {
        players[0].placePanel.SetActive(false);
        players[0].shootPanel.SetActive(false);

        players[1].placePanel.SetActive(false);
        players[1].shootPanel.SetActive(false);
    }

    //PLACE PANEL BTN P1
    public void P1PlaceShips()
    {
        gameState = GameStates.P1_PLACE_SHIPS;
        
    }

    //PLACE PANEL BTN P2
    public void P2PlaceShips()
    {
        gameState = GameStates.P2_PLACE_SHIPS;
    }

    //READY BTN

    public void SelectReady()
    {
        if (activePlayer == 0)
        {
            //HIDE SHIPS
            HideAllShips();

            //SWITCH Player 
            SwitchPlayer();

            //MOVE CAM
            StartCoroutine(MoveCamera(players[activePlayer].cameraPos));

            //ACTIVATE P2 PANELS
            players[activePlayer].placePanel.SetActive(true);

            //RETURN
            return;

        }

        if (activePlayer == 1)
        {
            //HIDE SHIPS
            HideAllShips();

            //SWITCH Player
            SwitchPlayer();

            //MOVE CAM
            StartCoroutine(MoveCamera(WarCamPos));

            //ACTIVATE P1 KILL PANELS
            players[activePlayer].shootPanel.SetActive(true);

            //UNHIDE P1 SHIPS
            //UnHideAllShips(); //Not needed anymore 

            //TURN_OFF PLACING CANVAS
            placingCanvas.SetActive(false);

            //Game Start
            
        }

    }

       void HideAllShips()
        {
            foreach (var ship in players[activePlayer].placedShipList)
            {
                ship.GetComponent<MeshRenderer>().enabled = false;
            }
        }

       void UnHideAllShips()
        {
            foreach (var ship in players[activePlayer].placedShipList)
            {
                ship.GetComponent<MeshRenderer>().enabled = true;
            }
        }


    void SwitchPlayer()
    {
        activePlayer++;
        activePlayer %= 2;
    }

    IEnumerator MoveCamera(GameObject camObj)
    {
        if (CamMoved)
        {
            yield break;
        }

        CamMoved = true;

        float t = 0;
        float duration = 0.5f;

        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;

        Vector3 toPos = camObj.transform.position;
        Quaternion toRot = camObj.transform.rotation;

        while (t < duration)
        {
            t += Time.deltaTime;

            Camera.main.transform.position = Vector3.Lerp(startPos, toPos, t / duration);  //Duration always happens between "0 & 1". We need to define it as it can't be Higher than 1. 
                                                                                           //| 0 means 0%. | 0.5 means 50% | 1 means 100 % |

            Camera.main.transform.rotation = Quaternion.Lerp(startRot, toRot, t / duration);


            yield return null;
        }


        CamMoved = false;
    }

    //KILL PANEL BTN

    public void KillBtn()
    {
        UnHideAllShips();
        players[activePlayer].shootPanel.SetActive(false);
        gameState = GameStates.KILL;
    }

    int Rival()
    {
        int ap = activePlayer; 
        ap++;
        ap %= 2;

        int rival = ap;
        return rival;
    }

    public void CheckShot(int x, int z, TileInfo info)
    {
        StartCoroutine(IdentifyLocation(x, z, info));
    }

    IEnumerator IdentifyLocation(int x, int z, TileInfo info)
    {
        if (isShooting)
        {
            yield break;
        }
        isShooting = true;

        int rival = Rival();


        //IF YOUR TILE

        if (!players[rival].pgb.RequestTile(info))
        {
            //print("LOL");
            isShooting = false;
            yield break;
        }

        //IF TILE IS ALREADY HIT
        if (players[rival].revealGrid[x, z] == true)
        {
            //print("Location already Hit");
            isShooting = false;
            yield break;
        }

        //MISSILE
        Vector3 startPos = Vector3.zero;
        Vector3 aimPos = info.gameObject.transform.position;

        GameObject missile = Instantiate(missilePrefab, startPos, Quaternion.identity);

        while(MoveToTile(startPos,aimPos,0.5f, missile))
        {
            yield return null;
        }

        Destroy(missile);
        Timer = 0; //Reset missile timer

        //CHECK IF TILE BUSY
        if (players[rival].myGrid[x, z].IsOccupied())
        {
            //Damage SHIP

            bool sunk = players[rival].myGrid[x, z].placedShip.AbsorbDamage();

            if (sunk)
            {
                players[rival].placedShipList.Remove(players[rival].myGrid[x, z].placedShip.gameObject);
            }

            //HIGHLIGHT TILE
            //ADD [EXPLOSION + SOUND HERE]
            info.ActivateTop(3, true);

        }
        else
        {   //HIGHLIGHT TILE
            //ADD [EXPLOSION + SOUND HERE]

            //NOT HIT
            info.ActivateTop(2, true);
        }

        //REVEAL TILE
        players[rival].revealGrid[x, z] = true;

        //CHECK WIN STATUS

        if (players[rival].placedShipList.Count == 0)
        {
            //print("You Win!!");
            //LOGIC
            players[activePlayer].WinPanel.SetActive(true);
        
            yield break;
        }
        yield return new WaitForSeconds(1f);


        //HIDE SHIPS
        HideAllShips();
        //SWITCH PLAYER
        SwitchPlayer();
        //ACTIVATE PANEL
        players[activePlayer].shootPanel.SetActive(true);
        //SET IDLE STATE
        gameState = GameStates.IDLE;

        isShooting = false;
    }

    bool MoveToTile(Vector3 startPos, Vector3 aimPos, float speed, GameObject missile)
    {
        Timer += speed * Time.deltaTime;
        Vector3 nextPos = Vector3.Lerp(startPos, aimPos, Timer);
        nextPos.y = altitude * Mathf.Sin(Mathf.Clamp01(Timer) * Mathf.PI);
        missile.transform.LookAt(nextPos);

        return aimPos != (missile.transform.position = Vector3.Lerp(missile.transform.position,nextPos,Timer));
    }
}

