using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Globalization;
using System.Collections.Generic;

namespace fl8_1
{
    public partial class Form1 : Form
    {
        private GLControl _glControl;

        private int _amountOfVertices;

        private List<int> _vertexIndices = new List<int>();
        private List<int> _normalIndices = new List<int>();

        private int _program;

        private int _uModelMatrixLocation;
        private int _uViewMatrixLocation;
        private int _uProjMatrixLocation;

        private List<float> _vertices = new List<float>();
        private List<float> _colors = new List<float>();
        private List<float> _normals = new List<float>();

        private PointF _firstPoint = new PointF(-0.5f, -0.5f);
        private PointF _secondPoint = new PointF(0.5f, -0.5f);
        private PointF _thirdPoint = new PointF(-0.5f, 0.5f);

        private Color4 _objectColor = new Color4(0.1f, 0.7f, 0.2f, 1f);
        private Color4 _backgroundColor = new Color4(0.5f, 0.5f, 0.75f, 1f);

        // переменные для координат мыши и флаги - для отслеживания нажатия
        private float _mouseClickXPos = 0;
        private float _mouseClickYPos = 0;
        private bool _isLeftDown = false;
        private bool _isRightDown = false;

        private SimpleRotator _simpleRotator;

        private float _objectXPos = 0f;
        private float _objectYPos = 0f;
        private float _objectZPos = 0f;

