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
        public string name;


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
                name = openFileDialog1.FileName;
                Opening();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                name = openFileDialog1.FileName;
                Opening();
            }

        } //open file

        /// <summary>
        /// Считывание файла и заполнение алфавита
        /// </summary>
        void Opening()
        {
            label2.Text = "Подождите, идет подготовка!";
            Cfile = new List<symbol>();
            System.IO.BinaryReader sr = new
                   System.IO.BinaryReader(new FileStream(openFileDialog1.FileName, FileMode.Open)); // got file to code
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
            // for (int i = 0; i < 256; i++)
            //           Console.Write(alphabet[i]);
            //       Console.WriteLine("");
            progressBar1.Maximum = N*2;
            sr.Close();
            Sort();
            Packing();
            label2.Text = "Успешно закодировано!";
            progressBar1.Value = 0;
        }


        /// <summary>
        ///  функция сравнения алфавита по частоте
        /// </summary>
        public class NameComparer : IComparer<symbol>
        {
            public int Compare(symbol k1, symbol k2)
            {
                if (k1.prob > k2.prob)
                {
           //         Console.WriteLine(k1.prob + " > " + k2.prob);
                    return -1; //меньше
                }
                else if (k1.prob < k2.prob)
                {
           //         Console.WriteLine(k1.prob + " < " + k2.prob + " or " + k1.ch + " < " + k2.ch);
                    return 1; //больше
                }
                else return 0; // равно
            }

        }
        /// <summary>
        /// функция сравнения алфавита по байту
        /// </summary>
        public class NameComparer1 : IComparer<symbol>
        {
            public int Compare(symbol k1, symbol k2)
            {
                if (k1.ch > k2.ch)
                {
          //          Console.WriteLine(k1.ch + " > " + k2.ch);
                    return 1; //больше
                }
                else if (k1.ch < k2.ch)
                {
           //         Console.WriteLine(k1.ch + " < " + k2.ch);
                    return -1; //меньше
                }
                else return 0; // равно
            }

        }
        /// <summary>
        /// сортировка алфавита по частоте
        /// </summary>
        void Sort()
        {
            NameComparer1 cn1 = new NameComparer1();
            Cfile.Sort(cn1);
            NameComparer cn = new NameComparer();
            Cfile.Sort(cn);
            CreateTree();
        }

        /// <summary>
        /// создание дерева и его корня
        /// </summary>
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

        /// <summary>
        ///   построение дерева рекурсивно
        /// </summary>
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
         //   Console.WriteLine("SumL - " + sumL);
            Tree<symbol> node2 = new Tree<symbol>();
            node2.weight = sumR;
         //   Console.WriteLine("SumR - " + sumR);
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
                        progressBar1.Value++;
                        // node1.leave.byteList.Add(0);
                        //  node1.leave.byteList.Add(Convert.ToByte(0, 2));
                        //        Console.WriteLine("leave L - " + node1.leave.ch + " " + node1.leave.code + " byte ");
                    }
                    else
                    {
                    //    Console.WriteLine("Recursion L -");
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
                        progressBar1.Value++;
                        //      Console.WriteLine("Leave R - " + node2.leave.ch + " " + node2.leave.code + " byte ");
                    }
                    else
                    {
                      //  Console.WriteLine("Recursion R - ");
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
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, name + ".fan");//name+".fan""output.bin"
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
                  System.IO.BinaryReader(new FileStream(name, FileMode.Open)); // got file to code
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
                    int b = 0;
                    int count = 0;
                    for (int t = 0; t < N; t++)
                    {
                        progressBar1.Value++;
                        byte buf = sr.ReadByte();
                        for (int i = 0; i < Cfile.Count; i++)
                        {
                           
                            if (buf == Cfile[i].ch)
                            {
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
                                  //      Console.WriteLine("Pushed symbol " + k);
                                    }
                                    else
                                    {
                                        b = Convert.ToInt32(b) << 1;
                                    }
                                   
                                }
                          //      Console.WriteLine("Read symbol " + buf);
                           //     Console.WriteLine("Code symbol " + Cfile[i].code);
                                 break;
                            }


                        }

                    }
                    if (count != 0)
                    {
                        byte k = Convert.ToByte(b % 256);
                        fs.WriteByte(k);
                   //     Console.WriteLine("Pushed symbol " + k);
                    }

                }
                sr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        void Decoder(string name)
        {
            FileStream fs = new FileStream(name+".fan", FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);
            byte b = 0;
            int k;
            int count = 0;
            N = 0;
           
            N = r.ReadInt32();
            progressBar1.Maximum=N;
            
            Cfile = null;
            int per = 0;
            Cfile = new List<symbol>();
            for (int i = 0; i < 256; i++)
            {
                //progressBar1.Value++;
                per = r.ReadInt32();
                if (per != 0)
                {
                   
                    symbol c = new symbol();
                    c.ch = (byte)i;
                    c.prob = per;
                    Cfile.Add(c);
                }
            }

            tree = null;
            Sort();
            Tree<symbol> current = tree;
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, name);
            try
            {// Delete the file if it exists.
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                int g=0;
                progressBar1.Value = 0;
                using (FileStream stream = File.Create(path))
                {
                    while (g < N)
                    {
                       
                        if (count == 0)
                        {
                            b = r.ReadByte();
                            
                         //   Console.WriteLine("Read " + b);
                            count = 8;
                        }

                        count--;
                        k = ((Convert.ToInt32(b)) >> count) & 1;
                        if (k == 0)//в левое поддерево
                        {
                            current = current.Left;
                         //   Console.WriteLine("to left");
                        }
                        else {
                            current = current.Right;
                         //   Console.WriteLine("to right");
                        }

                        if (current.leave != null)
                        {
                            stream.WriteByte(current.leave.ch);
                            g++;
                            progressBar1.Value=N;
                         //   Console.WriteLine("Write " + current.leave.ch);
                            current = tree;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                label2.Text = "Подождите, идет подготовка!";
                openFileDialog1 = new OpenFileDialog();
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox2.Text = openFileDialog1.FileName;
                    name = openFileDialog1.FileName;
                }
                int len = name.Length - 4;
                name = name.Remove(len);
                Decoder(name);

                label2.Text = "Успешно декодировано!";
                progressBar1.Value = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
