using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        // The universe array
        bool[,] universe = new bool[10, 10];

        // The ScratchPad array
        bool[,] scratchPad = new bool[10, 10];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        //Misc.
        bool neighborCountOn = true;
        bool gridLinesOn = true;
        bool finiteOn = true;
        bool toroidalOn = false;
        string BoundryType = "Finite";
        bool HUDon = true;
        int cellsAlive = 0;

        public Form1()
        {
            InitializeComponent();

            //setting preferences
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            timer.Interval = Properties.Settings.Default.Timer;

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            // iterate through the universe, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // iterate through the universe, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // int count = count neighbor
                    int count = 0;
                    if (finiteOn == true)
                        count = CountNeighborsFinite(x, y);
                    else if (toroidalOn == true)
                        count = CountNeighborsToroidal(x, y);

                    // Apply the rules
                    // Rule 1
                    if (universe[x, y] == true && count < 2)
                        scratchPad[x, y] = false;
                    // Rule 2
                    if (universe[x, y] == true && count > 3)
                        scratchPad[x, y] = false;
                    // Rule 3
                    if (universe[x, y] == true && count == 2 || count == 3)
                        scratchPad[x, y] = true;
                    // Rule 4
                    if (universe[x, y] == false)    //crash error here problem with universe resizing and rules
                    {
                        if (count == 3)
                            scratchPad[x, y] = true;
                    }

                    //Cells alive count
                    if (universe[x, y] == true)
                        cellsAlive++;
                }
            }

            // copy from scratchpad to universe
            // Swap them...
            bool[,] temp = universe;
            universe = scratchPad;
            scratchPad = temp;

            //clear scratchpad
            for (int y = 0; y < scratchPad.GetLength(1); y++)
            {
                for (int x = 0; x < scratchPad.GetLength(0); x++)
                {
                    scratchPad[x, y] = false;
                }
            }

            //cell alive count display
            toolStripStatusLabel1.Text = $"Cells Alive = " + cellsAlive;

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

            // Invalidate
            graphicsPanel1.Invalidate();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // FLOATS!!!!!
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            //cell neighbor count
            Font font = new Font("Microsoft Sans Serif", 12f);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen
                    if (gridLinesOn == true)
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                    //neighbor count enable/disable
                    if (neighborCountOn == true)
                    {
                        // cell neighbor count finite
                        if (finiteOn == true)
                        {
                            int neighbors = CountNeighborsFinite(x, y);

                            if (neighbors < 2 && neighbors > 0)
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, cellRect, stringFormat);
                            else if (neighbors == 2 || neighbors == 3)
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Orange, cellRect, stringFormat);
                            else if (neighbors > 3 && neighbors <= 5)
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Yellow, cellRect, stringFormat);
                            else if (neighbors > 5)
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.LawnGreen, cellRect, stringFormat);
                            else
                                e.Graphics.DrawString(" ", font, Brushes.White, cellRect, stringFormat);
                        }

                        //cell neighbor count toroidal
                        if (toroidalOn == true)
                        {
                            int neighbor = CountNeighborsToroidal(x, y);

                            if (neighbor < 2 && neighbor > 0)
                                e.Graphics.DrawString(neighbor.ToString(), font, Brushes.Red, cellRect, stringFormat);
                            else if (neighbor == 2 || neighbor == 3)
                                e.Graphics.DrawString(neighbor.ToString(), font, Brushes.Orange, cellRect, stringFormat);
                            else if (neighbor > 3 && neighbor <= 5)
                                e.Graphics.DrawString(neighbor.ToString(), font, Brushes.Yellow, cellRect, stringFormat);
                            else if (neighbor > 5)
                                e.Graphics.DrawString(neighbor.ToString(), font, Brushes.LawnGreen, cellRect, stringFormat);
                            else
                                e.Graphics.DrawString(" ", font, Brushes.White, cellRect, stringFormat);
                        }
                    }
                }
            }
            //HUD Display
            if (HUDon == true)
            {
                //formatting
                Font fontHUD = new Font("Microsoft Sans Serif", 12f);

                StringFormat stringFormatHUD = new StringFormat();
                stringFormatHUD.Alignment = StringAlignment.Near;
                stringFormatHUD.LineAlignment = StringAlignment.Far;

                Rectangle HUDd = Rectangle.Empty;
                HUDd.Width = graphicsPanel1.ClientSize.Width;
                HUDd.Height = graphicsPanel1.ClientSize.Height;

                string HUD = $"Generations: {generations}\nCell Count: {cellsAlive}\nBoundry Type: {BoundryType}";

                //Draw Hud
                e.Graphics.DrawString(HUD, fontHUD, Brushes.Red, HUDd, stringFormatHUD);
            }

            //cells alive reset
            cellsAlive = 0;

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        //Mouse Click
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        //Exit Button
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Star, Stop, And Next Generation Buttons
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //New Button
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }
            timer.Enabled = false;
            generations = 0;
            graphicsPanel1.Invalidate();
        }

        //Neighbor Counting Methods
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    // if xCheck is less than 0 then continue
                    if (xCheck < 0)
                    {
                        continue;
                    }
                    // if yCheck is less than 0 then continue
                    if (yCheck < 0)
                    {
                        continue;
                    }
                    // if xCheck is greater than or equal too xLen then continue
                    if (xCheck >= xLen)
                    {
                        continue;
                    }
                    // if yCheck is greater than or equal too yLen then continue
                    if (yCheck >= yLen)
                    {
                        continue;
                    }

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    // if xCheck is less than 0 then set to xLen - 1
                    if (xCheck < 0)
                    {
                        xCheck = xLen - 1;
                    }
                    // if yCheck is less than 0 then set to yLen - 1
                    if (yCheck < 0)
                    {
                        yCheck = yLen - 1;
                    }
                    // if xCheck is greater than or equal too xLen then set to 0
                    if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    // if yCheck is greater than or equal too yLen then set to 0
                    if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        //Color Customization
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog backColor = new ColorDialog();

            backColor.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == backColor.ShowDialog())
            {
                graphicsPanel1.BackColor = backColor.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cellcolor = new ColorDialog();

            cellcolor.Color = cellColor;

            if (DialogResult.OK == cellcolor.ShowDialog())
            {
                cellColor = cellcolor.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog gridcolor = new ColorDialog();

            gridcolor.Color = gridColor;

            if (DialogResult.OK == gridcolor.ShowDialog())
            {
                gridColor = gridcolor.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //HUD Toggle
        private void hUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //HUD button toggle
            if (HUDon == true)
            {
                HUDon = false;
            }
            else if (HUDon == false)
                HUDon = true;
            graphicsPanel1.Invalidate();
        }

        //Neighbor Count Toggle
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (neighborCountOn == true)
            {
                neighborCountOn = false;
            }
            else if (neighborCountOn == false)
            {
                neighborCountOn = true;
            }
            graphicsPanel1.Invalidate();
        }

        //Grid Toggle
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridLinesOn == true)
            {
                gridLinesOn = false;
            }
            else if (gridLinesOn == false)
            {
                gridLinesOn = true;
            }
            graphicsPanel1.Invalidate();
        }

        //Options Dialog Box
        private void optionsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form2 dlg = new Form2();
            dlg.setTimer(100);
            dlg.setXaxis(10);
            dlg.setYaxis(10);

            if (DialogResult.OK == dlg.ShowDialog())
            {
                // modify
                timer.Interval = dlg.getTimer();
                int width = dlg.getXaxis();
                int height = dlg.getYaxis();

                universe = new bool[width, height];     //width & height are the inputs from dialog box
                scratchPad = new bool[width, height];

                graphicsPanel1.Invalidate();
            }
        }

        //Finite and Toroidal Switch
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finiteOn = true;
            toroidalOn = false;
            BoundryType = "Finite";
            graphicsPanel1.Invalidate();
        }

        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finiteOn = false;
            toroidalOn = true;
            BoundryType = "Toroidal";
            graphicsPanel1.Invalidate();
        }

        //Randomization
        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //Reset and Reload
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            //setting preferences
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            timer.Interval = Properties.Settings.Default.Timer;
        }

        private void revertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            //setting preferences
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            timer.Interval = Properties.Settings.Default.Timer;
        }

        //Form Closed
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //update settings
            Properties.Settings.Default.BackColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.Timer = timer.Interval;

            Properties.Settings.Default.Save();
        }

        //Save and Open
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";


            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!Save File");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.
                        if (universe[x, y] == true)
                            currentRow += 'O';

                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                        else if (universe[x, y] == false)
                            currentRow += '.';
                    }

                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    writer.WriteLine(currentRow);
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;
                int yPos = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    // and should be ignored.
                    if (row.StartsWith("!"))
                        continue;

                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.
                    if (row.StartsWith("!") == false)
                        maxHeight++;

                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                    maxWidth = row.Length;
                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.
                int width = maxWidth;
                int height = maxHeight;
                universe = new bool[width, height];
                scratchPad = new bool[width, height];

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.
                    if (row.StartsWith("!"))
                        continue;

                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos] == 'O')
                            universe[xPos, yPos] = true;

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        if(row[xPos] == '.')
                            universe[xPos, yPos] = false;

                        yPos++;
                    }
                }

                // Close the file.
                reader.Close();
            }
        }

        private void Randomize()
        {
            //Random rand = new Random(); Time
            //taks a seed for a seed

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //Call next

                    //if random number == 0; turn on

                }
            }
        }
    }
}
