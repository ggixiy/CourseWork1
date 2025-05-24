using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AlmoustCourseWork
{
    public class EightQueensProblemForm : Form
    {
        private Button[,] boardButtons;
        private ChessBoard boardModel;
        private List<int[]> solutionSteps;
        private List<int[]> initialPositions;
        private int currentStepIndex;
        private bool isHistoryViewMode = false;
       

        private Button btnRBFS, btnAStar, btnClear, btnRandom, btnPrev, btnNext, btnShowAll, btnSave, btnMetrics;
        private const int MaxQueens = ChessBoard.MaxQueens;

        public EightQueensProblemForm()
        {
            boardModel = new ChessBoard();
            solutionSteps = new List<int[]>();
            initialPositions = new List<int[]>();
            currentStepIndex = -1;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.BackColor = System.Drawing.Color.AliceBlue;
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            this.Width = 700;
            this.Height = 750;
            this.Text = "8 Queens Problem";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int cellSize = 60;
            int offset = 30;
            int boardSize = cellSize * MaxQueens;

            // Панель для шахівниці
            var boardPanel = new Panel
            {
                Width = boardSize + 2,
                Height = boardSize + 2,
                Left = (this.ClientSize.Width - boardSize) / 2,
                Top = 20,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(boardPanel);

            boardButtons = new Button[MaxQueens, MaxQueens];

            for (int i = 0; i < MaxQueens; i++)
            {
                for (int j = 0; j < MaxQueens; j++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = j * cellSize,
                        Top = i * cellSize,
                        BackColor = ((i + j) % 2 == 0) ? Color.White : Color.LightGray,
                        Font = new Font("Calibri", 24, FontStyle.Regular),
                        Tag = new int[] { i, j },
                        FlatStyle = FlatStyle.Flat
                    };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Click += BoardButtonClick;
                    boardButtons[i, j] = btn;
                    boardPanel.Controls.Add(btn);
                }
            }

            //Горизонтальні координати (A–H) зверху і знизу
            for (int col = 0; col < MaxQueens; col++)
            {
                var lblTop = new Label
                {
                    Text = ((char)('a' + col)).ToString(),
                    Width = cellSize,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Left = boardPanel.Left + col * cellSize,
                    Top = boardPanel.Top - 20,
                    Font = new Font("Calibri", 12, FontStyle.Bold)
                };
                var lblBottom = new Label
                {
                    Text = ((char)('a' + col)).ToString(),
                    Width = cellSize,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Left = boardPanel.Left + col * cellSize,
                    Top = boardPanel.Bottom,
                    Font = new Font("Calibri", 12, FontStyle.Bold)
                };
                this.Controls.Add(lblTop);
                this.Controls.Add(lblBottom);
            }

            //Вертикальні координати (1–8) зліва і справа
            for (int row = 0; row < MaxQueens; row++)
            {
                var lblLeft = new Label
                {
                    Text = (MaxQueens - row).ToString(),
                    Height = cellSize,
                    Width = 25,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Top = boardPanel.Top + row * cellSize,
                    Left = boardPanel.Left - 25,
                    Font = new Font("Calibri", 12, FontStyle.Bold)
                };
                var lblRight = new Label
                {
                    Text = (MaxQueens - row).ToString(),
                    Height = cellSize,
                    Width = 25,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Top = boardPanel.Top + row * cellSize,
                    Left = boardPanel.Right + 2,
                    Font = new Font("Calibri", 12, FontStyle.Bold)
                };
                this.Controls.Add(lblLeft);
                this.Controls.Add(lblRight);
            }

            //Розташування кнопок нижче
            int buttonTop = boardPanel.Bottom + 50;

            btnRBFS = new Button { Text = "RBFS", Width = 100, Left = 170, Top = buttonTop, BackColor = Color.White };
            btnAStar = new Button { Text = "A*", Width = 100, Left = 290, Top = buttonTop, BackColor = Color.White };
            btnClear = new Button { Text = "Reset", Width = 100, Left = 410, Top = buttonTop, BackColor = Color.White };
            btnRandom = new Button { Text = "Random arrangement", Width = 200, Left = 240, Top = buttonTop + 50, BackColor = Color.White };

            btnRBFS.Click += (s, e) => OnSolveRBFS();
            btnAStar.Click += (s, e) => OnSolveAStar();
            btnClear.Click += (s, e) => ResetBoard(0);
            btnRandom.Click += (s, e) => RandomizeBoard();

            this.Controls.AddRange(new Control[] { btnRBFS, btnAStar, btnClear, btnRandom });

            btnPrev = new Button { Text = "<", Width = 40, Left = btnRandom.Left - 50, Top = btnRandom.Top + 50, BackColor = Color.White, Enabled = false };
            btnNext = new Button { Text = ">", Width = 40, Left = btnRandom.Left + btnRandom.Width + 10, Top = btnRandom.Top + 50, BackColor = Color.White, Enabled = false };
            btnPrev.Click += (s, e) => PrevStep();
            btnNext.Click += (s, e) => NextStep();
            this.Controls.AddRange(new[] { btnPrev, btnNext });

            btnShowAll = new Button
            {
                Text = "Show all",
                Width = 200,
                Left = btnRandom.Left,
                Top = btnRandom.Top + 50,
                BackColor = Color.White,
                Enabled = false
            };
            btnShowAll.Click += (s, e) => ShowAll();
            this.Controls.Add(btnShowAll);

            btnSave = new Button
            {
                Text = "Save",
                Width = 100,
                Left = btnShowAll.Left + btnShowAll.Width + 10,
                Top = btnRandom.Top,
                BackColor = Color.White,
                Enabled = false
            };
            btnSave.Click += (s, e) => SaveSolutionToFile();
            this.Controls.Add(btnSave);

            btnMetrics = new Button
            {
                Text = "Complexity",
                Width = 100,
                Left = btnShowAll.Left - 110,
                Top = btnRandom.Top,
                BackColor = Color.White,
                Enabled = false
            };
            btnMetrics.Click += (s, e) => ShowMetrics();
            this.Controls.Add(btnMetrics);
        }

        private void BoardButtonClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var pos = (int[])btn.Tag;

            if (isHistoryViewMode)
            {
                MessageBox.Show("You cannot change positions of queens in the history view mode. Press Reset to exit this mode.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                bool placed = boardModel.ToggleCell(pos[0], pos[1]);
                if (placed)
                {
                    btn.BackColor = System.Drawing.Color.Pink;
                    btn.Text = "♕";
                }
                else
                {
                    btn.Text = "";
                    btn.BackColor = ((pos[0] + pos[1]) % 2 == 0)
                        ? System.Drawing.Color.White
                        : System.Drawing.Color.LightGray;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            ResetHistory();
        }

        private void RandomizeBoard()
        {
            boardModel.GenerateRandom();
            for (int i = 0; i < MaxQueens; i++)
                for (int j = 0; j < MaxQueens; j++)
                {
                    if (boardModel.Board[i, j] == 1)
                    {
                        boardButtons[i, j].BackColor = System.Drawing.Color.Pink;
                        boardButtons[i, j].Text = "♕";
                    }
                    else
                    {
                        boardButtons[i, j].BackColor = ((i + j) % 2 == 0)
                            ? System.Drawing.Color.White
                            : System.Drawing.Color.LightGray;
                        boardButtons[i, j].Text = "";
                    }
                }
            ResetHistory();
        }

        private void ResetBoard(int mode)
        {
            boardModel.Clear(mode);
            
            //Оновлюємо текст всіх клітинок
            foreach (var btn in boardButtons)
            {
                var pos = (int[])btn.Tag;
                btn.BackColor = ((pos[0] + pos[1]) % 2 == 0)
                    ? System.Drawing.Color.White
                    : System.Drawing.Color.LightGray;
                btn.Text = "";
            }

            //Скидаємо історію вирішення
            if (mode == 0)
            {
                isHistoryViewMode = false;
                ResetHistory();
            }
        }

        private void ResetHistory()
        {
            solutionSteps.Clear();
            initialPositions.Clear();
            currentStepIndex = -1;
            btnPrev.Enabled = false;
            btnNext.Enabled = false;
            btnShowAll.Enabled = false;
            btnSave.Enabled = false;
            btnMetrics.Enabled = false;
            btnRandom.Enabled = true;
        }

        private void OnSolveRBFS()
        {
            if (!boardModel.HasFullQueens)
            {
                MessageBox.Show("You have to place 8 queens!", "RBFS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            initialPositions = boardModel.GetQueenPositions();

            bool isAlreadySolved;

            isAlreadySolved = IsSolved();
            if (isAlreadySolved)
            {
                MessageBox.Show("The 8 queens puzzle is already solved.", "RBFS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }           

            var result = Solver.SolveRBFS(boardModel.Board);

            if (result == null)
            {
                MessageBox.Show("Solution was not found.", "RBFS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            BuildSolutionSteps();
            currentStepIndex = solutionSteps.Count - 1;
            btnShowAll.Enabled = true;
            btnSave.Enabled = true;
            btnMetrics.Enabled = true;
            btnRandom.Enabled = false;
            DisplayStep(solutionSteps.Count - 1);
        }

        private void OnSolveAStar()
        {
            if (!boardModel.HasFullQueens)
            {
                MessageBox.Show("You have to place 8 queens!", "A*", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            initialPositions = boardModel.GetQueenPositions();

            bool isAlreadySolved;

            isAlreadySolved = IsSolved();
            if (isAlreadySolved)
            {
                MessageBox.Show("The 8 queens puzzle is already solved.", "A*", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = Solver.SolveAStar(boardModel.Board);

            if (result == null)
            {
                MessageBox.Show("Solution was not found.", "A*", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            BuildSolutionSteps();
            currentStepIndex = solutionSteps.Count - 1;
            btnShowAll.Enabled = true;
            btnSave.Enabled = true;
            btnMetrics.Enabled = true;
            btnRandom.Enabled = false;
            DisplayStep(solutionSteps.Count - 1);
        }

        private void ShowMetrics()
        {
            string msg;
            if(Solver.LastMemoryRBFS == 0)
            {
                msg = $"Last metrics:\n\n" +
                         $"RBFS → There is no data about last usage of RBFS\n" +
                         $"A*   → Number of nodes: {Solver.GeneratedNodesAStar}, deployed nodes: {Solver.DeployedAStar} (time: {Solver.LastTimeAStar} ms, memory: {Solver.LastMemoryAStar} bytes)";
            }
            else if(Solver.LastMemoryAStar == 0)
            {
                msg = $"Last metrics:\n\n" +
                         $"RBFS → Number of nodes: {Solver.GeneratedNodesRBFS}, deployed nodes: {Solver.DeployedRBFS} (time: {Solver.LastTimeRBFS} ms, memory: {Solver.LastMemoryRBFS} bytes)\n" +
                         $"A* → There is no data about last usage of A*\n";
            }
            else
            {
                msg = $"Last metrics:\n\n" +
                         $"RBFS → Number of nodes: {Solver.GeneratedNodesRBFS}, deployed nodes: {Solver.DeployedRBFS} (time: {Solver.LastTimeRBFS} ms, memory: {Solver.LastMemoryRBFS} bytes)\n" +
                         $"A* → Number of nodes: {Solver.GeneratedNodesAStar}, deployed nodes: {Solver.DeployedAStar} (time: {Solver.LastTimeAStar} ms, memory: {Solver.LastMemoryAStar} bytes)";
            }
            MessageBox.Show(msg, "Metrics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool IsSolved()
        {
            int[] state = new int[MaxQueens];

            for (int i = 0; i < MaxQueens; i++)
            {
                bool found = false;
                for (int j = 0; j < MaxQueens; j++)
                {
                    if (boardButtons[i, j].Text == "♕")
                    {
                        if (found)
                        {
                            return false;//У рядку вже стоїть ферзь — другий бути не може
                        }
                        state[i] = j;
                        found = true;
                    }
                }

                if (!found)
                    return false; //Якщо хоч в одному рядку немає ферзя — не розв’язано
            }

            //Перевіряємо, що ферзі не атакують одне одного
            for (int i = 0; i < MaxQueens; i++)
            {
                for (int j = i + 1; j < MaxQueens; j++)
                {
                    if (state[i] == state[j] || Math.Abs(state[i] - state[j]) == Math.Abs(i - j))
                        return false; //атакують: одна колонка або одна діагональ
                }
            }

            return true;
        }

        private void BuildSolutionSteps()
        {
            solutionSteps.Clear();
            foreach (var state in Solver.FullTrace)
                solutionSteps.Add((int[])state.Clone());
        }

        private void DisplayStep(int stepIndex)
        {
            isHistoryViewMode = true;

            ResetBoard(1);
            foreach (var pos in initialPositions)
                boardButtons[pos[0], pos[1]].BackColor = System.Drawing.Color.Pink;

            var state = solutionSteps[stepIndex];
            for (int row = 0; row < MaxQueens; row++)
            {
                if (state[row] >= 0) //Перевірка на валідність колонки
                    boardButtons[row, state[row]].Text = "♕";           
            }

            currentStepIndex = stepIndex;
            btnShowAll.Enabled = (stepIndex < solutionSteps.Count - 1);
            btnPrev.Enabled = (stepIndex > 0);
            btnNext.Enabled = (stepIndex < solutionSteps.Count - 1);
        }

        private void PrevStep()
        {
            isHistoryViewMode = true;
            if (currentStepIndex > 0)
                DisplayStep(currentStepIndex - 1);
        }

        private void NextStep()
        {
            isHistoryViewMode = true;
            if (currentStepIndex < solutionSteps.Count - 1)
                DisplayStep(currentStepIndex + 1);
        }

        private void ShowAll()
        {
            isHistoryViewMode = true;
            ResetBoard(1);
            foreach (var pos in initialPositions)
                boardButtons[pos[0], pos[1]].BackColor = System.Drawing.Color.Pink;

            var finalState = solutionSteps.Last();
            for (int i = 0; i < MaxQueens; i++)
                boardButtons[i, finalState[i]].Text = "♕";

            currentStepIndex = solutionSteps.Count - 1;
            btnPrev.Enabled = true;
            btnNext.Enabled = false;
            btnShowAll.Enabled = false;
        }

        private void SaveSolutionToFile()
        {
            using (var dlg = new SaveFileDialog { Filter = "Text Files|*.txt", FileName = "solution.txt" })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var w = new StreamWriter(dlg.FileName))
                    {
                        var finalState = solutionSteps.Last();
                        for (int i = 0; i < MaxQueens; i++)
                        {
                            for (int j = 0; j < MaxQueens; j++)
                            {
                                w.Write(finalState[i] == j ? "♕ " : ". ");
                            }
                            w.WriteLine();
                        }
                    }

                    MessageBox.Show($"Solution is saved in:\n{dlg.FileName}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while saving file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