        int coord_index = 0, Fase_index = 0;
        float coords;
        int Index;
        struct Coord
        {
            public float x;
            public float y;
            public float z;
            public Coord(string[] args)
            {
                //x = Convert.ToSingle(args[0]);
                x = float.Parse(args[0], CultureInfo.InvariantCulture.NumberFormat);
                y = float.Parse(args[1], CultureInfo.InvariantCulture.NumberFormat);
                z = float.Parse(args[2], CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        struct Poligons
        {
            //Coord point;
            public int v1, v2, v3; // три вершины полигона
            public Poligons(string t1, string t2, string t3)
            {
                v1 = int.Parse(t1);
                v2 = int.Parse(t2);
                v3 = int.Parse(t3);
            }
        }

        struct TempMass
        {
            public string w1, w2, w3;
            public TempMass(string[] args)
            {
                w1 = args[0];
                w2 = args[1];
                w3 = args[2];
            }
        }


        Coord Mymass;
        Poligons models;
        int point = 0;
        int p = 0;



        private enum ActivePlane    // Активная плоскость 0 - XY, 1 - XZ
        {
            XY, XZ
        }

        private ActivePlane _activePlane = ActivePlane.XY;

        public Form1()  // инициализация формы
        {
            InitializeComponent(); // инициализация компонентов

            _glControl = new GLControl();
            _glControl.Load += GLControl_Load;
            _glControl.Paint += GLControl_Paint;
            _glControl.MouseDown += GLControl_MouseDown;
            _glControl.MouseUp += GLControl_MouseUP;
            _glControl.MouseMove += GLControl_MouseMove;
            _glControl.Dock = DockStyle.Fill;
            _tableLayoutPanel.Controls.Add(_glControl, 0, 0);
            _tableLayoutPanel.SetColumnSpan(_glControl, 2);
        }

        //private Vector2 GetOpenGLMouseCoord(Vector2 coord)
        //{
        //    Vector2 newCoord = new Vector2();
        //    newCoord.X = coord.X / _glControl.Width * 2f;
        //    newCoord.X -= 1f;
        //    newCoord.Y = (2 - 2 * coord.Y / _glControl.Height);
        //    newCoord.Y -= 1;
        //    return newCoord;
        //}

        private void GLControl_MouseMove(object sender, MouseEventArgs e)   // перетаскивание объекта мышью
        {
            //Vector2 mouseCoord = GetOpenGLMouseCoord(new Vector2(e.X, e.Y));
            //_simpleRotator.DoMouseDrag(mouseCoord.X, mouseCoord.Y);

            _simpleRotator.DoMouseDrag(e.X, e.Y);
            _glControl.Invalidate();
            //if (_isRightDown)
            //{
            //    //Console.WriteLine(string.Format("({0}, {1})", e.X - _mouseClickXPos, _mouseClickYPos - e.Y));
            //    _objectXPos = currentMouseXPos;
            //    _objectYPos = currentMouseYPos;
            //}
            //_glControl.Invalidate();
        }

        private void GLControl_MouseDown(object sender, MouseEventArgs e)    // контроль нажатия кнопок мыши
        {
            //Vector2 mouseCoord = GetOpenGLMouseCoord(new Vector2(e.X, e.Y));
            //_simpleRotator.DoMouseDown(mouseCoord.X, mouseCoord.Y);

            _simpleRotator.DoMouseDown(e.X, e.Y);
            _glControl.Invalidate();
            //if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            //{
            //    Vector2 mouseCoord = GetOpenGLMouseCoord(new Vector2(e.X, e.Y));
            //    _mouseClickXPos = mouseCoord.X;
            //    _mouseClickYPos = mouseCoord.Y;
            //    _objectXPos = _mouseClickXPos;
            //    _objectYPos = _mouseClickYPos;

            //    if (e.Button == MouseButtons.Left)  //  если левая кнопка
            //    {
            //        _isLeftDown = true; // установить левый флаг
            //    }
            //    else       // иначе
            //    {
            //        _isRightDown = true;    // установить правый
            //    }
            //}
            //_glControl.Invalidate();
        }

        private void GLControl_MouseUP(object sender, MouseEventArgs e)    // контроль отжатия кнопок мыши
        {
            _simpleRotator.DoMouseUp();
            _glControl.Invalidate();

            //if (e.Button == MouseButtons.Left)
            //{
            //    _isLeftDown = false;    // снять флаг с левой кнопки
            //}
            //else if (e.Button == MouseButtons.Right)
            //{
            //    _isRightDown = false;   // снять флаг с правой кнопки
            //}
        }

        private void GLControl_Load(object sender, EventArgs e)     // активация функций перед рисованием 
        {
            _simpleRotator = new SimpleRotator(DrawObject, _glControl.Width, _glControl.Height, 35f);

            // активация функций перед рисованием 
            GL.Enable(EnableCap.DepthTest);

            // Пример плоскости из двух треугольников
            // 
            // (v4)
            // v2------v3
            // | \     |
            // |   \   |
            // |     \ |
            // v0------v1

            // Координаты вершин.
            float[] vertices = new float[]
            {
                // v0, v1, v2
                -0.5f, -0.5f, 0f, 0.5f, -0.5f, 0f, -0.5f, 0.5f, 0f,
                // v3-v4-v5
                0.5f, 0.5f, 0f, -0.5f, -0.5f, 0f, 0.5f, -0.5f, 0f,
                // v6-v7-v8
                0.5f, 0.5f, 0f, 0.5f, -0.5f, 0f, 0.5f, -0.5f, -1f
            };

            // Цвета вершин.
            // Пример. v0: (0f, 1f, 0f) - зелёный цвет
            float[] colors = new float[]
            {
                // v0-v1-v2
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f,
                // v3-v4-v5
                0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,
                // v6-v7-v8
                1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f
            };

            // Индексы вершин
            int[] indices = new int[]
            {
                // v0-v1-v2
                0, 1, 2,
                // v3-v4-v5
                3, 4, 5,
                // v6-v7-v8
                6, 7, 8
            };

            // Инициализируем шейдеры и получаем ссылку на шейдерную программу
            _program = ShaderProgram.InitAndGetShaderProgramId(
                "Shaders/ColorVertexShader.glsl",
                "Shaders/ColorFragmentShaser.glsl");

            // Получаем ссылки на матрицы из вершинного шейдера
            _uModelMatrixLocation = GetUniformLocation("uModelMatrix");
            _uViewMatrixLocation = GetUniformLocation("uViewMatrix");
            _uProjMatrixLocation = GetUniformLocation("uProjMatrix");
            if (_uModelMatrixLocation < 0 || _uViewMatrixLocation < 0 || _uProjMatrixLocation < 0) return;

            // Создаём матрицы
            Matrix4 modelMatrix =
                Matrix4.CreateScale(5f) *
                Matrix4.CreateTranslation(new Vector3(0f, 0f, 0f));
            Matrix4 viewMatrix = _simpleRotator.GetViewMatrix();
            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50), _glControl.Width / (float)_glControl.Height, 0.1f, 1000f);
            // Передаём в вертексный шейдер
            GL.UniformMatrix4(_uModelMatrixLocation, false, ref modelMatrix);
            GL.UniformMatrix4(_uViewMatrixLocation, false, ref viewMatrix);
            GL.UniformMatrix4(_uProjMatrixLocation, false, ref projMatrix);
        }

