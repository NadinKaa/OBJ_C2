using OpenTK;
using System;

namespace fl8_1
{
    // Объект класса SimpleRotator нужен для вычисления положение камеры
    // Новое положение камеры считывается с помощью метода GetViewMatrix
    // В данном объекте используются обработчики мыши. Каждый раз, когда
    // пользователь нажимает, передвигает и отпускает мышь, то после отпускания
    // вызывается callback функция, которая должна быть функцией перерисовки
    // То есть при создании объекта класса SimpleRotator нужно в качестве
    // callback-фукнции передать ссылку на функцию draw
    class SimpleRotator
    {
        // Данные переменные хранят текущий угол поворота вокруг OX и OY
        private float _rotateX;
        private float _rotateY;

        // В переменные prevX и prevY мы сохраняем координату клика мыши
        private float _prevX, _prevY;

        // Ограничение угла поворота вокруг оси X
        private readonly float _xLimit = 85f;

        // Эти коээфициенты нужны для корректировки в расчётах углов поворота
        private float _degreesPerPixelX;
        private float _degreesPerPixelY;

        // Расстояние от камеры до объекта
        private float _viewDistance;

        // Функция обратного вызова, чтобы отрисовать объект после
        // изменения положения камеры
        private Action DrawCallback;

        public SimpleRotator(
            Action DrawCallback, int glControlWidth, int glControlHeight,
            float viewDistance, float rotX = 0f, float rotY = 0f)
        {
            _rotateX = rotX;
            _rotateY = rotY;

            _degreesPerPixelX = 90f / glControlHeight;
            _degreesPerPixelY = 180f / glControlWidth;

            _viewDistance = viewDistance;

            this.DrawCallback = DrawCallback;
        }

        // IsDragging хранит true если пользователь нажал кнопку мыши и
        // не отпускает. Как только пользователь опустит кнопку мыши, то в переменную
        // dragging будет записано значение false
        private bool _isDragging = false;

        // Метод getViewMatrix возвращает новую матрица вида, которая определяет
        // положение камеры
        public Matrix4 GetViewMatrix()
        {
            float cosX = (float)Math.Cos(_rotateX / 180f * (float) Math.PI);
            float sinX = (float)Math.Sin(_rotateX / 180f * (float)Math.PI);
            float cosY = (float)Math.Cos(_rotateY / 180f * (float)Math.PI);
            float sinY = (float)Math.Sin(_rotateY / 180f * (float)Math.PI);

            Matrix4 matrix1 = Matrix4.LookAt(new Vector3(0f, 0f, 10f), Vector3.Zero, new Vector3(0, 1, 0));

            Matrix4 matrix = new Matrix4();

            matrix.Column0 = new Vector4(cosY, 0, sinY, 0f);
            matrix.Column1 = new Vector4(sinX * sinY, cosX, -sinX * cosY, 0f);
            matrix.Column2 = new Vector4(-cosX * sinY, sinX, cosX * cosY, -_viewDistance);
            matrix.Column3 = new Vector4(0f, 0f, 0f, 1f);

            //matrix.Column0 = new Vector4(cosY, sinX * sinY, -cosX * sinY, 0f);
            //matrix.Column1 = new Vector4(0f, cosX, sinX, 0f);
            //matrix.Column2 = new Vector4(sinY, -sinX * cosY, cosX * cosY, 0f);
            //matrix.Column3 = new Vector4(0f, 0f, _viewDistance, 1f);

            return matrix;
        }

        // Метод DoMouseDown срабатывает в момент клика мыши (клика с удерживанием)
        public void DoMouseDown(float x, float y)
        {
            // Сохраняем информацию, что кнопка мыши зажата и удерживается
            _isDragging = true;

            // Эти координаты клика мыши будут сохранены один раз в момент клика с
            // удерживанием и будут сохраняться до момента отпускания кнопки мыши
            _prevX = x;
            _prevY = y;
        }

        // Метод DoMouseDrag срабатывает, когда пользователь изменил положение мыши с
        // зажатой кнопкой мыши
        public void DoMouseDrag(float x, float y)
        {
            // Если мы двигаем мышь без зажатой кнопки мыши, то выходим
            if (!_isDragging)
            {
                return;
            }

            // Рассчитывам новые углы поворота
            float newRotX = _rotateX + _degreesPerPixelX * (y - _prevY);
            float newRotY = _rotateY + _degreesPerPixelY * (x - _prevX);

            // Корректируем угол поворота вокруг оси X, чтобы он оставался
            // всегда в промежутке от -xLimit до xLimit
            newRotX = Math.Max(-_xLimit, Math.Min(_xLimit, newRotX));

            // Сохраняем текущие координаты мыши
            _prevX = x;
            _prevY = y;

            // Если обновлённые углы поворота не равны старым углам поворота, то
            // выполняем этот блок кода
            if (newRotX != _rotateX || newRotY != _rotateY)
            {
                // Сохраняем обновлённые углы поворота
                _rotateX = newRotX;
                _rotateY = newRotY;

                // Вызываем функцию обратного вызова, которая считает новое
                // положение камеры (матрицу вида) с помощью метода getViewMatrix
                DrawCallback();
            }
        }

        // Метод DoMouseUp срабатывает в момент отпускания мыши
        public void DoMouseUp()
        {
            // Если на момент отпускания мыши не было перемещений мыши, то сразу выходим
            if (!_isDragging)
            {
                return;
            }

            // Если были перемещения мыши с зажатой кнопкой мыши, то при отпускании
            // сохраняем статус, что эти перемещения завершены
            _isDragging = false;
        }
    }
}
