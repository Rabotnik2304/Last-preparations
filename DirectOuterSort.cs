using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterSorts
{
    public class DirectOuterSort
    {
        //Сюда будет сохраняться строка с заголовками таблицы
        private string? _headers;
        //поле lineCountInChain указывает, сколько элементов в одной отсортированной цепочке 
        //на данный момент.
        //поле _segments показывает, сколько отсортированных цепочек у нас есть в обоих
        //файлах в сумме
        private long lineCountInChain, _segments;
        //здесь в нужном порядке будут храниться типы данных каждого столбца 
        //(понадобится для корректного сравения элементов)
        private readonly Type[] _typesOfTableColumns
          = { typeof(int), typeof(string), typeof(DateTime) };
        //Индекс выбранного поля, по которому будем сортировать
        private readonly int sortingColumnIndex;

        public DirectOuterSort(int chosenField)
        {
            sortingColumnIndex = chosenField;
            lineCountInChain = 1;
        }
        private void SplitToFiles()
        {
            _segments = 1;
            using var fileA = new StreamReader("A.csv");
            _headers = fileA.ReadLine();

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");

            //В этой переменной будет храниться очередная считанная из исходного файла запись
            string? currentRecord = fileA.ReadLine();
            bool flag = true;
            int counter = 0;
            //цикл прекратится, когда мы дойдём до конца исходного файла
            while (currentRecord is not null)
            {
                //дошли до конца цепочки, переключаемся на запись новой
                if (counter == lineCountInChain)
                {
                    counter = 0;
                    flag = !flag;
                    _segments++;
                }

                if (flag)
                {
                    //Запись отправляется в подфайл В
                    fileB.WriteLine(currentRecord);
                }
                else
                {
                    //Запись отправляется в подфайл С
                    fileC.WriteLine(currentRecord);
                }

                //считываем следующую запись
                currentRecord = fileA.ReadLine();
                counter++;
            }
        }
        private void MergePairs()
        {
            using var writerA = new StreamWriter("A.csv");
            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");

            //Не забываем вернуть заголовки таблицы на своё место, в начало исходного файла
            writerA.WriteLine(_headers);

            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();

            int counterB = 0;
            int counterC = 0;
            //Итерации будут происходить, когда 
            while (elementB is not null || elementC is not null)
            {
                string? currentRecord;
                bool flag = false;

                //Обрабатываем случай, когда закончился весь файл B, или цепочка из данной пары 
                //в нём
                if (elementB is null || counterB == lineCountInChain)
                {
                    currentRecord = elementC;
                }
                else if (elementC is null || counterC == lineCountInChain) //аналогично предыдущему блоку if, но для подфайла С
                {
                    currentRecord = elementB;
                    flag = true;
                }
                else
                {
                    //Если оба подфайла ещё не закончились, то сравниваем записи по нужному полю
                    if (CompareElements(elementB, elementC))
                    {
                        //Если запись из файла В оказалась меньше
                        currentRecord = elementB;
                        flag = true;
                    }
                    else
                    {
                        //Если запись из файла С оказалась меньше
                        currentRecord = elementC;
                    }
                }

                //Записываем в исходный файл выбранную нами запись
                writerA.WriteLine(currentRecord);

                if (flag)
                {
                    elementB = readerB.ReadLine();
                    counterB++;
                }
                else
                {
                    elementC = readerC.ReadLine();
                    counterC++;
                }

                if (counterB != lineCountInChain || counterC != lineCountInChain)
                {
                    continue;
                }

                //Если серии в обоих файлах закончились, то обнуляем соответствующие счётчики
                counterC = 0;
                counterB = 0;
            }

            lineCountInChain *= 2;
        }

        //Метод сравнения записей по выбранному полю, учитывая его тип данных
        //Вернёт true, если element1 меньше element2
        private bool CompareElements(string? element1, string? element2)
        {
            if (_typesOfTableColumns[sortingColumnIndex].IsEquivalentTo(typeof(int)))
            {
                return int.Parse(element1.Split(';')[sortingColumnIndex])
                  .CompareTo(int.Parse(element2.Split(';')[sortingColumnIndex])) < 0;
            }

            if (_typesOfTableColumns[sortingColumnIndex].IsEquivalentTo(typeof(DateTime)))
            {
                return DateTime.Parse(element1.Split(';')[sortingColumnIndex])
                  .CompareTo(DateTime.Parse(element2.Split(';')[sortingColumnIndex])) < 0;
            }

            return string.Compare(element1.Split(';')[sortingColumnIndex],
                                  element2.Split(';')[sortingColumnIndex],
                                  StringComparison.Ordinal) < 0;
        }
        public void Sort()
        {
            while (true)
            {
                //Разбиваем записи на подфайлы
                SplitToFiles();
                //Если после разделения цепочка осталась одна, значит, записи в файле отсортированы
                if (_segments == 1)
                {
                    break;
                }

                //Сливаем вместе цепочки из под файлов
                MergePairs();
            }
        }
    }
}
