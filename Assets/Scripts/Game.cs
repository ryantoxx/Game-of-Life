using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    private static int SCREEN_WIDTH = 64;       //1024 px
    private static int SCREEN_HEIGHT = 48;      //768 px

    public float speed = 0.1f;
    private float timer = 0;

    public bool simulationEnabled = false;

    public bool chaosRule = false; 
    public float chaosProbability = 0.05f; //small value 5%

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
        if (simulationEnabled)
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

                if (chaosRule)
                {
                    ChaosRule();
                }
                
                numGenerations--;
            }
            else
            {
                // amount of time passed since last frame
                timer += Time.deltaTime;
            }
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
            simulationEnabled = false;

        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            // Resume Simulation
            simulationEnabled = true;
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

                // Rule 1: A living cell with two or three living neighbors survives to the next generation
                if (cell.isAlive && (cell.numNeighbours == 2 || cell.numNeighbours == 3))
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

    void ChaosRule() 
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {           
                Cell cell = grid[x, y];

                if (Random.value < chaosProbability)
                {
                    cell.SetAlive(!cell.isAlive);
                }
            }
        }      
    }

    bool RandomAliveCell ()
    {
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand > 75)
        {
            return true;
        }

        return false;
    }
}
