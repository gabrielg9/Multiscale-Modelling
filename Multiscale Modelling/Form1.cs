using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Media;

namespace Multiscale_Modelling
{
    public partial class Form1 : Form
    {
        int[,] tablica;
        int[,] tablicaGrainBoundary;
        int[,] poprzednia_tablica;
        int val = 1;
        private Graphics g;
        Bitmap DrawArea;
        Bitmap BMPfromFile;
        Bitmap BmpTest;
        int r1, r2, ilosc;
        float r1_f, r2_f, size_x, size_y;
        Siatka s;
        SolidBrush czarny = new SolidBrush(Color.Black);
        SolidBrush bialy = new SolidBrush(Color.White);
        SolidBrush zolty = new SolidBrush(Color.Yellow);
        Random random = new Random();
        SolidBrush blackBrush = new SolidBrush(Color.Black);
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        SolidBrush brownBrush = new SolidBrush(Color.Brown);
        SolidBrush blueBrush = new SolidBrush(Color.Blue);
        SolidBrush greenBrush = new SolidBrush(Color.Green);
        SolidBrush pinkBrush = new SolidBrush(Color.Pink);
        SolidBrush colorBrush = new SolidBrush(Color.Aqua);
        SolidBrush[] solidBrushes;
        bool periodyczne;
        bool absorbujace;
        bool rozrost_ziaren;
        bool isCircle = false;
        bool importedBMP, importedTXT = false;
        int numberOfInclusions, sizeOfInclusions;
        bool inclusionsBefore = false, isInclusionsBefore = false;
        int[,] boundaryTable;
        int probability;
        bool GrainBondaryMethod = false;
        bool selectGrainstoStructure = false;
        int[,] tableDualPhase, tableStructure;
        int selectedColorDualPhase;
        bool gotColorDualPhase = false, readyForGrowing = false;
        int numberOfGrainsSecondGrowth;
        int[,] helpTable, helpTableOnlyGrains, mergedTable;
        int GBsize;
        int[,] borderTable, helpBorderTable;
        bool secondGrowed = false, selectGrainsToClearSpace = false;
        public Form1()
        {
            InitializeComponent();
            //InitializeComboBox();

            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            g = Graphics.FromImage(DrawArea);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            periodyczne = true;
            absorbujace = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            absorbujace = true;
            periodyczne = false;
        }

        private void button1_Click(object sender, EventArgs e)//Simulation
        {
            pobierz_dane();
            wyznacz_kolory();
            rozrost_ziaren = true;
            Graphics g;
            g = Graphics.FromImage(DrawArea);

            if(isInclusionsBefore == false)
            {
                tablica = new int[r2, r1];
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                        tablica[i, j] = 0;
            }
            isInclusionsBefore = true;
            

            g.Clear(Color.DarkGray);
            Random rand = new Random();
            if(importedBMP == false)
                ilosc = int.Parse(textBox3.Text);
            for (int i = 1; i < ilosc + 1; i++)
            {
                int a = rand.Next(r2);
                int b = rand.Next(r1);
                if (tablica[a, b] == 0)
                    tablica[a, b] = i;
            }
            if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            } 
        }

        private void nowy_watek()
        {
            while (rozrost_ziaren)
            {
                if (importedBMP)
                    rysuj_ziarna(BmpTest);
                else
                    rysuj_ziarna(DrawArea);
                Thread.Sleep(1000);
            }
        }

        private void rysuj_ziarna(Bitmap bitmap)
        {
            lock (g)
            {
                Graphics grp;
                grp = Graphics.FromImage(bitmap);
                if(readyForGrowing == false)
                {
                    for (int i = 0; i < r2; i++)
                    {
                        for (int j = 0; j < r1; j++)
                        {
                            for (int k = 0; k < 1001; k++)
                            {
                                if (tablica[i, j] == k)
                                    grp.FillRectangle(solidBrushes[k], j * size_x, i * size_y, size_x, size_y);
                            }
                        }
                    }
                    if (GrainBondaryMethod)
                    {
                        if (periodyczne)
                            tablica = s.periodicGrainBoundary(tablica, r2, r1, probability);
                        else
                            tablica = s.absorbicGrainBoundary(tablica, r2, r1, probability);
                        //MessageBox.Show("stop during growing");
                    }
                    else
                    {
                        if (periodyczne)
                        {
                            tablica = s.sprawdz_warunki_brzegowe_moor_periodyczne(tablica, r2, r1);
                        }
                        else//absorbujace
                        {
                            tablica = s.sprawdz_warunki_brzegowe_moor_absorbujace(tablica, r2, r1);
                        }
                    }
                }
                else
                {//secondGrowing
                    for (int i = 0; i < r2; i++)
                    {
                        for (int j = 0; j < r1; j++)
                        {
                            if(helpTable[i,j] != 0)
                            {
                                for (int k = 0; k < 1001; k++)
                                {
                                    if (helpTable[i, j] == k)
                                        grp.FillRectangle(solidBrushes[k], j * size_x, i * size_y, size_x, size_y);
                                }
                            }
                            else
                            {
                                for (int k = 0; k < 1001; k++)
                                {
                                    if (helpTableOnlyGrains[i, j] == k)
                                        grp.FillRectangle(solidBrushes[k], j * size_x, i * size_y, size_x, size_y);
                                }
                            }
                        }
                    }
                    if (periodyczne)
                        helpTableOnlyGrains = s.sprawdz_warunki_brzegowe_moor_periodyczne(helpTableOnlyGrains, r2, r1);
                    else
                        helpTableOnlyGrains = s.sprawdz_warunki_brzegowe_moor_absorbujace(helpTableOnlyGrains, r2, r1);
                }
                
                pictureBox1.Image = bitmap;
                grp.Dispose();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fromTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importedTXT = true;
            string tokens = File.ReadAllText(@"structure.txt");
            string[] lines = tokens.Split(' ', '\n');
            r1 = int.Parse(lines[0]);
            r2 = int.Parse(lines[1]);
            ilosc = int.Parse(lines[2]);

            int indexOfGrain = 3;
            tablica = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                {
                    tablica[i, j] = int.Parse(lines[2 + indexOfGrain]);
                    indexOfGrain += 3;
                }

            pobierz_dane();
            wyznacz_kolory();
            rozrost_ziaren = true;
            Graphics g;
            g = Graphics.FromImage(DrawArea);

            if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            }
        }

