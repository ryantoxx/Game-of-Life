using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{

    private static int SCREEN_WIDTH = 64;       //1024 px
    private static int SCREEN_HEIGHT = 48;      //768 px

    public float speed = 0.1f;
    private float timer = 0;

    public bool StartSimulation = false;
    public bool clearGrid = false;

    public bool StartChaosZone = false; 
    public float chaosProbability = 0;

    public bool randomBirthsRule = false;
    public float birthProbability = 0;

    public bool randomPattern = false;
    public bool reflector = false;
    public bool gliderGun = false;

    public int numGenerations = 1000;

    public bool uniRule = false;

    Canvas mainMenuCanvas;
    InputField numGen;
    Slider speedSlider, chaosSlider, birthSlider;
    Text speedText, chaosText, birthText, generationText;
    Toggle toggle, uniToggle, birthToggle;
    Button startButton, stopButton, clearButton;
    Button randomButton, reflectorButton, gunButton;
    bool inMainMenu = false;
    bool currentState = false;

    Cell[,] grid = new Cell[SCREEN_WIDTH, SCREEN_HEIGHT];

    // Start is called before the first frame update
    void Start()
    {
        PlaceCells();
        mainMenuCanvas = GameObject.Find("MainMenu").GetComponent<Canvas>();

        //Nr of Generation Text
        generationText = GameObject.Find("TextGen").GetComponent<Text>();

        //Sliders
        speedSlider = GameObject.Find("Slider").GetComponent<Slider>();
        speedText = GameObject.Find("SliderSpeedText").GetComponent<Text>();
        chaosSlider = GameObject.Find("SliderChaos").GetComponent<Slider>();
        chaosText = GameObject.Find("SliderTextChaos").GetComponent<Text>();
        birthSlider = GameObject.Find("SliderBirth").GetComponent<Slider>();
        birthText = GameObject.Find("SliderTextBirth").GetComponent<Text>();

        //Toggle
        toggle = GameObject.Find("ToggleChaos").GetComponent<Toggle>();
        uniToggle = GameObject.Find("ToggleUni").GetComponent<Toggle>();
        birthToggle = GameObject.Find("ToggleBirth").GetComponent<Toggle>();

        //Buttons
        startButton = GameObject.Find("StartSim").GetComponent<Button>();
        startButton.onClick.AddListener(OnButtonClickStart);
        stopButton = GameObject.Find("StopSim").GetComponent<Button>();
        stopButton.onClick.AddListener(OnButtonClickStop);
        clearButton = GameObject.Find("ClearSim").GetComponent<Button>();
        clearButton.onClick.AddListener(OnButtonClickClear);

        randomButton =  GameObject.Find("ButtonRandom").GetComponent<Button>();
        randomButton.onClick.AddListener(OnButtonClickRandom);
        reflectorButton =  GameObject.Find("ButtonReflector").GetComponent<Button>();
        reflectorButton.onClick.AddListener(OnButtonClickReflector);
        gunButton =  GameObject.Find("ButtonGun").GetComponent<Button>();
        gunButton.onClick.AddListener(OnButtonClickGun);

        //Input Field
        numGen = GameObject.Find("InputNum").GetComponent<InputField>();
        numGen.onEndEdit.AddListener(OnEndEdit);

        toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(toggle);
        });

        uniToggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(uniToggle);
        });

        birthToggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(birthToggle);
        });

        generationText.text = "Generation: " + numGenerations.ToString();
        speedText.text = "Speed: "+ speedSlider.value.ToString("F2");
        chaosText.text = "Chaos Probability: "+ chaosSlider.value.ToString("F2");
        birthText.text = "Birth Probability: "+ birthSlider.value.ToString("F2");
        mainMenuCanvas.gameObject.SetActive(false);
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
                if (StartChaosZone)
                {
                    ChaosZone();
                }
                PopulationControl();    
                
                numGenerations--;
            }
            else
            {
                // amount of time passed since last frame
                timer += Time.deltaTime;
            }
        }

        if (StartChaosZone && StartSimulation)
        {
            ChaosZone();
        }

        UserInput();
        speed = speedSlider.value;
        chaosProbability = chaosSlider.value;
        birthProbability = birthSlider.value;
        speedText.text = "Speed: "+ speedSlider.value.ToString("F2");
        chaosText.text = "Chaos Probability: "+ chaosSlider.value.ToString("F2");
        birthText.text = "Birth Probability: "+ birthSlider.value.ToString("F2");
        generationText.text = "Generation: " + numGenerations.ToString();
    }

    void UserInput()
    {
        if (Input.GetMouseButtonDown(0) && inMainMenu == false)
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

        if (Input.GetKeyUp(KeyCode.P) && inMainMenu == false)
        {
            // Pause Simulation
            StartSimulation = false;

        }

        if (Input.GetKeyUp(KeyCode.B) && inMainMenu == false)
        {
            // Resume Simulation
            StartSimulation = true;
        }
        /*
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if(inMainMenu == false)
            {
                currentState = StartSimulation;
                inMainMenu = true;
                StartSimulation = false;
                mainMenuCanvas.gameObject.SetActive(true);
            }
            else
            {
                inMainMenu = false;
                StartSimulation = currentState;
                speed = speedSlider.value;
                chaosProbability = chaosSlider.value;
                birthProbability = birthSlider.value;
                mainMenuCanvas.gameObject.SetActive(false);
            }
        }
        */
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

                // uni Rule
                if (cell.isAlive && uniRule)
                {
                    UniReproduction();
                }
                else if (cell.isAlive && randomBirthsRule && Random.value <= birthProbability)
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
        float prob = chaosProbability / 100;
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {   
                Cell cell = grid[x, y];

                if (x >= 0 && x <= 64 && y >= 0 && y <= 12)
                {
                    if (Random.value < prob)
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

    public void ClearGrid()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {           
                grid[x, y].SetAlive(false);
            }
        } 
    }

    void ToggleValueChanged(Toggle toggle)
    {
        // Set target object's active status to match toggle value
        if(toggle.isOn)
        {
            Text label = toggle.GetComponentInChildren<Text>();
            string textValue = label.text;
            if(string.Equals(textValue,"Chaos Zone"))
            {
                StartChaosZone = true;
            }
            else if(string.Equals(textValue,"UniProduction Rule"))
            {
                uniRule = true;
            }
            else if(string.Equals(textValue,"Birth Rule"))
            {
                randomBirthsRule = true;
            }
        }
        else
        {
            Text label = toggle.GetComponentInChildren<Text>();
            string textValue = label.text;
            if(string.Equals(textValue,"Chaos Zone"))
            {
                StartChaosZone = false;
            }
            else if(string.Equals(textValue,"UniProduction Rule"))
            {
                uniRule = false;
            }
            else if(string.Equals(textValue,"Birth Rule"))
            {
                randomBirthsRule = false;
            }
        }
    }

    void OnButtonClickStart()
    {
        Text label = startButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Start"))
        {
            StartSimulation = true;
        }
    }

    void OnButtonClickStop()
    {
        Text label = stopButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Stop"))
        {
            StartSimulation = false;
        }
    }

    void OnButtonClickClear()
    {
        Text label = clearButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Clear"))
        {
            ClearGrid();
        }
    }

    void OnButtonClickRandom()
    {
        Text label = randomButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Random"))
        {
            ClearGrid();
            RandomConfiguration();
        }
    }

    void OnButtonClickReflector()
    {
        Text label = reflectorButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Reflector"))
        {
            ClearGrid();
            Reflector();
        }
    }

    void OnButtonClickGun()
    {
        Text label = gunButton.GetComponentInChildren<Text>();
        string textValue = label.text;
        if(string.Equals(textValue,"Glider Gun"))
        {
            ClearGrid();
            GliderGun();
        }
    }

    void OnEndEdit(string text)
    {
        int value = int.Parse(text); 
        numGenerations = value;
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

    Cell RandomNeighbour(int x, int y)
    {
        int xoffset = 0;
        int yoffset = 0;
        while((xoffset == 0) && (yoffset == 0)){
            xoffset = Random.Range(-1, 1);
            yoffset = Random.Range(-1, 1);
        }
        if(xoffset + x < 0 || xoffset+x > SCREEN_WIDTH || yoffset+y < 0 || yoffset+y >SCREEN_HEIGHT){
            Cell cell = grid[x,y];
             return cell;
        }else{
            Cell cell = grid[x+xoffset,y+yoffset];
             return cell;
        }
    }

    void UniReproduction() 
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {           
                Cell cell = grid[x, y];
                
                if (cell.isAlive && (cell.numNeighbours == 0))
                {
                    int number = Random.Range(1,4);
                    if( number == 3){
                        Cell neighbour1 = RandomNeighbour(x,y);
                        Cell neighbour2 = RandomNeighbour(x,y);
                        neighbour1.SetAlive(true);
                        neighbour2.SetAlive(true);
                        
                    }
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
