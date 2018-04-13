using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace UdarProject
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string file;
                if (args.Length > 0)
                    file = args[0];
                else
                {
                    Console.WriteLine("Программа запущена без параметров.");
                    return;
                }
                if (file == "-e")
                {
                    Console.WriteLine("Файл лога ошибок:");
                    Console.WriteLine(ErrorLogger.FileName);
                    return;
                }
                var qm = QueueManager.LoadFromFile(file);
                Console.TreatControlCAsInput = false;
                Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true;
                        qm.Stop();
                    };
                var qms = new QMSetting()
                {
                    preExecute = taskCount =>
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.CursorVisible = false;
                            Console.Clear();
                            Console.Title = "Axial2D. Всего задач: " + taskCount.ToString();
                        },
                    postExecute = () =>
                        {
                            Console.CursorVisible = true;
                            Console.WriteLine();
                        },
                    preCICalc = (nomber, name, startTime) =>
                        {
                            string dt = startTime.ToString("dd.MM.yyyy HH:mm:ss.fff").PadRight(62);
                            if (name != null)
                                name = name.Trim();
                            else
                                name = "<Без названия>";
                            if (name.Length > 62)
                                name = name.Substring(0, 59) + "...";
                            if (name.Length < 62)
                                name = name.PadRight(62);
                            if (nomber == 1)
                                Console.WriteLine("+----+--------+---------------------------------------------------------------+");
                            else
                                Console.WriteLine("+----+========+===============================================================+");
                            Console.WriteLine("|" + nomber.ToString().PadLeft(4) + "| Задача | " + name + "|");
                            Console.WriteLine("+----+--------+---------------------------------------------------------------+");
                            Console.WriteLine("     | Запуск | " + dt + "|");
                            Console.WriteLine("     +----+---+----------------+----------------------------------------------+");
                            Console.WriteLine("     |  0%|                    |                                              |");
                            Console.WriteLine("     +----+---+----------------+----------------------------------------------+");
                            Console.WriteLine("     | Статус | Выполняется                                                   |");
                            Console.WriteLine("     +--------+---------------------------------------------------------------+");
                        },
                    CIPCRef = (percent, time) =>
                        {
                            Console.CursorLeft = 6;
                            Console.CursorTop = Console.CursorTop - 4;
                            Console.Write(percent.ToString().PadLeft(3));
                            Console.CursorLeft = 33;
                            Console.Write(time.ToString("dd.MM.yyyy HH:mm:ss.fff"));
                            Console.CursorLeft = 11;
                            Console.Write(string.Empty.PadLeft(percent / 5, '█'));
                            Console.CursorTop = Console.CursorTop + 4;
                        },
                    CISUpd = (status) =>
                        {
                            Console.CursorTop = Console.CursorTop - 2;
                            Console.CursorLeft = 16;
                            switch (status)
                            {
                                case QMSetting.WorkStatus.Complete:
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.Write("Выполнено успешно            ");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                                case QMSetting.WorkStatus.Run:
                                    Console.Write("Выполняется                  ");
                                    break;
                                case QMSetting.WorkStatus.Error:
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.Write("Ошибка!                      ");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                                case QMSetting.WorkStatus.Lag:
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.Write("Процесс молчит более 10 минут");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                            }
                            Console.CursorTop = Console.CursorTop + 2;
                        },
                    postCIGood = () =>
                        {
                            ;
                        },
                    postCIError = (msg) =>
                        {
                            //Console.CursorTop = Console.CursorTop + 2;
                            Console.CursorLeft = 0;
                            Console.WriteLine("     | " + msg.PadRight(71).Substring(0, 71) + "|");
                            Console.WriteLine("     +------------------------------------------------------------------------+");
                        },
                    postCIAny = () =>
                        {
                            Console.CursorLeft = 0;
                            Console.CursorTop = Console.CursorTop - 1;
                        }
                };
                qm.Execute(qms);
            }
            catch (Exception e)
            {
                Console.WriteLine(ErrorLogger.Log(e));
            }
            //Console.WriteLine();
            /*Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey(true);*/
        }
    }
}
