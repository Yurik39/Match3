using System;
using System.Drawing;


namespace Match3_GameForest
{
    class Board
    {
        //поля
        private readonly Tile[,] mTiles;
        private readonly int mRows, mColumns, mTileSize;
        private readonly int[,] mIntArray;

        public Board(int Rows, int Columns, int TileSize, int BoardOffset, int[,] IntArray)
        {
            this.mRows = Rows;
            this.mColumns = Columns;
            this.mTileSize = TileSize;
            this.mIntArray = IntArray;

            mTiles = new Tile[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    mTiles[i, j] = new Tile
                    {
                        Size = new Size(TileSize, TileSize),
                        Location = new Point(j * TileSize + BoardOffset, i * TileSize + BoardOffset),
                        BackColor = Color.Black
                    };
                }
            }
        }

        //получаем количество элементов
        public Tile[,] Tiles 
        {
            get { return mTiles; }
        }

        public int Rows 
        {
            get { return mRows; }
        }

        public int Columns 
        {
            get { return mColumns; }
        }

        public void SetAllImages()
        {
            for (int i = 0; i < mRows; i++)
                for (int j = 0; j < mColumns; j++)
                    SetNewImage(i, j);
        }

        public void SetNewImage(int Row, int Columns)
        {
            Image PicToUse;
            if (mIntArray[Row, Columns] == 1)
                PicToUse = Sprites.Bubble1;
            else if (mIntArray[Row, Columns] == 2)
                PicToUse = Sprites.Bubble2;
            else if (mIntArray[Row, Columns] == 3)
                PicToUse = Sprites.Bubble3;
            else if (mIntArray[Row, Columns] == 4)
                PicToUse = Sprites.Bubble4;
            else if (mIntArray[Row, Columns] == 5)
                PicToUse = Sprites.Bubble5;
            else PicToUse = Sprites.BLANK_ICON; //пустой

            mTiles[Row,Columns].Image = PicToUse;
        }
    }
}