        private void fromBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BMPfromFile = (Bitmap)Image.FromFile(@"test.bmp");
            BmpTest = new Bitmap(Bitmap.FromFile(@"test.bmp"));
            pictureBox1.Image = BmpTest;
            importedBMP = true;
            //pictureBox1.Image = BMPfromFile;
            var lines = File.ReadAllLines(@"helpText.txt");
            r1 = int.Parse(lines[0]);
            r2 = int.Parse(lines[1]);
            ilosc = int.Parse(lines[2]);
            /*tablica = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                {
                    tablica[i, j] = 0;
                }
            pobierz_dane();
            wyznacz_kolory();
            rozrost_ziaren = true;
            Graphics g;
            g = Graphics.FromImage(DrawArea);

            if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            }*/
        }

        private void toTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rozrost_ziaren = false;
            string Text = r1 + " " + r2 + " " + ilosc + Environment.NewLine;
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    Text += (j + 1) + " " + (i + 1) + " " + tablica[i, j] + Environment.NewLine;

            File.WriteAllText(@"structure.txt", Text);
        }

        private void toBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rozrost_ziaren = false;
            DrawArea.Save(@"test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            string helpText = r1.ToString() + Environment.NewLine + r2.ToString() + Environment.NewLine + ilosc.ToString() ;
            File.WriteAllText(@"helpText.txt", helpText);
        }

        private void button4_Click(object sender, EventArgs e)//add inclusions before
        {
            Graphics g;
            g = Graphics.FromImage(DrawArea);
            g.Clear(Color.DarkGray);
            pictureBox1.Image = DrawArea;
            if(isInclusionsBefore == false)
            {
                inclusionsBefore = true;
                pobierz_dane();
                pobierz_dane_inclusions();
                tablica = new int[r2, r1];
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                    {
                        tablica[i, j] = 0;
                    }
                for (int i = 0; i < r2; i++)
                {
                    for (int j = 0; j < r1; j++)
                    {
                        if (tablica[i, j] == 1)
                            g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                        else
                            g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);
                    }
                }
            }
            isInclusionsBefore = true;
        }

        private void button5_Click(object sender, EventArgs e)//add inclusions after
        {
            Graphics g;
            g = Graphics.FromImage(DrawArea);
            
            boundaryTable = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                {
                    boundaryTable[i, j] = 0;
                }

            Random rand = new Random();
            pobierz_dane_inclusions();
            boundaryTable = s.checkBoundaryConditions(tablica, r2, r1, sizeOfInclusions, false);
            
            for(int q=0; q<numberOfInclusions; q++)
            {
                int a = rand.Next(r2);
                int b = rand.Next(r1);
                if (boundaryTable[a, b] == 1001)
                    boundaryTable[a, b] = 1000;
                else
                    q--;
            }

            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    if (boundaryTable[i, j] == 1001 || boundaryTable[i, j] == 0)
                        boundaryTable[i, j] = 0;
                }
            }

            int[,] helpBoundaryTable = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    helpBoundaryTable[i, j] = 0;

            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    if (boundaryTable[i, j] == 1000)
                        helpBoundaryTable[i, j] = 1;
                }
            }


            for (int i=0; i<r2; i++)
            {
                for(int j=0; j<r1; j++)
                {
                    for (int w = sizeOfInclusions - 1; w >= 0; w--)
                    {
                        for (int d = sizeOfInclusions - 1; d >= 0; d--)
                        {
                            if (helpBoundaryTable[i, j] == 1)
                                boundaryTable[i + w, j + d] = 1000;
                        }
                    }
                }
            }

            tablica = s.matchBoundaryAndGrain(tablica, boundaryTable, r2, r1);

            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    for (int k = 0; k < 1001; k++)
                    {
                        if (tablica[i, j] == k)
                            g.FillRectangle(solidBrushes[k], j * size_x, i * size_y, size_x, size_y);
                    }
                }
            }
            pictureBox1.Image = DrawArea;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            isCircle = true;
        }

        private void button6_Click(object sender, EventArgs e)//Grain boundary
        {
            /*pobierz_dane();
            wyznacz_kolory();
            rozrost_ziaren = true;
            Graphics g;
            g = Graphics.FromImage(DrawArea);
            probability = decimal.ToInt32(numericUpDown3.Value);

            tablicaGrainBoundary = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    tablicaGrainBoundary[i, j] = 0;

            g.Clear(Color.DarkGray);
            Random rand = new Random();
            if (importedBMP == false)
                ilosc = int.Parse(textBox3.Text);
            for (int i = 1; i < ilosc + 1; i++)
            {
                int a = rand.Next(r2);
                int b = rand.Next(r1);
                if (tablicaGrainBoundary[a, b] == 0)
                    tablicaGrainBoundary[a, b] = i;
            }*/
            probability = decimal.ToInt32(numericUpDown3.Value);
            GrainBondaryMethod = true;
            /*if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            }*/
        }

        private void button7_Click(object sender, EventArgs e)//substructure / dual phase selection
        {
            selectGrainstoStructure = true;
            tableStructure = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                {
                    tableStructure[i, j] = 0;
                }

            tableDualPhase = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                {
                    tableDualPhase[i, j] = 0;
                }
        }


        private void button8_Click(object sender, EventArgs e)//second grain growth
        {
            helpTable = new int[r2, r1];
            if (comboBox1.SelectedItem.ToString() == "Substructure")
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r2; j++)
                        helpTable[i, j] = tableStructure[i, j];
            else
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                        helpTable[i, j] = tableDualPhase[i, j];

            if (comboBox1.SelectedItem.ToString() == "Substructure")
                tableStructure = generateGrainsStructureTable(tableStructure);
            else
                tableDualPhase = generateGrainsStructureTable(tableDualPhase);

            helpTableOnlyGrains = new int[r2, r1];
            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    if (helpTable[i, j] != 0)
                        helpTableOnlyGrains[i, j] = 0;
                    else
                    {
                        if (comboBox1.SelectedItem.ToString() == "Substructure")
                        {
                            //MessageBox.Show("Test3");
                            helpTableOnlyGrains[i, j] = tableStructure[i, j];
                        }
                        else
                            helpTableOnlyGrains[i, j] = tableDualPhase[i, j];
                    }
                }
            }

            readyForGrowing = true;
            rozrost_ziaren = true;
            if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            }
        }

        private int[,] generateGrainsStructureTable(int[,] table)
        {
            numberOfGrainsSecondGrowth = decimal.ToInt32(numericUpDown4.Value);
            Random rand = new Random();
            int added = 0;
            for (int i = 1; i < numberOfGrainsSecondGrowth + 1 + added; i++)
            {
                int a = rand.Next(r2);
                int b = rand.Next(r1);
                if (table[a, b] == 0)
                {
                    bool flag = true;
                    for (int k = 0; k < r2; k++)
                    {
                        for (int l = 0; l < r1; l++)
                        {
                            if (flag)
                            {
                                if (table[k, l] != i)
                                    flag = true;
                                else
                                {
                                    flag = false;
                                }
                            }
                        }
                    }
                    if (flag)
                        table[a, b] = i;
                    else
                        added++;
                }
                else
                {
                    if (i > 1)
                        i--;
                }
            }
            return table;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(selectGrainsToClearSpace)
            {
                MouseEventArgs me = (MouseEventArgs)e;
                int x = me.Location.X;
                int y = me.Location.Y;

                x = me.Location.X;
                y = me.Location.Y;

                float j_f = x / size_x;
                float i_f = y / size_y;
                int j_i = (int)j_f;
                int i_i = (int)i_f;

                int currentColor;
                if (secondGrowed)
                    currentColor = mergedTable[i_i, j_i];
                else
                    currentColor = tablica[i_i, j_i];

                if(secondGrowed)
                {
                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            if (mergedTable[i, j] == currentColor)
                                borderTable[i, j] = currentColor;
                }
                else
                {
                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            if (tablica[i, j] == currentColor)
                                borderTable[i, j] = currentColor;
                }
            }
            if(selectGrainstoStructure)
            {
                if(comboBox1.SelectedItem.ToString() == "Substructure")
                {
                    MouseEventArgs me = (MouseEventArgs)e;
                    int x = me.Location.X;
                    int y = me.Location.Y;

                    x = me.Location.X;
                    y = me.Location.Y;

                    float j_f = x / size_x;
                    float i_f = y / size_y;
                    int j_i = (int)j_f;
                    int i_i = (int)i_f;

                    int currentColor = tablica[i_i, j_i];

                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            if (tablica[i, j] == currentColor)
                                tableStructure[i, j] = currentColor;
                }
                else//Dual phase
                {
                    MouseEventArgs me = (MouseEventArgs)e;
                    int x = me.Location.X;
                    int y = me.Location.Y;

                    x = me.Location.X;
                    y = me.Location.Y;

                    float j_f = x / size_x;
                    float i_f = y / size_y;
                    int j_i = (int)j_f;
                    int i_i = (int)i_f;

                    int currentColor = tablica[i_i, j_i];
                    if (gotColorDualPhase == false)
                        setColorOfGrainDualPhase(tablica[i_i, j_i]);

                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            if(tablica[i, j] == currentColor)
                                tableDualPhase[i, j] = selectedColorDualPhase;
                }
            }
            else
            {
                if (inclusionsBefore)
                {
                    Graphics g;
                    g = Graphics.FromImage(DrawArea);
                    MouseEventArgs me = (MouseEventArgs)e;
                    int x = me.Location.X;
                    int y = me.Location.Y;

                    x = me.Location.X;
                    y = me.Location.Y;

                    float j_f = x / size_x;
                    float i_f = y / size_y;
                    int j_i = (int)j_f;
                    int i_i = (int)i_f;

                    if (isCircle)
                    {
                        if (numberOfInclusions > 0)
                        {
                            for (int w = sizeOfInclusions - 1; w >= 0; w--)
                            {
                                for (int d = sizeOfInclusions - 1; d >= 0; d--)
                                {
                                    if (tablica[i_i, j_i] == 1000)
                                        tablica[i_i + w, j_i + d] = 0;
                                    else
                                    {
                                        if ((w < Math.Abs((sizeOfInclusions - 1) / 2) && d < Math.Abs((sizeOfInclusions - 1) / 2) && w * d == 0) || (w < Math.Abs((sizeOfInclusions - 1) / 2) && d > Math.Abs((sizeOfInclusions - 1) / 2) && (w % (sizeOfInclusions - 1) == 0 || d % (sizeOfInclusions - 1) == 0)) || (w > Math.Abs((sizeOfInclusions - 1) / 2) && d < Math.Abs((sizeOfInclusions - 1) / 2) && (w % (sizeOfInclusions - 1) == 0 || d % (sizeOfInclusions - 1) == 0)) || (w > Math.Abs((sizeOfInclusions - 1) / 2) && d > Math.Abs((sizeOfInclusions - 1) / 2) && (w % (sizeOfInclusions - 1) == 0 || d % (sizeOfInclusions - 1) == 0)))
                                            tablica[i_i + w, j_i + d] = 0;
                                        else
                                            tablica[i_i + w, j_i + d] = 1000;
                                    }
                                }
                            }


                            for (int i = 0; i < r2; i++)
                            {
                                for (int j = 0; j < r1; j++)
                                {
                                    if (tablica[i, j] == 1000)
                                        g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                                    else
                                        g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (numberOfInclusions > 0)
                        {
                            for (int w = sizeOfInclusions - 1; w >= 0; w--)
                            {
                                for (int d = sizeOfInclusions - 1; d >= 0; d--)
                                {
                                    if (tablica[i_i, j_i] == 1000)
                                        tablica[i_i + w, j_i + d] = 0;
                                    else
                                        tablica[i_i + w, j_i + d] = 1000;
                                }
                            }

                            for (int i = 0; i < r2; i++)
                            {
                                for (int j = 0; j < r1; j++)
                                {
                                    if (tablica[i, j] == 1000)
                                        g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                                    else
                                        g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);
                                }
                            }
                        }
                    }

                    numberOfInclusions--;
                    pictureBox1.Image = DrawArea;
                }
                if (numberOfInclusions == 0)
                    inclusionsBefore = false;
            }
        }

        private void setColorOfGrainDualPhase(int color)
        {
            selectedColorDualPhase = color;
            gotColorDualPhase = true;
        }

        private void button9_Click(object sender, EventArgs e)//show distribution
        {
            secondGrowed = true;
            mergedTable = new int[r2, r1];
            for(int i=0; i<r2; i++)
                for(int j=0; j<r1; j++)
                {
                    if (helpTable[i, j] != 0)
                        mergedTable[i, j] = helpTable[i, j];
                    else
                        mergedTable[i, j] = helpTableOnlyGrains[i, j];
                }

            int counter = 0;
            string distributionText = "id " + "size " + "%" + Environment.NewLine;
            for(int k=0; k<1000; k++)
            {
                counter = 0;
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                        if (mergedTable[i, j] == k)
                            counter++;
                distributionText += k.ToString() + " " + counter.ToString() + " " + (100.0 * (double)counter / ((double)r2 * (double)r1)).ToString() + Environment.NewLine;
            }
            File.WriteAllText(@"distribution.txt", distributionText);
        }

        private void button10_Click(object sender, EventArgs e)//Border all grains
        {
            GBsize = decimal.ToInt32(numericUpDown5.Value);
            Graphics g;
            g = Graphics.FromImage(DrawArea);

            borderTable = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    borderTable[i, j] = 0;
            if (secondGrowed)
                borderTable = s.checkBoundaryConditions(mergedTable, r2, r1, GBsize, true);
            else
                borderTable = s.checkBoundaryConditions(tablica, r2, r1, GBsize, true);
            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    if (borderTable[i, j] == 0)
                        g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);//solidbrush[k] zrobi kolorowe bordery :)
                    else
                        g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                }
            }

            if(GBsize > 1)
            {
                int[,] helpBorderTableAllGrains = new int[r2, r1];
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                        helpBorderTableAllGrains[i, j] = 0;
                for (int i = 0; i < r2; i++)
                    for (int j = 0; j < r1; j++)
                        helpBorderTableAllGrains[i, j] = borderTable[i,j];
                for(int s=1; s<GBsize; s++)
                {
                    for(int i=0; i<r2; i++)
                    {
                        for(int j=0; j<r1; j++)
                        {
                            if (i != 0 && j != 0 && i != r2 - 1 && j != r1 - 1)
                            {
                                bool flaga = false;
                                for (int v = -1; v < 2; v++)
                                {
                                    for (int b = -1; b < 2; b++)
                                    {
                                        if(flaga == false)
                                        {
                                            if (borderTable[i + v, j + b] == 1001 && v * b == 0)
                                            {
                                                helpBorderTableAllGrains[i, j] = 1001;
                                                flaga = true;
                                            }
                                            else
                                                helpBorderTableAllGrains[i, j] = borderTable[i, j];
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            borderTable[i, j] = helpBorderTableAllGrains[i, j];

                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            helpBorderTableAllGrains[i, j] = 0;
                }

                for (int i = 0; i < r2; i++)
                {
                    for (int j = 0; j < r1; j++)
                    {
                        if (borderTable[i, j] == 0)
                            g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);//solidbrush[k] zrobi kolorowe bordery :)
                        else
                            g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                    }
                }
            }
            pictureBox1.Image = DrawArea;
        }

        private void button11_Click(object sender, EventArgs e)//Border selected grains
        {
            GBsize = decimal.ToInt32(numericUpDown5.Value);
            borderTable = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    borderTable[i, j] = 0;
            selectGrainsToClearSpace = true;

        }

        private void button12_Click(object sender, EventArgs e)//Clear space in selected grains
        {
            //zaznaczyc kilka ziaren. Niezaznaczone wymazać, przejść po całej tablicy i jak dla jakiegoś id są różni sąsiedzi tzn że jest na granicy i zmienić kolor na czarny 
            //(jednocześnie sprawdzać żeby sąsiad nie był czarny)
            helpBorderTable = new int[r2, r1];
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    helpBorderTable[i, j] = 0;
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    helpBorderTable[i, j] = borderTable[i, j];
            int[,] OneForHelpBordertable = new int[r2, r1];
           

            borderTable = s.checkBoundaryConditions(borderTable, r2, r1, GBsize, true);
            for (int i = 0; i < r2; i++)
                for (int j = 0; j < r1; j++)
                    OneForHelpBordertable[i, j] = borderTable[i, j];
            for (int i = 0; i < r2; i++)
            {
                for (int j = 0; j < r1; j++)
                {
                    if (borderTable[i, j] == 0)
                        g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);//solidbrush[k] zrobi kolorowe bordery :)
                    else
                        g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                }
            }

            if(GBsize > 1)
            {
                for(int s = 1; s<GBsize; s++)
                {
                    for (int i = 0; i < r2; i++)
                    {
                        for (int j = 0; j < r1; j++)
                        {
                            if (helpBorderTable[i, j] != 0)
                            {
                                if (i != 0 && j != 0 && i != r2 - 1 && j != r1 - 1)
                                {
                                    bool flaga = false;
                                    for (int v = -1; v < 2; v++)
                                    {
                                        for (int b = -1; b < 2; b++)
                                        {
                                            if(flaga == false)
                                            {
                                                if (borderTable[i + v, j + b] == 1001 && v*b == 0)
                                                {
                                                    OneForHelpBordertable[i, j] = 1001;
                                                    flaga = true;
                                                }

                                                else
                                                    OneForHelpBordertable[i, j] = borderTable[i, j];
                                            }
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < r2; i++)
                    {
                        for (int j = 0; j < r1; j++)
                        {
                            if(helpBorderTable[i,j] != 0)
                                borderTable[i, j] = OneForHelpBordertable[i, j];
                        }
                    }
                    for (int i = 0; i < r2; i++)
                        for (int j = 0; j < r1; j++)
                            OneForHelpBordertable[i, j] = 0;
                }

                for (int i = 0; i < r2; i++)
                {
                    for (int j = 0; j < r1; j++)
                    {
                        if (borderTable[i, j] == 0)
                            g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x, size_y);//solidbrush[k] zrobi kolorowe bordery :)
                        else
                            g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x, size_y);
                    }
                }

            }

            pictureBox1.Image = DrawArea;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            rozrost_ziaren = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            rozrost_ziaren = true;
            if (rozrost_ziaren)
            {
                Thread th = new Thread(nowy_watek);
                th.Start();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pobierz_dane()
        {
            if (importedBMP == false && importedTXT == false)
            {
                r1 = int.Parse(textBox1.Text);
                r2 = int.Parse(textBox2.Text);
            }
            r1_f = (float)r1;
            r2_f = (float)r2;
            size_x = pictureBox1.Size.Width / r1_f;
            size_y = pictureBox1.Size.Height / r2_f;
            if (size_x < size_y)
                size_y = size_x;
            else
                size_x = size_y;

            s = new Siatka();
        }

        private void pobierz_dane_inclusions()
        {
            numberOfInclusions = decimal.ToInt32(numericUpDown2.Value);
            sizeOfInclusions = decimal.ToInt32(numericUpDown1.Value);
        }

        private void wyznacz_kolory()
        {
            Random rand = new Random();
            int r, g, b;
            solidBrushes = new SolidBrush[1001];
            solidBrushes[0] = new SolidBrush(Color.White);
            for (int i = 1; i < 1000; i++)
            {
                r = rand.Next(1, 255);
                g = rand.Next(1, 255);
                b = rand.Next(1, 255);
                solidBrushes[i] = new SolidBrush(Color.FromArgb(r, g, b));
            }
            solidBrushes[1000] = blackBrush;
        }

        public class Siatka
        {
            //static int value = 1;

            public int[,] sprawdz_warunki_brzegowe_moor_periodyczne(int[,] tab, int m, int n)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                int aktualna_wartosc = 0;
                int max_wartosc = 0;
                int licznik = 0;
                int max_licznik = 0;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (tab[i, j] == 0)
                        {
                            for (int r = -1; r < 2; r++)
                                for (int t = -1; t < 2; t++)
                                {
                                    licznik = 0;
                                    aktualna_wartosc = tab[(i + r + m) % m, (j + t + n) % n];
                                    for (int k = -1; k < 2; k++)
                                    {
                                        for (int l = -1; l < 2; l++)
                                        {
                                            if ((aktualna_wartosc == tab[(i + k + m) % m, (j + l + n) % n]) && tab[(i + k + m) % m, (j + l + n) % n] != 0)
                                                licznik++;
                                        }
                                    }
                                    if (licznik > max_licznik)
                                    {
                                        max_licznik = licznik;
                                        max_wartosc = aktualna_wartosc;
                                    }
                                    licznik = 0;
                                }

                            if (max_wartosc == 1000)
                                max_wartosc = 0;
                            tab1[i, j] = max_wartosc;
                            max_wartosc = 0;
                            max_licznik = 0;
                        }
                        else
                            tab1[i, j] = tab[i, j];
                    }
                }
                return tab1;
            }

            public int[,] sprawdz_warunki_brzegowe_moor_absorbujace(int[,] tab, int m, int n)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                int wartosc = 0;
                int max_wartosc = 0;
                int licznik = 0;
                int max_licznik = 0;
                int[] zbior;
                zbior = new int[9];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        wartosc = 0; licznik = 0; max_licznik = 0;
                        if (tab[i, j] == 0)
                        {
                            if (j == 0 & i != 0 && i != m - 1)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (j == n - 1 && i != 0 && i != m - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == 0 && j != 0 && j != n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == m - 1 && j != 0 && j != n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == 0 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == 0 && j == n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            for (int l = 0; l < 9; l++)
                            {
                                wartosc = zbior[l];
                                for (int k = 0; k < 9; k++)
                                {
                                    if (wartosc == zbior[k] && zbior[k] != 0)
                                    {
                                        licznik++;
                                    }
                                }
                                if (licznik > max_licznik)
                                {
                                    max_licznik = licznik;
                                    max_wartosc = wartosc;
                                }
                                licznik = 0;

                            }
                            if (max_wartosc == 1000)
                                max_wartosc = 0;
                            tab1[i, j] = max_wartosc;
                            max_wartosc = 0;
                        }
                        else
                            tab1[i, j] = tab[i, j];
                    }
                }

                return tab1;
            }


            public int[,] checkBoundaryConditions(int[,] tab, int m, int n, int sizeOfInclusion, bool flag)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                bool isBorder = false;
                int wartosc;
                int[] zbior;
                zbior = new int[9];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        wartosc = 0;
                        isBorder = false;
                        if (tab[i,j] != 0)
                        {
                            if (j == 0 & i != 0 && i != m - 1)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (j == n - 1 && i != 0 && i != m - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == 0 && j != 0 && j != n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == m - 1 && j != 0 && j != n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == 0 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == 0 && j == n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            for (int l = 0; l < 9; l++)
                            {
                                wartosc = zbior[4];
                                if (wartosc != zbior[l])
                                    isBorder = true;

                            }

                            if (((i == 0 || j == 0 || i == m - 1 || j == n - 1 || i + sizeOfInclusion >= m - 1 || j + sizeOfInclusion >= n - 1) && flag == false) || (flag == true && (i == 0 || j == 0 || i == m - 1 || j == n - 1)))
                                tab1[i, j] = 0;
                            else
                            {
                                if (isBorder)
                                {
                                    tab1[i, j] = 1001;
                                }

                            }
                        }
                    }
                }
                return tab1;
            }

            public int[,] matchBoundaryAndGrain(int[,] Grain, int[,] Boundary, int m, int n)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (Boundary[i, j] == 1000)
                            tab1[i, j] = 1000;
                        else
                            tab1[i, j] = Grain[i, j];
                    }
                }
                
                return tab1;
            }


            public int[,] periodicGrainBoundary(int[,] tab, int m, int n, int probability)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                int aktualna_wartosc = 0;
                int max_wartosc = 0;
                int licznik = 0;
                int max_licznik = 0;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (tab[i, j] == 0)
                        {
                            for (int r = -1; r < 2; r++)
                            {
                                for (int t = -1; t < 2; t++)
                                {
                                    licznik = 0;
                                    aktualna_wartosc = tab[(i + r + m) % m, (j + t + n) % n];
                                    for (int k = -1; k < 2; k++)
                                    {
                                        for (int l = -1; l < 2; l++)
                                        {
                                            if ((aktualna_wartosc == tab[(i + k + m) % m, (j + l + n) % n]) && tab[(i + k + m) % m, (j + l + n) % n] != 0)
                                                licznik++;
                                        }
                                    }
                                    if (licznik > max_licznik)
                                    {
                                        max_licznik = licznik;
                                        max_wartosc = aktualna_wartosc;
                                    }
                                    licznik = 0;
                                }
                            }

                            if (max_wartosc == 1000)
                                max_wartosc = 0;
                            if(max_licznik >=5)
                            {
                                tab1[i, j] = max_wartosc;
                                //MessageBox.Show("Zadziałał Moor" + i.ToString() + " " + j.ToString());
                            }
                            else
                            {//rule2
                                int wartosc1 = 0;
                                int max_wartosc1 = 0;
                                int licznik1 = 0;
                                int max_licznik1 = 0;
                                int[] zbior1;
                                zbior1 = new int[4];

                                wartosc1 = 0; licznik1 = 0; max_licznik1 = 0;
                                if (tab[i, j] == 0)
                                {
                                    if (j == 0 & i != 0 && i != m - 1)
                                    {
                                        zbior1[0] = tab[i, n - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                    }
                                    else if (j == n - 1 && i != 0 && i != m - 1)
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, 0]; zbior1[3] = tab[i + 1, j];
                                    }
                                    else if (i == 0 && j != 0 && j != n - 1)
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[m - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                    }
                                    else if (i == m - 1 && j != 0 && j != n - 1)
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[0, j];
                                    }
                                    else if (i == 0 && j == 0)
                                    {
                                        zbior1[0] = tab[i, n - 1]; zbior1[1] = tab[m - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                    }
                                    else if (i == 0 && j == n - 1)
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[m - 1, j]; zbior1[2] = tab[i, 0]; zbior1[3] = tab[i + 1, j];
                                    }
                                    else if (i == m - 1 && j == n - 1)
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, 0]; zbior1[3] = tab[0, j];
                                    }
                                    else if (i == m - 1 && j == 0)
                                    {
                                        zbior1[0] = tab[i, n - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[0, j];
                                    }
                                    else
                                    {
                                        zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                    }
                                    for (int l = 0; l < 4; l++)
                                    {
                                        wartosc1 = zbior1[l];
                                        for (int k = 0; k < 4; k++)
                                        {
                                            if (wartosc1 == zbior1[k] && zbior1[k] != 0)
                                            {
                                                licznik1++;
                                            }
                                        }
                                        if (licznik1 > max_licznik1)
                                        {
                                            max_licznik1 = licznik1;
                                            max_wartosc1 = wartosc1;
                                        }
                                        licznik1 = 0;

                                    }
                                    if (max_licznik1 >= 3)
                                    {
                                        tab1[i, j] = max_wartosc1;
                                        max_wartosc1 = 0;
                                        max_licznik1 = 0;
                                        //MessageBox.Show("Zadziała rule 2" + i.ToString() + " " + j.ToString());
                                    }
                                    else
                                    {//rule3
                                        int wartosc2 = 0;
                                        int max_wartosc2 = 0;
                                        int licznik2 = 0;
                                        int max_licznik2 = 0;
                                        int[] zbior2;
                                        zbior2 = new int[4];

                                        wartosc2 = 0; licznik2 = 0; max_licznik2 = 0;
                                        if (tab[i, j] == 0)
                                        {
                                            if (j == 0 & i != 0 && i != m - 1)
                                            {
                                                zbior2[0] = tab[i - 1, n - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = tab[i + 1, n - 1]; zbior2[3] = tab[i + 1, j + 1];
                                            }
                                            else if (j == n - 1 && i != 0 && i != m - 1)
                                            {
                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, 0]; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, 0];
                                            }
                                            else if (i == 0 && j != 0 && j != n - 1)
                                            {
                                                zbior2[0] = tab[m - 1, j - 1]; zbior2[1] = tab[m - 1, j + 1]; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, j + 1];
                                            }
                                            else if (i == m - 1 && j != 0 && j != n - 1)
                                            {
                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = tab[0, j - 1]; zbior2[3] = tab[0, j + 1];
                                            }
                                            else if (i == 0 && j == 0)
                                            {
                                                zbior2[0] = tab[m - 1, n - 1]; zbior2[1] = tab[m - 1, j + 1]; zbior2[2] = tab[i + 1, n - 1]; zbior2[3] = tab[i + 1, j + 1];
                                            }
                                            else if (i == 0 && j == n - 1)
                                            {
                                                zbior2[0] = tab[m - 1, j - 1]; zbior2[1] = tab[m - 1, n - 1]; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, 0];
                                            }
                                            else if (i == m - 1 && j == n - 1)
                                            {
                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, 0]; zbior2[2] = tab[0, j - 1]; zbior2[3] = tab[0, 0];
                                            }
                                            else if (i == m - 1 && j == 0)
                                            {
                                                zbior2[0] = tab[i - 1, n - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = tab[0, n - 1]; zbior2[3] = tab[0, j + 1];
                                            }
                                            else
                                            {
                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, j + 1];
                                            }
                                            for (int l = 0; l < 4; l++)
                                            {
                                                wartosc2 = zbior2[l];
                                                for (int k = 0; k < 4; k++)
                                                {
                                                    if (wartosc2 == zbior2[k] && zbior2[k] != 0)
                                                    {
                                                        licznik2++;
                                                    }
                                                }
                                                if (licznik2 > max_licznik2)
                                                {
                                                    max_licznik2 = licznik2;
                                                    max_wartosc2 = wartosc2;
                                                }
                                                licznik2 = 0;

                                            }
                                        }
                                        if (max_licznik2 >= 3)
                                        {
                                            tab1[i, j] = max_wartosc2;
                                            max_wartosc2 = 0;
                                            max_licznik2 = 0;
                                            //MessageBox.Show("Zadziałał rule 4" + i.ToString() + " " + j.ToString());
                                        }
                                        else
                                        {//rule4

                                            Random rand = new Random();
                                            int randProbability = rand.Next(1, 100);
                                            //MessageBox.Show(i.ToString() + " " + j.ToString() + " " + randProbability.ToString());
                                            //Thread.Sleep(1);
                                            if (randProbability < probability)
                                            {
                                                tab1[i, j] = max_wartosc;
                                                //MessageBox.Show("Zadziałał rule 4");
                                                Thread.Sleep(1);
                                            }
                                            else
                                            {
                                                tab1[i, j] = 0;
                                                //MessageBox.Show("Rules 4 nie zadziałał");
                                                Thread.Sleep(1);
                                            }
                                            
                                        }
                                    }
                                }

                            }
                            max_wartosc = 0;
                            max_licznik = 0;
                        }
                        else
                            tab1[i, j] = tab[i, j];
                    }
                }
                //MessageBox.Show("Koniec obiegu");

                return tab1;
            }

            public int[,] absorbicGrainBoundary(int[,] tab, int m, int n, int probability)
            {
                int[,] tab1 = new int[m, n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        tab1[i, j] = 0;

                int wartosc = 0;
                int max_wartosc = 0;
                int licznik = 0;
                int max_licznik = 0;
                int[] zbior;
                zbior = new int[9];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        wartosc = 0; licznik = 0; max_licznik = 0;
                        if (tab[i, j] == 0)
                        {
                            if (j == 0 & i != 0 && i != m - 1)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (j == n - 1 && i != 0 && i != m - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == 0 && j != 0 && j != n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == m - 1 && j != 0 && j != n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == 0 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            else if (i == 0 && j == n - 1)
                            {
                                zbior[0] = 0; zbior[1] = 0; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == n - 1)
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = 0; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = 0; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else if (i == m - 1 && j == 0)
                            {
                                zbior[0] = 0; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = 0; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = 0; zbior[7] = 0; zbior[8] = 0;
                            }
                            else
                            {
                                zbior[0] = tab[i - 1, j - 1]; zbior[1] = tab[i - 1, j]; zbior[2] = tab[i - 1, j + 1]; zbior[3] = tab[i, j - 1]; zbior[4] = tab[i, j]; zbior[5] = tab[i, j + 1]; zbior[6] = tab[i + 1, j - 1]; zbior[7] = tab[i + 1, j]; zbior[8] = tab[i + 1, j + 1];
                            }
                            for (int l = 0; l < 9; l++)
                            {
                                wartosc = zbior[l];
                                for (int k = 0; k < 9; k++)
                                {
                                    if (wartosc == zbior[k] && zbior[k] != 0)
                                    {
                                        licznik++;
                                    }
                                }
                                if (licznik > max_licznik)
                                {
                                    max_licznik = licznik;
                                    max_wartosc = wartosc;
                                }
                                licznik = 0;

                            }
                            if (max_wartosc == 1000)
                                max_wartosc = 0;
                            if(max_licznik >= 5)
                                tab1[i, j] = max_wartosc;
                            else
                            {//rule2
                                int wartosc1 = 0;
                                int max_wartosc1 = 0;
                                int licznik1 = 0;
                                int max_licznik1 = 0;
                                int[] zbior1;
                                zbior1 = new int[4];

                                        wartosc1 = 0; licznik1 = 0; max_licznik1 = 0;
                                        if (tab[i, j] == 0)
                                        {
                                            if (j == 0 & i != 0 && i != m - 1)
                                            {
                                                zbior1[0] = 0; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                            }
                                            else if (j == n - 1 && i != 0 && i != m - 1)
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = 0; zbior1[3] = tab[i + 1, j];
                                            }
                                            else if (i == 0 && j != 0 && j != n - 1)
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = 0; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                            }
                                            else if (i == m - 1 && j != 0 && j != n - 1)
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = 0;
                                            }
                                            else if (i == 0 && j == 0)
                                            {
                                                zbior1[0] = 0; zbior1[1] = 0; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                            }
                                            else if (i == 0 && j == n - 1)
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = 0; zbior1[2] = 0; zbior1[3] = tab[i + 1, j];
                                            }
                                            else if (i == m - 1 && j == n - 1)
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = 0; zbior1[3] = 0;
                                            }
                                            else if (i == m - 1 && j == 0)
                                            {
                                                zbior1[0] = 0; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = 0;
                                            }
                                            else
                                            {
                                                zbior1[0] = tab[i, j - 1]; zbior1[1] = tab[i - 1, j]; zbior1[2] = tab[i, j + 1]; zbior1[3] = tab[i + 1, j];
                                            }
                                            for (int l = 0; l < 4; l++)
                                            {
                                                wartosc1 = zbior1[l];
                                                for (int k = 0; k < 4; k++)
                                                {
                                                    if (wartosc1 == zbior1[k] && zbior1[k] != 0)
                                                    {
                                                        licznik1++;
                                                    }
                                                }
                                                if (licznik1 > max_licznik1)
                                                {
                                                    max_licznik1 = licznik1;
                                                    max_wartosc1 = wartosc1;
                                                }
                                                licznik1 = 0;

                                            }
                                            if(max_wartosc1 >= 3)
                                            {
                                                tab1[i, j] = max_wartosc1;
                                                max_wartosc1 = 0;
                                                max_licznik1 = 0;
                                            }
                                            else
                                            {//rule3
                                                int wartosc2 = 0;
                                                int max_wartosc2 = 0;
                                                int licznik2 = 0;
                                                int max_licznik2 = 0;
                                                int[] zbior2;
                                                zbior2 = new int[4];

                                                        wartosc2 = 0; licznik2 = 0; max_licznik2 = 0;
                                                        if (tab[i, j] == 0)
                                                        {
                                                            if (j == 0 & i != 0 && i != m - 1)
                                                            {
                                                                zbior2[0] = 0; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = 0; zbior2[3] = tab[i + 1, j + 1];
                                                            }
                                                            else if (j == n - 1 && i != 0 && i != m - 1)
                                                            {
                                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = 0; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = 0;
                                                            }
                                                            else if (i == 0 && j != 0 && j != n - 1)
                                                            {
                                                                zbior2[0] = 0; zbior2[1] = 0; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, j + 1];
                                                            }
                                                            else if (i == m - 1 && j != 0 && j != n - 1)
                                                            {
                                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = 0; zbior2[3] = 0;
                                                            }
                                                            else if (i == 0 && j == 0)
                                                            {
                                                                zbior2[0] = 0; zbior2[1] = 0; zbior2[2] = 0; zbior2[3] = tab[i + 1, j + 1];
                                                            }
                                                            else if (i == 0 && j == n - 1)
                                                            {
                                                                zbior2[0] = 0; zbior2[1] = 0; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = 0;
                                                            }
                                                            else if (i == m - 1 && j == n - 1)
                                                            {
                                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = 0; zbior2[2] = 0; zbior2[3] = 0;
                                                            }
                                                            else if (i == m - 1 && j == 0)
                                                            {
                                                                zbior2[0] = 0; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = 0; zbior2[3] = 0;
                                                            }
                                                            else
                                                            {
                                                                zbior2[0] = tab[i - 1, j - 1]; zbior2[1] = tab[i - 1, j + 1]; zbior2[2] = tab[i + 1, j - 1]; zbior2[3] = tab[i + 1, j + 1];
                                                            }
                                                            for (int l = 0; l < 4; l++)
                                                            {
                                                                wartosc2 = zbior2[l];
                                                                for (int k = 0; k < 4; k++)
                                                                {
                                                                    if (wartosc2 == zbior2[k] && zbior2[k] != 0)
                                                                    {
                                                                        licznik2++;
                                                                    }
                                                                }
                                                                if (licznik2 > max_licznik2)
                                                                {
                                                                    max_licznik2 = licznik2;
                                                                    max_wartosc2 = wartosc2;
                                                                }
                                                                licznik2 = 0;

                                                            }
                                                            if (max_licznik2 >= 3)
                                                            {
                                                                tab1[i, j] = max_wartosc2;
                                                                max_wartosc2 = 0;
                                                                max_licznik2 = 0;
                                                            }
                                                            else
                                                            {//rule4
                                                                Random rand = new Random();
                                                                int randProbability = rand.Next(1, 100);
                                                                if (randProbability < probability)
                                                                {
                                                                    tab1[i, j] = max_wartosc;
                                                                }
                                                            }

                                                        }

                                            }
                                            
                                        }

                            }
                            max_wartosc = 0;
                            max_licznik = 0;
                        }
                        else
                            tab1[i, j] = tab[i, j];
                    }
                }


                return tab1;
            }
        }
    }
}
