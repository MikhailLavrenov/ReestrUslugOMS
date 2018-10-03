using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReestrUslugOMS
{
    public partial class sp_ReportFactResult
    {
        public string GetValue (enDataType enu)
        {
            string result = null;

            if (enu== enDataType.КодВрача)
                result = this.CodDoc;
            else if (enu== enDataType.КодОтделения)
                result = this.CodOtd;
            else if (enu == enDataType.КодУслуги)
                result = this.CodUsl;

            return result;
        }

        public double GetValue(enResultType enu)
        {
            double result=0;

            if (enu == enResultType.Услуг)
                result = (double)Usl;
            else if (enu == enResultType.КоличествоУслуг)
                result = (double)ColUsl;
            else if (enu == enResultType.УЕТ)
                result = (double)UET;
            else if (enu == enResultType.УслугДо3Лет)
                result = (double)UslDo3;
            else if (enu == enResultType.УслугПосле3Лет)
                result = (double)UslPosle3;
            else if (enu == enResultType.ДиспансеризацияРазВ2Года)
                result = (double)DispIn2Y;

            return result;
        }

    }
}
