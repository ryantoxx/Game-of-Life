using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    private static int SCREEN_WIDTH = 64;       //1024 px
    private static int SCREEN_HEIGHT = 48;      //768 px

    public float speed = 0.1f;
    private float timer = 0;

    public bool StartSimulation = false;
    public bool clearGrid = false;

    public bool StartChaosZone = false; 
    public float chaosProbability = 0.05f; //small value 5%

    public bool randomBirthsRule = false;
    public float birthProbability = 0.1f;

    private bool times = true;
    public bool randomPattern = false;
    public bool reflector = false;
    public bool gliderGun = false;

    public int numGenerations = 100;


    Cell[,] grid = new Cell[SCREEN_WIDTH, SCREEN_HEIGHT];

    // Start is called before the first frame update
    void Start()
    {
        PlaceCells();
    }

    // Update is called once per frame
    void Update()
    {
        if (StartSimulation)
        {
            if (timer >= speed)
            {
                if (numGenerations <= 0)
                {
                    return;
                }
                
                timer = 0f;

                CountNeighbours();

                PopulationControl();    

                if (StartChaosZone)
                {
                    ChaosZone();
                }
                
                numGenerations--;
            }
            else
            {
                // amount of time passed since last frame
                timer += Time.deltaTime;
            }
        }

        if (!randomPattern)
        {
            times = true;
        }
        if (randomPattern && !StartSimulation && times)
        {
            ClearGrid();
            RandomConfiguration();
            times = false;
        }
        else if (reflector && !StartSimulation)
        {
            ClearGrid();
            Reflector();
        }
        else if (gliderGun && !StartSimulation)
        {
            ClearGrid();
            GliderGun();
        }

        if (clearGrid)
        {
            ClearGrid();
        } 

        UserInput();
    }

    void UserInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int x = Mathf.RoundToInt(mousePoint.x);
            int y = Mathf.RoundToInt(mousePoint.y);

            if (x >= 0 && y >= 0 && x < SCREEN_WIDTH && y < SCREEN_HEIGHT)
            {
                // In bounds
                grid[x, y].SetAlive(!grid[x, y].isAlive);
            } 
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            // Pause Simulation
            StartSimulation = false;

        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            // Resume Simulation
            StartSimulation = true;
        }
    }

    void PlaceCells ()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                Cell cell = Instantiate(Resources.Load("Prefabs/Cell", typeof(Cell)), new Vector2(x, y), Quaternion.identity) as Cell;
                grid[x, y] = cell;
                grid[x, y].SetAlive(false);
            }
        }
    }

    void CountNeighbours()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                int numNeighbours = 0;

                // North
                if ( y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x, y + 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }
                // East
                if (x + 1 < SCREEN_WIDTH)
                {
                    if (grid[x + 1, y].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                // South
                if (y - 1 >= 0) 
                {
                    if (grid[x, y - 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                // West
                if (x - 1 >= 0)
                {
                    if (grid[x - 1, y].isAlive)
                    {
                        numNeighbours++;
                    }
                }
                // North-East
                if (x + 1 < SCREEN_WIDTH && y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x + 1, y + 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                //North-West
                if (x - 1 >= 0 && y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x - 1,y + 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                //South-East
                if (x + 1 < SCREEN_WIDTH && y - 1 >= 0) 
                {
                    if (grid[x + 1, y - 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                // South-West
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (grid[x - 1, y - 1].isAlive)
                    {
                        numNeighbours++;
                    }
                }

                grid[x, y].numNeighbours = numNeighbours;
            }
        }
    }

    void PopulationControl ()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                /*
                    Basic Rules: 
                        1. A living cell with two or three living neighbors survives to the next generation;
                        2. A living cell with fewer than two living neighbors dies from underpopulation;
                        3. A living cell with more than three living neighbors dies from overpopulation;
                        4. A dead cell with exactly three living neighbors becomes a living cell by reproduction.
                */
                // Get the current cell
                Cell cell = grid[x, y];

                if (cell.isAlive && randomBirthsRule && Random.value <= birthProbability)
                {
                    RandomBirths(x, y);
                }

                // Rule 1: A living cell with two or three living neighbors survives to the next generation
                else if (cell.isAlive && (cell.numNeighbours == 2 || cell.numNeighbours == 3))
                {
                    // Cell stays alive
                    cell.SetAlive(true);
                }
                // Rule 2: A living cell with fewer than two living neighbors dies from underpopulation
                else if (cell.isAlive && cell.numNeighbours < 2)
                {
                    cell.SetAlive(false);
                }
                // Rule 3: A living cell with more than three living neighbors dies from overpopulation
                else if (cell.isAlive && cell.numNeighbours > 3)
                {
                    cell.SetAlive(false);
                }
                // Rule 4: A dead cell with exactly three living neighbors becomes a living cell by reproduction
                else if (!cell.isAlive && cell.numNeighbours == 3)
                {
                    cell.SetAlive(true);
                }

            }
        }
    }

    void ChaosZone() 
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {   
                Cell cell = grid[x, y];

                if (x >= 0 && x <= 64 && y >= 0 && y <= 12)
                {
                    if (Random.value < chaosProbability)
                    {
                        cell.SetAlive(!cell.isAlive);
                    }
                }
            }
        }      
    }

    void RandomBirths(int x, int y)
    {
        int numBirths = UnityEngine.Random.Range(1, 3);

        for (int i = 1; i <= numBirths; i++)
        {
            int posX = UnityEngine.Random.Range(-1, 2);
            int posY = UnityEngine.Random.Range(-1, 2);

            if (x > 0 && x < (SCREEN_WIDTH - 1) && y > 0 && y < (SCREEN_HEIGHT - 1))
            {
                grid[x + posX, y + posY].SetAlive(true);
            } 
        }
    }

    void ClearGrid()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {           
                grid[x, y].SetAlive(false);
            }
        } 
    }

    void RandomConfiguration()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {           
                Cell cell = grid[x, y];

                if (Random.value > 0.75f)
                {
                    cell.SetAlive(true);
                }
            }
        }   
    }

    void Reflector()
    {
        int x = SCREEN_WIDTH/2 - 5;
        int y = SCREEN_HEIGHT/2;
        grid[x, y].SetAlive(true);
        grid[x+1, y].SetAlive(true);
        grid[x+2, y-1].SetAlive(true);
        grid[x+2, y+1].SetAlive(true);
        grid[x+3, y].SetAlive(true);
        grid[x+4, y].SetAlive(true);
        grid[x+5, y].SetAlive(true);
        grid[x+6, y].SetAlive(true);
        grid[x+7, y-1].SetAlive(true);
        grid[x+7, y+1].SetAlive(true);
        grid[x+8, y].SetAlive(true);
        grid[x+9, y].SetAlive(true);
    }

    void GliderGun()
    {
        int x = 0;
        int y = 30;
        grid[x+1, y+5].SetAlive(true);
        grid[x+2, y+5].SetAlive(true);
        grid[x+1, y+6].SetAlive(true);
        grid[x+2, y+6].SetAlive(true);

        grid[x+11, y+5].SetAlive(true);
        grid[x+11, y+6].SetAlive(true);
        grid[x+11, y+7].SetAlive(true);
        grid[x+12, y+4].SetAlive(true);
        grid[x+12, y+8].SetAlive(true);
        grid[x+13, y+3].SetAlive(true);
        grid[x+14, y+3].SetAlive(true);
        grid[x+13, y+9].SetAlive(true);
        grid[x+14, y+9].SetAlive(true);
        grid[x+15, y+6].SetAlive(true);
        grid[x+16, y+4].SetAlive(true);
        grid[x+16, y+8].SetAlive(true);
        grid[x+17, y+5].SetAlive(true);
        grid[x+17, y+6].SetAlive(true);
        grid[x+17, y+7].SetAlive(true);
        grid[x+18, y+6].SetAlive(true);

        grid[x+21, y+7].SetAlive(true);
        grid[x+22, y+7].SetAlive(true);
        grid[x+21, y+8].SetAlive(true);
        grid[x+22, y+8].SetAlive(true);
        grid[x+21, y+9].SetAlive(true);
        grid[x+22, y+9].SetAlive(true);
        grid[x+23, y+6].SetAlive(true);
        grid[x+23, y+10].SetAlive(true);
        grid[x+25, y+5].SetAlive(true);
        grid[x+25, y+6].SetAlive(true);
        grid[x+25, y+10].SetAlive(true);
        grid[x+25, y+11].SetAlive(true);

        grid[x+35, y+8].SetAlive(true);
        grid[x+36, y+8].SetAlive(true);
        grid[x+35, y+9].SetAlive(true);
        grid[x+36, y+9].SetAlive(true);
    }
}
