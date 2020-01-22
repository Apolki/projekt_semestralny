using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Miner
{
    public partial class Form1 : Form
    {
        int width = 10;
        int height = 10;    
        int offset = 30;
        int bombPercent = 5;
        bool isFirstClick = true;
        FieldButton[,] field;
        int cellsOpened = 0;
        int bombs = 0;
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Jest defoltną samą pierwszą metodą, która ustawia parametry 2chwymirowej macierzy i tworzy pole c:
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">event</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            field = new FieldButton[width, height];
            GenerateField();
        }
        /// <summary>
        /// Metoda, która wlaśnie tworzy z pomocą 2ch pętl przyciski a uzupewnia wirtualnie 
        /// naszą 2-wymirawo masyw przyciskami 
        /// </summary>
        public void GenerateField()
        {
            Random random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    FieldButton newButton = new FieldButton();
                    newButton.Location = new Point(x * offset, y * offset);
                    newButton.Size = new Size(offset, offset);
                    newButton.isClickable = true;
                    if (random.Next(0, 100) <= bombPercent)
                    {
                        newButton.isBomb = true;
                        bombs++;
                    }
                    newButton.xCoord = x;
                    newButton.yCoord = y;
                    Controls.Add(newButton);
                    newButton.MouseUp += new MouseEventHandler(FieldButtonClick);
                    field[x, y] = newButton;
                }
            }
        }
        /// <summary>
        /// Metoda, która sprawdza jakim przyciskiem był przycisknięty przycisk i
        /// wyłowa rużne metody w zależności od rużnych faktorów i po przycisku sprawdza czy wygraliśmy
        /// </summary>
        /// <param name="sender">to objeck, czyli sam nasz przycisk</param>
        /// <param name="e">event, czyli jak przycisknieliśmy</param>
        void FieldButtonClick(object sender, MouseEventArgs e)
        {
            FieldButton clickedButton = (FieldButton)sender;
            if (e.Button == MouseButtons.Left && clickedButton.isClickable)
            {
                if (clickedButton.isBomb)
                {
                    if (isFirstClick)
                    {
                        clickedButton.isBomb = false;
                        isFirstClick = false;
                        bombs--;
                        OpenRegion(clickedButton.xCoord, clickedButton.yCoord,clickedButton);
                    }
                    else
                    {
                        Explode();
                    }

                }
                else
                {
                    EmptyFieldButtonClick(clickedButton);
                }
                isFirstClick = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                clickedButton.isClickable = !clickedButton.isClickable;
                if (!clickedButton.isClickable)
                {
                    clickedButton.Text = "B";
                }
                else
                {
                    clickedButton.Text = "";
                }
            }
            CheckWin();
        }
        /// <summary>
        /// metoda przegrawania i wykrywania wszystkin bomb na polu
        /// </summary>
        void Explode()
        {
            foreach (FieldButton button in field)
            {
                if (button.isBomb)
                {
                    button.Text = "*";
                }
            }
            MessageBox.Show("You lost :(");
            Application.Restart();
        }
        /// <summary>
        /// metoda wykrywania koordynatów danego przycisku
        /// </summary>
        /// <param name="clickedButton">object, czyli sam przyczisk</param>
        void EmptyFieldButtonClick(FieldButton clickedButton)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (field[x, y] == clickedButton)
                    {
                        OpenRegion(x, y, clickedButton);
                    }
                }
            }
        }
        /// <summary>
        /// Metoda, która otwira danej przyczisknientej klatki u sprawdza ile bomb dowkola
        /// także sprawdza ktore klatki można otwirać "wodospadem" wszystkich pustych klatek bez 
        /// sąsiędnich bomb z pomocą "kolejki"
        /// </summary>
        /// <param name="xCoord">integer X koordynaty danego przyciska</param>
        /// <param name="yCoord">integer Y koordynaty danego przyciska</param>
        /// <param name="clickedButton">Object, czyli sam przycisk</param>
        void OpenRegion(int xCoord, int yCoord, FieldButton clickedButton)
        {
            Queue<FieldButton> queue = new Queue<FieldButton>();
            queue.Enqueue(clickedButton);
            clickedButton.wasAdded = true;
            while (queue.Count > 0)
            {
                FieldButton currentCell = queue.Dequeue();
                OpenCell(currentCell.xCoord, currentCell.yCoord, currentCell);
                cellsOpened++;
                if (CountBombsAround(currentCell.xCoord, currentCell.yCoord) == 0)
                {
                    for (int y = currentCell.yCoord - 1; y <= currentCell.yCoord + 1; y++)
                    {
                        for (int x = currentCell.xCoord - 1; x <= currentCell.xCoord + 1; x++)
                        {
                            if(x==currentCell.xCoord && y == currentCell.yCoord)
                            {
                                continue;
                            }
                            if (x >= 0 && x < width && y >= 0 && y < height)
                            {
                                if (!field[x, y].wasAdded)
                                {
                                    queue.Enqueue(field[x, y]);
                                    field[x, y].wasAdded = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// otwieranie danej klatki i zapisywanie ilości bomb w sąsiadnich klatkach
        /// </summary>
        /// <param name="x">X coordynata klatki</param>
        /// <param name="y">Y coordynata klatki</param>
        /// <param name="clickedButton">object, czyli sama klatka/przycisk </param>
        void OpenCell(int x, int y, FieldButton clickedButton)
        {
            int bombsAround = CountBombsAround(x,y);
            if (bombsAround == 0)
            {

            }
            else
            {
                clickedButton.Text = "" + bombsAround;
            }
            clickedButton.Enabled = false;
        }
        /// <summary>
        /// metoda, która wylicza ilość bomb w sąsiadnich klatkach
        /// </summary>
        /// <param name="xCoord">X coordynata klatki</param>
        /// <param name="yCoord">Y coordynata klatki</param>
        /// <returns></returns>
        int CountBombsAround(int xCoord, int yCoord)
        {
            int bombsAround = 0;
            for (int x = xCoord - 1; x <= xCoord + 1; x++)
            {
                for (int y = yCoord - 1; y <= yCoord + 1; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        if (field[x, y].isBomb == true)
                        {
                            bombsAround++;
                        }
                    }
                }
            }
            return bombsAround;
        }
        /// <summary>
        /// metoda, która sprawdza, czy odkryliśmy wszystkie klatki bez bomb i jeżeli tak, to wygrywamy!
        /// conglatulations c:
        /// </summary>
        void CheckWin()
        {
            int cells = width * height;
            int emptyCells = cells - bombs;
            if (cellsOpened >= emptyCells)
            {
                MessageBox.Show("You won! c:");
            }
        }
    }
    public class FieldButton : Button
    {
        public bool isBomb;
        public bool isClickable;
        public bool wasAdded;
        public int xCoord;
        public int yCoord;
    }
}