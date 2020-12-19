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
        bool importedBMP, importedTXT = false;
        int numberOfInclusions, sizeOfInclusions;
        bool inclusionsBefore = false, isInclusionsBefore = false;
        int[,] boundaryTable;
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
                if (periodyczne)
                {
                    tablica = s.sprawdz_warunki_brzegowe_moor_periodyczne(tablica, r2, r1);
                }

                else//absorbujace
                {
                    tablica = s.sprawdz_warunki_brzegowe_moor_absorbujace(tablica, r2, r1);
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
            tablica = new int[r2, r1];
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
            }
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
            boundaryTable = s.checkBoundaryConditions(tablica, r2, r1, sizeOfInclusions);
            
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(inclusionsBefore)
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
                

                if (numberOfInclusions > 0)
                {
                    for(int w = sizeOfInclusions - 1; w>=0; w--)
                    {
                        for(int d = sizeOfInclusions - 1; d>=0; d--)
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
                                g.FillRectangle(blackBrush, j * size_x, i * size_y, size_x , size_y);
                            else
                                g.FillRectangle(whiteBrush, j * size_x, i * size_y, size_x , size_y);
                        }
                    }

                    
                }
                numberOfInclusions--;
                pictureBox1.Image = DrawArea;
            }
            if (numberOfInclusions == 0)
                inclusionsBefore = false;
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
            static int value = 1;

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


            public int[,] checkBoundaryConditions(int[,] tab, int m, int n, int sizeOfInclusion)
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
                                wartosc = zbior[0];
                                if (wartosc != zbior[l])
                                    isBorder = true;
                                    
                            }

                        if (i == 0 || j == 0 || i == m - 1 || j == n - 1 || i + sizeOfInclusion >= m - 1 || j + sizeOfInclusion >= n - 1)
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
        }
    }
}