        private int GetUniformLocation(string name)
        {
            int location = GL.GetUniformLocation(_program, name);
            if (location < 0)
            {
                MessageBox.Show("Не могу получить ссылку на переменную с именем " + name);
                return -1;
            }
            return location;
        }

        private void DrawObject()
        {
            // Забираем матрицу из Simple Rotator и передаём в вертексный шейдер
            Matrix4 viewMatrix = _simpleRotator.GetViewMatrix();
            GL.UniformMatrix4(_uViewMatrixLocation, false, ref viewMatrix);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _amountOfVertices);
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height); // определение размера окна для вывода прорисовки
            // Set background color
            GL.ClearColor(_backgroundColor);    // становка цвета фона
            // Set object color
            GL.Color4(_objectColor);    // установка цвета 
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_amountOfVertices != 0)
            {
                DrawObject();
            }

            _glControl.SwapBuffers();
        }

        //private void btnSetBGColor_Click(object sender, EventArgs e) // кнопка вызова диалога выбора цвета
        //{
        //    using (var colorDialog = new ColorDialog())
        //    {
        //        if (colorDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            _backgroundColor = colorDialog.Color;
        //        }
        //    }
        //    _glControl.Invalidate();
        //}

        //private void btnSetObjColor_Click(object sender, EventArgs e) // кнопка вызова диалога выбора цвета
        //{
        //    using (var colorDialog = new ColorDialog())
        //    {
        //        if (colorDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            _objectColor = colorDialog.Color;
        //        }
        //    }
        //    _glControl.Invalidate();
        //}

        private void setObjectColorToolStripMenuItem_Click(object sender, EventArgs e)  // смена цвета объекта
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    _objectColor = colorDialog.Color;
                }
            }
            _glControl.Invalidate();
        }

        private void setBGColorToolStripMenuItem_Click(object sender, EventArgs e)  // смена цвета фона
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    _backgroundColor = colorDialog.Color;
                }
            }
            _glControl.Invalidate();
        }

        private void loadOBJToolStripMenuItem_Click(object sender, EventArgs e) // кнопка менню загрузки файла obj
        {
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*";

            // This will get the current WORKING directory (i.e. \bin\Debug)
            string workingDirectory = Environment.CurrentDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result
            // This will get the current PROJECT directory
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            OpenFile.InitialDirectory = projectDirectory;

            OpenFile.Title = "Please select an OBJ file to encrypt.";
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("File is opened!");
                string FileName = OpenFile.FileName;    // сохраняем имя файла
                //string AllText = File.ReadAllText(FileName); // весь текст - вроде не надо
                //StreamReader StroksFile = new StreamReader(FileName);
                //StroksFile.Close();
                //GLControl_Paint(sender, 0);
                // переход на функцию рид_файл_обж
                Read_File_Obj(FileName);

                _glControl.Invalidate();
            }
            else
            {
                MessageBox.Show("File not opened!");
            }
        }

        //void Read_File_Obj (string name_file)
        //{
        //    int counter = 0;
        //    string line;

        //    //var StroksFile = File.ReadAllLines(name_file);
        //    StreamReader StroksFile = new StreamReader(name_file);
        //    int File_Size = StroksFile.ReadToEnd().Length;
        //    //for (int i=0; i< len; i++)
        //    // создание динамических массивов???
        //    float Tmp_Coords = File_Size;
        //    int Tmp_FaseArray = File_Size;
        //    coord_index = 0;
        //    //int[] arr = new int[number];
        //    for (int i = 0; i < File_Size; i++)
        //    {
        //        line = StroksFile.ReadLine(); //Convert.ToString(File.ReadLines(name_file));

        //        int countpoint = 0;
        //        int countfase = 0;
        //        while ((line != null))  // не работает!!!
        //        {   // вершины
        //            char[] line_copy = line.ToCharArray(); // преобразуем строку в массив
        //            countpoint = line.Length;
        //            if ((line_copy[0] == 'v') && (line_copy[1] == ' '))
        //            {
        //                line_copy[0] = ' '; // обнуление символа 
        //                line_copy[1] = ' '; // обнуление символа
        //                line = new string(line_copy);
        //                line.TrimStart(' ');
        //                countpoint = line.Length;
        //            }
        //            counter++;
        //        }
        //        StroksFile.Close();

        //    }
        void Read_File_Obj(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            string materialFileName = null;
            List<Vector3> vertCoords = new List<Vector3>();
            string currentMaterial = null;
            Dictionary<string, Color4> colors = new Dictionary<string, Color4>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }

                string[] values = lines[i].Split(' ');

                if (values[0] == "v")
                {
                    float x = float.Parse(values[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(values[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(values[3], CultureInfo.InvariantCulture);
                    vertCoords.Add(new Vector3(x, y, z));
                }

                if (values[0] == "vn")
                {
                    float x = float.Parse(values[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(values[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(values[3], CultureInfo.InvariantCulture);
                    _normals.Add(x);
                    _normals.Add(y);
                    _normals.Add(z);
                }

                if (values[0] == "usemtl")
                {
                    currentMaterial = values[1];
                }

                if (values[0] == "f")
                {
                    List<int> face = new List<int>();
                    List<int> normals = new List<int>();

                    for (int j = 1; j < values.Length; j++)
                    {
                        string[] w = values[j].Split('/');
                        face.Add(int.Parse(w[0]) - 1);
                        normals.Add(int.Parse(w[2]) - 1);
                    }

                    _vertexIndices.AddRange(face);
                    _normalIndices.AddRange(normals);
                }

                if (values[0] == "mtllib")
                {
                    materialFileName = values[1];

                    string materialFilePath = Path.Combine(Path.GetDirectoryName(fileName), materialFileName);

                    string[] mtlLines = File.ReadAllLines(materialFilePath);

                    string currMat = null;

                    for (int k = 0; k < mtlLines.Length; k++)
                    {
                        if (mtlLines[i].StartsWith("#"))
                        {
                            continue;
                        }

                        string[] mtlValues = mtlLines[k].Split(' ');

                        if (mtlValues[0] == "newmtl")
                        {
                            colors.Add(mtlValues[1], new Color4());
                            currMat = mtlValues[1];
                        }

                        if (mtlValues[0] == "Kd")
                        {
                            float r = float.Parse(mtlValues[1], CultureInfo.InvariantCulture);
                            float g = float.Parse(mtlValues[2], CultureInfo.InvariantCulture);
                            float b = float.Parse(mtlValues[3], CultureInfo.InvariantCulture);
                            for (int c = 0; c < 3; c++)
                            {
                                _colors.Add(r);
                                _colors.Add(g);
                                _colors.Add(b);
                            }
                        }
                    }
                }
            }

            foreach (var index in _vertexIndices)
            {
                _vertices.Add(vertCoords[index].X);
                _vertices.Add(vertCoords[index].Y);
                _vertices.Add(vertCoords[index].Z);
            }

            if (materialFileName == null)
            {
                MessageBox.Show("Файл .mtl не найден");
                return;
            }

            // Инициализируем вершины и получаем количество вершин
            _amountOfVertices = VertexBuffers.InitAndGetAmountOfVertices(
                _program, _vertices.ToArray(), _colors.ToArray());


            //List<Vector3> vertices = new List<Vector3>();
            //List<Vector3> poligons = new List<Vector3>();

            //for (int i = lines.Length - 1; i >= 0; i--)
            //{
            //    if (lines[i].StartsWith("#"))
            //    {
            //        continue;
            //    }

            //    if (lines[i].StartsWith("v "))
            //    {
            //        string coordsString = lines[i].Substring(2);
            //        string[] coords = coordsString.Split(' ');
            //        float x = float.Parse(coords[0], CultureInfo.InstalledUICulture.NumberFormat);
            //        float y = float.Parse(coords[1], CultureInfo.InstalledUICulture.NumberFormat);
            //        float z = float.Parse(coords[2], CultureInfo.InstalledUICulture.NumberFormat);
            //        vertices.Add(new Vector3(x, float.Parse(coords[1]), float.Parse(coords[2])));
            //    }
            //}

            //string[] Lines;
            //string Vert = "v "; //вершины
            //string Poli = "f "; // полигоны
            //int counterV = 0;
            //int counterF = 0;
            //Lines = System.IO.File.ReadAllLines(name_file); // все строки в массив из файла
            //Coord[] Line_V = new Coord[Lines.Length];
            //string[] Line_F = new string[Lines.Length];
            //TempMass[] TM = new TempMass[Lines.Length];
            //// Poligons[] Line_F = new Poligons(Lines.Length); 
            //foreach (string Line in Lines)  // для каждой строки в массиве
            //{
            //    if (Line.StartsWith(Vert) != false) // если вершина
            //    {
            //        // отсечь первые два символа
            //        string Line1 = Line.Substring(2);
            //        Line_V[counterV] = new Coord (Line1.Split(' ')); // разбить на структуру координаты в структуру 
            //        counterV++;

            //    }

            //    if (Line.StartsWith(Poli) != false) // если полигон
            //    {
            //        // отсечь первые два символа
            //        string Line1 = Line.Substring(2);
            //        // разбить по " " 
            //        TM[counterF] = new TempMass(Line1.Split(' '));
            //        TM[counterF].w1 = TM[counterF].w1.Split('/')[0];
            //        TM[counterF].w2 = TM[counterF].w2.Split('/')[0];
            //        TM[counterF].w3 = TM[counterF].w3.Split('/')[0];
            //        string TempF = TM[counterF].w1.ToString() + " " + TM[counterF].w2.ToString() + " " + TM[counterF].w3.ToString();  // формируем строку "2 1 4"
            //        //Line_F[counterF] = new Poligons(TM[counterF].w1, TM[counterF].w2, TM[counterF].w3);  // или TempF.Split(' ')); //  в TempF лежит строка типа "2 1 4", разбиваем ее по " " в структуру 
            //        counterF++;
            //        // разбить по "/" в структуру полигоны
            //        // 
            //    }
            //}

        }

    }

    //private void btnLoadFileOBJ_Click(object sender, EventArgs e)
    //{
    //    FileStream FileObj = new FileStream("D:/12345.obj", FileMode.Open, FileAccess.Read);
    //    StreamReader StrObj = new StreamReader(FileObj, System.Text.Encoding.Default);
    //    string[] MasStr;
    //    int i = 0;
    //    try
    //    {
    //        string[] MasStr1 = StrObj.ReadToEnd().Split('\n');
    //        MasStr1 = MasStr1.Where(n => !string.IsNullOrEmpty(n)).ToArray();
    //    }
    //    //MasStr = StrFileObj.ReadToEnd();
    //    //char[] chm = MasStr.ToCharArray(); 

}
