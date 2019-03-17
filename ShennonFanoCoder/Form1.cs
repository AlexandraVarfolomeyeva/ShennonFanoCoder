using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShennonFanoCoder
{
    

    public partial class Form1 : Form
    {
        private OpenFileDialog openFileDialog1; //opening file
        int N; //amount of symbols
        public List<symbol> Cfile; //symbols of file to code in array = alphabet
        char[] messfile; //the whole message from the file to code
       // TreeView<symbol> Tree; //TreeNode -- node

        public class Tree<T>
        {
            public int weight=0;//sums of parts of table
            public Tree<T> Left, Right;
            public T leave=default(T);//null
            public Tree<T> parent;
        }

        /// <summary>
        /// Класс, содержащий байт, вероятность его встречи и его код
        /// </summary>
        public class symbol
        {
            public char ch; // the symbol itself
            public int prob=0; // number of symbol and their probability
            public string code=""; //the code they are going to have
        }



        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Выбор файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e) //open file
        {

            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }

            Opening();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
            Opening();
        } //open file

        void Opening()
        {
            Cfile = new List<symbol>();
            System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName); // got file to code
            N = Convert.ToInt32(sr.BaseStream.Length); //got amount of symbols in file to code                                                             
            sr.BaseStream.Position = 0;
            messfile = new char[N];
            label3.Text = "";
            for (int i = 0; i < N; i++)
            {
                messfile[i] = Convert.ToChar(sr.Read());
                label3.Text += messfile[i]; //getting the message
                if (Cfile.Count == 0) //if first symbol
                {
                    symbol c = new symbol();
                    c.ch = messfile[i];
                    c.code = null;
                    c.prob = 1;
                    Cfile.Add(c);
                }
                else // if not
                {
                    bool done = false; // if the alphabet was renewed
                    for (int j = 0; j < Cfile.Count; j++) //getting the alphabet
                    {

                        if (!done)
                        {
                            if (Cfile[j].ch == messfile[i]) // if exists 
                            {
                                Cfile[j].prob++;
                                done = true;
                            }
                            else
                            if (j == Cfile.Count - 1)
                            {
                                symbol c = new symbol();
                                c.ch = messfile[i];
                                c.code = null;
                                c.prob = 1;
                                Cfile.Add(c);
                                done = true;
                            }
                        }
                    }
                }
            }

            sr.Close();
            Sort();
        }

       

        public class NameComparer : IComparer<symbol>
        {
            public int Compare(symbol k1, symbol k2)
            {
                if (k1.prob > k2.prob)
                {
                    return -1;
                }
                else if (k1.prob < k2.prob)
                {
                    return 1;
                }

                return 0;
            }
        }
        void Sort()
        {
            NameComparer cn = new NameComparer();
            Cfile.Sort(cn);
            CreateTree();
        }

        void CreateTree()
        {
           Tree<symbol> tree = new Tree<symbol>();
          
            int sum=0;
            for (int i = 0; i < Cfile.Count; i++)
            {
                sum += Cfile[i].prob;
            }
            tree.weight = sum;
            tree.parent = null;
            BuildingTree(0,Cfile.Count-1,sum,tree);
        }

        void BuildingTree(int Li, int Ri, int sum, Tree<symbol> node) 
                                        //index Left, Right, Sum of all elements, parent
        {
            int Lb =Li, Rb = Ri;
            int sumL= Cfile[Li].prob, sumR= sum - Cfile[Li].prob; //sums of the parts
            
            for (int LeftIndex = Li+1; (LeftIndex < Cfile.Count) && (sumL + Cfile[LeftIndex].prob <= sumR - Cfile[LeftIndex].prob); LeftIndex++) //while  sums are not close
            {
                sumL += Cfile[LeftIndex].prob;
                sumR -= Cfile[LeftIndex].prob;
   
                Li++;
            }
            Tree<symbol> node1 = new Tree<symbol>();
            node1.weight = sumL;
            Console.WriteLine("SumL - " + sumL);
            Tree<symbol> node2 = new Tree<symbol>();
            node2.weight = sumR;
            Console.WriteLine("SumR - " + sumR);
            node.Left = node1;
            node.Right = node2;
            node1.parent = node;
            node2.parent = node;
            if (Li < Cfile.Count && Rb < Cfile.Count)
            {
                if (sumL>0)
                if (Li - Lb == 0) //if consists of one element
                {
                    node1.leave = Cfile[Lb];
                        node1.leave.code += "0";
                    Console.WriteLine("leave L - " + node1.leave.ch + " " + node1.leave.code);
                }
                else
                {
                    Console.WriteLine("Recursion L -");
                        for (int i=Lb;i<=Li;i++)
                        {
                            Cfile[i].code += "0";
                        }
                    BuildingTree(Lb, Li, sumL, node1);

                }
                if (sumR>0)
                if (Rb - (Li+1) == 0) //if consists of one element
                {
                    
                    node2.leave = Cfile[Rb];
                        node2.leave.code += "1";
                        Console.WriteLine("Leave R - " + node2.leave.ch + " " + node2.leave.code);
                }
                else
                {
                    Console.WriteLine("Recursion R - ");
                        for (int i = Li+1; i <= Rb; i++)
                        {
                            Cfile[i].code += "1";
                        }
                        BuildingTree(Li+1, Rb, sumR, node2);

                }
            }
        }

        /*  void Quick_sort(symbol[] b, char left, char right)
          {
              char i = left, j = right;
              symbol swap;
              //   int t = b[(i + j) >> 1].prob;
              int t = b[0].prob;
              while (i <= j)
              {
                  while (b[i].prob > t)
                      i++;
                  while (t > b[j].prob)
                      j--;

                  if (i <= j)
                  {
                      if ((b[i].prob < b[j].prob))
                      {
                          swap = b[i];
                          b[i] = b[j];
                          b[j] = swap;
                      }

                      i++;
                      j--;
                  }
              }
        
              if (left < j)
                  Quick_sort(b, left, j);
              if (i < right)
                  Quick_sort(b, i, right);
          }
          */

        void Coding_Shannon(symbol[] a, int left, int right, int sum)
        {
            int rest = 0;     // Sum of probability in one half of the prob table
            int merge = 0;     // Number of merge character
            int fringe = 0;   // Merge probability

            if (left != right)
                if (right != 0)
                {
                    for (int i = left; i <= right; i++)
                    {
                        rest += a[i].prob; //Left part of the table
                        if (a[i].prob > ((sum + 1) >> 1)) // Prob of symbol > sum/2, then usual table cannot be built => need handling
                        {                                 // Here it is...
                            //a[i].code.push_back(1);
                            merge = i;
                            fringe = a[i].prob;
                            continue;
                        }
                        if (rest <= ((sum + 1) >> 1))     // Symbol is in the first half of the prob table
                        {
                            //a[i].code.push_back(1);       // Write '1'
                            fringe = rest;
                            merge = i;
                        }
                        else                              // Symbol is in the second half of the prob table
                        {
                            //a[i].code.push_back(0);       // Write '0'
                        }
                    }
                    if (left <= merge && merge + 1 <= right)  // Divide and handle both halfs
                    {
                        Coding_Shannon(a, left, merge, fringe);
                        Coding_Shannon(a, merge + 1, right, sum - fringe);
                    }
                }
        }


        //void FormHeader(FILE* f, symbol* &symb, unsigned short alpha)
        //{
        //    if (alpha == 1)
        //    {
        //        fwrite(&alpha, 1, 1, f);
        //        fputc(symb[0].ch, f);
        //        fputc(0, f);                        // Write pairs of type: <Count, Length>
        //        fputc(1, f);
        //        printf("\nHeader size 4\n");
        //    }
        //    else
        //    {
        //        fwrite(&alpha, 1, 1, f);            // Write the number of characters, used in the code table
        //                                            // In another words - the alphabet of the message
        //        for (unsigned short i = 0; i < alpha; i++)
        //        {
        //            fputc(symb[i].ch, f);           // Write sequentially symbols from the code table
        //        }

        //        unsigned int count = 0;             // This will count the number of codes with the same length in succession
        //        unsigned int hsize = alpha;
        //        for (unsigned short i = 0; i < alpha; i++)
        //        {
        //            while (symb[i].code.size() == symb[i + 1].code.size())
        //            {
        //                ++count;                    // Here compare code length of the neighbouring symbols
        //                ++i;                        // and count them, if codes are of the same length
        //            }
        //            fputc(count, f);                // Write pairs of type: <Count, Length>
        //            fputc(symb[i].code.size(), f);
        //            hsize += 2;
        //            count = 0;
        //        }

        //        printf("\nHeader size %d\n", hsize + 1);
        //    }
        //}


        // -----------------------------------------------------------------------
        // Write coded info into file

        //void Pack(symbol* &symb, unsigned int size, unsigned char* &message, unsigned short alpha)
        //{
        //    unsigned char c;                  // Char to form a byte
        //    unsigned char cnt = 0;
        //    double aver = 0.0;
        //    FILE* f = fopen("Target.b", "wb");

        //    FormHeader(f, symb, alpha);       // Put the service info

        //    // Here, coding the message
        //    for (unsigned int k = 0; k < size; k++)
        //    {
        //        for (unsigned short i = 0; i < alpha; i++)
        //        {
        //            if (symb[i].ch != message[k]) continue;
        //            for (vector < unsigned char >::iterator j = symb[i].code.begin(); j != symb[i].code.end(); j++)
        //            {
        //                if (*j == 1)
        //                {
        //                    c = (c << 1) | 1;      // Write '1'
        //                    ++cnt;
        //                }
        //                else
        //                {
        //                    ++cnt;                 // Write '0'
        //                    c <<= 1;
        //                }
        //                if (cnt == 7)              // Have a byte
        //                {
        //                    aver += 8;
        //                    cnt = 0;
        //                    fprintf(f, "%c", c);
        //                    c = 0;
        //                }
        //            }
        //        }
        //    }
        //    aver = aver + cnt + 1;
        //    unsigned char shift = 0;
        //    if (cnt != 0)
        //    {
        //        shift = 7 - cnt;
        //        while (cnt != 7)
        //        {
        //            c <<= 1;
        //            ++cnt;
        //        }
        //        fprintf(f, "%c", c);
        //    }

        //    fwrite(&shift, 1, 1, f);

        //    printf("Average quantity of bits per symbol: %f\n", aver / ((float)size));

        //    fclose(f);
        //}



    }
}
