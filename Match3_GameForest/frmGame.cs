using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Match3_GameForest
{
    public partial class frmGame : Form
    {
        //глобальные переменные
        const int TileSize = 64; //высота и ширина клетки
        const int BoardOffset = 64; //расстояние от левой и верхней границы
        
        List<Point> GlobalDeleteList;
        List<int> ColumnsToDrop;
        List<int> UniqueValuesToDelete;
        List<Label> PointLabelList;

        Point ClickOne, ClickTwo, Difference;
        
        Tile SelectedTileMarker;

        int Score, AnimationTickCounter, AnimationLength, TimeLeft;
        

        
        Board mBoard;
        int[,] IntArray;
        
        


        enum State
        {
            Play,
            Swapping,
            SwappingBack,
            Deleting,
            Dropping,
        };

        State GameState = State.Play;

        

        public frmGame()
        {
            InitializeComponent();
                                

            IntArray = new int[8, 8];

            mBoard = new Board(8, 8, TileSize, BoardOffset, IntArray);
            PointLabelList = new List<Label>();
            GlobalDeleteList = new List<Point>();

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    this.Controls.Add(mBoard.Tiles[i, j]);

            Random r = new Random();
            int RandomNum = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    do
                    {
                        do
                        {
                            RandomNum = r.Next(1, 6);
                        } while (IntArray[i, j] == RandomNum);
                        IntArray[i, j] = RandomNum;
                    } while (CheckForMatch(i, j, true));

                }
            }

            ClickOne = new Point(-1, -1);
            ClickTwo = new Point(-1, -1);

            Score = 0;
            lblScore.Text = "0";

            TimeLeft = 60;

            mBoard.SetAllImages();

        }

       
        private void frmGame_MouseClick(object sender, MouseEventArgs e)
        {
            

            if (e.X > BoardOffset && e.X < TileSize * mBoard.Columns + BoardOffset &&
                    e.Y > BoardOffset && e.X < TileSize * mBoard.Rows + BoardOffset)
            {


                //какую плитку пользователь выбрал
                Point Temp = new Point((e.X - BoardOffset) / TileSize, (e.Y - BoardOffset) / TileSize);

                
                if (ClickOne == new Point(-1, -1))
                {
                    
                    ClickOne = Temp;

                    //добавляет SelectedTileMarker к выбранной плитке 
                    SelectedTileMarker = new Tile
                    {
                        Size = new Size(64, 64),
                        Image = Sprites.WhiteBorder,
                        BackColor = Color.Transparent
                    };
                    mBoard.Tiles[ClickOne.Y, ClickOne.X].Controls.Add(SelectedTileMarker);
                    SelectedTileMarker.BringToFront();
                }

                else
                {
                    //сохраняем как второй клик и удаляем SelectedTileMarker
                    ClickTwo = Temp;
                    SelectedTileMarker.Dispose();

                    //проверить есть ли рядом
                    if ((Math.Abs(ClickOne.X - ClickTwo.X) == 1 && ClickOne.Y == ClickTwo.Y) ||
                        (Math.Abs(ClickOne.Y - ClickTwo.Y) == 1 && ClickOne.X == ClickTwo.X))
                    {
                        //swap
                        PreSwap(true);
                    }
                    else
                    {
                        //обнуляем первый клик
                        ClickOne = new Point(-1, -1);
                    }


                }
            }
        }

        private bool CheckForMatch(int Row, int Col, bool IsReseting)
        {
            if (CheckForMatchDirectional(Row, Col, IsReseting, true) | CheckForMatchDirectional(Row, Col, IsReseting, false))
                return true;
            else
                return false;
        }

        private bool CheckForMatchDirectional(int Row, int Col, bool IsReseting, bool IsCheckingVertically) //checks for a match in a specific direction
        {
            //значения для поиска
            int ValueToFind = IntArray[Row, Col];

            //начинаем MiniDeleteList
            List<Point> MiniDeleteList = new List<Point>
            {
                new Point(Col, Row)
            };

            if (IsCheckingVertically) //проверка по вертикали
            {
                
                int RowToCheck = Row - 1;
                while (RowToCheck >= 0 && IntArray[RowToCheck, Col] == ValueToFind) 
                {
                    //каждый найденный добавить в лист
                    MiniDeleteList.Add(new Point(Col, RowToCheck));
                    RowToCheck--;
                }

                
                RowToCheck = Row + 1;
                while (RowToCheck <= 7 && IntArray[RowToCheck, Col] == ValueToFind)
                {
                    MiniDeleteList.Add(new Point(Col, RowToCheck));
                    RowToCheck++;
                }
            }
            else //проверка по горизонтали
            {
                
                int ColToCheck = Col - 1;
                while (ColToCheck >= 0 && IntArray[Row, ColToCheck] == ValueToFind)
                {
                    MiniDeleteList.Add(new Point(ColToCheck, Row));
                    ColToCheck--;
                }

                
                ColToCheck = Col + 1;
                while (ColToCheck <= 7 && IntArray[Row, ColToCheck] == ValueToFind)
                {
                    MiniDeleteList.Add(new Point(ColToCheck, Row));
                    ColToCheck++;
                }
            }

            if (MiniDeleteList.Count >= 3) //если найдено 3 или более вернуть true
            {
                if (IsReseting == false) //если метод вызывается во время игры
                {
                    //добавить все точки для удаления в глобальный список
                    GlobalDeleteList.AddRange(MiniDeleteList);

                    
                    foreach (Point ThisPoint in MiniDeleteList)
                        mBoard.Tiles[ThisPoint.Y, ThisPoint.X].PointValue = 1 * MiniDeleteList.Count;
                }

                return true;
            }
            else
                return false;
        }

        private void PreSwap(bool IsFirstSwap) //до анимации перемещения
        {
            //находим разницу между двумя кликами
            Difference.X = ClickOne.X - ClickTwo.X;
            Difference.Y = ClickOne.Y - ClickTwo.Y;

            //запускаем анимацию обмена или возврата
            if (IsFirstSwap)
                StartNewAnimation(State.Swapping, TileSize / 2);
            else
                StartNewAnimation(State.SwappingBack, TileSize / 2);
        }

        private void PostSwap(bool FirstSwap) //после анимации перемещения
        {
            //выполнить обмен
            int Temp = IntArray[ClickOne.Y, ClickOne.X];
            IntArray[ClickOne.Y, ClickOne.X] = IntArray[ClickTwo.Y, ClickTwo.X];
            IntArray[ClickTwo.Y, ClickTwo.X] = Temp;

            //переназначить расположение изображений 
            mBoard.Tiles[ClickOne.Y, ClickOne.X].Location = new Point(BoardOffset + ClickOne.X * TileSize, BoardOffset + ClickOne.Y * TileSize);
            mBoard.Tiles[ClickTwo.Y, ClickTwo.X].Location = new Point(BoardOffset + ClickTwo.X * TileSize, BoardOffset + ClickTwo.Y * TileSize);
            mBoard.SetNewImage(ClickOne.Y, ClickOne.X);
            mBoard.SetNewImage(ClickTwo.Y, ClickTwo.X);

            if (FirstSwap)
            {
                //проверить на совпадение
                if (CheckForMatch(ClickOne.Y, ClickOne.X, false) | CheckForMatch(ClickTwo.Y, ClickTwo.X, false))
                {
                    //если совпадение найдено, удаляем поля
                    PreDeleteTiles();
                }
                else
                    PreSwap(false); //если нет, возвращаем на место
            }
            else
            {
                //если совпадение найдено, обнуляем первый клик
                ClickOne = new Point(-1, -1);
                GameState = State.Play;
            }

        }

        private void PreDeleteTiles() //перед удалением
        {
            
            GlobalDeleteList = GlobalDeleteList.Distinct().ToList();

            //создаем новый список значений который будет удален
            UniqueValuesToDelete = new List<int>();
            foreach (Point Point in GlobalDeleteList)
                UniqueValuesToDelete.Add(IntArray[Point.Y, Point.X]);
            UniqueValuesToDelete = UniqueValuesToDelete.Distinct().ToList();

            //запуск анимации удаления
            StartNewAnimation(State.Deleting, 16);
        }

        private void PostDeleteTiles() //после удаления
        {
            
            foreach (Point ThisPoint in GlobalDeleteList)
            {
                IntArray[ThisPoint.Y, ThisPoint.X] = 0; //показать пустые места

                
                int ThisPointValue = (int)(mBoard.Tiles[ThisPoint.Y, ThisPoint.X].PointValue);

                Score += ThisPointValue;
                
               
               
                
                mBoard.SetNewImage(ThisPoint.Y, ThisPoint.X);
            }


            lblScore.Text = Score.ToString();

            ColumnsToDrop = new List<int>(GlobalDeleteList.Select(x => x.X).Distinct().ToList());  
            GlobalDeleteList = new List<Point>(); 
            PreDropColumns(); 
        }

        

        private void PreDropColumns() //перед анимацией падения
        {
            
            StartNewAnimation(State.Dropping, TileSize);
        }

        private void PostDropColumns() //после анимации падения
        {
            
            foreach (Label PointLabel in PointLabelList)
                PointLabel.Dispose();
            PointLabelList = new List<Label>();

            Random r = new Random();
            List<Point> MovedTilesToCheck = new List<Point>(); 

            foreach (int Col in ColumnsToDrop)
            {
                int EmptySpacesInColumn = 0;

                
                for (int Row = 0; Row < 8; Row++)
                {
                    mBoard.Tiles[Row, Col].Location = new Point(BoardOffset + Col * TileSize, BoardOffset + Row * TileSize);
                    if (IntArray[Row, Col] == 0)
                        EmptySpacesInColumn++;
                }

               
                for (int Row = 7; Row >= 0; Row--)
                {
                    if (Row >= EmptySpacesInColumn) 
                    {
                        if (IntArray[Row, Col] == 0) 
                        {
                            for (int UpperRow = Row - 1; UpperRow >= 0; UpperRow--)
                            {
                                if (IntArray[UpperRow, Col] != 0)
                                {
                                    IntArray[Row, Col] = IntArray[UpperRow, Col];
                                    IntArray[UpperRow, Col] = 0;
                                    
                                    mBoard.SetNewImage(Row, Col);
                                    MovedTilesToCheck.Add(new Point(Col, Row));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        
                        IntArray[Row, Col] = r.Next(1, 6);
                        MovedTilesToCheck.Add(new Point(Col, Row));
                        mBoard.SetNewImage(Row, Col);
                    }
                }
            }
            CheckNewTiles(MovedTilesToCheck);
        }

        private void tmrLeft_Tick(object sender, EventArgs e)
        {
            if (TimeLeft > 0)
            {
                TimeLeft = TimeLeft - 1;
                lblTimeLeft.Text = TimeLeft + " second";
            }
            else
            {
                tmrLeft.Enabled = false;
                lblTimeLeft.Text = "Time's up!!!";
                frmEnd NewWindow;
                NewWindow = new frmEnd();
                NewWindow.ShowDialog();
                NewWindow.Dispose();
                this.Close();
            }
        }

        private void CheckNewTiles(List<Point> TilesToCheck) //проверка перемещенных плиток
        {
            
            foreach (Point Tile in TilesToCheck)
                CheckForMatch(Tile.Y, Tile.X, false);
           
            TilesToCheck = new List<Point>();

            if (GlobalDeleteList.Count > 0)
            {
                
                
                
                PreDeleteTiles();
            }
            else
            {
                
                ClickOne = new Point(-1, -1);
                GameState = State.Play;
                
            }
        }

        private Bitmap ChangeOpacity(Image Image, int Alpha) 
        {
           
            Bitmap Original = new Bitmap(Image);
            Bitmap NewImage = new Bitmap(Image.Width, Image.Height);
            Color OriginalColor;
            Color NewColor;

           
            for (int i = 0; i < Image.Width; i++)
                for (int j = 0; j < Image.Height; j++)
                {
                   
                    OriginalColor = Original.GetPixel(i, j);
                    NewColor = Color.FromArgb(Alpha, OriginalColor.R, OriginalColor.G, OriginalColor.B);
                    NewImage.SetPixel(i, j, NewColor);
                }

           
            return NewImage;
        }

        private void StartNewAnimation(State AnimationType, int Length)
        {
            //обнуляем и запускаем анимацию
            AnimationTickCounter = 0;
            GameState = AnimationType;
            AnimationLength = Length;

            
            tmrAnimation.Start();
        }

        private void tmrAnimation_Tick(object sender, EventArgs e)
        {
            if (GameState == State.Swapping || GameState == State.SwappingBack)
            {
                //приближаем каждую плитку на 2 пикселя ближе к исходной позиции другой
                mBoard.Tiles[ClickOne.Y, ClickOne.X].Left -= Difference.X * 2;
                mBoard.Tiles[ClickTwo.Y, ClickTwo.X].Left += Difference.X * 2;
                mBoard.Tiles[ClickOne.Y, ClickOne.X].Top -= Difference.Y * 2;
                mBoard.Tiles[ClickTwo.Y, ClickTwo.X].Top += Difference.Y * 2;
                AnimationTickCounter++;

                //если анимация завершена, остановить таймер и запустить PostSwap
                if (AnimationTickCounter == AnimationLength)
                {
                    tmrAnimation.Stop();
                    if (GameState == State.Swapping)
                        PostSwap(true);
                    else
                        PostSwap(false);
                }
            }

            else if (GameState == State.Deleting)
            {
               
                Image NewBubble1 = null;
                Image NewBubble2 = null;
                Image NewBubble3 = null;
                Image NewBubble4 = null;
                Image NewBubble5 = null;


               
                int Alpha = (255 - AnimationTickCounter * 16 - 1);

               
                foreach (int Value in UniqueValuesToDelete)
                {
                    if (Value == 1)
                        NewBubble1 = ChangeOpacity(Sprites.Bubble1, Alpha);
                    else if (Value == 2)
                        NewBubble2 = ChangeOpacity(Sprites.Bubble2, Alpha);
                    else if (Value == 3)
                        NewBubble3 = ChangeOpacity(Sprites.Bubble3, Alpha);
                    else if (Value == 4)
                        NewBubble4 = ChangeOpacity(Sprites.Bubble4, Alpha);
                    else if (Value == 5)
                        NewBubble5 = ChangeOpacity(Sprites.Bubble5, Alpha);
                    
                }
               
                foreach (Point Point in GlobalDeleteList)
                {
                    if (IntArray[Point.Y, Point.X] == 1)
                        mBoard.Tiles[Point.Y, Point.X].Image = NewBubble1;
                    else if (IntArray[Point.Y, Point.X] == 2)
                        mBoard.Tiles[Point.Y, Point.X].Image = NewBubble2;
                    else if (IntArray[Point.Y, Point.X] == 3)
                        mBoard.Tiles[Point.Y, Point.X].Image = NewBubble3;
                    else if (IntArray[Point.Y, Point.X] == 4)
                        mBoard.Tiles[Point.Y, Point.X].Image = NewBubble4;
                    else if (IntArray[Point.Y, Point.X] == 5)
                        mBoard.Tiles[Point.Y, Point.X].Image = NewBubble5;
                    
                }

                AnimationTickCounter++;

                
                if (AnimationTickCounter == AnimationLength)
                {
                    tmrAnimation.Stop();
                    PostDeleteTiles();
                }
            }
            else if (GameState == State.Dropping)
            {
               
                foreach (int Col in ColumnsToDrop)
                {
                    
                    int EmptySpaceCounter = 0;
                    for (int Row = 7; Row >= 0; Row--)
                    {
                        if (IntArray[Row, Col] == 0)
                            EmptySpaceCounter++;
                        else
                            mBoard.Tiles[Row, Col].Top += 2 * EmptySpaceCounter;
                    }
                }

                AnimationTickCounter++;

               
                if (AnimationTickCounter == AnimationLength / 2)
                {
                    tmrAnimation.Stop();
                    PostDropColumns();
                }
            }


        }

       
    }
}
