using System;
using System.IO;
using System.Numerics;



namespace ConsoleApplication1
{
    class Program
    {
       /// Нужно использовать именно такой PI (20 знаков, после запятой).
       public  const double PI = 3.14159265358979323846;
       public static int N; // размерность входящей матрицы

       /// Пока что просто берем константы из cpp-файла, описания сообщат позже.
       /// m - номер первого канала
       /// n - номер максимальное несущей (частоты)
       /// N - количество отсчетов на интервале ортогональности (основа преобразования Фурье)
       /// L - количество отсчетов на защитном интервале
       /// lt - это столбцы входной матрицы
       /// from_lt начальная точка отсчета
       /// until_lt конечная точка отсчета
       public static  int n, L,m,from_lt,until_lt;
       public static  int R; // длительность импульсной реакции (зависит от размера файла)
       /// g - вектор импульсных реакций
       public static  double[] g = new double[N];
       /// power - вектор мощностей сигналов
       public static  double[] power = new double[n];
       /// PSD - маска сигнала
       public static  double[] PSD = new double[n];
       /// njuArray - выходной массив (результат)
       /// cur_njuArray
       /// Размерность nju матриц должна быть N + L.
       public static  double[,] njuArray= new double[N,N];
       public static  double[,] cur_njuArray = new double[N,N];


        /// Расчет корреляции сигнала
        /// raznitsa - разница между отсчетами
        double B(int raznitsa)
        {
            double summ = 0;
            for (int p = 1; p <= n; p++)
            {
                summ += power[p + m - 1] * Math.Cos(PI * raznitsa * (p + m - 1) / n);
            }
            return summ;
        }


