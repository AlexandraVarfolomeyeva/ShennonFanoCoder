using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
     //   byte[] messfile; //the whole message from the file to code
                         // TreeView<symbol> Tree; //TreeNode -- node
        int[] alphabet = new int[256];
        public class Tree<T>
        {
            public int weight = 0;//sums of parts of table
            public Tree<T> Left, Right;
            public T leave = default(T);//null
            public Tree<T> parent;
        }
        Tree<symbol> tree;
        /// <summary>
        /// Класс, содержащий байт, вероятность его встречи и его код
        /// </summary>
        public class symbol
        {
            public byte ch; // the symbol itself
            public int prob = 0; // number of symbol and their probability
            public string code = ""; //the code they are going to have
        }



        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 256; i++)
            {
                alphabet[i] = 0;
            }
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
                Opening();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                Opening();
            }
        } //open file

        /// <summary>
        /// Считывание файла и заполнение алфавита
        /// </summary>
        void Opening()
        {
            Cfile = new List<symbol>();
            System.IO.BinaryReader sr = new
                   System.IO.BinaryReader(new FileStream(openFileDialog1.FileName,FileMode.Open)); // got file to code
            N = Convert.ToInt32(sr.BaseStream.Length); //got amount of symbols in file to code                                                             
            sr.BaseStream.Position = 0;
         //   messfile = new byte[N];
         //   label3.Text = "";
            byte buf = default(byte);
            for (int i = 0; i < N; i++)
            {
             //   messfile[i] = (sr.ReadByte());//Convert.ToByte
             //   label3.Text += messfile[i]; //getting the message
                buf = sr.ReadByte();
                alphabet[(int)buf]++;
                if (Cfile.Count == 0) //if first symbol
                {
                    symbol c = new symbol();
                    c.ch = buf;
                    c.code = "";
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
                            if (Cfile[j].ch == buf) // if exists 
                            {
                                Cfile[j].prob++;
                                done = true;
                            }
                            else
                            if (j == Cfile.Count - 1)
                            {
                                symbol c = new symbol();
                                c.ch = buf;
                                c.code = null;
                                c.prob = 1;
                                Cfile.Add(c);
                                done = true;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < 256; i++)
                Console.Write(alphabet[i]);
            Console.WriteLine("");
            sr.Close();
            Sort();
        }


        //функция сравнения алфавита по частоте
        public class NameComparer : IComparer<symbol>
        {
            public int Compare(symbol k1, symbol k2)
            {
                if (k1.prob > k2.prob) 
                {
                    return -1; //не меняем
                }
                else if (k1.prob < k2.prob)
                {
                    return 1; //меняем
                }

                return 0; // равно
            }

            public int Compare2(symbol k1, symbol k2)
            {
                if (k1.ch > k2.ch)
                {
                    return -1; //не меняем
                }
                else if ((k1.ch > k2.ch) && (k1.prob == k2.prob))
                {
                    return 1; //меняем
                }
                return 0; // равно
            }
        }
        //сортировка алфавита по частоте
        void Sort()
        {
            NameComparer cn = new NameComparer();
            Cfile.Sort(cn);
            CreateTree();
            Packing();
        }

        //создание дерева и его корня
        void CreateTree()
        {
            tree = new Tree<symbol>();

            int sum = 0;
            for (int i = 0; i < Cfile.Count; i++)
            {
                sum += Cfile[i].prob;
            }
            tree.weight = sum;
            tree.parent = null;
            BuildingTree(0, Cfile.Count - 1, sum, tree);
        }

        //построение дерева рекурсивно
        void BuildingTree(int Li, int Ri, int sum, Tree<symbol> node)
        //index Left, Right, Sum of all elements, parent
        {
            int Lb = Li, Rb = Ri;
            int sumL = Cfile[Li].prob, sumR = sum - Cfile[Li].prob; //sums of the parts

            for (int LeftIndex = Li + 1; (LeftIndex < Cfile.Count) && (sumL + Cfile[LeftIndex].prob <= sumR - Cfile[LeftIndex].prob); LeftIndex++) //while  sums are not close
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
                if (sumL > 0)
                    if (Li - Lb == 0) //if consists of one element
                    {
                        node1.leave = Cfile[Lb];
                        node1.leave.code += "0";
                       // node1.leave.byteList.Add(0);
                      //  node1.leave.byteList.Add(Convert.ToByte(0, 2));
                        Console.WriteLine("leave L - " + node1.leave.ch + " " + node1.leave.code + " byte " );
                    }
                    else
                    {
                        Console.WriteLine("Recursion L -");
                        for (int i = Lb; i <= Li; i++)
                        {
                            Cfile[i].code += "0";
                         //   Cfile[i].byteList.Add(0);
                        }
                        BuildingTree(Lb, Li, sumL, node1);

                    }
                if (sumR > 0)
                    if (Rb - (Li + 1) == 0) //if consists of one element
                    {

                        node2.leave = Cfile[Rb];
                        node2.leave.code += "1";
                     //   node2.leave.byteList.Add(1);
                        Console.WriteLine("Leave R - " + node2.leave.ch + " " + node2.leave.code + " byte " );
                    }
                    else
                    {
                        Console.WriteLine("Recursion R - ");
                        for (int i = Li + 1; i <= Rb; i++)
                        {
                            Cfile[i].code += "1";
                       //     Cfile[i].byteList.Add(1);
                        }
                        BuildingTree(Li + 1, Rb, sumR, node2);

                    }
            }
        }

        void Packing()
        {
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, "output.bin");
            try
            {// Delete the file if it exists.
                if (File.Exists(path))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(path);
                }
                System.IO.BinaryReader sr = new
                  System.IO.BinaryReader(new FileStream(openFileDialog1.FileName, FileMode.Open)); // got file to code
                N = Convert.ToInt32(sr.BaseStream.Length); //got amount of symbols in file to code                                                             
                sr.BaseStream.Position = 0;
                // Create the file.
                using (FileStream fs = File.Create(path))
                {
                    // длина входного потока
                    byte[] bytes = BitConverter.GetBytes(N);
                    fs.Write(bytes, 0, bytes.Length);

                    //таблица кодирования
                    for (int i = 0; i < 256; i++)
                    {
                        bytes = BitConverter.GetBytes(alphabet[i]);
                        fs.Write(bytes, 0, bytes.Length);
                    }

                    //сообщение
                    int b=0;
                    int count=0;
                    for (int t = 0; t < N; t++)
                    {
                        byte buf = sr.ReadByte();
                        for (int i = 0; i < Cfile.Count; i++)
                        {

                            if (buf == Cfile[i].ch)
                            {
                                // Byte[] info = new UTF8Encoding(true).GetBytes(Cfile[i].code);
                                //  fs.WriteByte(Convert.ToByte(Cfile[i].code));
                                //fs.Write(Cfile[i].byteList.ToArray(), 0, Cfile[i].byteList.Count);
                                for (int j = 0; j < Cfile[i].code.Length; j++)
                                {
                                    b += Convert.ToInt32(Cfile[i].code[j]) - 48;
                                    count++;
                                    if (count == 8)
                                    {
                                        byte k = Convert.ToByte(b % 256);
                                        fs.WriteByte(k);
                                        count = 0;
                                        b = 0;
                                    }
                                    else
                                    {
                                        b = Convert.ToInt32(b) << 1;
                                    }
                                }
                                break;
                            }


                        }

                    }
                    if (count != 0)
                    {
                        byte k = Convert.ToByte(b % 256);
                        fs.WriteByte(k);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

    }
}
