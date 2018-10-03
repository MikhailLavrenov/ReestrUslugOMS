using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReestrUslugOMS;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Формула - интерпретируемые данные по которым происходит расчет значений в отчете
    /// </summary>
    public partial class dbtFormula
    {
        /// <summary>
        /// Копирование текущего экземпляра класса без создания ссылок на связанные классы
        /// </summary>
        /// <returns>Новый экземпляр класса</returns>
        public dbtFormula PartialCopy()
        {
            var newInstance = new dbtFormula();

            newInstance.NodeId = NodeId;
            newInstance.ResultType = ResultType;
            newInstance.DataType = DataType;
            newInstance.DataValue = string.Copy(DataValue);
            newInstance.Operation = Operation;
            newInstance.FactorValue = FactorValue;
            newInstance.DateBegin = DateBegin;
            newInstance.DateEnd = DateEnd;

            return newInstance;
        }

        /// <summary>
        /// Производит заданную операцию с Входным числом и учетом Вида операции 2ой ноды, на пересечении которых рассчитывается значение
        /// </summary>
        /// <param name="value">Входное число</param>
        /// <param name="secondOperation">Вид операции 2ой ноды</param>
        /// <returns></returns>
        public double Calculate(double value, enOperation? secondOperation)
        {
            if (FactorValue != null)
            {
                if (Operation == enOperation.Умножение)
                    value = value * (double)FactorValue;
                else if (Operation == enOperation.Деление)
                    value = value / (double)FactorValue;
                else if ((Operation == enOperation.УмножениеВыборочно) && (secondOperation != enOperation.НеПрименятьВыборочно))
                    value = value * (double)FactorValue;
            }
            return value;
        }



    }
}