        /// Главный виновник торжества - расчитывает каждый элемент в матрицах nju*
        double Nju(int k, int q, int lt)
        {
            if (lt == from_lt)
            {
                double element = 0;

                if (k <= R - 2 - lt && q <= R - 2 - lt)
                {
                    for (int i = k + lt + 1; i <= R - 1; i++)
                    {
                        for (int j = q + lt + 1; j <= R - 1; j++)
                        {
                            element += g[i] * g[j] * B(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }

                else if (k >= N + L - lt && q >= N + L - lt)
                {
                    for (int i = 0; i <= k + lt - N - L; i++)
                    {
                        for (int j = 0; j <= q + lt - N - L; j++)
                        {
                            element += g[i] * g[j] * B(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }

                else if (k <= R - 2 - lt && q >= N + L - lt)
                {
                    for (int i = k + lt + 1; i <= R - 1; i++)
                    {
                        for (int j = 0; j <= q + lt - N - L; j++)
                        {
                            element += g[i] * g[j] * B(2 * N + L + k + j - q - i);
                        }
                    }
                    return element;
                }

                else if (k >= N + L - lt && q <= R - 2 - lt)
                {
                    for (int i = 0; i <= k + lt - N - L; i++)
                    {
                        for (int j = q + lt + 1; j <= R - 1; j++)
                        {
                            element += g[i] * g[j] * B(2 * N + L + q + i - k - j);
                        }
                    }
                    return element;
                }

                else
                {
                    return 0;
                }
            }

            else
            {
                if (k == N - 1 || q == N - 1)
                {
                    double element = 0;

                    if (k <= R - 2 - lt && q <= R - 2 - lt)
                    {
                        for (int i = k + lt + 1; i <= R - 1; i++)
                        {
                            for (int j = q + lt + 1; j <= R - 1; j++)
                            {
                                element += g[i] * g[j] * B(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }

                    else if (k >= N + L - lt && q >= N + L - lt)
                    {
                        for (int i = 0; i <= k + lt - N - L; i++)
                        {
                            for (int j = 0; j <= q + lt - N - L; j++)
                            {
                                element += g[i] * g[j] * B(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }

                    else if (k <= R - 2 - lt && q >= N + L - lt)
                    {
                        for (int i = k + lt + 1; i <= R - 1; i++)
                        {
                            for (int j = 0; j <= q + lt - N - L; j++)
                            {
                                element += g[i] * g[j] * B(2 * N + L + k + j - q - i);
                            }
                        }
                        return element;
                    }

                    else if (k >= N + L - lt && q <= R - 2 - lt)
                    {
                        for (int i = 0; i <= k + lt - N - L; i++)
                        {
                            for (int j = q + lt + 1; j <= R - 1; j++)
                            {
                                element += g[i] * g[j] * B(2 * N + L + q + i - k - j);
                            }
                        }
                        return element;
                    }

                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return njuArray[k + 1,q + 1];
                }
            }
        }

        /// Расчет мощности интерференционной помехи
        double Interf(int p)
        {
            double summ = 0;
            for (int k = 0; k <= N - 1; k++)
            {
                for (int q = 0; q <= N - 1; q++)
                {
                    summ += njuArray[k,q] * Math.Cos(2 * PI * (p + m - 1) * (k - q) / N);
                }
            }
            return summ;
        }

        /// Расчет мощности сигнала
        /// Переделать расчет на API от класса Complex
        /// Возможно, нужно явно(!) подключить Complex в настройках проекта.
        double Signal(int p)
{
            Complex summ = new Complex();
    const Complex  J = new Complex(0, 1);
    for (int i = 0; i <= N - 1; i++)
    {
        summ += g[i] * Math.Exp((-J) * 2.0 * PI * (double)(p + m - 1) * (double)i / (double)N);
    }
    return Math.Pow(Math.Abs(summ), 2) * N * N / 2.0 * power[p + m - 1];
}

        /// Соотношение сигнал/шум
        double Ratio(int p)
        {
            double ratio;
            ratio = Math.Sqrt(Interf(p) / Signal(p));
            return ratio * 100.0;
        }

        /// Программа расчета для традиционных систем. Пример, файл - TWP_GFAST_150m_TR.cpp
        /// Для начала, реализовать выбор входных данных только через конфиги.
        /// Ввводные файлы должны быть прописаны в конфигах, а также должна быть возможность
        /// вводить кастомные имена через консоль.
        /// Начать работу с ввода информации (заполнение массивов).
        static void Main(string[] args)
        {
            Console.Write("Enter N ");
           N =int.Parse(Console.ReadLine());
           Console.Write("Enter n ");
           n = int.Parse(Console.ReadLine());

           Console.Write("Enter L ");
           L = int.Parse(Console.ReadLine());
           Console.Write("Enter m ");
           m = int.Parse(Console.ReadLine());
           Console.Write("Enter R ");
           R = int.Parse(Console.ReadLine());
           Console.Write("Enter Start lt ");
           from_lt = int.Parse(Console.ReadLine());
           Console.Write("Enter end lt ");
           until_lt = int.Parse(Console.ReadLine());

           /// При чтении файлов, случайным образом возникает ошибка выхода за границы массива.
           /// Особенно, это происходит на 254 строке.
           /// N. B. Ошибка не возникает когда выполняется чтение без остальной логики.

           //"IRGFAST_twp_150m954"//заменить
           /// 954 и есть R - длительность импульсной реакции
           using (StreamReader sr = new StreamReader(@"IRGFAST_twp_150m954"))
           {
               string line;
               while ((line = sr.ReadLine()) != null)
               {
                   string[] text = line.Split(' ');

                   for (int i = 0; i < N - 1; i++)
                   {
                       g[i] = Double.Parse(text[i]);
                   }
               }
           }

           /// GfastPSD - это спектральная маска сигнала
           /// Этот файл существует по-умолчанию (почти не меняется). Другими словами, он обязан быть при выполнении.
           using (StreamReader sr = new StreamReader(@"GfastPSD_dB_0_2047"))
           {
               string line;
               while ((line = sr.ReadLine()) != null)
               {
                   string[] text = line.Split(' ');
                   for (int i = 0; i < m + n + 1; i++)
                   {
                       PSD[i] = Double.Parse(text[i]);

                       /// не редактируй это, оставь как есть
                       power[i] = Math.Pow(10, 0.1 * (PSD[i] + 80));  // прибавляем 80, чтобы не работать со слишком маленькими числами (результат работы программы зависит только от соотношения мощностей на несущих)
                   }
               }
           }

           /// Дописать логику вывода результатов.
           /// Файловый вывод (как в оригинале).
        }
    }
}
