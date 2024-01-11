using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterSorts
{
    public class MultiThreadNaturalOuterSort
    {
        private string? _headers;
        private readonly int sortingColumnIndex;

        //Нету lineCountInChain и segments, но есть
        private readonly List<int> _series = new();
        
        private readonly Type[] typesOfTableColumns =
            { typeof(int), typeof(string), typeof(DateTime) };

        public MultiThreadNaturalOuterSort(int chosenField)
        {
            sortingColumnIndex = chosenField;
        }
        //Запись похожа на естественную, только флаг - инт, и
        //GetMinOfElements(firstStr, secondStr)==0
        private void SplitToFiles()
        {
            using var fileA = new StreamReader("A.csv");
            _headers = fileA.ReadLine();

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");
            using var fileD = new StreamWriter("D.csv");
            string? firstStr = fileA.ReadLine();
            string? secondStr = fileA.ReadLine();
            //переменная flag поменяла свой тип с bool на int, т.к. теперь нам нужно больше
            //двух значений
            int flag = 0;
            int counter = 0;
            while (firstStr is not null)
            {
                int tempFlag = flag;

                if (secondStr is not null)
                {
                    if (GetMinOfElements(firstStr, secondStr)==0)
                    {
                        counter++;
                    }
                    else
                    {
                        //Если серия прервалась, то записываем её длину в список и обнуляем счётчик
                        tempFlag = (tempFlag+1)%3;
                        _series.Add(counter + 1);
                        counter = 0;
                    }
                }


                switch (flag)
                {
                    case 0:
                        fileB.WriteLine(firstStr);
                        break;
                    case 1:
                        fileC.WriteLine(firstStr);
                        break;
                    case 2:
                        fileD.WriteLine(firstStr);
                        break;
                }

                firstStr = secondStr;
                secondStr = fileA.ReadLine();
                flag = tempFlag;
            }
            _series.Add(counter + 1);
        }

        private void MergeFiles()
        {
            using var writerA = new StreamWriter("A.csv");

            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");
            using var readerD = new StreamReader("D.csv");

            writerA.WriteLine(_headers);

            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();
            string? elementD = readerD.ReadLine();

            int counterB = 0;
            int counterC = 0;
            int counterD = 0;

            // Есть вот эти трое!
            int indexB = 0;
            int indexC = 1;
            int indexD = 2;
            while (elementB is not null || elementC is not null || elementD is not null)
            {
                string? currentRecord;
                int flag;
                // и вот эта проверка
                if (counterB == _series[indexB] && counterC == _series[indexC] && counterD == _series[indexD])
                {
                    //Случай, когда мы дошли до конца серий в обоих подфайлах
                    counterB = 0;
                    counterC = 0;
                    counterD = 0;
                    indexB += 3;
                    indexC += 3;
                    indexD += 3;
                    continue;
                }
                else if (CheckElement(counterB, indexB) && !CheckElement(counterC, indexC) && !CheckElement(counterD, indexD))
                {
                    //Случай, когда цепочка закончилась только в файле B
                    (currentRecord, flag) = GetMinOfElements(
                            elementC,
                            elementD) switch
                    {
                        0 => (elementC, 1),
                        1 => (elementD, 2)
                    };
                }
                else if (CheckElement(counterC, indexC) && !CheckElement(counterB, indexB) && !CheckElement(counterD, indexD))
                {
                    //Случай, когда цепочка закончилась только в файле С
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementD) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementD, 2)
                    };
                }
                else if (CheckElement(counterD, indexD) && !CheckElement(counterB, indexB) && !CheckElement(counterC, indexC))
                {
                    //Случай, когда цепочка закончилась только в файле D
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementC) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementC, 1)
                    };
                }
                // тут теперь Везде CheckElement
                else if (CheckElement(counterB, indexB) && CheckElement(counterC, indexC))
                {
                    //Случай, когда цепочки закончились в файлах В и С
                    currentRecord = elementD;
                    flag = 2;
                }
                else if (CheckElement(counterB, indexB) && CheckElement(counterD, indexD))
                {
                    //Случай, когда цепочки закончились в файлах В и D
                    currentRecord = elementC;
                    flag = 1;
                }
                else if (CheckElement(counterC, indexC) && CheckElement(counterD, indexD))
                {
                    //Случай, когда цепочки закончились в файлах C и D
                    currentRecord = elementB;
                    flag = 0;
                }
                else
                {
                    //Случай, когда не закончилась ни одна из 3 цепочек
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementC,
                            elementD) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementC, 1),
                        2 => (elementD, 2)
                    };
                }

                writerA.WriteLine(currentRecord);

                switch (flag)
                {
                    case 0:
                        elementB = readerB.ReadLine();
                        counterB++;
                        break;
                    case 1:
                        elementC = readerC.ReadLine();
                        counterC++;
                        break;
                    case 2:
                        elementD = readerD.ReadLine();
                        counterD++;
                        break;
                }
            }
        }
        //Вот этот ещё изменился
        private bool CheckElement(int counter, int index)
            => index >= _series.Count || counter == _series[index];

        //Ниже дан ряд методов для поиска минимального из 3 элементов (с учётом того),
        //что некоторые из них могут отсутствовать
        private int GetMinOfElements(params string?[] elements)
        {
            if (elements.Contains(null))
            {
                switch (elements.Length)
                {
                    case 2:
                        return elements[0] is null ? 1 : 0;
                    case 3 when elements[0] is null && elements[1] is null:
                        return 2;
                    case 3 when elements[0] is null && elements[2] is null:
                        return 1;
                    case 3 when elements[1] is null && elements[2] is null:
                        return 0;
                }
            }

            if (typesOfTableColumns[sortingColumnIndex].IsEquivalentTo(typeof(int)))
            {
                return GetMinInt(elements.Select(s => int.Parse(s.Split(';')[sortingColumnIndex])).ToArray());
            }
            if (typesOfTableColumns[sortingColumnIndex].IsEquivalentTo(typeof(DateTime)))
            {
                return GetMinDateTime(elements.Select(s => DateTime.Parse(s.Split(";")[sortingColumnIndex])).ToArray());
            }
            return GetMinString(elements.Select(s => s.Split(";")[sortingColumnIndex]).ToArray());
        }

        private int GetMinString(string[] elements)
        {
            string min = elements[0];
            int minIndex = 0;
            for (int i = 1; i < elements.Length; i++)
            {
                if (string.Compare(elements[i], min, StringComparison.Ordinal) < 0)
                {
                    min = elements[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        private int GetMinInt(int[] elements)
        {
            int minIndex = elements.ToList().IndexOf(elements.Min());

            return minIndex;
        }

        private int GetMinDateTime(DateTime[] elements)
        {
            int minIndex = elements.ToList().IndexOf(elements.Min());

            return minIndex;
        }
        // Этот тоже что логично изменился
        public void Sort()
        {
            while (true)
            {
                _series.Clear();
                SplitToFiles();
                //Если у нас осталась всего одна серия, значит, записи в файле отсортированы
                if (_series.Count == 1)
                {
                    break;
                }

                MergeFiles();
            }
        }
    }
}
