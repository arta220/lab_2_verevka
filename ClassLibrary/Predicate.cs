using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    // класс предиката
    // ожидаются методы:
    //определние вида предиката
    //методы для работы с операциями в предикатах

    public class Predicate
    {
        // проверка выполнимости
        public string FeasibilityCheck(string assigment)
        {
            string feasibility = "";

            return feasibility;
        }

        //операция инверсии
        public string Inversion(string assigment)
        {
            return assigment;
        }

        //операция дизъюнкции
        public string Disjunction(string assigment)
        {
            return assigment;
        }
        // операция конъюнкции
        public string Conjunction(string assigment)
        {
            return assigment;
        }
        //операция имликации
        public string Implication(string assigment)
        {
            return assigment;
        }

        //операция эквивалентности
        public string Equivalence(string assigment)
        {
            return assigment;
        }
    }
}
