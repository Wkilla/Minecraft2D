using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minecraft2D
{
    public partial class Form1 : Form
    {
        // Константы для размеров блоков, размеров мира и физических параметров
        private const int BlockSize = 20; // Размер одного блока в пикселях
        private const int WorldHeight = 30, WorldWidth = 80; // Размеры игрового мира в блоках
        private const int JumpForce = -4, Gravity = 1, MaxFallSpeed = 1; // Физические параметры движения
        private int[,] world = new int[WorldHeight, WorldWidth]; // Двумерный массив, представляющий мир

        // Координаты игрока и его состояние
        private int playerX = 5, playerY = 12, velocityY = 0;
        private bool isOnGround = false, isJumping = false;

        // Выбранный блок для размещения и высота игрока
        private int selectedBlock = 1;
        private const int PlayerHeight = 3; // Высота игрока в блоках

        // Конструктор формы
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load; // Подписка на событие загрузки формы
            InitializeWorld(); // Инициализация мира
        }

        private void Form1_Load(object sender, EventArgs e) => timer1.Start(); // Запуск таймера при загрузке

        private void timer1_Tick(object sender, EventArgs e)
        {
            HandleMovement(); // Обработка движения игрока
            pictureBox1.Invalidate(); // Перерисовка игрового экрана
        }

        private void HandleMovement()
        {
            // Применение гравитации, если игрок не на земле
            if (!isOnGround)
            {
                velocityY = Math.Min(velocityY + Gravity, MaxFallSpeed); // Ускорение под действием гравитации
            }

            int newY = playerY + velocityY; // Вычисление новой координаты Y игрока

            if (velocityY > 0) // Падение
            {
                if (IsCollision(newY, playerX)) // Проверка на столкновение с объектами
                {
                    while (IsCollision(newY, playerX)) newY--; // Регулировка положения для предотвращения провала
                    isOnGround = true; // Игрок на земле
                    isJumping = false;
                    velocityY = 0;
                    playerY = newY;
                }
                else
                {
                    isOnGround = false;
                    playerY = newY;
                }
            }
            else if (velocityY < 0) // Прыжок
            {
                if (IsCollision(newY, playerX))
                {
                    velocityY = 0;
                    isJumping = false;
                }
                else
                {
                    playerY = newY;
                }
            }

            isOnGround = IsGrounded(playerY, playerX); // Обновление статуса нахождения на земле
        }

        private bool IsGrounded(int y, int x)
        {
            // Проверка на наличие блока под ногами игрока
            return y + 1 < WorldHeight && world[y + 1, x] != 0;
        }

        private bool IsCollision(int y, int x)
        {
            // Проверка на столкновение с границами мира или другими блоками
            if (y >= WorldHeight || y < 0 || x < 0 || x >= WorldWidth)
                return true;

            // Проверка каждой части высоты игрока на наличие столкновений
            for (int i = 0; i < PlayerHeight; i++)
            {
                if (y - i >= 0 && world[y - i, x] != 0)
                    return true;
            }
            return false;
        }

        private void MovePlayer(int dx)
        {
            // Перемещение игрока влево или вправо
            int newX = playerX + dx;
            if (!IsCollision(playerY, newX)) playerX = newX;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // Обработка кликов мыши для размещения или удаления блоков
            int x = e.X / BlockSize, y = e.Y / BlockSize;
            if (x < 0 || x >= WorldWidth || y < 0 || y >= WorldHeight) return;

            if (e.Button == MouseButtons.Left && world[y, x] == 0)
                world[y, x] = selectedBlock; // Размещение блока
            else if (e.Button == MouseButtons.Right && world[y, x] != 0)
                world[y, x] = 0; // Удаление блока

            pictureBox1.Invalidate(); // Перерисовка экрана
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Обработка нажатий клавиш для управления игроком
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
                selectedBlock = e.KeyCode - Keys.D1 + 1; // Выбор типа блока

            if (e.KeyCode == Keys.A)
                MovePlayer(-1); // Перемещение влево
            else if (e.KeyCode == Keys.D)
                MovePlayer(1); // Перемещение вправо
            else if (e.KeyCode == Keys.W && isOnGround) // Прыжок, если на земле
            {
                isJumping = true;
                isOnGround = false;
                velocityY = JumpForce; // Задание начальной скорости прыжка
            }

            pictureBox1.Invalidate(); // Перерисовка экрана
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Рисование игрового мира и игрока
            DrawWorld(e.Graphics);
            DrawPlayer(e.Graphics);
        }

        private void DrawWorld(Graphics g)
        {
            // Отрисовка блоков мира
            for (int y = 0; y < WorldHeight; y++)
            {
                for (int x = 0; x < WorldWidth; x++)
                {
                    if (world[y, x] != 0)
                    {
                        g.FillRectangle(GetBrushForBlock(world[y, x]), x * BlockSize, y * BlockSize, BlockSize, BlockSize);
                        g.DrawRectangle(Pens.Black, x * BlockSize, y * BlockSize, BlockSize, BlockSize);
                    }
                }
            }
        }

        private Brush GetBrushForBlock(int blockType)
        {
            // Выбор цвета блока по его типу
            switch (blockType)
            {
                case 1: return Brushes.Green;
                case 2: return Brushes.Brown;
                case 3: return Brushes.Gray;
                case 4: return Brushes.Red;
                case 5: return Brushes.Blue;
                case 6: return Brushes.Yellow;
                case 7: return Brushes.Purple;
                case 8: return Brushes.Orange;
                case 9: return Brushes.Pink;
                default: return Brushes.White;
            }
        }

        private void DrawPlayer(Graphics g)
        {
            // Рисование игрока (головы, тела, ног)
            int x = playerX * BlockSize, y = playerY * BlockSize;

            g.FillRectangle(Brushes.SaddleBrown, x + 6, y - 8, 8, 8); // Голова
            g.FillRectangle(Brushes.Cyan, x + 4, y, 12, 12); // Тело
            g.FillRectangle(Brushes.DarkBlue, x + 4, y + 12, 4, 9); // Левая нога
            g.FillRectangle(Brushes.DarkBlue, x + 12, y + 12, 4, 9); // Правая нога
        }

        private void InitializeWorld()
        {
            // Инициализация мира: верхний слой — трава, нижний — земля
            for (int x = 0; x < WorldWidth; x++) world[14, x] = 1; // Трава
            for (int y = 15; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++) world[y, x] = 2; // Земля
        }
    }
}
