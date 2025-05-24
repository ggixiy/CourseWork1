using System;
using System.Collections.Generic;

namespace AlmoustCourseWork
{
    public class ChessBoard
    {
        public int[,] Board { get; private set; }
        public int QueenCount { get; private set; }
        public const int MaxQueens = 8;
        private static Random rand = new Random();

        public ChessBoard()
        {
            Board = new int[MaxQueens, MaxQueens];
            QueenCount = 0;
        }

        public bool ToggleCell(int row, int col)
        {

            if (Board[row, col] == 1)
            {
                Board[row, col] = 0;
                QueenCount--;
                return false;
            }

            if (QueenCount >= MaxQueens)
                throw new InvalidOperationException("Cannot place more than 8 queens.");

            Board[row, col] = 1;
            QueenCount++;
            return true;
        }

        public void GenerateRandom()
        {
            Clear(0);
            for(int i = 0; i < MaxQueens; i++)
            {
                int c = rand.Next(MaxQueens);
                Board[i, c] = 1;
                QueenCount++;
            }
        }

        public void Clear(int mode)
        {
            for (int i = 0; i < MaxQueens; i++)
                for (int j = 0; j < MaxQueens; j++)
                    Board[i, j] = 0;
            if (mode == 0) QueenCount = 0;
        }

        public bool HasFullQueens => QueenCount == MaxQueens;

        public List<int[]> GetQueenPositions()
        {
            var list = new List<int[]>();
            for (int i = 0; i < MaxQueens; i++)
                for (int j = 0; j < MaxQueens; j++)
                    if (Board[i, j] == 1)
                        list.Add(new[] { i, j });
            return list;
        }
    }
}
