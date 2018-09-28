using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReestrUslugOMS;

namespace ReestrUslugOMS
{
    public partial class dbtFormula
    {
        public dbtFormula PartialCopy()
        {
            var newInstance = new dbtFormula();

            newInstance.NodeId = NodeId;
            newInstance.ResultType = ResultType;
            newInstance.DataType = DataType;
            newInstance.DataValue = String.Copy(DataValue);
            newInstance.Operation = Operation;
            newInstance.FactorValue = FactorValue;
            newInstance.DateBegin = DateBegin;
            newInstance.DateEnd = DateEnd;

            return newInstance;
        }

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
