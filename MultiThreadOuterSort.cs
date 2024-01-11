using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OuterSorts
{
    public class MultiThreadOuterSort
    {
        private string? _headers;
        private readonly int sortingColumnIndex;
        private long lineCountInChain, _segments;
        private readonly Type[] typesOfTableColumns =
            { typeof(int), typeof(string), typeof(DateTime) };

        public MultiThreadOuterSort(int chosenField)
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
            using var fileD = new StreamWriter("D.csv");
            string? currentRecord = fileA.ReadLine();
            //переменная flag поменяла свой тип с bool на int, т.к. теперь нам нужно больше
            //двух значений
            int flag = 0;
            int counter = 0;
            while (currentRecord is not null)
            {
                if (counter == lineCountInChain)
                {
                    //Случай, когда мы дошли до конца цепочки
                    counter = 0;
                    flag = (flag + 1) % 3;
                    _segments++;
                }

                switch (flag)
                {
                    case 0:
                        fileB.WriteLine(currentRecord);
                        break;
                    case 1:
                        fileC.WriteLine(currentRecord);
                        break;
                    case 2:
                        fileD.WriteLine(currentRecord);
                        break;
                }

                currentRecord = fileA.ReadLine();
                counter++;
            }
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
            while (elementB is not null || elementC is not null || elementD is not null)
            {
                string? currentRecord;
                int flag;

                if (CheckElement(elementB, counterB) && !CheckElement(elementC, counterC) && !CheckElement(elementD, counterD))
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
                else if (CheckElement(elementC, counterC) && !CheckElement(elementB, counterB) && !CheckElement(elementD, counterD))
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
                else if (CheckElement(elementD, counterD) && !CheckElement(elementB, counterB) && !CheckElement(elementC, counterC))
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
                else if (counterB == lineCountInChain && counterC == lineCountInChain)
                {
                    //Случай, когда цепочки закончились в файлах В и С
                    currentRecord = elementD;
                    flag = 2;
                }
                else if (counterB == lineCountInChain && counterD == lineCountInChain)
                {
                    //Случай, когда цепочки закончились в файлах В и D
                    currentRecord = elementC;
                    flag = 1;
                }
                else if (counterC == lineCountInChain && counterD == lineCountInChain)
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

                if (counterB != lineCountInChain || counterC != lineCountInChain || counterD != lineCountInChain)
                {
                    continue;
                }

                //Обнуляем все 3 счётчика, если достигли конца всех цепочек во всех файлах
                counterC = 0;
                counterB = 0;
                counterD = 0;
            }

            lineCountInChain *= 3;
        }
        private bool CheckElement(string? element, int counter)
            => element is null || counter == lineCountInChain;

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
            for( int i=1; i < elements.Length; i++)
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
        public void Sort()
        {
            while (true)
            {
                SplitToFiles();

                if (_segments == 1)
                {
                    break;
                }

                MergeFiles();
            }
        }
    }
}
