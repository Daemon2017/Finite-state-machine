using System;
using System.Drawing;
using System.Windows.Forms;

namespace tko
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        float neededTemperature,
              currentTemperature;

        float probableTemperature,
              previousProbableTemperature;

        int bestState;

        int[] usedState = { 0 };

        float CmN1,//Второе слева                               
              CmN2;//Первое слева

        float NmC1,//Второе справа                   
              NmC2;//Первое справа

        //Вектор возможных состояний
        int[] possibleState = { -2, -1, 0, 1, 2 };

        private void button2_Click(object sender, EventArgs e)
        {
            Graphics g = pictureBox1.CreateGraphics();

            g.Clear(Color.White);

            Pen net = new Pen(Brushes.Black, 1);

            for (int i = 10; i < 480; i += 10)
            {
                g.DrawLine(net, new Point(i, 0), new Point(i, 100));
            }

            Pen figure = new Pen(Brushes.Red, 2);

            //Отрисовываем термы слева
            g.DrawLine(figure, new Point((int)(CmN2 * 10 + 240), 100), new Point((int)((CmN2 + CmN1) / 2 * 10 + 240), 0));
            g.DrawLine(figure, new Point((int)((CmN2 + CmN1) / 2 * 10 + 240), 0), new Point((int)(CmN1 * 10 + 240), 100));

            g.DrawLine(figure, new Point((int)(CmN1 * 10 + 240), 100), new Point((int)(CmN1 / 2 * 10 + 240), 0));
            g.DrawLine(figure, new Point((int)(CmN1 / 2 * 10 + 240), 0), new Point(240, 100));

            //Отрисовываем терм нуля
            g.DrawLine(figure, new Point(240, 0), new Point(240, 100));

            //Отрисовываем термы справа
            g.DrawLine(figure, new Point(240, 100), new Point((int)(NmC1 / 2 * 10 + 240), 0));
            g.DrawLine(figure, new Point((int)(NmC1 / 2 * 10 + 240), 0), new Point((int)(NmC1 * 10 + 240), 100));

            g.DrawLine(figure, new Point((int)(NmC1 * 10 + 240), 100), new Point((int)((NmC1 + NmC2) / 2 * 10 + 240), 0));
            g.DrawLine(figure, new Point((int)((NmC1 + NmC2) / 2 * 10 + 240), 0), new Point((int)(NmC2 * 10 + 240), 100));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int startTemperature;

            //Считываем входные данные
            startTemperature = Convert.ToInt32(numericUpDown1.Value);
            neededTemperature = Convert.ToInt32(numericUpDown2.Value);

            float differenceBetween = neededTemperature - startTemperature;

            currentTemperature = startTemperature;

            bool allChecked = false;

            for (int i = 0; i < possibleState.Length; i++)
            {
                //Если все возможные значения перебраны - разрешаем сохранение наилучшего варианта
                if (i == possibleState.Length - 1)
                {
                    allChecked = true;
                }

                //Если мы только начали работу - альтернативой возможному варианту считаем начальное значение
                if (i == 0)
                {
                    previousProbableTemperature = startTemperature;
                }

                probableTemperature = states(possibleState[i], currentTemperature);
                previousProbableTemperature = quality(probableTemperature, previousProbableTemperature, possibleState[i], allChecked);
            }

            learningAlgorithm(differenceBetween, bestState);

            label4.Text = "Текущая температура: " + currentTemperature.ToString();
            label2.Text = "Значения НЛ: " + CmN2.ToString() + " " + CmN1.ToString() + " " + "0" + " " + NmC1.ToString() + " " + NmC2.ToString();
        }

        //Список возможных состояний
        float states(int stateNumber,
                     float probableTemperature)
        {
            switch (stateNumber)
            {
                case -2:
                    probableTemperature--;
                    probableTemperature--;
                    break;
                case -1:
                    probableTemperature--;
                    break;
                case 0:
                    break;
                case 1:
                    probableTemperature++;
                    break;
                case 2:
                    probableTemperature++;
                    probableTemperature++;
                    break;
            }

            return probableTemperature;
        }

        //Оцениваем качество произведенных изменений
        float quality(float probableTemperature,
                      float previousProbableTemperature,
                      int probableState,
                      bool allChecked)
        {
            //Если предлагаемое изменение УМЕНЬШИЛО разницу между нынешним значением и требуемым
            //а также если оно лучше, чем прошлые предлагаемые изменения
            if ((Math.Abs(neededTemperature - probableTemperature) < Math.Abs(neededTemperature - currentTemperature)) &&
                (Math.Abs(neededTemperature - probableTemperature) < Math.Abs(neededTemperature - previousProbableTemperature)))
            {
                //то всё хорошо, запоминаем и перебираем дальше: вдруг есть значения еще лучше?
                previousProbableTemperature = probableTemperature;
                bestState = probableState;
            }

            //Если все значения перебраны - сохраняем
            if (allChecked == true)
            {
                currentTemperature = previousProbableTemperature;
            }

            return previousProbableTemperature;
        }

        //Обучаем НЛ
        void learningAlgorithm(float differenceBetween,
                               int currentChosenState)
        {
            int notUnicalStates = 0;

            //Проверяем, видели ли мы такое состояние ранее
            for (int i = 0; i < usedState.Length; i++)
            {
                if (currentChosenState == usedState[i])
                {
                    notUnicalStates++;
                }
            }

            //Если данное состояние не уникально
            if (notUnicalStates >= 1)
            {
                //Если разница положительна
                if (differenceBetween > 0)
                {
                    //Если разница больше, чем первое справа значение
                    if (differenceBetween > NmC2)
                    {
                        //Но меньше, чем второе справа значение
                        if (differenceBetween < NmC1)
                        {
                            //Первое справа обновляем
                            NmC2 = NmC1;
                            //Второе справа обновляем
                            NmC1 = differenceBetween;
                        }

                        //Да еще и больше, чем второе справа значение
                        else
                        {
                            //Первое справа обновляем
                            NmC2 = differenceBetween;
                        }
                    }

                    //Если разница меньше, чем первое справа значение
                    else
                    {
                        //Да еще и меньше, чем второе справа значение
                        if (differenceBetween < NmC1)
                        {
                            //Второе справа обновляем
                            NmC1 = differenceBetween;
                        }
                    }
                }

                //Если разница отрицательна
                if (differenceBetween < 0)
                {
                    //Если разница меньше, чем первое слева значение
                    if (differenceBetween < CmN2)
                    {
                        //Но больше, чем второе слева значение
                        if (differenceBetween > CmN1)
                        {
                            //Первое слева обновляем
                            CmN2 = CmN1;
                            //Второе слева обновляем
                            CmN1 = differenceBetween;
                        }

                        //Да еще и меньше, чем второе слева значение
                        else
                        {
                            //Первое слева обновляем
                            CmN2 = differenceBetween;
                        }
                    }

                    //Если разница больше, чем первое слева значение
                    else
                    {
                        //Если разница больше, чем второе слева значение
                        if (differenceBetween > CmN1)
                        {
                            //Второе слева обновляем
                            CmN1 = differenceBetween;
                        }
                    }
                }
            }

            //Если уникально
            else
            {
                //Добавляем это состояние в список известных
                Array.Resize(ref usedState, usedState.Length + 1);
                usedState[usedState.Length - 1] = currentChosenState;

                //Если разница положительна
                if (differenceBetween > 0)
                {
                    //Если разница больше, чем второе справа значение
                    if (differenceBetween > NmC1 && NmC1 > 0)
                    {
                        //Да еще и больше, чем первое справа значение
                        if (differenceBetween > NmC2)
                        {
                            //Первое справа обновляем
                            NmC2 = differenceBetween;
                        }
                    }

                    //Если разница меньше, чем второе справа значение
                    else
                    {
                        //Но больше, чем первое справа значение
                        if (differenceBetween > NmC2)
                        {
                            //Первое справа обновляем
                            NmC2 = NmC1;
                        }

                        //Второе справа обновляем
                        NmC1 = differenceBetween;
                    }
                }

                //Если разница отрицательна
                if (differenceBetween < 0)
                {
                    //Если разница меньше, чем второе слева значение
                    if (differenceBetween < CmN1 && CmN1 < 0)
                    {
                        //Да еще и меньше, чем первое слева значение
                        if (differenceBetween < CmN2)
                        {
                            //Первое слева обновляем
                            CmN2 = differenceBetween;
                        }
                    }

                    //Если разница больше, чем второе слева значение
                    else
                    {
                        //Второе слева обновляем
                        CmN1 = differenceBetween;
                    }
                }
            }
        }
    }
}