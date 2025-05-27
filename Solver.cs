using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace AlmoustCourseWork
{
    public class Solver
    {
        private const int MaxQueens = ChessBoard.MaxQueens;
        public static double LastTimeRBFS { get; private set; }
        public static double LastMemoryRBFS { get; private set; }
        public static double LastTimeAStar { get; private set; }
        public static double LastMemoryAStar { get; private set; }

        public static double DeployedAStar { get; private set; }
        public static double DeployedRBFS { get; private set; }

        public static int GeneratedNodesAStar { get; private set; }
        public static int GeneratedNodesRBFS { get; private set; }


        public static List<int[]> FullTrace { get; private set; } = new List<int[]>();

        public static int[] SolveRBFS(int[,] board)
        {

            Node root = FindFirstQueen(board);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memBefore = GC.GetTotalMemory(false);


            DeployedRBFS = 0;
            GeneratedNodesRBFS = 0;
            FullTrace.Clear();

            var sw = Stopwatch.StartNew();
            var (solutionNode, _) = RBFS(root, int.MaxValue);
            sw.Stop();

            long memAfter = GC.GetTotalMemory(false);
            LastTimeRBFS = sw.Elapsed.TotalMilliseconds;
            LastMemoryRBFS = memAfter - memBefore;


            if (solutionNode != null)
                return solutionNode.State;
            else
                return null;
        }

        private static (Node, int) RBFS(Node node, int fLimit)
        {
            DeployedRBFS++;
            FullTrace.Add((int[])node.State.Clone());

            if (node.Depth == MaxQueens)
                return (node, 0);

            var successors = new List<Node>();

            int row = node.Depth;
            for (int col = 0; col < MaxQueens; col++)
            {
                if (IsSafe(node.State, row, col))
                {
                    var newState = (int[])node.State.Clone();
                    newState[row] = col;
                    int g = node.Depth + 1;
                    int h = Heuristic(newState, g);
                    var child = new Node(newState, g, h);
                    GeneratedNodesRBFS++;
                    successors.Add(child);
                }
            }

            if (successors.Count == 0)
                return (null, int.MaxValue);

            foreach (var s in successors)
                s.F = Math.Max(s.F, node.F);

            while (successors.Count > 0)
            {
                successors.Sort((a, b) => a.F.CompareTo(b.F));
                var best = successors[0];

                if (best.F > fLimit)
                    return (null, best.F);

                int alternative = successors.Count > 1 ? successors[1].F : int.MaxValue;
                var (solutionNode, F) = RBFS(best, Math.Min(fLimit, alternative));
                best.F = F;
                if (solutionNode != null)
                    return (solutionNode, F);
            }

            return (null, int.MaxValue);
        }

        public static int[] SolveAStar(int[,] board)
        {
            Node root = FindFirstQueen(board);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memBefore = GC.GetTotalMemory(false);

            DeployedAStar = 0;
            GeneratedNodesAStar = 0;
            FullTrace.Clear();

            var sw = Stopwatch.StartNew();
            var result = AStar(root);
            sw.Stop();

            long memAfter = GC.GetTotalMemory(false);
            LastTimeAStar = sw.Elapsed.TotalMilliseconds;
            LastMemoryAStar = memAfter - memBefore;

            return result;
        }

        private static int[] AStar(Node root)
        {
            var open = new PriorityQueue();
            var closed = new HashSet<string>();

            open.Enqueue(root);

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                DeployedAStar++;

                FullTrace.Add((int[])current.State.Clone());
                string key = string.Join(",", current.State);
                if (!closed.Add(key))
                    continue;

                bool allPlaced = true;
                for (int i = 0; i < MaxQueens; i++)
                    if (current.State[i] == -1)
                    {
                        allPlaced = false;
                        break;
                    }
                if (allPlaced)
                    return current.State;

                int row = current.Depth;
                for (int col = 0; col < MaxQueens; col++)
                {
                    if (!IsSafe(current.State, row, col))
                        continue;

                    var newState = (int[])current.State.Clone();
                    newState[row] = col;
                    int g = current.Depth + 1;
                    int h = Heuristic(newState, g);
                    var successor = new Node(newState, g, h);
                    GeneratedNodesAStar++;

                    string succKey = string.Join(",", newState);
                    if (closed.Contains(succKey))
                        continue;

                    open.Enqueue(successor);
                }
            }

            return null;
        }

        private static Node FindFirstQueen(int[,] board)
        {
            int[] State = new int[MaxQueens];
            for (int i = 0; i < MaxQueens; i++)
            {
                State[i] = -1;
            }

            int firstQueenCol = -1;
            for (int col = 0; col < MaxQueens; col++)
            {
                if (board[0, col] == 1)
                {
                    firstQueenCol = col;
                    break;
                }
            }

            Node root; 

            if (firstQueenCol != -1)
            {
                //Якщо є ферзь у першому рядку — встановлюємо його
                State[0] = firstQueenCol;
                int h = Heuristic(State, 1);
                root = new Node(State, 1, h);
            }
            else
            {
                //Якщо немає — починаємо з нуля
                int h = Heuristic(State, 0);
                root = new Node(State, 0, h);
            }

            return root;
        }

        private static bool IsSafe(int[] state, int row, int col)
        {
            for (int i = 0; i < row; i++)
                if (state[i] == -1) continue; //Пропускаємо незаповнені
                if (state[i] == col || Math.Abs(state[i] - col) == row - i)
                    return false;
            return true;
        }

        private static int Heuristic(int[] state, int depth)
        {
            // Для кожного ще не заповненого рядка рахуємо, скільки у нього безпечних стовпців, і беремо мінімальне
            int minOptions = MaxQueens;
            for (int row = depth; row < MaxQueens; row++)
            {
                int options = 0;
                for (int col = 0; col < MaxQueens; col++)
                    if (IsSafe(state, row, col))
                        options++;
                minOptions = Math.Min(minOptions, options);
                if (minOptions == 0) break;  // гілка мертва

            }
            // Чим менше варіантів — тим більша евристика
            return MaxQueens - minOptions - depth;
        }
    }
}
